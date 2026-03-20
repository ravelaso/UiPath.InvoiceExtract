# Invoice-Extract for UiPath

This framework is for UiPath developers who need reliable invoice data extraction from digital PDF documents with known layouts.

You define the invoice shape (record properties + `IInvoiceData`) and extraction logic in one processor class, and the library generates a ready-to-use UiPath activity for you. Just import the NuGet package into your UiPath custom activity project.

🎯 Why this is useful for UiPath users

- Focus on 2 things only:
  - Data: define the invoice model (properties you require)
  - Extraction: implement `MapByZone`, `MapByRegex`, `ProcessInvoice`

- The generator hides registration plumbing:
  - decorate processor with `[InvoiceProfile("YourKey", DisplayName="Extract YourKey Invoice")]`
  - library auto-registers and emits `ExtractYourKeyInvoice`

- No manual cast in workflows (strongly-typed result object from activity)

🔍 Developer productivity boosters

- Zone-based and regex-based mapping with minimal plumbing
- Helpers included: `AfterLineBreak`, `CaptureRegex`, `CompiledRegex`
- Visual analysis support: generate a PDF with Docstrum zones highlighted (`InvoiceHelper.AnalyzePdf(...)`) to validate mapping before extraction

🧩 Outcome

A small, maintainable code surface:
- record definition + processor class
- low maintenance when layout changes
- auto-generated UiPath activity appears in toolbox

Note:
- This library itself does not ship UiPath assemblies.
- In your CustomActivity .NET project, add UiPath dependencies separately.
- Recommended template: https://github.com/ravelaso/UiPath.Activities.Template

