using System.Text.RegularExpressions;
using Ravelaso.UiPath.InvoiceExtract.Core.Models;

namespace Ravelaso.UiPath.InvoiceExtract.Core.Interfaces;

/// <summary>
/// Represents the layout type detected in a PDF document.
/// </summary>
/// <remarks>
/// Used by the layout detection system to identify different invoice formats
/// and apply appropriate processing rules.
/// </remarks>
public record LayoutType
{
    /// <summary>
    /// Gets or sets the layout number identifier.
    /// </summary>
    /// <value>
    /// The numeric identifier for the detected layout type.
    /// A value of 0 typically indicates no specific layout detected.
    /// </value>
    public int Layout { get; set; }

    /// <summary>
    /// Gets or sets the page number where the layout was detected.
    /// </summary>
    /// <value>
    /// The 1-based page number where the layout identification occurred.
    /// A value of 0 typically indicates layout was not detected.
    /// </value>
    public int Page { get; set; }
}

/// <summary>
/// Defines the contract for invoice processors that extract data from PDF documents.
/// </summary>
/// <typeparam name="T">
/// The type of invoice data to be extracted. Must implement <see cref="IInvoiceData"/>.
/// </typeparam>
/// <remarks>
/// This interface provides a flexible framework for processing different types of invoices.
/// Implementations define which zones to process, how to extract text, and how to map
/// extracted data to the appropriate fields.
/// </remarks>
public interface IInvoiceProcessor<out T> where T : IInvoiceData
{
    /// <summary>
    /// Gets the array of zone numbers that should be processed for this invoice type.
    /// </summary>
    /// <value>
    /// An array of integers representing zone identifiers used during PDF segmentation.
    /// Zones correspond to regions in the PDF identified by the page segmentation algorithm.
    /// </value>
    int[] Zones { get; }

    /// <summary>
    /// Gets the configuration for word extraction from PDF pages.
    /// </summary>
    /// <returns>
    /// A <see cref="WordExtractorConfiguration"/> that defines how words should be
    /// extracted from PDF letters. Default implementation returns
    /// <see cref="DefaultWordExtractorConfiguration"/>.
    /// </returns>
    /// <remarks>
    /// Override this method to customize how text is extracted from individual letters
    /// in the PDF, such as handling special spacing or character grouping rules.
    /// </remarks>
    WordExtractorConfiguration GetWordExtractorConfiguration()
    {
        return new DefaultWordExtractorConfiguration();
    }

    /// <summary>
    /// Gets the configuration for page segmentation.
    /// </summary>
    /// <returns>
    /// A <see cref="PageSegmenterConfiguration"/> that defines how pages should be
    /// divided into zones or blocks for text extraction.
    /// </returns>
    /// <remarks>
    /// This method must be implemented to specify the segmentation algorithm to use.
    /// Common options include Docstrum or other layout analysis algorithms.
    /// </remarks>
    PageSegmenterConfiguration GetPageSegmenterConfiguration();

    /// <summary>
    /// Gets the configuration that specifies which pages should be processed.
    /// </summary>
    /// <returns>
    /// A <see cref="PageConfiguration"/> indicating which pages should undergo
    /// zone processing and regex processing. Default is all pages.
    /// </returns>
    /// <remarks>
    /// Override to optimize performance by processing only specific pages
    /// (e.g., first page only, last page only, or specific page ranges).
    /// </remarks>
    PageConfiguration GetPageConfiguration()
    {
        return PageConfiguration.Default;
    }

    /// <summary>
    /// Gets the configuration for layout detection.
    /// </summary>
    /// <returns>
    /// A <see cref="LayoutConfiguration"/> that defines rules for detecting different
    /// invoice layouts. Default is no layout detection.
    /// </returns>
    /// <remarks>
    /// Override this method when invoices from the same type can have multiple layouts
    /// that require different processing rules.
    /// </remarks>
    LayoutConfiguration GetLayoutConfiguration()
    {
        return LayoutConfiguration.None;
    }

