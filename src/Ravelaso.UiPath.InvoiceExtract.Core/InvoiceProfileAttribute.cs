namespace Ravelaso.UiPath.InvoiceExtract.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class InvoiceProfileAttribute : Attribute
{
    public InvoiceProfileAttribute(string key)
    {
        Key = key;
    }

    public string Key { get; }

    public string? DisplayName { get; init; }
}