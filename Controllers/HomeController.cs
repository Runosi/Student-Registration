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
        private readonly EmailService _emailService;

        public HomeController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // Home page
        public IActionResult Index()
        {
            return View();
        }

       public IActionResult Students(string searchString, string sortOrder)
        {
            // Start with all students
            var students = from s in _context.Students
                   select s;

            // Filter by search string
            if (!string.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.FirstName.ToLower().Contains(searchString.ToLower())
                            || s.LastName.ToLower().Contains(searchString.ToLower()));
            }

            // Sorting
            switch (sortOrder)
            {
                case "date_desc":
                    students = students.OrderByDescending(s => s.RegistrationDate);
                break;
                case "firstname_asc":
                    students = students.OrderBy(s => s.FirstName);
                break;
                case "firstname_desc":
                    students = students.OrderByDescending(s => s.FirstName);
                break;
                default:
                    students = students.OrderBy(s => s.RegistrationDate); // default ascending
                break;
            }

            // Pass current filter and sort order to the view
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;

            return View(students.ToList());
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

                // Get date of when submitted
                student.RegistrationDate = DateTime.Now;

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

                //Generate the verification link
                var verificationLink=Url.Action("VerifyEmail", "Home", new { email = student.Email, token = student.VerificationToken }, Request.Scheme);

                // Email content
                string subject = "Welcome to TRU";
                string body = $@"
                    <h3>Congratulations You're Registered!</h3>
                    <p>Click the link to confirm you're email:</p>
                    <a href='{verificationLink}'> Verify Email</a>";

                Console.WriteLine("Student Email: " + student.Email);
                _emailService.SendEmail(student.Email, subject, body);

                Console.WriteLine("POST received");

                return RedirectToAction("Submit", new { id = student.StudentIdInternal });
            }
            return View(student);
        }

        [HttpGet]
        public IActionResult Submit(int id)
        {
            var student = _context.Students.Find(id);
            return View(student);
        }

        //Email Verification
        public IActionResult VerifyEmail(string email, string token)
        {
            var student = _context.Students.FirstOrDefault(s => s.Email == email && s.VerificationToken == token);

            if (student != null)
            {
                student.IsVerified = 1;
                _context.SaveChanges();
                return Content($"Email ({email}) verified successfully!");
            }
            return Content("Invalid verification link.");
        }

        //Show details of a single student
        public IActionResult Details(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.StudentIdInternal == id);
            if (student == null)
            {
                return NotFound();
            }

            // Check if a student is logged in via cookie
            //var loggedInStudentId = Request.Cookies["StudentIdInternal"];
            //bool isStudentLoggedIn = loggedInStudentId != null;

            //ViewData["IsStudent"] = isStudentLoggedIn;
            //ViewData["LoggedInStudentId"] = loggedInStudentId;


            return View(student);
        }

        //Edit (GET) - Load student details in a form
        public IActionResult Edit(int id)
        {
            var student = _context.Students.FirstOrDefault( s => s.StudentIdInternal == id);
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

            // Only fetch this student's record
            var existingStudent = _context.Students.FirstOrDefault(s => s.StudentIdInternal == student.StudentIdInternal);
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

            return RedirectToAction("Details", new { id = existingStudent.StudentIdInternal });
        }


        //Delete (GET) - show confirmation
        public IActionResult Delete(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.StudentIdInternal == id);
            if (student == null) return NotFound();
            return View(student);
        }

        //Delete (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.StudentIdInternal == id);
            if (student == null ) return NotFound();

            _context.Students.Remove(student);
            _context.SaveChanges();
            return RedirectToAction("Students");
        }

        // GET: Admin login page
        public IActionResult AdminLogin()
        {
            return View();
        }

        // POST: Validate admin password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdminLogin(string password)
        {
            const string adminPassword = "Password123";

            if (password == adminPassword)
            {
                // Password correct, redirect to Students page
                return RedirectToAction("Students");
            }
            else
            {
                // Password wrong, show error
                ViewData["ErrorMessage"] = "Incorrect password!";
                return View();
            }
        }


        // GET: show login form
        public IActionResult StudentLogin()
        {
            return View();
        }

        // POST: process login
        [HttpPost]
        public IActionResult StudentLogin(string email, string password)
        {
            var student = _context.Students
                        .FirstOrDefault(s => s.Email.ToLower() == email.ToLower() && s.Password == password);

            if (student != null)
            {
                // Create a cookie to remember the logged-in student
                CookieOptions option = new CookieOptions();
                option.HttpOnly = true;
                option.Expires = DateTime.Now.AddHours(1); // cookie valid for 1 hour

                Response.Cookies.Append("StudentIdInternal", student.StudentIdInternal.ToString(), option);

                return RedirectToAction("StudentDetails", new { id = student.StudentIdInternal });
            }

            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        // Logout
        public IActionResult Logout()
        {
            Response.Cookies.Delete("StudentIdInternal");
            return RedirectToAction("StudentLogin");
        }

        public IActionResult StudentDetails(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.StudentIdInternal == id);
            if (student == null) return NotFound();
            return View(student);
        }

        [HttpGet]
        public IActionResult StudentEdit(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.StudentIdInternal == id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        //POST: For when a student is logged in
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StudentEdit(Student student)
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

            // Only fetch this student's record
            var existingStudent = _context.Students.FirstOrDefault(s => s.StudentIdInternal == student.StudentIdInternal);
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

            return RedirectToAction("StudentDetails", new { id = existingStudent.StudentIdInternal });
        }

    }
}