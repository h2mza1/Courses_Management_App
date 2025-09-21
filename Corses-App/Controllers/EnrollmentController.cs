using Corses_App.Data.Repostory;
using Courses_App.Core.DTO;
using Courses_App.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging.Signing;
using System.Threading.Tasks;

namespace Corses_App.Controllers
{
    //[Authorize(Roles ="Admin")]
    public class EnrollmentController : Controller
    {
        private readonly IEnreollmentRepostory _repostory;
        private readonly ICourseRepostory _courseRepostory;

        public EnrollmentController(IEnreollmentRepostory repostory , ICourseRepostory courseRepostory) 
        {
            _repostory = repostory;
            _courseRepostory = courseRepostory;

        }
        // GET: EnrollmentController
        public async Task<ActionResult> Index()
        {
            var enrrollments = await _repostory.GetAllAsync();
            return View(enrrollments);
        }
        //[HttpGet("GetStudentsNotEnrolledInCourse/{courseId}")]
        [HttpGet("Enrollment/GetStudentsNotEnrolledInCourse/{courseId}")]
        [Authorize(Roles ="Admin")]
        public async Task<ActionResult> GetStudentsNotEnrolledInCourse(int courseId)
        {
            if (courseId == 0)
                return BadRequest($"Course Id : {courseId} Not Found");

            var students = await _repostory.GetStudentsNotEnrolledInCourseAsync(courseId);
            if(students.Any())
                return Ok(new {success=true , data = students });
            return NotFound(new {success=false,data = students});

        }

        // GET: EnrollmentController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: EnrollmentController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EnrollmentController/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([FromBody]EnrollmentCreateDTO collection)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, data = collection, message = "An Error On Modal" });
            var enrollment = await _repostory.AddAsync(collection);
            if (enrollment == null)
                return Json(new { success = false, data = collection, message = "The student already founded" });
            
            return Json(new { success = true, data = enrollment, message = "Enrollmnet Added Successfully" });

        }

        // GET: EnrollmentController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: EnrollmentController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: EnrollmentController/Delete/5
        [Authorize(Roles = "Admin")]

        public async Task<ActionResult> Delete(int id)
        {
            var enroll = await _repostory.GetByIdAsync(id);
            if (enroll == null)
                return NotFound("The enrollment Not founed");
            var enrollDto = new EnrollmentReadDTO
            {
                Id = enroll.Id,
                CourseId = enroll.CourseId,
                CourseName = enroll.CourseName,
                StudentId = enroll.StudentId,
                StudentName = enroll.StudentName,
                EnrollmentDate = enroll.EnrollmentDate,
            };
            return Ok(enrollDto);
        }

        // POST: EnrollmentController/Delete/5
        [Authorize(Roles = "Admin")]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, EnrollmentReadDTO collection)
        {
            var enroll = await _repostory.GetByIdAsync(id);
            if (enroll == null)
                return NotFound("The enrollment Not founed");
            var result = await _repostory.DeleteAsync(id);
            if(result)
                return Ok(new {success= true , message=$"Enrollement for student {enroll.StudentName} deleted successfully"});
            return BadRequest(result);
        }
    }
}
