namespace GCAT.Models;

// A single sidebar navigation entry.
public class NavItem
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;

    // Placeholder path - drop a matching icon into Assets/Icons and it will pick it up automatically.
    public string IconPath { get; set; } = string.Empty;
}
