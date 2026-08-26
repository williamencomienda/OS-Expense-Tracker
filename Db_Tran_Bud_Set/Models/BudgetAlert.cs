namespace GCAT.Models
{
    public enum AlertSeverity
    {
        Warning,
        OverLimit,
        Healthy
    }

    /// <summary>
    /// A single row in the "alerts" panel (e.g. "food nearly maxed" -> 97%).
    /// </summary>
    public class BudgetAlert
    {
        public string Label { get; set; } = string.Empty;
        public string BadgeText { get; set; } = string.Empty;
        public AlertSeverity Severity { get; set; }

        // Convenience flags used by the view to pick which Badge.* style class applies.
        public bool IsWarning => Severity == AlertSeverity.Warning;
        public bool IsOverLimit => Severity == AlertSeverity.OverLimit;
        public bool IsHealthy => Severity == AlertSeverity.Healthy;
    }
}
