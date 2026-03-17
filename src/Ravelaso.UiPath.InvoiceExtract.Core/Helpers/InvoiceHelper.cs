// InvoiceHelper.cs

using System.Text;
using System.Text.RegularExpressions;
using Ravelaso.UiPath.InvoiceExtract.Core.Interfaces;
using Ravelaso.UiPath.InvoiceExtract.Core.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector;

namespace Ravelaso.UiPath.InvoiceExtract.Core.Helpers;

public static class InvoiceHelper
{
    private static readonly RenderingReadingOrderDetector Renderer = new();

    public static T ProcessInvoice<T>(IInvoiceProcessor<T> processor, string pdfPath) where T : IInvoiceData
    {
        // Handle no PDF Found.
        if (!File.Exists(pdfPath))
            throw new FileNotFoundException("File not found", pdfPath);

        // Set document name
        var documentName = Path.GetFileName(pdfPath);
        processor.SetDocumentName(documentName); // Using the new interface method

        // Process the document
        using var document = PdfDocument.Open(pdfPath);
        var allPages = document.GetPages().ToList();
        var pageCount = allPages.Count;
        processor.SetDocumentPages(pageCount);

        // Check if layout detection is needed
        var layoutConfig = processor.GetLayoutConfiguration();
        if (layoutConfig.UseLayoutDetection)
        {
            var layoutType = DetectLayout(allPages, layoutConfig);
            processor.ApplyLayout(layoutType);
        }

        // Get page configuration from processor
        var pageConfig = processor.GetPageConfiguration();

        // Process zones for configured pages
        if (pageConfig.ZonePages != null)
        {
            var zonePagesToProcess = pageConfig.GetZonePagesToProcess(pageCount);

            foreach (var pageNumber in zonePagesToProcess.Where(p => p <= pageCount))
            {
                var page = document.GetPage(pageNumber);
                PopulateByZones(processor, page, pageNumber);
            }
        }

        // Process regex for configured pages
        if (pageConfig.RegexPages != null)
        {
            var regexPagesToProcess = pageConfig.GetRegexPagesToProcess(pageCount);

            foreach (var pageNumber in regexPagesToProcess.Where(p => p <= pageCount))
            {
                var page = allPages[pageNumber - 1]; // Pages are 1-indexed
                var pageText = GetPageText(page);
                processor.MapByRegex(pageText, pageNumber);
            }
        }

        // Return the processed invoice data
        return processor.ProcessInvoice();
    }

    // public static object ProcessInvoice(InvoiceType invoiceType, string pdfPath)
    // {
    //     var dataType = InvoiceRegistry.GetDataType(invoiceType);
    //
    //     // Find the generic ProcessInvoice<T> method
    //     var methods =
    //         typeof(InvoiceHelper).GetMethods(BindingFlags.Public |
    //                                          BindingFlags.Static);
    //     var genericMethod = methods.FirstOrDefault(m =>
    //         m is { Name: nameof(ProcessInvoice), IsGenericMethodDefinition: true } &&
    //         m.GetParameters().Length == 2 &&
    //         m.GetParameters()[0].ParameterType.IsGenericType &&
    //         m.GetParameters()[1].ParameterType == typeof(string));
    //
    //     if (genericMethod == null)
    //         throw new InvalidOperationException("Could not find ProcessInvoice<T> method");
    //
    //     var method = genericMethod.MakeGenericMethod(dataType);
    //     var processor = InvoiceRegistry.Create(invoiceType, dataType);
    //
    //     return method.Invoke(null, [processor, pdfPath])
    //            ?? throw new InvalidOperationException("Failed to process invoice");
    // }

    private static void PopulateByZones<T>(IInvoiceProcessor<T> processor, Page page, int pageNumber)
        where T : IInvoiceData
    {
        var letters = page.Letters;

        // Get the configured word extractor
        var extractorConfig = processor.GetWordExtractorConfiguration();
        var extractor = extractorConfig.GetExtractor();
        var words = extractor.GetWords(letters);

        // Get the configured page segmenter
        var segmenterConfig = processor.GetPageSegmenterConfiguration();
        var segmenter = segmenterConfig.GetSegmenter();

        var blocks = segmenter.GetBlocks(words);
        var orderedBlocks = Renderer.Get(blocks);
        var zoneSet = new HashSet<int>(processor.Zones);

        foreach (var block in orderedBlocks)
        {
            var zone = block.ReadingOrder;
            if (!zoneSet.Contains(zone)) continue;

            processor.MapByZone(zone, block.Text, pageNumber);
        }
    }

    private static string GetPageText(Page page)
    {
        var stringBuilder = new StringBuilder();
        foreach (var word in page.GetWords())
        {
            stringBuilder.Append(word);
            stringBuilder.Append(' ');
        }

        return stringBuilder.ToString();
    }

    // This method is only useful for analysis
    // public static void AnalyzePdf<T>(string pdfPath, InvoiceType invoiceType) where T : IInvoiceData
    // {
    //     var fileName = Path.GetFileNameWithoutExtension(pdfPath);
    //     var outputFile = Path.Combine(Path.GetDirectoryName(pdfPath)!, $"{fileName}_analyzed.pdf");
    //     PaintBlocksByDocstrum<T>(pdfPath, outputFile, invoiceType);
    // }

