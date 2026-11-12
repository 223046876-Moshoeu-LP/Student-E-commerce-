using System.ComponentModel.DataAnnotations;

namespace Website.Models
{
    //
    //Class Name : Cart
    //Purpose : Represents an item in a user's shopping cart before checkout
    //          Stores denormalized content data to preserve pricing and titles
    //          even if the original content is later modified or deleted
    //Re-use : None - Standalone model class
    //Input Parameter : None
    //Output Type : None
    //
    public class Cart
    {
        //
        //Property Name : Id
        //Purpose : Primary key identifier for the cart item record
        //Re-use : Key attribute - Marks as primary key in database
        //Input Parameter : None
        //Output Type : int - Unique identifier for the cart entry
        //
        [Key]
        public int Id { get; set; }
        
        //
        //Property Name : UserId
        //Purpose : Identifier for the user who owns the cart item
        //Re-use : None
        //Input Parameter : None
        //Output Type : string - User ID
        //
        public string UserId { get; set; } = string.Empty;
        
        //
        //Property Name : ContentId
        //Purpose : Identifier for the content item in the cart
        //Re-use : None
        //Input Parameter : None
        //Output Type : int - Content ID
        //
        public int ContentId { get; set; }
        
        //
        //Property Name : Quantity
        //Purpose : Quantity of the content item in the cart
        //Re-use : None
        //Input Parameter : None
        //Output Type : int - Quantity
        //
        public int Quantity { get; set; } = 1;
        
        //
        //Property Name : AddedDate
        //Purpose : Date and time when the item was added to the cart
        //Re-use : None
        //Input Parameter : None
        //Output Type : DateTime - Added date and time
        //
        public DateTime AddedDate { get; set; } = DateTime.Now;
        // ==================== DENORMALIZED CONTENT FIELDS ====================
        // These fields store content data directly to preserve pricing and titles
        // even if the original content is later modified or deleted

        //
        //Property Name : ContentTitle
        //Purpose : Denormalized copy of the content title at time of adding to cart
        //Re-use : Displayed in cart and checkout summary pages
        //Input Parameter : None
        //Output Type : string - Title text, defaults to empty string
        //
        // Store content info directly (no foreign keys)
        public string ContentTitle { get; set; } = string.Empty;
        //
        //Property Name : ContentPrice
        //Purpose : Denormalized copy of the content price at time of adding to cart
        //Re-use : Used for price calculations; prevents price changes from affecting existing carts
        //Input Parameter : None
        //Output Type : decimal - Price value at cart addition time
        //
        public decimal ContentPrice { get; set; }

        //
        //Property Name : ContentThumbnailUrl
        //Purpose : Denormalized copy of the content thumbnail URL
        //Re-use : Displays thumbnail image in cart and checkout pages
        //Input Parameter : None
        //Output Type : string? - Nullable string containing image path
        //
        public string? ContentThumbnailUrl { get; set; }
    }
}