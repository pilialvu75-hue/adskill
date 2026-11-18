using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AdsKill.Windows;

/// <summary>Checks public GitHub releases and downloads a SHA-256 verified ZIP.
/// No unsigned program is ever installed or launched automatically.</summary>
public static class GithubReleaseUpdater
{
    public sealed record ReleaseInfo(string Tag, string Url, string Sha256);
    private const string CurrentTag = "adskill-windows-v0.2.0-beta1";
    private const string ReleasePrefix = "https://github.com/pilialvu75-hue/adskill/releases/download/";
    private static readonly Regex TagSyntax = new(
        @"^adskill-windows-v(?<v>\d+\.\d+\.\d+)(?:-beta(?<beta>\d+))?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static (Version v, int beta)? Parse(string tag)
    {
        var match = TagSyntax.Match(tag);
        if (!match.Success || !Version.TryParse(match.Groups["v"].Value, out var version))
            return null;
        if (!match.Groups["beta"].Success) return (version, 0);
        return int.TryParse(match.Groups["beta"].Value, out int beta) && beta > 0
            ? (version, beta) : null;
    }

    public static bool IsNewer(string tag, string installed = CurrentTag)
    {
        var candidate = Parse(tag);
        var current = Parse(installed);
        if (candidate is null || current is null) return false;
        var numeric = candidate.Value.v.CompareTo(current.Value.v);
        if (numeric != 0) return numeric > 0;
        if (candidate.Value.beta == 0) return current.Value.beta != 0;
        return current.Value.beta != 0 && candidate.Value.beta > current.Value.beta;
    }

    private static HttpClient NewClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromMinutes(3) };
        client.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("AdsKillWindows", "0.2"));
        client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        return client;
    }

    public static async Task<ReleaseInfo?> CheckAsync(CancellationToken cancellationToken)
    {
        using var client = NewClient();
        using var response = await client.GetAsync(
            "https://api.github.com/repos/pilialvu75-hue/adskill/releases?per_page=30",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(stream,
            cancellationToken: cancellationToken);
        ReleaseInfo? result = null;
        foreach (var release in doc.RootElement.EnumerateArray())
        {
            if (release.GetProperty("draft").GetBoolean()) continue;
            var tag = release.GetProperty("tag_name").GetString() ?? "";
            if (!IsNewer(tag)) continue;
            var exactName = "AdsKill-Windows-v" +
                tag.Substring("adskill-windows-v".Length) + "-win-x64.zip";
            foreach (var asset in release.GetProperty("assets").EnumerateArray())
            {
                if (asset.GetProperty("name").GetString() != exactName) continue;
                var url = asset.GetProperty("browser_download_url").GetString() ?? "";
                var digest = asset.TryGetProperty("digest", out var field)
                    ? field.GetString() ?? "" : "";
                if (!url.StartsWith(ReleasePrefix + tag + "/", StringComparison.OrdinalIgnoreCase)
                    || !Regex.IsMatch(digest, @"^sha256:[a-fA-F0-9]{64}$")) continue;
                var candidate = new ReleaseInfo(tag, url,
                    digest.Substring(7).ToLowerInvariant());
                if (result is null || IsNewer(candidate.Tag, result.Tag))
                    result = candidate;
            }
        }
        return result;
    }

    public static async Task<string> DownloadAsync(ReleaseInfo release,
        CancellationToken cancellationToken)
    {
        if (!IsNewer(release.Tag)
            || !release.Url.StartsWith(ReleasePrefix + release.Tag + "/",
                StringComparison.OrdinalIgnoreCase)
            || !Regex.IsMatch(release.Sha256, "^[a-f0-9]{64}$"))
            throw new InvalidDataException("Release non verificabile.");
        var filename = Path.GetFileName(new Uri(release.Url).AbsolutePath);
        if (filename != "AdsKill-Windows-v" +
            release.Tag.Substring("adskill-windows-v".Length) + "-win-x64.zip")
            throw new InvalidDataException("Archivio non riconosciuto.");
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AdsKill", "updates");
        Directory.CreateDirectory(directory);
        var finalPath = Path.Combine(directory, filename);
        var tempPath = finalPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using var client = NewClient();
            using var response = await client.GetAsync(release.Url,
                HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using (var input =
                await response.Content.ReadAsStreamAsync(cancellationToken))
            await using (var output = new FileStream(tempPath, FileMode.CreateNew,
                FileAccess.Write, FileShare.None, 81920, useAsync: true))
                await input.CopyToAsync(output, cancellationToken);
            string actual;
            await using (var check = File.OpenRead(tempPath))
                actual = Convert.ToHexString(await SHA256.HashDataAsync(
                    check, cancellationToken)).ToLowerInvariant();
            if (!CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(actual), Convert.FromHexString(release.Sha256)))
                throw new InvalidDataException("SHA-256 errato; download rifiutato.");
            File.Move(tempPath, finalPath, overwrite:true);
            return finalPath;
        }
        finally
        {
            try { if (File.Exists(tempPath)) File.Delete(tempPath); }
            catch (IOException) { }
        }
    }
}
