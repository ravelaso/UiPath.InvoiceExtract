using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;

namespace Ravelaso.UiPath.InvoiceExtract.Core.Models;

/// <summary>
/// Represents the types of page segmenters used for document layout analysis.
/// </summary>
public enum PageSegmenterType
{
    /// Represents the Docstrum page segmentation algorithm in the context of
    /// document layout analysis. The Docstrum algorithm is commonly used
    /// for grouping text elements on a page by analyzing their spatial
    /// relationships to extract logical structure.
    Docstrum,

    /// Represents the Recursive XY-Cut page segmentation algorithm type.
    /// This algorithm is used for segmenting a page into regions based
    /// on recursive cutting along the X and Y axes. It is commonly applied
    /// in document layout analysis to identify distinct components
    /// such as paragraphs, images, or tables by analyzing spatial relationships.
    RecursiveXyCut,

    /// Represents the default page segmentation strategy used for dividing a page
    /// into logical components. This segmenter is typically a general-purpose
    /// option that provides a standard segmentation approach without requiring
    /// additional configuration.
    DefaultPageSegmenter
}

/// <summary>
/// Represents the configuration for a page segmenter, which determines the algorithm
/// and settings used to divide a page into distinct zones or blocks for text extraction purposes.
/// </summary>
/// <remarks>
/// Extend this abstract class to define custom configurations for specific segmentation approaches,
/// such as Docstrum, RecursiveXYCut, or a default page segmenter. Each derived configuration must
/// specify the type of segmentation it supports and provide an implementation for obtaining the page segmenter.
/// </remarks>
public abstract class PageSegmenterConfiguration
{
    /// <summary>
    /// Represents the type of the page segmentation algorithm used within the configuration.
    /// This property must be implemented by derived classes and indicates which page segmentation
    /// strategy should be used (e.g., Docstrum, RecursiveXyCut, or DefaultPageSegmenter).
    /// </summary>
    public abstract PageSegmenterType Type { get; }

    /// Retrieves the configured page segmenter instance from the derived PageSegmenterConfiguration classes.
    /// The returned page segmenter is used for segmenting a page into logical blocks.
    /// <returns>A specific implementation of IPageSegmenter based on the configuration.</returns>
    public abstract IPageSegmenter GetSegmenter();
}

/// <summary>
/// Represents the configuration for the Docstrum page segmentation algorithm.
/// </summary>
/// <remarks>
/// This class allows for configuring and instantiating the Docstrum page segmentation strategy
/// for document layout analysis. It inherits from the <see cref="PageSegmenterConfiguration"/> base class
/// and specifies Docstrum as the segmenter type.
/// </remarks>
public class DocstrumConfiguration(DocstrumBoundingBoxes boundingBoxes) : PageSegmenterConfiguration
{
    /// <summary>
    /// Gets the type of the page segmentation approach used by the configuration.
    /// This property defines the specific segmentation algorithm to be utilized
    /// when processing documents, such as Docstrum, Recursive XY Cut, or Default Page Segmenter.
    /// </summary>
    public override PageSegmenterType Type => PageSegmenterType.Docstrum;

    /// <summary>
    /// Represents the bounding boxes used in the Docstrum configuration for page segmentation.
    /// This property defines the structural boundaries for analyzing document layouts
    /// when using the Docstrum-based page segmentation method.
    /// </summary>
    public DocstrumBoundingBoxes BoundingBoxes { get; set; } =
        boundingBoxes ?? throw new ArgumentNullException(nameof(boundingBoxes));

    /// <summary>
    /// Retrieves the page segmenter instance based on the current configuration.
    /// </summary>
    /// <returns>
    /// An object implementing <see cref="IPageSegmenter"/> that defines the algorithm
    /// or method for segmenting a page into distinct parts.
    /// </returns>
    public override IPageSegmenter GetSegmenter()
    {
        return BoundingBoxes;
    }
}

