using System.Globalization;
using System.Text.RegularExpressions;
using Ravelaso.UiPath.InvoiceExtract.Core;
using Ravelaso.UiPath.InvoiceExtract.Core.Helpers;
using Ravelaso.UiPath.InvoiceExtract.Core.Interfaces;
using Ravelaso.UiPath.InvoiceExtract.Core.Models;


namespace InvoiceExtract.Example;

public record ExampleData : IInvoiceData
{
    public string DocumentName { get; set; } = string.Empty;
    public int DocumentPages { get; set; }
}

[InvoiceProfile("ExampleInvoice", DisplayName = "Example Invoice Extract")]
public class ExampleProcessor : BaseInvoiceProcessor<ExampleData>
{
    public override int[] Zones { get; } =
        [1];

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
            RegexPages = null
        };
    }

    public override void MapByZone(int zone, string txt, int pageNumber)
    {

    }

    public override void MapByRegex(string text, int pageNumber)
    {
    }


    public override ExampleData ProcessInvoice()
    {
        return Data;
    }
}