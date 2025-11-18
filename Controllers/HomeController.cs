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

        //Show details of a single student
        public IActionResult Details(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.StudentID == id.ToString());
            if (student == null) return NotFound();
            return View(student);
        }

        //Edit (GET) - Load student details in a form
        public IActionResult Edit(int id)
        {
            var student = _context.Students.FirstOrDefault( s => s.StudentID == id.ToString());
            if (student == null) return NotFound();
            return View(student);
        }

        //Edit (POST) - Save changes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Student student)
        {
            if (!ModelState.IsValid)
            {
                //log all model errors
                foreach (var kv in ModelState)
                {
                    foreach (var error in kv.Value.Errors)
                    {
                        Console.WriteLine($"Error in {kv.Key}: {error.ErrorMessage}");
                    }
                }
                return View(student);
            }

            Console.WriteLine($"Editing student ID: {student.StudentID}");

            if(string.IsNullOrWhiteSpace(student.StudentID))
            {
                ModelState.AddModelError("", "Student ID is required.");
                return View(student);
            }

                var existingStudent = _context.Students.FirstOrDefault(s => s.StudentID.Trim() == student.StudentID.Trim());
                if (existingStudent == null)
                {
                    return NotFound();
                }

                 // Update all fields
                existingStudent.FirstName = student.FirstName;
                existingStudent.LastName = student.LastName;
                existingStudent.Email = student.Email;
                existingStudent.Phone = student.Phone;
                existingStudent.Program = student.Program;
                existingStudent.EnrollmentType = student.EnrollmentType;
                existingStudent.ExpectedGraduationYear = student.ExpectedGraduationYear;
                existingStudent.DateOfBirth = student.DateOfBirth;
                existingStudent.Gender = student.Gender;
                existingStudent.Nationality = student.Nationality;
                existingStudent.StreetAddress = student.StreetAddress;
                existingStudent.City = student.City;
                existingStudent.Province = student.Province;
                existingStudent.ZipCode = student.ZipCode;
                existingStudent.Country = student.Country;
                existingStudent.AdditionalInfo = student.AdditionalInfo;

                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                     // Log exception and show error
                    Console.WriteLine("Error updating student: " + ex.Message);
                    ModelState.AddModelError("", "Unable to save changes. Try again.");
                    return View(student);
                }

                return RedirectToAction("Details", new { id = existingStudent.StudentID });
        }



        //Delete (GET) - show confirmation
        public IActionResult Delete(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.StudentID == id.ToString());
            if (student == null) return NotFound();
            return View(student);
        }

        //Delete (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.StudentID == id.ToString());
            if (student == null ) return NotFound();

            _context.Students.Remove(student);
            _context.SaveChanges();
            return RedirectToAction("Students");
        }
    }
}
