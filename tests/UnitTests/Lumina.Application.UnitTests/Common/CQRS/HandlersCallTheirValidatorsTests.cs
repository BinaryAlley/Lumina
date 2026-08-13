#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Infrastructure.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
#endregion

namespace Lumina.Application.UnitTests.Common.CQRS;

/// <summary>
/// Contains tests that enforce the convention that command and query handlers always call the validator defined for their command or query.
/// </summary>
[ExcludeFromCodeCoverage]
public class HandlersCallTheirValidatorsTests
{
    private static readonly Assembly s_applicationAssembly = typeof(ICommand).Assembly;
    private static readonly Dictionary<short, OpCode> s_opCodes = BuildOpCodeLookup();

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

    /// <summary>
    /// Checks whether the IL of the specified <paramref name="method"/> contains a call to <see cref="IValidator{TRequest}.Validate"/>.
    /// </summary>
    /// <param name="method">The method whose IL is inspected.</param>
    /// <param name="validatorInterface">The closed validator interface whose <c>Validate</c> method is being looked for.</param>
    /// <returns><see langword="true"/> if the method calls the validator, <see langword="false"/> otherwise.</returns>
    private static bool CallsValidatorMethod(MethodInfo method, Type validatorInterface)
    {
        MethodBody? body = method.GetMethodBody();
        if (body is null)
            return false;

        byte[]? il = body.GetILAsByteArray();
        if (il is null)
            return false;

        Module module = method.Module;
        int offset = 0;
        while (offset < il.Length)
        {
            // read the opcode, handling the 0xFE prefix used by two-byte opcodes
            short opCodeValue = il[offset];
            int opCodeSize = 1;
            if (opCodeValue == 0xFE && offset + 1 < il.Length)
            {
                opCodeValue = (short)((0xFE << 8) | il[offset + 1]);
                opCodeSize = 2;
            }

            if (!s_opCodes.TryGetValue(opCodeValue, out OpCode opCode))
                return false;

            offset += opCodeSize;
            if (offset >= il.Length)
                break;

            if (opCode == OpCodes.Call || opCode == OpCodes.Callvirt)
            {
                int token = ReadInt32(il, offset);
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
            }

            if (opCode.OperandType == OperandType.InlineSwitch)
            {
                int switchCases = ReadInt32(il, offset);
                offset += 4 + (switchCases * 4);
            }
            else
                offset += GetOperandSize(opCode);
        }
        return false;
    }

    /// <summary>
    /// Builds a lookup table of all opcodes, keyed by their numeric value.
    /// </summary>
    /// <returns>The built lookup table.</returns>
    private static Dictionary<short, OpCode> BuildOpCodeLookup()
    {
        Dictionary<short, OpCode> opCodes = [];
        foreach (FieldInfo field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
            if (field.GetValue(null) is OpCode opCode)
                opCodes[opCode.Value] = opCode;
        return opCodes;
    }

    /// <summary>
    /// Gets the size in bytes of the operand of the specified <paramref name="opCode"/>.
    /// </summary>
    /// <param name="opCode">The opcode whose operand size is returned.</param>
    /// <returns>The size of the operand in bytes.</returns>
    private static int GetOperandSize(OpCode opCode)
    {
        return opCode.OperandType switch
        {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineI => 1,
            OperandType.ShortInlineVar => 1,
            OperandType.ShortInlineBrTarget => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineBrTarget => 4,
            OperandType.InlineField => 4,
            OperandType.InlineMethod => 4,
            OperandType.InlineType => 4,
            OperandType.InlineString => 4,
            OperandType.InlineSig => 4,
            OperandType.ShortInlineR => 4,
            OperandType.InlineI => 4,
            OperandType.InlineR => 8,
            OperandType.InlineI8 => 8,
            _ => 0,
        };
    }

    /// <summary>
    /// Reads a 4-byte little-endian integer from the specified offset in the IL stream.
    /// </summary>
    /// <param name="il">The IL byte stream.</param>
    /// <param name="offset">The offset at which to read the integer.</param>
    /// <returns>The read integer.</returns>
    private static int ReadInt32(byte[] il, int offset)
    {
        return il[offset] | (il[offset + 1] << 8) | (il[offset + 2] << 16) | (il[offset + 3] << 24);
    }
}
