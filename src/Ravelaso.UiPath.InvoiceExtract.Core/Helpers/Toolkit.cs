using System.Collections;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Ravelaso.UiPath.InvoiceExtract.Core.Interfaces;


namespace Ravelaso.UiPath.InvoiceExtract.Core.Helpers;

/// <summary>
///     Miscellaneous helper/utility methods that are reused by several profiles
///     and by the console app.
///     The class is intentionally <c>partial</c> so additional helpers can be
///     added elsewhere without touching this file.
/// </summary>
public static class Toolkit
{

    /* ────────────────────── generic CSV export ──────────────────── */

    /// <summary>Adds instance as a row to <paramref name="table" />.</summary>
    public static void AddPdfDataRowToDataTable(
        DataTable table,
        IInvoiceData data,
        Func<PropertyInfo, object?, string?>? customConverter = null)
    {
        // Use the concrete type, not IInvoiceData
        var props = data.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToArray();

        var row = table.NewRow();

        foreach (var prop in props)
        {
            // Skip properties that are not present in the DataTable
            if (!table.Columns.Contains(prop.Name)) continue;

            var raw = prop.GetValue(data);
            var flat = FlattenValue(prop, raw, customConverter);
            row[prop.Name] = (object?)flat ?? DBNull.Value;
        }

        table.Rows.Add(row);
    }


    /* ────────────────────── shared helper code ──────────────────── */

    public static DataTable CreateDataTableFromType(Type type)
    {
        var dt = new DataTable(type.Name);
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                     .Where(p => p.CanRead))
            dt.Columns.Add(new DataColumn(prop.Name, typeof(string)));

        return dt;
    }

    private static string? FlattenValue(
        PropertyInfo prop,
        object? value,
        Func<PropertyInfo, object?, string?>? custom)
    {
        // user-supplied override wins
        var customResult = custom?.Invoke(prop, value);
        if (customResult != null) return customResult;

        switch (value)
        {
            case null:
                return null;
            case string s:
                return s;
            // Collections – join with ';'
            case IEnumerable enumerable:
            {
                var items = enumerable.OfType<object>()
                    .Select(item => item.ToString() ?? string.Empty).ToList();

                return string.Join(";", items);
            }
            default:
                // Fallback
                return value.ToString();
        }
    }

    public static void SaveDatatableToCsv(DataTable dataTable, string outputFilePath)
    {
        using var writer = new StreamWriter(outputFilePath, false, Encoding.UTF8);

        // header
        var columnNames = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName);
        writer.WriteLine(string.Join(",", columnNames));

        // rows
        foreach (DataRow row in dataTable.Rows)
        {
            var fields = row.ItemArray.Select(EscapeCsv);
            writer.WriteLine(string.Join(",", fields));
        }
    }

    private static string EscapeCsv(object? field)
    {
        if (field == null || field == DBNull.Value) return string.Empty;

        var str = field.ToString() ?? string.Empty;

        // Need quoting?
        if (str.Contains(',') || str.Contains('"') || str.Contains('\n'))
        {
            str = str.Replace("\"", "\"\"");
            return $"\"{str}\"";
        }

        return str;
    }

    /* ───────────────────────── utilities ───────────────────────── */

    /// <summary>
    ///     Normalises line-breaks and trims superfluous whitespace.
    /// </summary>
    public static string NormalizeString(string input)
    {
        // Convert to uppercase
        var upper = input.ToUpper();

        // Replace new line characters with a space
        upper = upper.Replace(Environment.NewLine, "")
            .Replace("\n", "")
            .Replace("\r", "");

        var replace = upper.Replace(" ", string.Empty);
        return replace.ToUpper(); // Return the normalized string
    }

    /// <summary>
    ///     Tries to parse the supplied date (in EU or US style) and returns it
    ///     as <c>yyyy-MM-dd</c>.
    ///     If parsing fails, the original string is returned unchanged.
    /// </summary>
    public static string ParseDate(string dateString)
    {
        // Define possible date formats
        string[] daymonthyear =
        [
            "d/M/yy",
            "d/M/yyyy",
            "d-M-yy",
            "d-M-yyyy",
            "dd/MM/yy",
            "dd/MM/yyyy",
            "dd-MM-yy",
            "dd-MM-yyyy"
        ];

        string[] monthdayyear =
        [
            "M/d/yy", // Allow single-digit month/day
            "M/d/yyyy",
            "M-d-yy",
            "M-d-yyyy",
            "MM/dd/yy",
            "MM/dd/yyyy",
            "MM-dd-yy",
            "MM-dd-yyyy"
        ];
        // Try to parse the date

        return Convert.ToString(DateTime.TryParseExact(dateString, daymonthyear,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsedDate)
            ? // Asks if parsed.
            parsedDate.ToString("M/d/yyyy") // Then return the date in MM/dd/yyyy format
            : // Otherwise...
            DateTime.ParseExact(dateString, monthdayyear,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None)
                .ToString("M/d/yyyy")); // Parse the Date as Month/Day/YYYY and return proper string.
    }

    /// <summary>
    ///     Parses a string that may contain currency symbols, thousand separators,
    ///     or EU/US decimal separators and returns the numeric value.
    /// </summary>
    public static double ParseCurrency(string input)
    {
        // Remove any non-numeric characters except for the decimal and thousands separators
        var cleanedInput = Regex.Replace(input, @"[^\d.,]", "");

        // Determine the format based on the presence of commas and periods
        if (cleanedInput.Contains(',') && cleanedInput.Contains('.'))
        {
            // If both are present, determine which is the decimal separator
            // Assume the last occurrence of ',' is the decimal separator
            // This handles cases like "1,136.56" (US format)
            var lastCommaIndex = cleanedInput.LastIndexOf(',');
            var lastDotIndex = cleanedInput.LastIndexOf('.');

            if (lastCommaIndex > lastDotIndex)
            {
                // Treat the last comma as the decimal separator
                var wholeNumberPart = cleanedInput.Substring(0, lastCommaIndex).Replace(".", "");
                var decimalPart = cleanedInput.Substring(lastCommaIndex + 1);
                cleanedInput = wholeNumberPart + "." + decimalPart;
            }
            else
            {
                // Treat the last dot as the decimal separator
                var wholeNumberPart = cleanedInput.Substring(0, lastDotIndex).Replace(",", "");
                var decimalPart = cleanedInput.Substring(lastDotIndex + 1);
                cleanedInput = wholeNumberPart + "." + decimalPart;
            }
        }
        else if (cleanedInput.Contains(','))
        {
            // If only a comma is present, treat it as the decimal separator (EU format)
            var parts = cleanedInput.Split(',');
            var wholeNumberPart = parts[0]; // Get Whole Amount before comma.
            var decimalPart = parts.Length > 1 ? parts[1] : "0"; // Handle case without decimal part
            cleanedInput = wholeNumberPart + "." + decimalPart;
        }
        else if (cleanedInput.Contains('.'))
        {
            // If only a dot is present, treat it as the decimal separator (US format)
            var parts = cleanedInput.Split('.');
            var wholeNumberPart = parts[0]; // Get Whole Amount before dot.
            var decimalPart = parts.Length > 1 ? parts[1] : "0"; // Handle case without decimal part
            cleanedInput = wholeNumberPart + "." + decimalPart;
        }

        // Parse the cleaned string to double
        return double.Parse(cleanedInput, CultureInfo.InvariantCulture);
    }
}