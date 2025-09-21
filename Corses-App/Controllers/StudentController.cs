using Corses_App.Repostory;
using Courses_App.Core.DTO;
using Courses_App.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace Corses_App.Controllers
{
    [Authorize(Roles = "Admin")]

    public class StudentController : Controller
    {
        private readonly IStudentRepostory _repostory;
        private readonly UserManager<User> _userManager;

        public StudentController(IStudentRepostory repostory , UserManager<User> userManager)
        {
            _repostory = repostory;
            _userManager=userManager;
        }
        // GET: StudentController
        public async Task<IActionResult> Index()
        {
            var users = await _repostory.GetAllAsync();
            return View(users);
        }
        //Get: StudentController / GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var students = await _repostory.GetAll();
            if (students == null || students.Count == 0)
                return Json(new { success = false, message = "No studenta found" });
            return Json(new { success = true,data=students, message = "Load students Successfully" });
        }
        //Get: StudentController/GetCourses/UserId
        [HttpGet]
        public async Task<ActionResult>GetCourses(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
                return BadRequest("Student ID Is Null Or Empty");
            var courses = await _repostory.GetCoursesByStudentId(studentId);
            return Ok(courses);
        }

        // GET: StudentController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: StudentController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: StudentController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync(UserDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { Success = false, message = "Model is not valid" });
            }

            if (model.Password != model.ConfirmPassword)
            {
                return Json(new { Success = false, message = "Passwords do not match" });
            }

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                // أي خصائص إضافية حسب ما هو معرف في كلاس User
            };

            var student = await _repostory.AddAsync(user, model.Password);

            if (student != null)
            {
                return Json(new { Success = true, Data = student, message = "Student Added Successfully" });
            }

            return Json(new { Success = false, message = "Failed to add user" });
        }


        // GET: StudentController/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
           var result = await _repostory.GetByIdAsync(id);
            if (result != null)
            {
                return Json(new {success=true,Data = result , message="Student Founded Successfully"});
            }
            return Json(new { success = false, Data = result, message = "Student  Not Found" });
        }

        // POST: StudentController/Edit/5
        [HttpPost("Student/Update/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, UserDto user)
        {
            var student = await _repostory.GetByIdAsync(id);
            int result = 0;

            if (student == null)
            {
                return Json(new { success = false, message = "Student not found" });
            }

            // تحديث الخصائص حسب الفرق
            if (!string.IsNullOrEmpty(user.Email) && user.Email != student.Email)
            {
                student.Email = user.Email;
                result = 1;
            }

            if (!string.IsNullOrEmpty(user.FullName) && user.FullName != student.FullName)
            {
                student.FullName = user.FullName;
                result = 1;
            }

            if (!string.IsNullOrEmpty(user.Phone) && user.Phone != student.PhoneNumber)
            {
                student.PhoneNumber = user.Phone;
                result = 1;
            }

            // إذا تم إدخال كلمة سر جديدة، نستخدم UserManager لتحديثها
            if (!string.IsNullOrEmpty(user.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(student);
                var passwordResult = await _userManager.ResetPasswordAsync(student, token, user.Password);
                if (!passwordResult.Succeeded)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Password update failed",
                        errors = passwordResult.Errors.Select(e => e.Description)
                    });
                }
                result = 1;
            }

            if (result != 0)
            {
                // يتم تمرير الكائن student (من نوع User) إلى UpdateAsync
                await _repostory.UpdateAsync(student);
                return Json(new { success = true, data = student, message = "Student updated successfully" });
            }

            return Json(new { success = false, message = "No changes detected" });
        }



        // GET: StudentController/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            var student = await _repostory.GetByIdAsync(id);

            if (student != null)
            {
                return Json(new { success = true, data = student, message = "Success" });
            }
            return Json(new { success = false, error = "Student not found" });
        }

        // POST: StudentController/Delete/5
        [HttpPost("Student/DeleteConfirmed/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id, UserDto student)
        {
            var stu = await _repostory.GetByIdAsync(id);
            if (stu != null) {
            
                await _repostory.DeleteAsync(id);
                return Json(new { success = true, data=student,message="Student Deleted Successfully !" });
            }
            return Json(new { success = false, message = "The Student Not Found" });
        }
        [HttpPost]
        public async Task<ActionResult> DeletePermanent(string id)
        {
            var user = await _repostory.ConfirmDelete(id);
            if (!user)
                return NotFound("Student Not Found");
            return Ok(user);
        }
        [HttpGet]
        public async Task<ActionResult> GetDeletedStudents()
        {
            var student = await _repostory.GetDeletedStudents();
            if (student != null)
                return Ok(student);
            return NotFound();
        }
        [HttpPost]
        public async Task<ActionResult> Restore(string id)
        {
            var student = await _repostory.Restore(id);
            if(student)
                return Ok(student);
            return NotFound();
        }
    }
}
