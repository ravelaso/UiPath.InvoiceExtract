using System.Globalization;
using System.Text.RegularExpressions;
using Ravelaso.UiPath.InvoiceExtract.Core;
using Ravelaso.UiPath.InvoiceExtract.Core.Helpers;
using Ravelaso.UiPath.InvoiceExtract.Core.Interfaces;
using Ravelaso.UiPath.InvoiceExtract.Core.Models;
// ReSharper disable UnusedAutoPropertyAccessor.Global


namespace InvoiceExtract.Example;


public record ExampleData : IInvoiceData
{
    public string DocumentName { get; set; } = string.Empty;
    public int DocumentPages { get; set; }
    public string? BilTo { get; set; }
    public string? ShipTo { get; set; }
    public string? ServiceAddress { get; set; }
    public double AmmountDue { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? CustomerId { get; set; }
    public DateTime? InvoiceDate { get; set; }
}

[InvoiceProfile("ExampleInvoice", DisplayName = "Example Invoice Extract")]
public class ExampleProcessor : BaseInvoiceProcessor<ExampleData>
{
    public override int[] Zones { get; } =
        [5,6,7, 55];

    public override PageSegmenterConfiguration GetPageSegmenterConfiguration()
    {
        return new DocstrumConfiguration(
            new(new()
            {
                WithinLineBounds = new(-60, 60),
                AngularDifferenceBounds = new(-60, 60),
                BetweenLineBounds = new(-60, 60),
                BetweenLineMultiplier = 1.5
            }));
    }

    public override PageConfiguration GetPageConfiguration()
    {
        return new()
        {
            ZonePages = [1],
            RegexPages = [1]
        };
    }

    public override void MapByZone(int zone, string txt, int pageNumber)
    {
        switch  (zone)
        {
            case 5:
                Data.BilTo = AfterLineBreak(txt);
                break;
            case 6:
                Data.ShipTo = AfterLineBreak(txt);
                break;
            case 7:
                Data.ServiceAddress = AfterLineBreak(txt);
                break;
            case 55:
                Data.AmmountDue = Toolkit.ParseCurrency(txt);
                break;
        }
    }

    public override void MapByRegex(string text, int pageNumber)
    {
        var invoiceMatch = Regex.Match(text, @"INVOICE:\s*(\S+)", RegexOptions.IgnoreCase);
        if (invoiceMatch.Success)
            Data.InvoiceNumber = invoiceMatch.Groups[1].Value.Trim();

        var dateMatch = Regex.Match(text, @"INVOICE DATE:\s*(\S+)", RegexOptions.IgnoreCase);
        if (dateMatch.Success)
            Data.InvoiceDate = Convert.ToDateTime(dateMatch.Groups[1].Value.Trim(), new CultureInfo("EN-US"));

        var customerIdMatch = Regex.Match(text, @"CUSTOMER ID:\s*(\S+)", RegexOptions.IgnoreCase);
        if (customerIdMatch.Success)
            Data.CustomerId = customerIdMatch.Groups[1].Value.Trim();

    }


    public override ExampleData ProcessInvoice()
    {
        return Data;
    }
}