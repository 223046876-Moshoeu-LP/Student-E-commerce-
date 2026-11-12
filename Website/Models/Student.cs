using System.ComponentModel.DataAnnotations;

namespace Website.Models
{
    //
    //Class Name : Student
    //Purpose : Represents a student entity for educational or administrative management purposes
    //Re-use : Key attribute for Entity Framework primary key mapping
    //Properties : Includes student identification, personal details, enrollment information, and contact data
    //Output Type : Model class for student records management
    //
    public class Student
    {
        //
        //Property Name : StudentNumber
        //Purpose : Unique identifier for each student (primary key) - typically a university ID or registration number
        //Re-use : Key attribute to mark as primary key in Entity Framework
        //Data Type : string? - Nullable string containing the student's unique identification number
        //
        [Key]
        public string? StudentNumber { get; set; }

        //
        //Property Name : FirstName
        //Purpose : Stores the student's first name for identification and communication
        //Re-use : None
        //Data Type : string? - Nullable string containing the student's first name
        //
        public string? FirstName { get; set; }

        //
        //Property Name : Surname
        //Purpose : Stores the student's surname/last name for identification and record keeping
        //Re-use : None
        //Data Type : string? - Nullable string containing the student's surname
        //
        public string? Surname { get; set; }

        //
        //Property Name : EnrollmentDate
        //Purpose : Records the date when the student enrolled in the program or institution
        //Re-use : None
        //Data Type : DateTime - Timestamp of student enrollment
        //
        public DateTime EnrollmentDate { get; set; }

        //
        //Property Name : Photo
        //Purpose : URL or path to the student's profile photo for identification purposes
        //Re-use : None
        //Data Type : string? - Nullable string containing image path or URL
        //
        public string? Photo { get; set; }

        //
        //Property Name : Email
        //Purpose : Stores the student's email address for communication and account management
        //Re-use : None
        //Data Type : string? - Nullable string containing the student's email address
        //
        public string? Email { get; set; }
    }
}