    /// <summary>
    /// Sets the document name for the invoice being processed.
    /// </summary>
    /// <param name="name">The filename of the PDF document.</param>
    void SetDocumentName(string name);

    /// <summary>
    /// Sets the total number of pages in the invoice document.
    /// </summary>
    /// <param name="pages">The total page count.</param>
    void SetDocumentPages(int pages);

    /// <summary>
    /// Maps extracted text from a specific zone to the appropriate invoice data field.
    /// </summary>
    /// <param name="zone">The zone number from which the text was extracted.</param>
    /// <param name="value">The extracted text value from that zone.</param>
    /// <param name="pageNumber">The 1-based page number where the zone appears.</param>
    /// <remarks>
    /// This method is called for each zone defined in the <see cref="Zones"/> property.
    /// Implementations should parse and assign the value to the appropriate field
    /// in the invoice data object based on the zone number.
    /// </remarks>
    void MapByZone(int zone, string value, int pageNumber);

    /// <summary>
    /// Maps extracted text using regular expression patterns to invoice data fields.
    /// </summary>
    /// <param name="text">The full text content of a page.</param>
    /// <param name="pageNumber">The 1-based page number being processed.</param>
    /// <remarks>
    /// This method is called for each page that requires regex-based extraction.
    /// Implementations should define regex patterns to locate and extract specific
    /// data fields from the full page text.
    /// </remarks>
    void MapByRegex(string text, int pageNumber);

    /// <summary>
    /// Finalizes processing and returns the extracted invoice data.
    /// </summary>
    /// <returns>
    /// An instance of <typeparamref name="T"/> containing all extracted invoice data.
    /// </returns>
    /// <remarks>
    /// This method is called after all zones and regex processing is complete.
    /// Implementations can perform any final data validation or transformation here.
    /// </remarks>
    T ProcessInvoice();

    /// <summary>
    /// Applies detected layout information to adjust processing behavior.
    /// </summary>
    /// <param name="layout">The detected layout type with layout number and page information.</param>
    /// <remarks>
    /// Default implementation does nothing. Override this method to handle multiple
    /// invoice layouts dynamically, such as adjusting zone mappings or processing rules
    /// based on the detected layout.
    /// </remarks>
    void ApplyLayout(LayoutType layout)
    {
    }
}

/// <summary>
/// Provides a base implementation of <see cref="IInvoiceProcessor{T}"/> with common functionality.
/// </summary>
/// <typeparam name="T">
/// The type of invoice data to be extracted. Must implement <see cref="IInvoiceData"/>
/// and have a parameterless constructor.
/// </typeparam>
/// <remarks>
/// This abstract class handles basic document metadata management and provides utility
/// methods for text processing. Derived classes need to implement zone/regex mapping
/// and define which zones to process.
/// </remarks>
public abstract class BaseInvoiceProcessor<T> : IInvoiceProcessor<T> where T : IInvoiceData, new()
{
    /// <summary>
    /// The invoice data object being populated during processing.
    /// </summary>
    protected T Data = new();

    /// <summary>
    /// Sets the document name in the invoice data.
    /// </summary>
    /// <param name="name">The filename of the PDF document.</param>
    public void SetDocumentName(string name)
    {
        Data.DocumentName = name;
    }

    /// <summary>
    /// Sets the total number of pages in the invoice data.
    /// </summary>
    /// <param name="pages">The total page count.</param>
    public void SetDocumentPages(int pages)
    {
        Data.DocumentPages = pages;
    }

    /// <summary>
    /// Gets the configuration for word extraction from PDF pages.
    /// </summary>
    /// <returns>
    /// A <see cref="DefaultWordExtractorConfiguration"/> instance.
    /// </returns>
    /// <remarks>
    /// Override to provide custom word extraction configuration for specific invoice types.
    /// </remarks>
    public virtual WordExtractorConfiguration GetWordExtractorConfiguration()
    {
        return new DefaultWordExtractorConfiguration();
    }