    // public static void AnalyzePdf(InvoiceType invoiceType, string pdfPath)
    // {
    //     var dataType = InvoiceRegistry.GetDataType(invoiceType);
    //     var method = typeof(InvoiceHelper)
    //         .GetMethod(nameof(AnalyzePdf), 1, [typeof(string), typeof(InvoiceType)])
    //         ?.MakeGenericMethod(dataType);
    //     method?.Invoke(null, [pdfPath, invoiceType]);
    // }

    // This method is only useful for analysis
    // public static void AnalyzePdfsInFolder<T>(string folderPath, InvoiceType invoiceType) where T : IInvoiceData
    // {
    //     var analyzedFolderPath = Path.Combine(folderPath, "Analyzed");
    //     Directory.CreateDirectory(analyzedFolderPath);
    //
    //     var pdfFiles = Directory.GetFiles(folderPath, "*.pdf");
    //     foreach (var file in pdfFiles)
    //     {
    //         var fileName = Path.GetFileNameWithoutExtension(file);
    //         var outputFilePath = Path.Combine(analyzedFolderPath, $"{fileName}_analyzed.pdf");
    //         PaintBlocksByDocstrum<T>(file, outputFilePath, invoiceType);
    //     }
    // }

    // public static void AnalyzePdfsInFolder(InvoiceType invoiceType, string folderPath)
    // {
    //     var dataType = InvoiceRegistry.GetDataType(invoiceType);
    //     var method = typeof(InvoiceHelper)
    //         .GetMethod(nameof(AnalyzePdfsInFolder), 1, [typeof(string), typeof(InvoiceType)])
    //         ?.MakeGenericMethod(dataType);
    //     method?.Invoke(null, [folderPath, invoiceType]);
    // }

    // This method is only useful for analysis
    // private static void PaintBlocksByDocstrum<T>(string inputFilePath, string outputFilePath, InvoiceType invoiceType)
    //     where T : IInvoiceData
    // {
    //     using var document = PdfDocument.Open(inputFilePath);
    //     var builder = new PdfDocumentBuilder();
    //     var font = builder.AddStandard14Font(Standard14Font.Helvetica);
    //     var processor = InvoiceRegistry.Create<T>(invoiceType);
    //
    //     // Get the configured word extractor
    //     var extractorConfig = processor.GetWordExtractorConfiguration();
    //     var extractor = extractorConfig.GetExtractor();
    //
    //     // Get the configured page segmenter
    //     var segmenterConfig = processor.GetPageSegmenterConfiguration();
    //     var segmenter = segmenterConfig.GetSegmenter();
    //
    //     var pageCount = document.NumberOfPages;
    //
    //     // Process each page
    //     for (var pageNum = 1; pageNum <= pageCount; pageNum++)
    //     {
    //         var page = document.GetPage(pageNum);
    //         var pageBuilder = builder.AddPage(document, pageNum);
    //         pageBuilder.SetStrokeColor(0, 255, 0);
    //
    //         // Extract words and create blocks for this page
    //         var letters = page.Letters;
    //         var words = extractor.GetWords(letters);
    //         var blocks = segmenter.GetBlocks(words);
    //         var orderedTextBlocks = Renderer.Get(blocks);
    //
    //         // Paint each block with its zone number (zones reset per page)
    //         foreach (var block in orderedTextBlocks)
    //         {
    //             var bbox = block.BoundingBox;
    //             pageBuilder.DrawRectangle(bbox.BottomLeft, bbox.Width, bbox.Height);
    //             pageBuilder.AddText(block.ReadingOrder.ToString(), 8, bbox.TopLeft, font);
    //         }
    //     }
    //
    //     var fileBytes = builder.Build();
    //     File.WriteAllBytes(outputFilePath, fileBytes);
    //     Console.WriteLine($@"Analyzed successfully: {Path.GetFileName(inputFilePath)}");
    // }

    private static LayoutType DetectLayout(IEnumerable<Page> pages, LayoutConfiguration config)
    {
        if (!config.UseLayoutDetection || config.Rules.Count == 0)
            return new() { Layout = 0, Page = 0 };

        foreach (var page in pages)
        {
            var text = GetPageText(page);

            // Check each rule in order (first match wins)
            foreach (var rule in config.Rules)
            {
                if (string.IsNullOrEmpty(rule.Key))
                    continue;

                // Pattern to match just "Key:"
                var patternKey = $@"{Regex.Escape(rule.Key)}\s*:";

                // Check if the key exists on this page
                if (!Regex.IsMatch(text, patternKey, RegexOptions.IgnoreCase))
                    continue;

                // If we have a specific value to check
                if (!string.IsNullOrEmpty(rule.Value))
                {
                    // Pattern to match "Key:" followed by optional whitespace and the value
                    var patternWithValue = $@"{Regex.Escape(rule.Key)}\s*:\s*{Regex.Escape(rule.Value)}";

                    // Check if the specific value exists with the key
                    if (Regex.IsMatch(text, patternWithValue, RegexOptions.IgnoreCase))
                        return new()
                        {
                            Layout = rule.LayoutNumber,
                            Page = page.Number
                        };
                }
                else
                {
                    // No specific value to check, just return that we found the key
                    return new()
                    {
                        Layout = rule.LayoutNumber,
                        Page = page.Number
                    };
                }
            }
        }

        // If no rule matched on any page
        return new()
        {
            Layout = 0,
            Page = 0
        };
    }
}