using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
using UglyToad.PdfPig.Util;

namespace Ravelaso.UiPath.InvoiceExtract.Core.Models;

/// <summary>
/// Specifies the type of word extraction strategy used when processing PDF documents.
/// </summary>
public enum WordExtractorType
{
    /// <summary>
    /// Represents the default word extraction type to be used for extracting words from a document.
    /// This type typically uses the default implementation provided by the system for processing
    /// and extracting words without any specialized or custom logic.
    /// </summary>
    Default,

    /// Represents the NearestNeighbour word extraction method configuration type.
    /// This type indicates the use of the Nearest Neighbour algorithm
    /// for extracting words from a PDF layout using word spacing and positioning.
    NearestNeighbour
}

/// <summary>
/// Represents a configuration for word extraction from PDF documents.
/// </summary>
/// <remarks>
/// This abstract class serves as a blueprint for defining specific word extraction
/// configurations. Implementations of this class should provide the extractor type
/// and the logic to retrieve a corresponding word extractor.
/// </remarks>
public abstract class WordExtractorConfiguration
{
    /// <summary>
    /// Gets the type of the word extractor used in the configuration.
    /// This property allows retrieval of the specific <see cref="WordExtractorType"/>
    /// associated with the current configuration.
    /// </summary>
    public abstract WordExtractorType Type { get; }

    /// <summary>
    /// Retrieves the specific word extractor implementation based on the configuration.
    /// </summary>
    /// <returns>
    /// An instance of <see cref="IWordExtractor"/> that corresponds to the configured extraction strategy.
    /// </returns>
    public abstract IWordExtractor GetExtractor();
}

/// <summary>
/// Provides the default implementation of <see cref="WordExtractorConfiguration"/>
/// for extracting words from PDF content.
/// </summary>
/// <remarks>
/// This class uses the <see cref="DefaultWordExtractor"/> instance to define the default
/// behavior for extracting words from PDFs. It is intended to be used as the standard
/// configuration for word extraction unless a custom configuration is specified.
/// </remarks>
public class DefaultWordExtractorConfiguration : WordExtractorConfiguration
{
    /// <summary>
    /// Specifies the type of word extractor configuration associated with this implementation.
    /// </summary>
    /// <remarks>
    /// The <see cref="Type"/> property defines the strategy or algorithm used for extracting
    /// words from a document. It may represent configurations such as default extraction
    /// or nearest neighbour methods.
    /// </remarks>
    public override WordExtractorType Type => WordExtractorType.Default;

    /// <summary>
    /// Retrieves an instance of the designated word extractor.
    /// </summary>
    /// <returns>
    /// An implementation of <see cref="IWordExtractor"/> that corresponds to the configuration
    /// type set in the derived class.
    /// </returns>
    /// <remarks>
    /// This method provides a way to initialize and return the specific word extractor
    /// defined by the configuration. Derived classes of <see cref="WordExtractorConfiguration"/>
    /// should override this method to supply the extractor suited to their configuration.
    /// </remarks>
    public override IWordExtractor GetExtractor()
    {
        return DefaultWordExtractor.Instance;
    }
}

/// <summary>
/// Represents the configuration for the Nearest Neighbour word extraction technique.
/// This class determines how words are extracted from a PDF document using the
/// Nearest Neighbour algorithm provided by the PDF parsing library.
/// </summary>
public class NearestNeighbourWordExtractorConfiguration : WordExtractorConfiguration
{
    /// <summary>
    /// Gets the type of word extractor configuration.
    /// </summary>
    /// <value>
    /// A <see cref="WordExtractorType"/> value that specifies the type of word extraction
    /// strategy to be used, such as <c>Default</c> or <c>NearestNeighbour</c>.
    /// </value>
    /// <remarks>
    /// This property is abstract, and its implementation must return the type of
    /// word extraction strategy associated with the specific configuration.
    /// </remarks>
    public override WordExtractorType Type => WordExtractorType.NearestNeighbour;

    /// <summary>
    /// Retrieves an implementation of the <see cref="IWordExtractor"/> based on the current configuration.
    /// </summary>
    /// <returns>
    /// An instance of <see cref="IWordExtractor"/> that is configured to extract words
    /// according to the specific implementation of the <see cref="WordExtractorConfiguration"/>.
    /// </returns>
    /// <remarks>
    /// Override this method to define custom behavior or use specific extractor implementations
    /// for handling word extraction from PDF content.
    /// </remarks>
    public override IWordExtractor GetExtractor()
    {
        return NearestNeighbourWordExtractor.Instance;
    }
}