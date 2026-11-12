using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website.Models
{
    //
    //Class Name : OrderItem
    //Purpose : Represents an individual line item within an order, linking orders to purchased content
    //Re-use : Key, ForeignKey attributes for Entity Framework relationships
    //Properties : Includes quantity, price, and navigation properties to Order and Content
    //Output Type : Model class for order line items and transaction details
    //
    public class OrderItem
    {
        //
        //Property Name : Id
        //Purpose : Unique identifier for each order item (primary key)
        //Re-use : Key attribute to mark as primary key in Entity Framework
        //Data Type : int - Auto-incrementing integer ID
        //
        [Key]
        public int Id { get; set; }

        //
        //Property Name : OrderId
        //Purpose : Foreign key referencing the parent Order that contains this item
        //Re-use : ForeignKey attribute links to Order navigation property
        //Data Type : int - ID of the associated Order entity
        //
        public int OrderId { get; set; }

        //
        //Property Name : ContentId
        //Purpose : Foreign key referencing the Content item that was purchased
        //Re-use : ForeignKey attribute links to Content navigation property
        //Data Type : int - ID of the associated Content entity
        //
        public int ContentId { get; set; }

        //
        //Property Name : ContentTitle
        //Purpose : Denormalized field storing the title of the content at time of purchase (preserves data even if content is later modified)
        //Re-use : None
        //Data Type : string - Non-nullable string containing the content title
        //
        public string ContentTitle { get; set; } = string.Empty;

        //
        //Property Name : Price
        //Purpose : Price of the content item at the time of purchase (preserves historical pricing data)
        //Re-use : None
        //Data Type : decimal - Decimal value representing the unit price
        //
        public decimal Price { get; set; }

        //
        //Property Name : Quantity
        //Purpose : Number of units of this content item purchased (typically 1 for digital content)
        //Re-use : None
        //Data Type : int - Integer representing purchase quantity
        //
        public int Quantity { get; set; }

        //
        //Property Name : Order
        //Purpose : Navigation property to access the parent Order entity
        //Re-use : ForeignKey attribute specifies which property is the foreign key, Virtual keyword for lazy loading
        //Data Type : Order? - Nullable reference to Order entity
        //
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        //
        //Property Name : Content
        //Purpose : Navigation property to access the associated Content entity
        //Re-use : ForeignKey attribute specifies which property is the foreign key, Virtual keyword for lazy loading
        //Data Type : Content? - Nullable reference to Content entity
        //
        [ForeignKey("ContentId")]
        public virtual Content? Content { get; set; }
    }
}