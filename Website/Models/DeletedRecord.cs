using System.ComponentModel.DataAnnotations;

namespace Website.Models
{
    //
    //Class Name : DeletedRecord
    //Purpose : Tracks soft-deleted user accounts for audit and recovery purposes
    //Re-use : Key attribute for Entity Framework primary key mapping
    //Properties : Stores information about deleted users and who performed the deletion
    //Output Type : Audit model for deleted user records
    //
    public class DeletedRecord
    {
        //
        //Property Name : Id
        //Purpose : Unique identifier for each deleted record (primary key)
        //Re-use : Key attribute to mark as primary key in Entity Framework
        //Data Type : int - Auto-incrementing integer ID
        //
        [Key]
        public int Id { get; set; }

        //
        //Property Name : UserId
        //Purpose : Stores the unique identifier of the deleted user from Identity system
        //Re-use : None
        //Data Type : string? - Nullable string containing the deleted user's ID
        //
        public string? UserId { get; set; }

        //
        //Property Name : UserEmail
        //Purpose : Stores the email address of the deleted user for identification and audit purposes
        //Re-use : None
        //Data Type : string? - Nullable string containing the deleted user's email
        //
        public string? UserEmail { get; set; }

        //
        //Property Name : UserRole
        //Purpose : Stores the role (Admin, Content_Creator, User) of the deleted user at time of deletion
        //Re-use : None
        //Data Type : string? - Nullable string containing the role name
        //
        public string? UserRole { get; set; }

        //
        //Property Name : DeletedByUserId
        //Purpose : Stores the unique identifier of the admin who performed the deletion
        //Re-use : None
        //Data Type : string? - Nullable string containing the deleting admin's ID
        //
        public string? DeletedByUserId { get; set; }

        //
        //Property Name : DeletedByEmail
        //Purpose : Stores the email address of the admin who performed the deletion for audit trail
        //Re-use : None
        //Data Type : string? - Nullable string containing the deleting admin's email
        //
        public string? DeletedByEmail { get; set; }

        //
        //Property Name : DeletedByRole
        //Purpose : Stores the role of the person who performed the deletion (typically "Admin")
        //Re-use : None
        //Data Type : string? - Nullable string containing the deleter's role
        //
        public string? DeletedByRole { get; set; }

        //
        //Property Name : DeletedAt
        //Purpose : Records the date and time when the user account was deleted
        //Re-use : None
        //Data Type : DateTime - Timestamp of deletion action
        //
        public DateTime DeletedAt { get; set; }

        //
        //Property Name : Reason
        //Purpose : Stores the reason provided by the admin for deleting the user account
        //Re-use : None
        //Data Type : string? - Nullable string containing deletion justification
        //
        public string? Reason { get; set; }
    }
}