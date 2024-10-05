#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Mediator;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Behaviors.Fixtures;

/// <summary>
/// Fixture class for the <see cref="ValidationBehavior"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidationBehaviorFixture : IRequest<ErrorOr<ValidationBehaviorTestResponse>>
{
   
}
