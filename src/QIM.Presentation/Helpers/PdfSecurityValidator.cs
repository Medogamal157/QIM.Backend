using Microsoft.AspNetCore.Http;

namespace QIM.Presentation.Helpers;

public static class PdfSecurityValidator
{
    private static readonly string[] SuspiciousMarkers =
    {
        "/JavaScript",
        "/JS",
        "/OpenAction",
        "/Launch",
        "/EmbeddedFile",
        "/Filespec",
        "/URI",
        "/RichMedia",
        "/Flash",
        "/XFA"
    };

    public static async Task<bool> IsMaliciousPdfAsync(this IFormFile file)
    {
        if (file is null || file.Length == 0)
            return false;

        string pdfText;

        using (var stream = file.OpenReadStream())
        using (var reader = new StreamReader(stream))
        {
            pdfText = await reader.ReadToEndAsync();
        }

        pdfText = pdfText ?? string.Empty;

        if (!pdfText.Contains("%PDF"))
            return true;

        foreach (var marker in SuspiciousMarkers)
        {
            if (pdfText.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }

        return false;
    }
}
