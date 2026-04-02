namespace QIM.Application.Common.Exceptions;

/// <summary>
/// Thrown when a requested entity is not found.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"{name} with key ({key}) was not found.") { }
}
