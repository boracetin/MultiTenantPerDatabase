using System.Globalization;

namespace MultitenantPerDb.Core.Application.Extensions;

/// <summary>
/// String extension methods for Turkish culture-aware operations
/// </summary>
public static class StringExtensions
{
    private static readonly CultureInfo TurkishCulture = new CultureInfo("tr-TR");

    /// <summary>
    /// Converts string to uppercase using Turkish culture rules
    /// Properly handles Turkish characters (i -> İ, ı -> I)
    /// </summary>
    public static string ToTurkishUpper(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return input.ToUpper(TurkishCulture);
    }

    /// <summary>
    /// Converts string to lowercase using Turkish culture rules
    /// Properly handles Turkish characters (İ -> i, I -> ı)
    /// </summary>
    public static string ToTurkishLower(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return input.ToLower(TurkishCulture);
    }

    /// <summary>
    /// Normalizes string by replacing Turkish characters with ASCII equivalents and converting to uppercase
    /// ü→U, ğ→G, ş→S, ı→I, ç→C, ö→O, i→I, İ→I
    /// Used for case-insensitive comparison and database normalization
    /// </summary>
    public static string NormalizeTurkishUpper(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        input = input.Trim();

        // Replace Turkish characters with ASCII equivalents
        var normalized = input
            .Replace('ç', 'c').Replace('Ç', 'C')
            .Replace('ğ', 'g').Replace('Ğ', 'G')
            .Replace('ı', 'i').Replace('I', 'I')
            .Replace('İ', 'I').Replace('i', 'i')
            .Replace('ö', 'o').Replace('Ö', 'O')
            .Replace('ş', 's').Replace('Ş', 'S')
            .Replace('ü', 'u').Replace('Ü', 'U');

        // Convert to uppercase using invariant culture
        return normalized.ToUpperInvariant();
    }
}
