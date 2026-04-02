namespace QIM.Application.Common.Exceptions;

/// <summary>
/// Thrown when FluentValidation finds invalid request data.
/// </summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException() : base("One or more validation errors occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors) : this()
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string error) : this()
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, [error] }
        };
    }
}