/// <summary>
/// Provides configuration options for the Recursive XY-Cut page segmentation algorithm.
/// </summary>
/// <remarks>
/// The Recursive XY-Cut algorithm is a method for dividing a document page into discrete regions
/// by recursively splitting the page along horizontal and vertical lines based on whitespace analysis
/// and minimum width constraints.
/// </remarks>
public class RecursiveXyCutConfiguration : PageSegmenterConfiguration
{
    /// Represents the configuration for the Recursive X-Y Cut page segmenter algorithm.
    /// Provides a mechanism to configure and retrieve an instance of the Recursive X-Y Cut segmenter
    /// with optional customization, such as specifying a minimum width for the segmentation process.
    public RecursiveXyCutConfiguration()
    {
    }

    /// <summary>
    /// Represents the configuration for the RecursiveXYCut page segmentation method.
    /// </summary>
    /// <remarks>
    /// This configuration is used to set parameters specific to the RecursiveXYCut algorithm,
    /// which is a strategy for segmenting document pages based on recursive cutting of regions
    /// defined by whitespace.
    /// </remarks>
    public RecursiveXyCutConfiguration(int minWidth)
    {
        MinWidth = minWidth;
    }

    /// <summary>
    /// Represents the type of page segmenter configuration.
    /// </summary>
    /// <remarks>
    /// This property indicates the specific page segmentation approach adopted by
    /// a given configuration. It is typically used to determine the algorithm
    /// for splitting pages into zones, blocks, or structures for text processing.
    /// </remarks>
    public override PageSegmenterType Type => PageSegmenterType.RecursiveXyCut;

    /// Gets or sets the minimum width for segments when using the Recursive XY Cut page segmentation algorithm.
    /// This property determines the smallest allowable width for segments created by the algorithm,
    /// and affects how the content on a page is divided into sections. A higher value can result in larger
    /// segments, while a lower value allows for finer granularity in segmentation.
    private int MinWidth { get; set; }

    /// <summary>
    /// Gets the page segmenter instance used for document layout analysis.
    /// </summary>
    /// <returns>
    /// An instance of <see cref="IPageSegmenter"/> configured based on the specific
    /// implementation details of the subclass.
    /// </returns>
    public override IPageSegmenter GetSegmenter()
    {
        return MinWidth > 0 ? new(new() { MinimumWidth = MinWidth }) : RecursiveXYCut.Instance;
    }
}

/// <summary>
/// Represents the default configuration for the page segmentation process in a PDF document.
/// </summary>
/// <remarks>
/// This class utilizes the default page segmentation algorithm provided by the system.
/// It acts as a fallback configuration for cases where no specific segmentation method
/// has been defined or required. The associated segmentation type is set to
/// <see cref="PageSegmenterType.DefaultPageSegmenter"/>.
/// This class provides a singleton instance of <see cref="DefaultPageSegmenter"/> as the segmenter.
/// </remarks>
public class DefaultPageSegmenterConfiguration : PageSegmenterConfiguration
{
    /// <summary>
    /// Gets the type of page segmenter to be used.
    /// </summary>
    /// <value>
    /// A <see cref="PageSegmenterType"/> indicating the strategy or algorithm
    /// for segmenting a page into different regions for processing.
    /// </value>
    /// <remarks>
    /// Implementations of <see cref="PageSegmenterConfiguration"/> must specify
    /// the type of segmenter to guide the page segmentation logic. Common types
    /// include Docstrum, RecursiveXyCut, or a default segmenter.
    /// </remarks>
    public override PageSegmenterType Type => PageSegmenterType.DefaultPageSegmenter;

    /// <summary>
    /// Obtains the page segmenter instance based on the current configuration.
    /// </summary>
    /// <returns>
    /// An instance of <see cref="IPageSegmenter"/> that performs the segmentation using
    /// the defined algorithm or segmentation strategy.
    /// </returns>
    /// <remarks>
    /// This method should be implemented to return a specific implementation of the
    /// <see cref="IPageSegmenter"/> interface, such as Docstrum or DefaultPageSegmenter,
    /// based on the configuration details.
    /// </remarks>
    public override IPageSegmenter GetSegmenter()
    {
        return DefaultPageSegmenter.Instance;
    }
}