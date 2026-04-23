using QIM.Application.Interfaces.Services;

namespace QIM.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private const string UploadsFolder = "uploads";

    public LocalFileStorageService(string webRootPath)
    {
        _basePath = Path.Combine(webRootPath, UploadsFolder);
        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, string folder, CancellationToken ct = default)
    {
        var folderPath = Path.Combine(_basePath, folder);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var uniqueName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var filePath = Path.Combine(folderPath, uniqueName);

        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await stream.CopyToAsync(fileStream, ct);

        return $"/{UploadsFolder}/{folder}/{uniqueName}";
    }

    public async Task<List<string>> UploadMultipleAsync(IEnumerable<(Stream stream, string fileName)> files, string folder, CancellationToken ct = default)
    {
        var urls = new List<string>();
        foreach (var (stream, fileName) in files)
        {
            var url = await UploadAsync(stream, fileName, folder, ct);
            urls.Add(url);
        }
        return urls;
    }

    public Task DeleteAsync(string relativePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return Task.CompletedTask;

        // relativePath like "/uploads/businesses/abc.jpg"
        var fullPath = Path.Combine(_basePath, "..", relativePath.TrimStart('/'));
        fullPath = Path.GetFullPath(fullPath);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}