    /// <summary>
    /// Gets the configuration for page segmentation.
    /// </summary>
    /// <returns>
    /// A <see cref="DefaultPageSegmenterConfiguration"/> instance as fallback.
    /// </returns>
    /// <remarks>
    /// Override this method to specify the appropriate segmentation algorithm
    /// for your invoice type (e.g., Docstrum, XY-Cut, etc.).
    /// </remarks>
    public virtual PageSegmenterConfiguration GetPageSegmenterConfiguration()
    {
        // Default fallback
        return new DefaultPageSegmenterConfiguration();
    }

    /// <summary>
    /// Gets the page configuration specifying which pages to process.
    /// </summary>
    /// <returns>
    /// The default <see cref="PageConfiguration"/> (processes all pages).
    /// </returns>
    /// <remarks>
    /// Override to limit processing to specific pages for performance optimization.
    /// </remarks>
    public virtual PageConfiguration GetPageConfiguration()
    {
        return PageConfiguration.Default;
    }

    /// <summary>
    /// Gets the layout detection configuration.
    /// </summary>
    /// <returns>
    /// <see cref="LayoutConfiguration.None"/> indicating no layout detection.
    /// </returns>
    /// <remarks>
    /// Override this method when you need to handle multiple invoice layouts
    /// for the same invoice type.
    /// </remarks>
    public virtual LayoutConfiguration GetLayoutConfiguration()
    {
        return LayoutConfiguration.None;
    }

    /// <summary>
    /// Applies detected layout information to adjust processing behavior.
    /// </summary>
    /// <param name="layoutType">The detected layout with layout number and page.</param>
    /// <remarks>
    /// Default implementation does nothing. Override in processors that support
    /// multiple layouts to adjust processing rules based on the detected layout.
    /// </remarks>
    public virtual void ApplyLayout(LayoutType layoutType)
    {
        // Default: do nothing
    }

    /// <summary>
    /// Gets the array of zone numbers to process for this invoice type.
    /// </summary>
    /// <remarks>
    /// Must be implemented to specify which zones contain relevant invoice data.
    /// </remarks>
    public abstract int[] Zones { get; }

    /// <summary>
    /// Maps extracted text from a specific zone to invoice data fields.
    /// </summary>
    /// <param name="zone">The zone number from which the text was extracted.</param>
    /// <param name="value">The extracted text value from that zone.</param>
    /// <param name="pageNumber">The 1-based page number where the zone appears.</param>
    /// <remarks>
    /// Must be implemented to define how each zone's content maps to invoice fields.
    /// </remarks>
    public abstract void MapByZone(int zone, string value, int pageNumber);

    /// <summary>
    /// Maps extracted text using regular expressions to invoice data fields.
    /// </summary>
    /// <param name="text">The full text content of a page.</param>
    /// <param name="pageNumber">The 1-based page number being processed.</param>
    /// <remarks>
    /// Must be implemented to define regex patterns for extracting specific data fields.
    /// </remarks>
    public abstract void MapByRegex(string text, int pageNumber);

    /// <summary>
    /// Finalizes processing and returns the extracted invoice data.
    /// </summary>
    /// <returns>The populated invoice data object.</returns>
    /// <remarks>
    /// Must be implemented to return the final processed invoice data,
    /// potentially with validation or additional transformations.
    /// </remarks>
    public abstract T ProcessInvoice();

    /// <summary>
    /// Determines if the specified page number is the last page of the document.
    /// </summary>
    /// <param name="pageNumber">The 1-based page number to check.</param>
    /// <returns>
    /// <c>true</c> if the page is the last page; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Useful for conditional processing logic that differs on the last page.
    /// </remarks>
    protected bool IsLastPage(int pageNumber)
    {
        return pageNumber == Data.DocumentPages;
    }

