namespace Lumina.Domain.Common.Primitives;

/// <summary>
/// Primitive struct for explicitly handling success and failure cases in a functional manner.
/// </summary>
/// <typeparam name="TValue">The type of the success value.</typeparam>
public readonly struct Result<TValue> : IEquatable<Result<TValue>>
{
    private readonly TValue? _value;
    private readonly List<Error>? _errors;

    /// <summary>
    /// Gets a value indicating whether this result represents a success.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether this result represents a failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the success value. Only valid when IsSuccess is true.
    /// </summary>
    public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("Cannot access Value of a failed result.");

    /// <summary>
    /// Gets the list of errors. Only valid when IsFailure is true.
    /// </summary>
    public List<Error> Errors => IsFailure
        ? _errors ?? []
        : throw new InvalidOperationException("Cannot access Errors of a successful result.");

    /// <summary>
    /// Returns the first error, if present.
    /// </summary>
    public Error FirstError => Errors[0];

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> structure.
    /// </summary>
    /// <param name="value">The success value.</param>
    /// <param name="errors">The list of errors.</param>
    /// <param name="isSuccess">Whether this represents a success or failure.</param>
    private Result(TValue? value, List<Error>? errors, bool isSuccess)
    {
        _value = value;
        _errors = errors;
        IsSuccess = isSuccess;
    }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The success value.</param>
    /// <returns>A successful result containing the specified value.</returns>
    public static Result<TValue> Success(TValue value)
    {
        return new Result<TValue>(value, default, true);
    }

    /// <summary>
    /// Creates a failed result with the specified errors.
    /// </summary>
    /// <param name="errors">The errors value.</param>
    /// <returns>A failed result containing the specified error.</returns>
    public static Result<TValue> Failure(List<Error> errors)
    {
        return new Result<TValue>(default, errors, false);
    }

    /// <summary>
    /// Creates a failed result with the specified errors.
    /// </summary>
    /// <param name="errors">The errors value.</param>
    /// <returns>A failed result containing the specified error.</returns>
    public static Result<TValue> Failure(params Error[] errors)
    {
        return new(default, [.. errors], false);
    }

    /// <summary>
    /// Custom implementation operator for converting <paramref name="value"/> to a <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="value">The value to be converted.</param>
    public static implicit operator Result<TValue>(TValue value)
    {
        return Success(value);
    }

    /// <summary>
    /// Custom implementation operator for converting <paramref name="error"/> to a failed <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="error">The error to be converted.</param>
    public static implicit operator Result<TValue>(Error error)
    {
        return Failure(error);
    }

    /// <summary>
    /// Custom implementation operator for converting <paramref name="errors"/> to a failed <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="errors">The errors to be converted.</param>
    public static implicit operator Result<TValue>(List<Error> errors)
    {
        return Failure(errors);
    }

    /// <summary>
    /// Maps the success value to a new type using the provided function.
    /// </summary>
    /// <typeparam name="TNewValue">The type of the new success value.</typeparam>
    /// <param name="mapper">The function to transform the success value.</param>
    /// <returns>A new result with the transformed value, or the original errors.</returns>
    public Result<TNewValue> Map<TNewValue>(Func<TValue, TNewValue> mapper)
    {
        return IsSuccess
            ? Result<TNewValue>.Success(mapper(Value))
            : Result<TNewValue>.Failure(Errors);
    }

    /// <summary>
    /// Chains another operation that returns a Result, allowing for monadic composition.
    /// </summary>
    /// <typeparam name="TNewValue">The type of the new success value.</typeparam>
    /// <param name="binder">The function that returns a new Results.</param>
    /// <returns>The result of the chained operation, or the original error.</returns>
    public Result<TNewValue> Bind<TNewValue>(Func<TValue, Result<TNewValue>> binder)
    {
        return IsSuccess ? binder(Value) : Result<TNewValue>.Failure(Errors);
    }

    /// <summary>
    /// Executes an action if the result is successful, otherwise does nothing.
    /// </summary>
    /// <param name="action">The action to execute on success.</param>
    /// <returns>The original result for chaining.</returns>
    public Result<TValue> OnSuccess(Action<TValue> action)
    {
        if (IsSuccess)
            action(Value);
        return this;
    }

    /// <summary>
    /// Executes an action if the result is a failure, otherwise does nothing.
    /// </summary>
    /// <param name="action">The action to execute on failure.</param>
    /// <returns>The original result for chaining.</returns>
    public Result<TValue> OnFailure(Action<IReadOnlyList<Error>> action)
    {
        if (IsFailure)
            action(Errors);
        return this;
    }

    /// <summary>
    /// Matches the result and executes the appropriate function.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="onSuccess">Function to execute on success.</param>
    /// <param name="onFailure">Function to execute on failure.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<List<Error>, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value) : onFailure(Errors);
    }

    /// <summary>
    /// Determines whether the specified Result is equal to the current Results.
    /// </summary>
    /// <param name="other">The Result to compare with the current Results.</param>
    /// <returns>true if the specified Result is equal to the current Result; otherwise, false.</returns>
    public bool Equals(Result<TValue> other)
    {
        if (IsSuccess != other.IsSuccess)
            return false;

        return IsSuccess
           ? EqualityComparer<TValue>.Default.Equals(Value, other.Value)
           : Errors.SequenceEqual(other.Errors);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current Results.
    /// </summary>
    /// <param name="obj">The object to compare with the current Results.</param>
    /// <returns>true if the specified object is equal to the current Result; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is Result<TValue> other && Equals(other);
    }

    /// <summary>
    /// Determines whether two specified Result instances are equal.
    /// </summary>
    /// <param name="left">The first Result to compare.</param>
    /// <param name="right">The second Result to compare.</param>
    /// <returns>true if the two Result instances are equal; otherwise, false.</returns>
    public static bool operator ==(Result<TValue> left, Result<TValue> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two specified Result instances are not equal.
    /// </summary>
    /// <param name="left">The first Result to compare.</param>
    /// <param name="right">The second Result to compare.</param>
    /// <returns>true if the two Result instances are not equal; otherwise, false.</returns>
    public static bool operator !=(Result<TValue> left, Result<TValue> right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Returns the hash code for this Results.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return IsSuccess
            ? HashCode.Combine(IsSuccess, Value)
            : HashCode.Combine(IsSuccess, Errors);
    }

    /// <summary>
    /// Customized ToString() method.
    /// </summary>
    /// <returns>Custom string value showing relevant data for current result.</returns>
    public override string ToString()
    {
        return IsSuccess
            ? $"Success: {Value?.ToString()}"
            : $"Failure: [{string.Join(", ", Errors.Select(error => error.ToString()))}]";
    }
}
