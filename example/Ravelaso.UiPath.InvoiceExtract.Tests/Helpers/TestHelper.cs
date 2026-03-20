using System.Collections;
using System.Reflection;
using Xunit.Abstractions;

namespace Ravelaso.UiPath.InvoiceExtract.Tests.Helpers;

public static class TestHelper
{
    public static string GetTestInvoicePath(string fileName)
    {
        // Get the directory where the test assembly is located
        var testAssemblyPath = Assembly.GetExecutingAssembly().Location;
        var testDirectory = Path.GetDirectoryName(testAssemblyPath)!;

        // Build path to the Invoices folder
        var invoicePath = Path.Combine(testDirectory, "Invoices", fileName);

        return !File.Exists(invoicePath)
            ? throw new FileNotFoundException($"Test invoice file not found: {invoicePath}")
            : invoicePath;
    }

    public static void LogAllProperties<T>(ITestOutputHelper logger, T obj)
    {
        const string reset = "\u001b[0m";
        const string propertyNameColor = "\u001b[34m"; // Blue
        const string propertyValueColor = "\u001b[32m"; // Green

        if (obj is null)
        {
            logger.WriteLine($"{propertyNameColor}(null){reset}: {propertyValueColor}<null>{reset}");
            return;
        }

        // Use runtime type (helps if T is an interface/base type)
        var type = obj.GetType();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            object? value;
            try { value = prop.GetValue(obj); }
            catch { value = "<unreadable>"; }

            var formattedName =
                string.Concat(prop.Name.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));

            logger.WriteLine(
                $"{propertyNameColor}{formattedName}{reset}: {propertyValueColor}{FormatValue(value)}{reset}");
        }
    }
    private static string FormatValue(object? v, int depth = 0)
    {
        if (v is null) return "<null>";
        if (v is string str) return str; // don't treat string as IEnumerable<char>

        var vt = v.GetType();

        // simple-ish scalars
        if (vt.IsPrimitive || v is decimal || v is DateTime || v is DateTimeOffset || v is Guid || v is Enum)
            return v.ToString() ?? "<null>";

        // arrays / lists / etc.
        if (v is IEnumerable enumerable)
        {
            var items = new List<string>();
            foreach (var item in enumerable)
                items.Add(FormatValue(item, depth + 1));

            return "[ " + string.Join(", ", items) + " ]";
        }

        // nested objects (bounded recursion to avoid huge output)
        const int maxDepth = 2;
        if (depth >= maxDepth) return vt.Name;

        var props = vt.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        if (props.Length == 0) return v.ToString() ?? vt.Name;

        var parts = new List<string>();
        foreach (var p in props)
        {
            object? pv;
            try { pv = p.GetValue(v); }
            catch { pv = "<unreadable>"; }

            parts.Add($"{p.Name}={FormatValue(pv, depth + 1)}");
        }

        return $"{vt.Name} {{ " + string.Join(", ", parts) + " }}";
    }
}