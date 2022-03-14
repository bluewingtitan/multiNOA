using System.Text.RegularExpressions;

namespace MultiNoa.PlukinKit;

public struct NamespacedKey
{
    public string Namespace { get; init; }
    public string Subspace { get; init; }
    public string AssetName { get; init; }


    public static NamespacedKey? FromString(string keyString)
    {
        if (!Regex.IsMatch(keyString, @"^[a-z]+\/[a-z]+:[a-z]+$"))
            return null;

        var (space, other) = keyString.Split('/',2).DeconstructTwo();
        if (space == null || other == null) return null;
        var (sub, name) = other.Split(':',2).DeconstructTwo();
        if (sub == null || name == null) return null;

        return new NamespacedKey
        {
            Namespace = space,
            Subspace = sub,
            AssetName = name,
        };
    }
}


internal static class NamespaceHelpingExtension
{
    public static (string?, string?) DeconstructTwo(this string[] array)
        => array.Length switch
        {
            0 => (null, null),
            1 => (array[0], null),
            _ => (array[0], array[1])
        };
    
}