namespace Ravelaso.UiPath.InvoiceExtract.Core.Interfaces;

public interface IInvoiceData
{
    string DocumentName { get; set; }
    int DocumentPages { get; set; }
}