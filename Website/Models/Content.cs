using System.ComponentModel.DataAnnotations;

namespace Website.Models
{
    //
    //Class Name : Content
    //Purpose : Represents educational content such as courses, videos, podcasts, articles, or workshops created by content creators
    //Re-use : Validation attributes for data annotations
    //Properties : Includes content details, pricing, media files, and publication status
    //Output Type : Model class for content management throughout the application
    //
    public class Content
    {
        //
        //Property Name : Id
        //Purpose : Unique identifier for each content item (primary key)
        //Re-use : Key attribute to mark as primary key in Entity Framework
        //Data Type : int - Auto-incrementing integer ID
        //
        [Key]
        public int Id { get; set; }

        //
        //Property Name : Title
        //Purpose : The title or name of the content displayed to users
        //Re-use : Required attribute for validation
        //Data Type : string - Non-nullable string, defaults to empty string
        //
        [Required]
        public string Title { get; set; } = string.Empty;

        //
        //Property Name : Description
        //Purpose : Detailed description of the content explaining what users will learn or experience
        //Re-use : Required attribute for validation
        //Data Type : string - Non-nullable string, defaults to empty string
        //
        [Required]
        public string Description { get; set; } = string.Empty;

        //
        //Property Name : Category
        //Purpose : Classification of content type (e.g., Video, Podcast, Article, Workshop, Course)
        //Re-use : Required attribute for validation
        //Data Type : string - Non-nullable string, defaults to empty string
        //
        [Required]
        public string Category { get; set; } = string.Empty;

        //
        //Property Name : Price
        //Purpose : The cost of the content in dollars (0 for free content)
        //Re-use : None
        //Data Type : decimal - Decimal value for monetary amount
        //
        public decimal Price { get; set; }

        //
        //Property Name : ContentUrl
        //Purpose : Path or URL to the actual content file (video file, audio file, PDF document, etc.)
        //Re-use : None
        //Data Type : string? - Nullable string containing file path or URL
        //
        public string? ContentUrl { get; set; }

        //
        //Property Name : ThumbnailUrl
        //Purpose : Path or URL to the thumbnail/preview image displayed in content listings
        //Re-use : None
        //Data Type : string? - Nullable string containing image path or URL
        //
        public string? ThumbnailUrl { get; set; }

        //
        //Property Name : CreatedDate
        //Purpose : Records the date and time when the content was created
        //Re-use : None
        //Data Type : DateTime - Date and time of content creation
        //
        public DateTime CreatedDate { get; set; }

        //
        //Property Name : CreatedByUserId
        //Purpose : Foreign key reference to the ApplicationUser who created this content
        //Re-use : None
        //Data Type : string? - Nullable string containing user ID from Identity system
        //
        public string? CreatedByUserId { get; set; }

        //
        //Property Name : IsPublished
        //Purpose : Indicates whether the content is published and visible to learners (false means draft/saved but not public)
        //Re-use : None
        //Data Type : bool - Boolean flag (true = published, false = draft)
        //
        public bool IsPublished { get; set; }

        //
        //Property Name : Duration
        //Purpose : Length of the content in minutes (for videos/podcasts/workshops) or estimated reading time for articles
        //Re-use : None
        //Data Type : int - Integer value representing duration in minutes
        //
        public int Duration { get; set; }
    }
}