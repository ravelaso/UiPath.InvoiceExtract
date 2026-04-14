using System.Activities;
using InvoiceExtract.Example;
using Ravelaso.UiPath.InvoiceExtract.Generated;
using Ravelaso.UiPath.InvoiceExtract.Core.Helpers;
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
        var y = InvoiceCatalog.ExampleInvoice;
        var exampleFile = GetTestInvoicePath("example.pdf");
        var processor = y.CreateProcessor();
        var data = InvoiceHelper.ProcessInvoice(processor, exampleFile);
        LogAllProperties(_testOutputHelper,data);
    }

    [Fact]
    public void ExtractFolder_ShouldCreateJsonAndCsv()
    {
        // Arrange
        var registration = InvoiceCatalog.ExampleInvoice;
        var exampleFile = GetTestInvoicePath("example.pdf");
        var folder = Path.GetDirectoryName(exampleFile)!;
        var extractedDir = Path.Combine(folder, "Extracted");

        // Clean up before test
        if (Directory.Exists(extractedDir))
            Directory.Delete(extractedDir, true);

        // Act
        // Use the generated registration's CreateProcessor as factory
        InvoiceHelper.ExtractInvoicesInFolder(
            registration.CreateProcessor,
            folder);

        // Assert
        Assert.True(Directory.Exists(extractedDir));
        Assert.True(File.Exists(Path.Combine(extractedDir, "extract.json")));
        Assert.True(File.Exists(Path.Combine(extractedDir, "invoices.csv")));

        var jsonContent = File.ReadAllText(Path.Combine(extractedDir, "extract.json"));
        Assert.Contains("example.pdf", jsonContent);

        var csvContent = File.ReadAllText(Path.Combine(extractedDir, "invoices.csv"));
        Assert.Contains("DocumentName", csvContent);
        Assert.Contains("example.pdf", csvContent);
    }

}