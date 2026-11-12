using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Website.Models
{
    //
    //Class Name : ApplicationUser
    //Purpose : Represents the application user entity extending IdentityUser with additional profile properties
    //Re-use : IdentityUser - Base class from ASP.NET Core Identity
    //Properties : Includes user personal details, social media links, profile image, and account status
    //Output Type : Model class for user management throughout the application
    //
    public class ApplicationUser : IdentityUser
    {
        //
        //Property Name : FirstName
        //Purpose : Stores the user's first name for personalization and display
        //Re-use : Display attribute for form labeling
        //Data Type : string? - Nullable string
        //
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        //
        //Property Name : LastName
        //Purpose : Stores the user's last name for personalization and display
        //Re-use : Display attribute for form labeling
        //Data Type : string? - Nullable string
        //
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        //
        //Property Name : ProfileImageUrl
        //Purpose : Stores the URL/path to the user's profile image for avatar display
        //Re-use : Display attribute for form labeling
        //Data Type : string? - Nullable string containing image path
        //
        [Display(Name = "Profile Image")]
        public string? ProfileImageUrl { get; set; }

        //
        //Property Name : Bio
        //Purpose : Stores a brief biography or description about the user
        //Re-use : Display attribute for form labeling
        //Data Type : string? - Nullable string for user's bio text
        //
        [Display(Name = "Bio")]
        public string? Bio { get; set; }

        //
        //Property Name : Expertise
        //Purpose : Stores the user's areas of expertise or professional skills
        //Re-use : Display attribute for form labeling
        //Data Type : string? - Nullable string listing expertise areas
        //
        [Display(Name = "Expertise")]
        public string? Expertise { get; set; }

        //
        //Property Name : Website
        //Purpose : Stores the user's personal or professional website URL
        //Re-use : Display attribute for form labeling
        //Data Type : string? - Nullable string containing website URL
        //
        [Display(Name = "Website")]
        public string? Website { get; set; }

        //
        //Property Name : Twitter
        //Purpose : Stores the user's Twitter profile handle or URL
        //Re-use : Display attribute for form labeling
        //Data Type : string? - Nullable string for Twitter profile link
        //
        [Display(Name = "Twitter")]
        public string? Twitter { get; set; }

        //
        //Property Name : LinkedIn
        //Purpose : Stores the user's LinkedIn profile URL
        //Re-use : Display attribute for form labeling
        //Data Type : string? - Nullable string for LinkedIn profile link
        //
        [Display(Name = "LinkedIn")]
        public string? LinkedIn { get; set; }

        //
        //Property Name : CreatedAt
        //Purpose : Records the date and time when the user account was created
        //Re-use : None
        //Data Type : DateTime? - Nullable DateTime, defaults to current time
        //
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        //
        //Property Name : IsDeleted
        //Purpose : Soft delete flag to mark user accounts as deleted without removing from database
        //Re-use : None
        //Data Type : bool - Boolean flag, defaults to false for active accounts
        //
        public bool IsDeleted { get; set; } = false;
    }
}