using Website.Models;

namespace Website.Interface
{
    //
    //Interface Name : IStudent
    //Purpose : Defines the contract for student data access and management operations
    //Re-use : Student model for data operations
    //Methods : Includes CRUD operations and lookup methods for student records
    //Output Type : Interface for implementing student repository pattern
    //
    public interface IStudent
    {
        //
        //Method Name : Details
        //Purpose : Retrieves a specific student by their unique identifier
        //Re-use : Student model
        //Input Parameter : string id - The unique student number/identifier to search for
        //Output Type : Student? - Returns the Student object if found, null otherwise
        //
        Student? Details(string id);

        //
        //Method Name : ByEmail
        //Purpose : Retrieves a student record using their email address
        //Re-use : Student model
        //Input Parameter : string id - The email address of the student to search for
        //Output Type : Student? - Returns the Student object if found, null otherwise
        //
        Student? ByEmail(string id);

        //
        //Method Name : Create
        //Purpose : Adds a new student record to the data store
        //Re-use : Student model
        //Input Parameter : Student student - The Student object containing the data to create
        //Output Type : void - No return value
        //
        void Create(Student student);

        //
        //Method Name : Edit
        //Purpose : Updates an existing student record with modified data
        //Re-use : Student model
        //Input Parameter : Student student - The Student object containing the updated data
        //Output Type : void - No return value
        //
        void Edit(Student student);

        //
        //Method Name : Delete
        //Purpose : Removes a student record from the data store using their identifier
        //Re-use : None
        //Input Parameter : string id - The unique student number/identifier of the student to delete
        //Output Type : void - No return value
        //
        void Delete(string id);

        //
        //Method Name : GetAll
        //Purpose : Retrieves all student records from the data store
        //Re-use : Student model
        //Input Parameter : None
        //Output Type : IEnumerable<Student> - Collection of all Student objects
        //
        IEnumerable<Student> GetAll();
    }
}