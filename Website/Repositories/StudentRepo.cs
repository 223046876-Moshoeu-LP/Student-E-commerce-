using Website.Data;
using Website.Interface;
using Website.Models;

namespace Website.Repositories
{
    public class StudentRepo: IStudent
    {
        private readonly SQLiteDBContext _context;

        public StudentRepo(SQLiteDBContext context)
        {
            _context = context;
        }

        public Student? Details(string id)
        {
            return _context.Students?.FirstOrDefault(x => x.StudentNumber == id);
        }

        public Student? ByEmail(string id)
        {
            return _context.Students?.FirstOrDefault(x => x.Email == id);
        }

        public void Create(Student student)
        {
            _context.Students?.Add(student);
            _context.SaveChanges();
        }

        public void Edit(Student student)
        {
            _context.Students?.Update(student);
            _context.SaveChanges();
        }

        public void Delete(string id)
        {
            var student = Details(id);
            if (student != null)
            {
                _context.Students?.Remove(student);
                _context.SaveChanges();
            }
        }

        public IEnumerable<Student> GetAll()
        {
            return _context.Students?.ToList() ?? new List<Student>();
        }
    }
}
