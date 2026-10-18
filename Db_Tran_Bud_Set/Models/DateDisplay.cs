using System;
using System.IO;
using System.Text.Json;

namespace GCAT.Models;

public static class DateDisplay
{
    public const string DefaultPattern = "MMM DD, YYYY";

    private static readonly string SettingsPath =
        Path.Combine(AppContext.BaseDirectory, "settings.json");

    public static string Pattern { get; private set; } = DefaultPattern;

    // Raised whenever the user picks a new format.
    public static event Action? Changed;

    // "MMM DD, YYYY" -> "MMM dd, yyyy" (.NET format strings are case-sensitive)
    private static string NetFormat => Pattern.Replace("DD", "dd").Replace("YYYY", "yyyy");

    public static string Format(DateTime date) => date.ToString(NetFormat);

    public static void Set(string pattern)
    {
        if (pattern == Pattern) return;
        Pattern = pattern;
        Save();
        Changed?.Invoke();
    }

    public static void Load()
    {
        try
        {
            if (!File.Exists(SettingsPath)) return;
            var saved = JsonSerializer.Deserialize<SavedSettings>(File.ReadAllText(SettingsPath));
            if (!string.IsNullOrWhiteSpace(saved?.DatePattern))
                Pattern = saved.DatePattern;
        }
        catch { /* corrupt or unreadable file: fall back to the default */ }
    }

    private static void Save()
    {
        try
        {
            File.WriteAllText(SettingsPath,
                JsonSerializer.Serialize(new SavedSettings { DatePattern = Pattern }));
        }
        catch { /* not being able to save shouldn't crash the app */ }
    }

    private class SavedSettings
    {
        public string? DatePattern { get; set; }
    }
}