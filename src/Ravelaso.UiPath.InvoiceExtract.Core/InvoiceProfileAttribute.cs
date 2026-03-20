namespace Ravelaso.UiPath.InvoiceExtract.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class InvoiceProfileAttribute(string key) : Attribute
{
    public string Key { get; } = key;

    public string? DisplayName { get; init; }
}