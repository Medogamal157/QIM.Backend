using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QIM.Application.Interfaces.Services;
using QIM.Presentation.Helpers;
using QIM.Shared.Models;

namespace QIM.Presentation.Endpoints;

[Route("api/files")]
[Authorize]
public class FilesController : ApiControllerBase
{
    private readonly IFileStorageService _storage;

    public FilesController(IFileStorageService storage) => _storage = storage;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".csv", ".zip", ".mp4", ".mp3"
    };

    private static string SanitizeFileName(string fileName)
    {
        // Strip path traversal components and return only the file name portion
        var safe = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safe)) safe = "upload";
        return safe;
    }

    private static bool IsAllowedExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        return !string.IsNullOrEmpty(ext) && AllowedExtensions.Contains(ext);
    }

    /// <summary>
    /// Upload a single file.
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string folder = "general")
    {
        if (file is null || file.Length == 0)
            return BadRequest(Result.Failure("No file provided."));

        var safeFileName = SanitizeFileName(file.FileName);

        if (!IsAllowedExtension(safeFileName))
            return BadRequest(Result.Failure($"File type '{Path.GetExtension(safeFileName)}' is not allowed."));

        if (Path.GetExtension(safeFileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase)
            && await file.IsMaliciousPdfAsync())
        {
            return UnprocessableEntity(Result.Failure(
                "The uploaded PDF appears to contain potentially malicious content and cannot be accepted."));
        }

        using var stream = file.OpenReadStream();
        var url = await _storage.UploadAsync(stream, safeFileName, folder);
        return Ok(Result<string>.Success(url, "File uploaded."));
    }

    /// <summary>
    /// Upload multiple files.
    /// </summary>
    [HttpPost("upload-multiple")]
    public async Task<IActionResult> UploadMultiple(List<IFormFile> files, [FromQuery] string folder = "general")
    {
        if (files is null || files.Count == 0)
            return BadRequest(Result.Failure("No files provided."));

        foreach (var f in files)
        {
            var safeExt = SanitizeFileName(f.FileName);
            if (!IsAllowedExtension(safeExt))
                return BadRequest(Result.Failure($"File type '{Path.GetExtension(safeExt)}' is not allowed."));

            if (Path.GetExtension(safeExt).Equals(".pdf", StringComparison.OrdinalIgnoreCase)
                && await f.IsMaliciousPdfAsync())
            {
                return UnprocessableEntity(Result.Failure(
                    $"The file '{f.FileName}' appears to contain potentially malicious content and cannot be accepted."));
            }
        }

        var pairs = new List<(Stream stream, string fileName)>();
        var streams = new List<Stream>();
        foreach (var f in files)
        {
            var s = f.OpenReadStream();
            streams.Add(s);
            pairs.Add((s, SanitizeFileName(f.FileName)));
        }

        try
        {
            var urls = await _storage.UploadMultipleAsync(pairs, folder);
            return Ok(Result<List<string>>.Success(urls, $"{urls.Count} file(s) uploaded."));
        }
        finally
        {
            foreach (var s in streams) s.Dispose();
        }
    }
}
