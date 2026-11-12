namespace Website.Interface
{
    //
    //Interface Name : IDBInitializer
    //Purpose : Defines the contract for database initialization and seeding operations
    //Re-use : None
    //Methods : Includes method for setting up initial database state with default data
    //Output Type : Interface for implementing database initialization pattern
    //
    public interface IDBInitializer
    {
        //
        //Method Name : Initialize
        //Purpose : Performs database initialization tasks such as creating tables, seeding default data,
        //          creating default roles (Admin, Content_Creator, User), and setting up admin accounts
        //Re-use : None
        //Input Parameter : None
        //Output Type : void - No return value
        //
        void Initialize();
    }
}