Dependencies:
- [PdfPig](https://github.com/UglyToad/PdfPig)

---


# 1 **Full Example**

#### 1. Create a .NET Custom UiPath Activity Project (You can use my templates or the UiPath Ones)

#### 2. Install the nuget package `Ravelaso.UiPath.InvoiceExtract`

#### 3. Create a class file and implement the `IInvoiceData` and `BaseProcessor<TInvoiceData>`

  ```csharp
   // Invoices/ContosoInvoice.cs
  
   using Ravelaso.UiPath.InvoiceExtract;
   
   namespace YourUiPathCustomActivityProject;
   
   /* Here is the record for our data.*/
   public record ContosoData : IInvoiceData
   {
       /* Required Property by the Interface */
       public string?  DocumentName   { get; set; }
       public int DocumentPages { get; set; }

       /* Additional Properties you need, example: */
       public string? InvoiceNumber  { get; set; }
       public string? Supplier       { get; set; }
       public string? Customer       { get; set; }
       public string? TotalAmount    { get; set; }
       public string? VatAmount      { get; set; }
       public string? PurchaseOrder  { get; set; }
   }
   
   /* Here is the processor for that data, marked with the Attribute (Decorator) */
   [InvoiceProfile("Contoso", DisplayName = "Extract Contoso Invoice")]
   public sealed class ContosoProcessor : BaseInvoiceProcessor<ContosoData>
   {
       /* ────────────── Docstrum Configuration ────────────── */
      // This defines the Boxes for each zone you will be able to extract, have a look at section 2 Analysis, down in the documentation
       public override DocstrumBoundingBoxes CreateBoundingBoxes() =>
           new(new DocstrumBoundingBoxes.DocstrumBoundingBoxesOptions
           {
               WithinLineBounds        = new(-45, 45),
               AngularDifferenceBounds = new(-45, 45),
               BetweenLineBounds       = new(-45, 45),
               BetweenLineMultiplier   = 1.4
           });

       // PageConfiguration defines which pages to use for each extraction method available.
       public override PageConfiguration GetPageConfiguration()
       {
         return new()
         {
               ZonePages = [1],  // You can give an array of pages which you can then take in the MapByZone()
               RegexPages = [PageConfiguration.AllPages] // AllPages will run the extraction method through all pages.
         };
       }
       // The zones you will take (See section 2 Analysis of the documentation)
       public override int[] Zones { get; } = [1, 2, 3, 4, 5];
   
       /* ────────────── Zone mapping - Docstrum  ────────────── */
       public override void MapByZone(int zone, string txt, int pageNumber)
       {
         // You can also switch for pageNumber if you have different extractions of zones per page, this case is only page 1
           switch (zone)
           {
               case 1: Data.Supplier      = txt.Trim();            break;
               case 2: Data.Customer      = txt.Trim();            break;
               case 3: Data.InvoiceNumber = AfterLineBreak(txt);   break;
               case 4: Data.TotalAmount   = AfterLineBreak(txt);   break; 
               case 5: Data.VatAmount     = AfterLineBreak(txt);   break;
           }
       }
   
       /* ────────────── Compiled regex definitions ────────────── */
       // This uses some of the library utilities to create Regex entries to check
       private static readonly Regex TotalAmountRx = CompiledRegex(@"(?i)\bTotal\b[:€]?\s*([\d.,]+)");
   
       private static readonly Regex VatAmountRx = CompiledRegex(@"(?i)\bVAT\b[:€]?\s*([\d.,]+)");
   
       private static readonly Regex PurchaseOrderRx = CompiledRegex(@"(?i)\bPO\s*#\s*(\w+)");
   

       /* ────────────── Regex mapping  ────────────── */
       public override void MapByRegex(string text, int pageNumber)
       {
         // We defined AllPages in our PageConfiguration, this means the regex will check against all pages. 
         // If you would like to run regex on different pages you could switch for pageNumber 
         // and also run our own private methods for each page, in this case we ignore pageNumbers

           Data.TotalAmount  ??= CaptureRegex(TotalAmountRx,  fullText, 1);
           Data.VatAmount    ??= CaptureRegex(VatAmountRx,    fullText, 1);
           Data.PurchaseOrder??= CaptureRegex(PurchaseOrderRx,fullText, 1);
       }
   
       public override ContosoData ProcessInvoice() 
       {
            return Data; 
            // Returns the processed data by this class, you could also format correctly the data here before exporting. 
            // Ex: You scanned a Date and you want to convert it to the same format before returning the data.
       };
   
   }
   ```

## 3. **Build the solution**  
   The generator will emit `Extract Contoso Invoice` activity automatically for UiPath, complete with correct generics and `using` directives.

## 4. **Use in UiPath**  
   - Import the package from your solution into UiPath
   - Drag **Extract Contoso Invoice** into the canvas
   - Set FilePath to the pdf, and result to a new variable.
   - Debug, and you will have a typed object with the data extracted. (Example: myInvoice.TotalAmount, myInvoice.InvoiceNumber)


# 2a BaseInvoiceProcessor & Overrides (per-invoice processor behavior)

### What `BaseInvoiceProcessor<T>` helps with
- `protected T Data` (your typed output record instance)
- Document metadata: `SetDocumentName`, `SetDocumentPages`
- Extraction helpers:
  - `AfterLineBreak`, `AfterLineBreakOnly`, `ExtractLine`
  - `CaptureRegex`, `CompiledRegex`
- Required mapping entrypoints:
  - `Zones`
  - `MapByZone(int zone, string text, int pageNumber)`
  - `MapByRegex(string text, int pageNumber)`
  - `ProcessInvoice()`

### Config overrides (customize for each invoice format)

1. `GetWordExtractorConfiguration()`
   - default: `DefaultWordExtractorConfiguration`
   - override to adjust word-splitting, min spacing, merging behavior

2. `GetPageSegmenterConfiguration()`
   - default: `DefaultPageSegmenterConfiguration`
   - override to tune layout analysis (Docstrum angle thresholds, between-line penalties, etc.)

3. `GetPageConfiguration()`
   - default: `PageConfiguration.Default` (all pages)
   - override to set:
     - `ZonePages = [1]`, `RegexPages = [PageConfiguration.AllPages]`, etc.
   - reduces processing load for known-page-only extraction

4. `GetLayoutConfiguration() + ApplyLayout(LayoutType)`
   - default: `LayoutConfiguration.None`
   - override for multi-layout invoice families
   - `ApplyLayout` can switch mapping behavior when `LayoutType.Layout` is detected

### Execution flow in one processor
1. Generator scans `[InvoiceProfile("Key", ...)]` processors → auto-registers
2. UIPath activity `Extract<Key>Invoice` is emitted
3. `InvoiceHelper.ProcessInvoice(processor, pdfPath)`:
   - sets document metadata (name/pages)
   - word extraction with `GetWordExtractorConfiguration`
   - page segmentation with `GetPageSegmenterConfiguration`
   - for each page from `GetPageConfiguration`:
     - zone mapping via `MapByZone(...)`
     - regex mapping via `MapByRegex(...)`
   - final object returned by `ProcessInvoice()` (typed record)

### Visual validation (Docstrum block analysis)
Use `InvoiceHelper.AnalyzePdf(...)` to produce a visual PDF with zones:
- checks `CreateBoundingBoxes()` and detected zones
- prints zone numbers + text in overlay boxes
- ideal to tune `Zones`, `MapByZone`, Docstrum settings before production


# 2b. Layout-based extraction (multi-template invoices)

Some invoices with the same `InvoiceProfile` may come in slightly different visual layouts.
For these cases you can use the layout detection API to switch mapping logic at runtime.

### Core build blocks

- `GetLayoutConfiguration()` returns a `LayoutConfiguration` containing one or more `LayoutRule`.
- `ApplyLayout(LayoutType layoutType)` receives the detected layout and stores it (or adjusts behavior).
- `MapByZone(...)` can branch on `Data.Layout.Layout` or `layoutType` to handle multiple layout formats.

### How to

`LayoutConfiguration` defines how invoice extraction selects a layout by matching document text to key/value rules.

- `UseLayoutDetection`: bool, enables or disables layout-based matching.
- `Rules`: array of `LayoutRule`.
  - `Key`: text fragment (e.g. `"Invoice Number"`).
  - `Value`: optional additional token after key (e.g. `"INV-"`).
  - `LayoutNumber`: numeric layout ID when matched (e.g. `1`, `2`).

When creating:
1. identify distinctive keywords for each invoice variant,
2. set optional value patterns to avoid ambiguity,
3. assign a unique layout number,
4. use `LayoutConfiguration.Create(new LayoutRule(...), ...)`,
5. use `LayoutConfiguration.None` when detection is not needed.

Example semantics:
- layout `1` if `"Invoice Number"` exists and contains `"INV-"`,
- layout `2` if `"Factuurnummer"` exists and contains `"FACT"`.

### Minimal concept example

```csharp
public record LayoutDemoData : IInvoiceData
{
    public string? DocumentName { get; set; }
    public int DocumentPages { get; set; }

    public LayoutType Layout { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? TotalAmount { get; set; }
}

[InvoiceProfile("DemoLayout", DisplayName = "Extract Demo Layout Invoice")]
public class LayoutDemoProcessor : BaseInvoiceProcessor<LayoutDemoData>
{
    public override int[] Zones => [1, 2, 3];

    public override PageConfiguration GetPageConfiguration() => new()
    {
        ZonePages = [PageConfiguration.AllPages],
        RegexPages = [PageConfiguration.AllPages]
    };

    public override LayoutConfiguration GetLayoutConfiguration() => LayoutConfiguration.Create(
        new LayoutRule("Invoice Number", 1, "INV-"),
        new LayoutRule("Factuurnummer", 2, "FACT")
    );

    public override void ApplyLayout(LayoutType layoutType)
    {
        Data.Layout = layoutType;
    }

    public override void MapByZone(int zone, string text, int pageNumber)
    {
        if (Data.Layout.Layout == 1)
        {
            // Layout 1 mapping
            if (zone == 1) Data.InvoiceNumber = text.Trim();
            if (zone == 2) Data.TotalAmount = text.Trim();
        }
        else if (Data.Layout.Layout == 2)
        {
            // Layout 2 mapping uses different zones
            if (zone == 2) Data.InvoiceNumber = text.Trim();
            if (zone == 3) Data.TotalAmount = text.Trim();
        }
    }

    public override void MapByRegex(string text, int pageNumber)
    {
        // optional fallback/validation
    }

    public override LayoutDemoData ProcessInvoice() => Data;
}
```

### What this gives you

- Detect invoice version automatically by scanning page text for layout markers
- Keep one processor / one activity for related layouts
- Switch field mapping rules dynamically in `MapByZone`, `MapByRegex`, or `ProcessInvoice`
- Useful when “same supplier, different template versions” exist

### Quick workflow

1. `InvoiceHelper.ProcessInvoice(processor, pdfPath)` runs layout detection
2. `ApplyLayout(...)` gets called with `LayoutType.Layout` and `LayoutType.Page`
3. `MapByZone` / `MapByRegex` use `Data.Layout` to choose mapping variant
4. `ProcessInvoice()` returns a typed object


---

# 3 Analysis

## Visual Analysis with InvoiceHelper

Use the `InvoiceHelper.AnalyzePdf()` static method to visually verify your zone mappings before extraction:

```csharp
// Analyze your PDF with your processor's Docstrum configuration
var processor = new ContosoProcessor();
var outputPath = "path/to/save/analyzed_invoice.pdf";

InvoiceHelper.AnalyzePdf(processor, "path/to/invoice.pdf", outputPath);
```

This generates a new PDF with all detected zones highlighted as colored boxes, showing:
- Zone boundaries based on your `CreateBoundingBoxes()` configuration
- Zone numbers for each extracted region
- Text content within each zone
- outputPath is optional, and if not provided it will create the analyzed pdf next to the pdf you used.

**Use this to:**
- Verify zone placement matches your `MapByZone()` logic
- Adjust `DocstrumBoundingBoxes` parameters if zones are misaligned
- Confirm all required data zones are correctly detected before deploying to UiPath


You can also use the `InvoiceHelper.AnalyzePdfsInFolder()` to analyze all pds of a folder based on your processor:

```csharp
// Analyze folder with PDFs with your processor's Docstrum configuration
var processor = new ContosoProcessor();

InvoiceHelper.AnalyzePdfsInFolder(processor, "Path/To/Folder");
```

This will create a folder `Analyzed` within the input folder,  with the processed pdfs inside them.

**Use this to:**

- Confirm your `DocstrumBoundingBoxes` works for the layout in multiple invoices at once.


## Docstrum & Document Layout Analysis

If you want to know more about the implementations of Docstrum (thanks to [PdfPig](https://github.com/UglyToad/PdfPig) library) please look at this reference links:

- [Docstrum Configuration](https://github.com/UglyToad/PdfPig/wiki/Document-Layout-Analysis#docstrum-for-bounding-boxes-method)

- [PageSegmenter Configuration](https://github.com/UglyToad/PdfPig/wiki/Document-Layout-Analysis#page-segmenters)

- [WordExtractor Configuration](https://github.com/UglyToad/PdfPig/wiki/Document-Layout-Analysis#word-extractors)


---

# 4 **Package overview**

```
Ravelaso.UiPath.InvoiceExtract.Generators      ← Roslyn generator  (netstandard2.0) (bundled with the package)
Ravelaso.UiPath.InvoiceExtract.Core          ← Runtime library   (net6.0) (bundled with the package)

YourUiPathProject                           ← Use a UiPath Custom Activity Template
└─ Invoices/
   ├─ ExampleInvoice.cs             ← Includes: IInvoiceProcessor<ExampleData> & IInvoiceData<ExampleData>
   ├─ Example2Invoice.cs                  ← Includes: IInvoiceProcessor<Example2Data> & IInvoiceData<Example2Data>
   └─ ...
```

1. Scans *Invoices* for processors (classes) marked with `[InvoiceProfile(...)]`.
2. Reads the processor type and its `IInvoiceProcessor<TData>` generic argument.
3. Emits a sealed UiPath activity named from the profile key, for example:

```csharp
// auto-generated

[DisplayName(@"Example Invoice Extract")]
public sealed class ExtractExampleInvoiceInvoice : CodeActivity
{
    [RequiredArgument]
    [DisplayName("File Path (Input)")]
    public InArgument<string> FilePath { get; set; }

    [RequiredArgument]
    [DisplayName("ExampleData (Output)")]
    public OutArgument<ExampleData> Result { get; set; }

    protected override void Execute(CodeActivityContext context)
    {
        try
        {
            var pdfPath = FilePath.Get(context);
            var processor = new ExampleProcessor();
            var data = InvoiceHelper.ProcessInvoice(processor, pdfPath);
            Result.Set(context, data);
        }
        catch (Exception e)
        {
            var runtime = context.GetExtension<IExecutorRuntime>();
            if (runtime is not null)
            {
                runtime.LogMessage(new LogMessage
                {
                    EventType = TraceEventType.Error,
                    Message = e.Message
                });
            }

            throw;
        }
    }
}

```
---





