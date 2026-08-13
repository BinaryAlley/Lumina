#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Infrastructure.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
#endregion

namespace Lumina.Application.UnitTests.Common.CQRS;

/// <summary>
/// Contains tests that enforce the convention that command and query handlers always call the validator defined for their command or query.
/// </summary>
[ExcludeFromCodeCoverage]
public class HandlersCallTheirValidatorsTests
{
    private static readonly Assembly s_applicationAssembly = typeof(ICommand).Assembly;

    /// <summary>
    /// Ensures every command or query that has a validator has a handler that calls that validator.
    /// </summary>
    [Fact]
    public void AllCommandAndQueryHandlers_WhenTheirRequestHasAValidator_ShouldCallIt()
    {
        // Arrange
        List<string> violations = [];

        // Act
        foreach (Type type in s_applicationAssembly.GetTypes().Where(IsConcreteClass))
        {
            foreach (Type implementedInterface in type.GetInterfaces().Where(IsValidatorInterface))
            {
                Type requestType = implementedInterface.GetGenericArguments()[0];
                if (!IsCommandOrQuery(requestType))
                    continue;

                Type? handlerType = FindHandlerFor(requestType);
                if (handlerType is null)
                {
                    violations.Add($"Validator '{type.FullName}' targets '{requestType.FullName}', which has no command or query handler.");
                    continue;
                }

                if (!CallsValidator(handlerType, requestType))
                    violations.Add($"Handler '{handlerType.FullName}' does not call 'IValidator<{requestType.Name}>.Validate'.");
            }
        }

        // Assert
        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
    }

    private static bool IsConcreteClass(Type type)
    {
        return !type.IsInterface && !type.IsAbstract && !type.IsGenericTypeDefinition;
    }

    private static bool IsValidatorInterface(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IValidator<>);
    }

    private static bool IsCommandOrQueryHandlerInterface(Type type)
    {
        if (!type.IsGenericType)
            return false;
        Type genericTypeDefinition = type.GetGenericTypeDefinition();
        return genericTypeDefinition == typeof(ICommandHandler<,>) || genericTypeDefinition == typeof(IQueryHandler<,>);
    }

    private static bool IsCommandOrQuery(Type type)
    {
        return typeof(ICommand).IsAssignableFrom(type) || typeof(IQuery).IsAssignableFrom(type);
    }

    private static Type? FindHandlerFor(Type requestType)
    {
        return s_applicationAssembly.GetTypes()
            .Where(IsConcreteClass)
            .FirstOrDefault(type => type.GetInterfaces().Any(@interface =>
                IsCommandOrQueryHandlerInterface(@interface) && @interface.GetGenericArguments()[0].Equals(requestType)));
    }

    private static bool CallsValidator(Type handlerType, Type requestType)
    {
        Type validatorInterface = typeof(IValidator<>).MakeGenericType(requestType);
        foreach (Type type in EnumerateTypeHierarchy(handlerType))
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                if (CallsValidatorMethod(method, validatorInterface))
                    return true;
        return false;
    }

    private static IEnumerable<Type> EnumerateTypeHierarchy(Type type)
    {
        yield return type;
        foreach (Type nestedType in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
            foreach (Type nestedDescendant in EnumerateTypeHierarchy(nestedType))
                yield return nestedDescendant;
    }

    private static bool CallsValidatorMethod(MethodInfo method, Type validatorInterface)
    {
        MethodBody? body = method.GetMethodBody();
        if (body is null)
            return false;

        byte[]? il = body.GetILAsByteArray();
        if (il is null)
            return false;

        Module module = method.Module;
        for (int i = 0; i < il.Length; i++)
        {
            // 0x28 is the call opcode and 0x6F is the callvirt opcode, both followed by a 4-byte metadata token
            if (il[i] != 0x28 && il[i] != 0x6F)
                continue;

            if (i + 4 >= il.Length)
                break;

            int token = il[i + 1] | (il[i + 2] << 8) | (il[i + 3] << 16) | (il[i + 4] << 24);
            try
            {
                if (module.ResolveMethod(token) is MethodInfo resolvedMethod &&
                    resolvedMethod.Name == nameof(IValidator<object>.Validate) &&
                    resolvedMethod.DeclaringType != null &&
                    resolvedMethod.DeclaringType.Equals(validatorInterface))
                    return true;
            }
            catch (ArgumentException)
            {
                // the token is not a valid method token, skip it
            }
            catch (BadImageFormatException)
            {
                // the token references corrupt metadata, skip it
            }
            catch (InvalidOperationException)
            {
                // the token cannot be resolved as a method, skip it
            }
            i += 4;
        }
        return false;
    }
}
