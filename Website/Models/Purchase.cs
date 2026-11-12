using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website.Models
{
    //
    //Class Name : Purchase
    //Purpose : Records completed purchases made by users, serving as a digital library of owned content
    //Re-use : Key, Required, Display, DataType, ForeignKey attributes for Entity Framework and UI
    //Properties : Includes purchase details, content information, and order reference
    //Output Type : Model class for tracking user content ownership and access rights
    //
    public class Purchase
    {
        //
        //Property Name : Id
        //Purpose : Unique identifier for each purchase record (primary key)
        //Re-use : Key attribute to mark as primary key in Entity Framework
        //Data Type : int - Auto-incrementing integer ID
        //
        [Key]
        public int Id { get; set; }

        //
        //Property Name : UserId
        //Purpose : Foreign key reference to the ApplicationUser who made the purchase
        //Re-use : Required attribute for validation, Display attribute for form labeling
        //Data Type : string - Non-nullable string containing user ID from Identity system
        //
        [Required]
        [Display(Name = "User ID")]
        public string UserId { get; set; } = string.Empty;

        //
        //Property Name : ContentId
        //Purpose : Foreign key reference to the Content item that was purchased
        //Re-use : Required attribute for validation, Display attribute for form labeling
        //Data Type : int - Non-nullable integer ID of purchased content
        //
        [Required]
        [Display(Name = "Content ID")]
        public int ContentId { get; set; }

        //
        //Property Name : ContentTitle
        //Purpose : Denormalized field storing the title of the purchased content (preserves data even if content is later modified)
        //Re-use : Required attribute for validation, Display attribute for form labeling
        //Data Type : string - Non-nullable string containing the content title
        //
        [Required]
        [Display(Name = "Content Title")]
        public string ContentTitle { get; set; } = string.Empty;

        //
        //Property Name : ContentCategory
        //Purpose : Denormalized field storing the category of purchased content (e.g., Video, Podcast, Article, Workshop)
        //Re-use : Display attribute for form labeling
        //Data Type : string - Non-nullable string, defaults to empty string
        //
        [Display(Name = "Content Category")]
        public string ContentCategory { get; set; } = string.Empty;

        //
        //Property Name : Price
        //Purpose : The price paid for the content at the time of purchase (preserves historical pricing)
        //Re-use : Display attribute for form labeling, DataType attribute for currency formatting
        //Data Type : decimal - Decimal value representing the purchase price
        //
        [Display(Name = "Price Paid")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        //
        //Property Name : ContentUrl
        //Purpose : URL or path to the actual content file (video, audio, PDF, etc.) at time of purchase
        //Re-use : Display attribute for form labeling
        //Data Type : string? - Nullable string containing file path or URL
        //
        [Display(Name = "Content URL")]
        public string? ContentUrl { get; set; }

        //
        //Property Name : ThumbnailUrl
        //Purpose : URL or path to the thumbnail/preview image of the purchased content
        //Re-use : Display attribute for form labeling
        //Data Type : string? - Nullable string containing image path or URL
        //
        [Display(Name = "Thumbnail")]
        public string? ThumbnailUrl { get; set; }

        //
        //Property Name : PurchaseDate
        //Purpose : Records the date and time when the purchase was completed
        //Re-use : Display attribute for form labeling
        //Data Type : DateTime - Timestamp of purchase transaction
        //
        [Display(Name = "Purchase Date")]
        public DateTime PurchaseDate { get; set; }

        //
        //Property Name : OrderNumber
        //Purpose : Reference to the original order number from the Order entity for tracking purposes
        //Re-use : Display attribute for form labeling
        //Data Type : string - Non-nullable string containing order number (e.g., ORD20231201123456)
        //
        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; } = string.Empty;
    }
}