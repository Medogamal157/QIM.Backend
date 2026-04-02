using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QIM.Application.Interfaces.Services;
using QIM.Shared.Models;

namespace QIM.Presentation.Endpoints;

[Route("api/files")]
[Authorize]
public class FilesController : ApiControllerBase
{
    private readonly IFileStorageService _storage;

    public FilesController(IFileStorageService storage) => _storage = storage;

    /// <summary>
    /// Upload a single file.
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string folder = "general")
    {
        if (file is null || file.Length == 0)
            return BadRequest(Result.Failure("No file provided."));

        using var stream = file.OpenReadStream();
        var url = await _storage.UploadAsync(stream, file.FileName, folder);
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

        var pairs = new List<(Stream stream, string fileName)>();
        var streams = new List<Stream>();
        foreach (var f in files)
        {
            var s = f.OpenReadStream();
            streams.Add(s);
            pairs.Add((s, f.FileName));
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
