using System.Net;
using System.Net.Sockets;

namespace AdsKill.Windows;

/// <summary>
/// Conservative user-triggered check. Validates our HOSTS entries and compares
/// Windows OS DNS resolution with 0.0.0.0/::, but never promises that browsers,
/// emulators or rewarded-ad SDKs use that resolver.
/// </summary>
public static class HostsDiagnostics
{
    public static readonly string[] SampleDomains =
    [
        "ad.doubleclick.net",
        "googleads.g.doubleclick.net",
        "pagead2.googlesyndication.com",
        "ads.applovin.com"
    ];

    public static bool HasBlockRows(string hosts, string domain)
    {
        if (!HostsSections.IsEnabled(hosts) ||
            !HostsSections.Domains.Contains(domain, StringComparer.OrdinalIgnoreCase))
            return false;
        var start = hosts.IndexOf(HostsSections.StartMarker, StringComparison.Ordinal);
        var end = hosts.IndexOf(HostsSections.EndMarker, start + HostsSections.StartMarker.Length, StringComparison.Ordinal);
        if (start < 0 || end < 0) return false;
        var section = hosts.Substring(start, end - start);
        var v4 = false;
        var v6 = false;
        foreach (var line in section.Split('\n'))
        {
            var trimmed = line.Split('#', 2)[0].Trim();
            if (trimmed.Length == 0) continue;
            var fields = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length < 2) continue;
            for (var i = 1; i < fields.Length; i++)
            {
                if (!fields[i].Equals(domain, StringComparison.OrdinalIgnoreCase)) continue;
                if (fields[0] == "0.0.0.0") v4 = true;
                if (fields[0] == "::") v6 = true;
            }
        }
        return v4 && v6;
    }

    public static async Task<string> RunAsync(CancellationToken cancel = default)
    {
        // Only explicit user action calls this; no telemetry or background scanning.
        string hosts;
        try { hosts = HostsSections.Decode(File.ReadAllBytes(HostsStore.HostsPath)); }
        catch (Exception ex) { return "HOSTS non leggibile: " + ex.Message; }
        if (!HostsSections.IsEnabled(hosts))
            return "Regole ADS Kill non attive nel file HOSTS. Il filtro Windows e disattivato.";

        var configured = SampleDomains.Count(d => HasBlockRows(hosts, d));
        if (configured != SampleDomains.Length)
            return $"Regole HOSTS incomplete: {configured}/{SampleDomains.Length} domini campione. " +
                "Disattiva e riattiva il filtro, senza modificare manualmente altre righe.";

        var blocked = 0;
        var bypassed = 0;
        var unavailable = 0;
        foreach (var domain in SampleDomains)
        {
            cancel.ThrowIfCancellationRequested();
            try
            {
                var addresses = await Dns.GetHostAddressesAsync(domain, cancel);
                if (addresses.Length > 0 && addresses.All(ip =>
                    ip.Equals(IPAddress.Any) || ip.Equals(IPAddress.IPv6Any)))
                    blocked++;
                else if (addresses.Length > 0)
                    bypassed++;
                else unavailable++;
            }
            catch (SocketException) { unavailable++; }
        }

        var report = $"HOSTS: {configured}/{SampleDomains.Length} regole campione presenti. " +
            $"DNS Windows: {blocked} bloccati, {bypassed} non bloccati, " +
            $"{unavailable} non verificabili. ";
        if (bypassed > 0)
            report += "Alcune risoluzioni Windows ignorano queste regole: controlla cache, DNS, VPN o proxy. ";
        if (unavailable > 0)
            report += "Una risoluzione senza indirizzo non prova che la pubblicita sia bloccata. ";
        return report +
            "TEST LIMITATO AL PC: non dimostra che WeWard, browser o emulatori Android " +
            "(es. WEmulDroid) usino il DNS di Windows. Un video premio puo restare visibile " +
            "anche se tutti i domini campione risultano bloccati.";
    }
}
