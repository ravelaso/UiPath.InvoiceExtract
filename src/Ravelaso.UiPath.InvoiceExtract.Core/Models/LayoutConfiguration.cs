namespace Ravelaso.UiPath.InvoiceExtract.Core.Models;

/// <summary>
/// Represents a rule for determining document layout based on key-value patterns.
/// </summary>
/// <param name="key">The key text to search for in the document.</param>
/// <param name="layoutNumber">The layout number to assign when this rule matches.</param>
/// <param name="value">Optional specific value to match after the key.</param>
public class LayoutRule(string key, int layoutNumber, string? value = null)
{
    /// <summary>
    /// Gets the key text to search for in the document.
    /// </summary>
    public string Key { get; init; } = key;

    /// <summary>
    /// Gets the optional specific value to match after the key.
    /// If null, only the key's presence is checked.
    /// </summary>

    public string? Value { get; init; } = value;

    /// <summary>
    /// Gets the layout number to assign when this rule matches.
    /// </summary>
    public int LayoutNumber { get; init; } = layoutNumber;
}

/// <summary>
/// Represents the configuration for layout detection in document processing.
/// </summary>
public class LayoutConfiguration
{
    /// <summary>
    /// Gets a value indicating whether layout detection should be used.
    /// </summary>
    public bool UseLayoutDetection { get; init; }

    /// <summary>
    /// Gets the list of layout rules to apply during detection.
    /// </summary>
    public List<LayoutRule> Rules { get; init; } = [];

    /// <summary>
    /// Gets a configuration instance that disables layout detection.
    /// </summary>
    public static LayoutConfiguration None => new()
    {
        UseLayoutDetection = false
    };

    /// <summary>
    /// Creates a new layout configuration with the specified rules.
    /// </summary>
    /// <param name="rules">The layout rules to include in the configuration.</param>
    /// <returns>A new LayoutConfiguration instance with layout detection enabled.</returns>
    public static LayoutConfiguration Create(params LayoutRule[] rules)
    {
        return new()
        {
            UseLayoutDetection = true,
            Rules = rules.ToList()
        };
    }
}