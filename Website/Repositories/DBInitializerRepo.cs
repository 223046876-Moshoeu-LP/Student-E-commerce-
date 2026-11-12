using Website.Data;
using Website.Interface;
using Website.Models;

public class DBInitializerRepo : IDBInitializer
{
    private readonly SQLiteDBContext _context;

    public DBInitializerRepo(SQLiteDBContext context)
    {
        _context = context;
    }

    public void Initialize()
    {
        if (_context.Students != null && !_context.Students.Any())
        {
            var students = new List<Student>
                {
                    new Student
                    {
                        StudentNumber = "2023000010",
                        FirstName = "Mickey",
                        Surname = "Mouse",
                        EnrollmentDate = DateTime.Parse("2024/01/04"),
                        Photo = "default.png",
                        Email = "mouse@gmail.com"
                    },
                    new Student
                    {
                        StudentNumber = "2023000011",
                        FirstName = "Minnie",
                        Surname = "Mouse",
                        EnrollmentDate = DateTime.Parse("2024/01/05"),
                        Photo = "default.png",
                        Email = "minnie@gmail.com"
                    }
                };

            _context.Students.AddRange(students);
            _context.SaveChanges();
        }
    }
}