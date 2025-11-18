using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProject.Data;
using WebProject.Models;
using System.Linq;

namespace WebProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Home page
        public IActionResult Index()
        {
            return View();
        }

        // Admin page - list all students
        public IActionResult Students()
        {
            var students = _context.Students.ToList();
            return View(students);
        }

        // Registration form
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(Student student)
        {
            if (ModelState.IsValid)
            {
                // Get next sequence value using ExecuteScalar via Database
                int nextVal;
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "SELECT STUDENT38.ISEQ$$_1324860.NEXTVAL FROM DUAL";
                    _context.Database.OpenConnection();
                    nextVal = Convert.ToInt32(command.ExecuteScalar());
                    _context.Database.CloseConnection();
                }

                student.StudentIdInternal = nextVal;

                // Generate verification token if missing
                student.VerificationToken = Guid.NewGuid().ToString();

                // Mark as not verified yet
                student.IsVerified = 0;

                _context.Students.Add(student);
                _context.SaveChanges();

                Console.WriteLine("VALID? " + ModelState.IsValid);
                foreach (var kv in ModelState)
                {
                    foreach (var error in kv.Value.Errors)
                    {
                        Console.WriteLine($"Error in {kv.Key}: {error.ErrorMessage}");
                    }
                }

                // Optionally: send confirmation email here

                Console.WriteLine("POST received");

                return RedirectToAction("Students");
            }
            return View(student);
        }
    }
}
