using System.Activities;
using InvoiceExtract.Example;
using Ravelaso.UiPath.InvoiceExtract.Core.Helpers;
using Ravelaso.UiPath.InvoiceExtract.Generated;
using Ravelaso.UiPath.InvoiceExtract.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;
using static Ravelaso.UiPath.InvoiceExtract.Tests.Helpers.TestHelper;

namespace Ravelaso.UiPath.InvoiceExtract.Tests;

public class ExtractionTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ExtractionTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }


    [Fact]
    public void ExtractExampleInvoice_ShouldExtractData()
    {
        // Arrange
        var activity = new ExtractExampleInvoiceInvoice()
        {
            FilePath = new(GetTestInvoicePath("example.pdf"))
        };

        // Act
        var exampleData = WorkflowInvoker.Invoke(activity)["Result"] as ExampleData;

        // Assert
        Assert.NotNull(exampleData);
        Assert.IsType<ExampleData>(exampleData);

        // Add specific assertions

        // Log the extracted data
        LogAllProperties(_testOutputHelper, exampleData);
    }

    [Fact]
    public void AnalyzeExampleInvoice()
    {
        var exampleFile = GetTestInvoicePath("example.pdf");

        InvoiceHelper.AnalyzePdf(new ExampleProcessor(), exampleFile);
    }

    [Fact]
    public void Registration()
    {
        var x = InvoiceCatalog.All;
        var y = InvoiceCatalog.Get(InvoiceProfileKey.ExampleInvoice);
        LogAllProperties(_testOutputHelper, y.ProcessorType);
    }

}