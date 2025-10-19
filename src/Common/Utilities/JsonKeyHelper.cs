namespace dotGeoMigrata.Common.Utilities;

using System.Text;
using System.Text.RegularExpressions;

public static partial class JsonKeyHelper
{
    public static string ToSafeKey(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return "Unnamed";

        var cleaned = Reg().Replace(displayName, "");

        var parts = cleaned
            .Split([' ', '_', '\t'], StringSplitOptions.RemoveEmptyEntries);

        var sb = new StringBuilder();
        foreach (var p in parts)
        {
            if (p.Length == 0) continue;
            var word = char.ToUpperInvariant(p[0]) + p.Substring(1);
            sb.Append(word);
        }

        var key = sb.ToString();
        return string.IsNullOrEmpty(key) ? "Unnamed" : key;
    }

    [GeneratedRegex(@"[<>?\.\!\[\]\{\}\""']")]
    private static partial Regex Reg();
}