    /// <summary>
    /// Extracts the text immediately after the first line break in the input string.
    /// </summary>
    /// <param name="input">The input string containing line breaks.</param>
    /// <returns>
    /// The text on the line immediately following the first line break,
    /// with carriage returns removed. Returns empty string if no line break is found.
    /// </returns>
    /// <remarks>
    /// Useful for extracting content when the first line is a label and the
    /// second line contains the actual value.
    /// </remarks>
    /// <example>
    /// Input: "Label:\nValue\nOther"
    /// Output: "Value"
    /// </example>
    protected static string AfterLineBreakOnly(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        // Locate the first LF
        var firstLf = input.IndexOf('\n');
        if (firstLf == -1) return string.Empty;

        var start = firstLf + 1; // first character after the break
        var secondLf = input.IndexOf('\n', start); // next break (if any)

        var length = (secondLf == -1 ? input.Length : secondLf) - start;
        var line = input.Substring(start, length);

        return line.TrimEnd('\r'); // remove a possible '\r'
    }

    /// <summary>
    /// Extracts a specific line from multi-line text by line index.
    /// </summary>
    /// <param name="lineIndex">The 0-based index of the line to extract.</param>
    /// <param name="input">The multi-line input string.</param>
    /// <returns>
    /// The text at the specified line index, or empty string if the index is
    /// out of range or input is invalid.
    /// </returns>
    /// <remarks>
    /// Lines are split by carriage return (\r) or line feed (\n) characters.
    /// Empty lines are removed from the array before indexing.
    /// </remarks>
    protected static string ExtractLine(int lineIndex, string input)
    {
        if (string.IsNullOrWhiteSpace(input) || lineIndex < 0) return string.Empty;

        var lines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        return lineIndex < lines.Length ? lines[lineIndex] : string.Empty;
    }

    /// <summary>
    /// Extracts all text after the first line break and removes subsequent line breaks.
    /// </summary>
    /// <param name="input">The input string containing line breaks.</param>
    /// <returns>
    /// The text after the first line break with all subsequent line breaks
    /// replaced by a single space. Returns empty string if no line break is found.
    /// </returns>
    /// <remarks>
    /// Useful for extracting multi-line values and converting them to a single line.
    /// </remarks>
    /// <example>
    /// Input: "Label:\nLine 1\nLine 2\nLine 3"
    /// Output: "Line 1 Line 2 Line 3"
    /// </example>
    protected static string AfterLineBreak(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        var lineBreakIndex = input.IndexOf('\n');
        if (lineBreakIndex == -1) return string.Empty;

        var text = input[(lineBreakIndex + 1)..];
        var cleanedText = Regex.Replace(text, @"\r\n?|\n", string.Empty.PadLeft(1));
        return cleanedText;
    }

    /// <summary>
    /// Captures a specific group from a regex match.
    /// </summary>
    /// <param name="rx">The compiled regular expression to apply.</param>
    /// <param name="src">The source text to match against.</param>
    /// <param name="group">The group number to capture (default is 2).</param>
    /// <returns>
    /// The value of the specified capture group if the match succeeds;
    /// otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This is a convenience method for extracting specific data using regex patterns.
    /// Group 0 is the entire match, group 1 is typically the first capture group, etc.
    /// The default group of 2 assumes patterns like: (label)(value)
    /// </remarks>
    protected static string? CaptureRegex(Regex rx, string src, int group = 2)
    {
        return rx.Match(src) is { Success: true } m ? m.Groups[group].Value : null;
    }

    /// <summary>
    /// Creates a compiled regular expression with standard options.
    /// </summary>
    /// <param name="pattern">The regex pattern string.</param>
    /// <param name="extraOptions">
    /// Additional regex options to combine with the defaults (default is IgnoreCase).
    /// </param>
    /// <returns>
    /// A compiled <see cref="Regex"/> instance with Compiled, CultureInvariant,
    /// and the specified extra options enabled.
    /// </returns>
    /// <remarks>
    /// Using compiled regex improves performance for patterns used repeatedly.
    /// CultureInvariant ensures consistent behavior across different locales.
    /// </remarks>
    protected static Regex CompiledRegex(string pattern,
        RegexOptions extraOptions = RegexOptions.IgnoreCase)
    {
        return new(pattern,
            RegexOptions.Compiled | RegexOptions.CultureInvariant | extraOptions);
    }
}