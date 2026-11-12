using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website.Models
{
    //
    //Class Name : Order
    //Purpose : Represents a customer purchase order in the e-commerce system
    //Re-use : Key, Display, DataType attributes for Entity Framework and UI formatting
    //Properties : Includes order details such as number, date, amount, status, and related order items
    //Output Type : Model class for order management and transaction tracking
    //
    public class Order
    {
        //
        //Property Name : Id
        //Purpose : Unique identifier for each order (primary key)
        //Re-use : Key attribute to mark as primary key in Entity Framework
        //Data Type : int - Auto-incrementing integer ID
        //
        [Key]
        public int Id { get; set; }

        //
        //Property Name : OrderNumber
        //Purpose : Unique human-readable order number for customer reference and tracking
        //Re-use : Display attribute for form labeling
        //Data Type : string - Non-nullable string containing formatted order number (e.g., ORD20231201123456)
        //
        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; } = string.Empty;

        //
        //Property Name : OrderDate
        //Purpose : Records the date and time when the order was placed by the customer
        //Re-use : Display attribute for form labeling
        //Data Type : DateTime - Timestamp of order creation
        //
        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; }

        //
        //Property Name : TotalAmount
        //Purpose : Total monetary amount of the order after any discounts or promotions
        //Re-use : Display attribute for form labeling, DataType attribute for currency formatting
        //Data Type : decimal - Decimal value representing the final order total
        //
        [Display(Name = "Total Amount")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        //
        //Property Name : Status
        //Purpose : Current status of the order (e.g., Pending, Completed, Cancelled, Shipped)
        //Re-use : Display attribute for form labeling
        //Data Type : string - Non-nullable string with default value "Pending"
        //
        [Display(Name = "Order Status")]
        public string Status { get; set; } = "Pending";

        //
        //Property Name : UserId
        //Purpose : Foreign key reference to the ApplicationUser who placed this order
        //Re-use : Display attribute for form labeling
        //Data Type : string - Non-nullable string containing user ID from Identity system
        //
        [Display(Name = "User ID")]
        public string UserId { get; set; } = string.Empty;

        //
        //Property Name : OrderItems
        //Purpose : Navigation property for the collection of items included in this order
        //Re-use : Virtual keyword for Entity Framework lazy loading
        //Data Type : ICollection<OrderItem> - Collection of OrderItem entities
        //
        // Order items navigation property
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}