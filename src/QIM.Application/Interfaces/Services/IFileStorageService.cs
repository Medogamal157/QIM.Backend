namespace QIM.Application.Interfaces.Services;

public interface IFileStorageService
{
    /// <summary>Upload a file and return the relative URL path.</summary>
    Task<string> UploadAsync(Stream stream, string fileName, string folder, CancellationToken ct = default);

    /// <summary>Upload multiple files.</summary>
    Task<List<string>> UploadMultipleAsync(IEnumerable<(Stream stream, string fileName)> files, string folder, CancellationToken ct = default);

    /// <summary>Delete a file by its relative URL path.</summary>
    Task DeleteAsync(string relativePath, CancellationToken ct = default);
}
