using Ravelaso.UiPath.InvoiceExtract.Core;
using Ravelaso.UiPath.InvoiceExtract.Core.Interfaces;
using Ravelaso.UiPath.InvoiceExtract.Core.Models;

namespace Example;
public record SampleInvoiceData : IInvoiceData
{
    public string DocumentName { get; set; } = string.Empty;
    public int DocumentPages { get; set; }
    public string? InvoiceNumber { get; set; }
}

[InvoiceProfile("Sample", DisplayName = "Extract Sample Invoice")]
public sealed class SampleInvoiceProcessor : BaseInvoiceProcessor<SampleInvoiceData>
{
    public override int[] Zones { get; } = [];

    public override PageConfiguration GetPageConfiguration()
    {
        return new()
        {
            ZonePages = null,
            RegexPages = [PageConfiguration.AllPages]
        };
    }

    public override void MapByZone(int zone, string value, int pageNumber)
    {
    }

    public override void MapByRegex(string text, int pageNumber)
    {
        Data.InvoiceNumber ??= "TEST-001";
    }

    public override SampleInvoiceData ProcessInvoice()
    {
        return Data;
    }
}