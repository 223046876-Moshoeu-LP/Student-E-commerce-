using System.ComponentModel.DataAnnotations;

namespace Website.Models
{
    //
    //Class Name : Promotion
    //Purpose : Represents promotional offers and discount campaigns for content on the platform
    //Re-use : Key, Required, DataType attributes for Entity Framework and validation
    //Properties : Includes promotion details, dates, discount information, and activation status
    //Output Type : Model class for managing marketing campaigns and discount codes
    //
    public class Promotion
    {
        //
        //Property Name : Id
        //Purpose : Unique identifier for each promotion (primary key)
        //Re-use : Key attribute to mark as primary key in Entity Framework
        //Data Type : int - Auto-incrementing integer ID
        //
        [Key]
        public int Id { get; set; }

        //
        //Property Name : Title
        //Purpose : The name or title of the promotion (also used as promo code)
        //Re-use : Required attribute for validation
        //Data Type : string - Non-nullable string, defaults to empty string
        //
        [Required]
        public string Title { get; set; } = string.Empty;

        //
        //Property Name : Description
        //Purpose : Detailed description of the promotion explaining the offer details
        //Re-use : Required attribute for validation
        //Data Type : string - Non-nullable string, defaults to empty string
        //
        [Required]
        public string Description { get; set; } = string.Empty;

        //
        //Property Name : ImageUrl
        //Purpose : URL or path to the promotional banner image displayed on the store
        //Re-use : None
        //Data Type : string? - Nullable string containing image path or URL
        //
        public string? ImageUrl { get; set; }

        //
        //Property Name : StartDate
        //Purpose : The date and time when the promotion becomes active
        //Re-use : DataType attribute for DateTime formatting in views
        //Data Type : DateTime - Timestamp marking promotion start
        //
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        //
        //Property Name : EndDate
        //Purpose : The date and time when the promotion expires and becomes inactive
        //Re-use : DataType attribute for DateTime formatting in views
        //Data Type : DateTime - Timestamp marking promotion expiration
        //
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        //
        //Property Name : DiscountPercentage
        //Purpose : The percentage discount offered by this promotion (e.g., 15 for 15% off)
        //Re-use : None
        //Data Type : decimal? - Nullable decimal representing discount percentage
        //
        public decimal? DiscountPercentage { get; set; }

        //
        //Property Name : ContentId
        //Purpose : Optional foreign key to restrict promotion to a specific content item (null for store-wide promotions)
        //Re-use : None
        //Data Type : int? - Nullable integer ID of associated content
        //
        public int? ContentId { get; set; }

        //
        //Property Name : IsActive
        //Purpose : Flag indicating whether the promotion is currently active and can be applied
        //Re-use : None
        //Data Type : bool - Boolean flag, defaults to true for active promotions
        //
        public bool IsActive { get; set; } = true;

        //
        //Property Name : CreatedAt
        //Purpose : Records the date and time when the promotion was created in the system
        //Re-use : DataType attribute for DateTime formatting in views
        //Data Type : DateTime - Timestamp of promotion creation, defaults to current time
        //
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}