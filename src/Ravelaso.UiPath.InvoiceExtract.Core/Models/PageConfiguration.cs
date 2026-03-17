#nullable enable
namespace Ravelaso.UiPath.InvoiceExtract.Core.Models;

/// <summary>
///     Defines which pages should be processed for zone and regex mapping.
/// </summary>
public class PageConfiguration
{
    /// <summary>
    ///     Pages to process for zone mapping. Use AllPages for all pages, or null to skip zone processing.
    /// </summary>
    public List<int>? ZonePages { get; set; }

    /// <summary>
    ///     Pages to process for regex mapping. Use AllPages for all pages, or null to skip regex processing.
    /// </summary>
    public List<int>? RegexPages { get; set; }

    /// <summary>
    ///     Sentinel value to indicate all pages should be processed.
    ///     Usage: ZonePages = PageConfiguration.AllPages
    /// </summary>
    public static int AllPages => -1; // Sentinel value

    /// <summary>
    ///     Sentinel value to indicate Last page should be processed.
    ///     Usage: ZonePages = PageConfiguration.LastPage
    /// </summary>
    public static int LastPage => -2; // Sentinel value

    /// <summary>
    ///     Default configuration: Zone mapping on page 1, no regex processing.
    /// </summary>
    public static PageConfiguration Default => new()
    {
        ZonePages = [1],
        RegexPages = null
    };

    /// <summary>
    ///     Check if this configuration uses all pages for zones.
    /// </summary>
    private bool IsAllPagesForZones()
    {
        return ZonePages?.Contains(-1) == true;
    }

    /// <summary>
    ///     Check if this configuration uses all pages for regex.
    /// </summary>
    private bool IsAllPagesForRegex()
    {
        return RegexPages?.Contains(-1) == true;
    }

    /// <summary>
    ///     Check if this configuration uses last page for zones.
    /// </summary>
    internal bool IsLastPageForZones()
    {
        return ZonePages?.Contains(-2) == true;
    }

    /// <summary>
    ///     Check if this configuration uses last page for regex.
    /// </summary>
    internal bool IsLastPageForRegex()
    {
        return RegexPages?.Contains(-2) == true;
    }

    /// <summary>
    ///     Get the actual page numbers to process for zones, resolving sentinel values.
    /// </summary>
    internal List<int> GetZonePagesToProcess(int totalPages)
    {
        if (ZonePages == null) return [];
        if (IsAllPagesForZones()) return Enumerable.Range(1, totalPages).ToList();

        var pages = new List<int>();
        foreach (var page in ZonePages)
            if (page == -2) // LastPage sentinel
                pages.Add(totalPages);
            else if (page > 0) // Regular page number
                pages.Add(page);
        // Ignore -1 (AllPages) if mixed with other values
        return pages.Distinct().OrderBy(p => p).ToList();
    }

    /// <summary>
    ///     Get the actual page numbers to process for regex, resolving sentinel values.
    /// </summary>
    internal List<int> GetRegexPagesToProcess(int totalPages)
    {
        if (RegexPages == null) return [];
        if (IsAllPagesForRegex()) return Enumerable.Range(1, totalPages).ToList();

        var pages = new List<int>();
        foreach (var page in RegexPages)
            if (page == -2) // LastPage sentinel
                pages.Add(totalPages);
            else if (page > 0) // Regular page number
                pages.Add(page);
        // Ignore -1 (AllPages) if mixed with other values
        return pages.Distinct().OrderBy(p => p).ToList();
    }
}