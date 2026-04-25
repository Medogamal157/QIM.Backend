using System.Net;
using System.Text.RegularExpressions;

namespace QIM.Application.Common.Security;

/// <summary>
/// Lightweight HTML sanitizer used to neutralize XSS payloads in user-supplied free-text
/// (review comments, suggestions, contact messages, business descriptions, blog bodies).
/// DEF-NEW-003: prevents persisted XSS by removing tags and HTML-encoding the remaining text.
/// </summary>
public static class HtmlSanitizer
{
    private static readonly Regex TagRegex = new(
        @"<[^>]*>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ScriptRegex = new(
        @"<\s*script[^>]*>.*?<\s*/\s*script\s*>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    private static readonly Regex StyleRegex = new(
        @"<\s*style[^>]*>.*?<\s*/\s*style\s*>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    private static readonly Regex JsProtocolRegex = new(
        @"javascript\s*:",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex EventHandlerRegex = new(
        @"\son\w+\s*=\s*([""'][^""']*[""']|[^\s>]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Strips all HTML tags and dangerous protocols, then HTML-encodes the remaining text
    /// so any leftover angle brackets / ampersands render harmlessly.
    /// Returns null/empty for null/empty input.
    /// </summary>
    public static string? Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        var s = input;
        s = ScriptRegex.Replace(s, string.Empty);
        s = StyleRegex.Replace(s, string.Empty);
        s = EventHandlerRegex.Replace(s, string.Empty);
        s = JsProtocolRegex.Replace(s, string.Empty);
        s = TagRegex.Replace(s, string.Empty);
        // Encode whatever remains so stray '<', '>', '&', quotes are inert.
        s = WebUtility.HtmlEncode(s);
        return s.Trim();
    }

    /// <summary>
    /// Less aggressive sanitizer for rich-text fields (e.g. blog post body) that need to retain
    /// basic formatting. Removes <script>, <style>, inline event handlers and javascript: URLs
    /// but leaves the remaining markup intact.
    /// </summary>
    public static string? SanitizeRich(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        var s = input;
        s = ScriptRegex.Replace(s, string.Empty);
        s = StyleRegex.Replace(s, string.Empty);
        s = EventHandlerRegex.Replace(s, string.Empty);
        s = JsProtocolRegex.Replace(s, string.Empty);
        return s;
    }
}
