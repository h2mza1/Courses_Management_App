using Corses_App.Data.Repostory;
using Courses_App.Core.DTO;
using Courses_App.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Caching.Memory;


namespace Corses_App.Controllers
{
    public class CourseController : Controller
    {
        private readonly ICourseRepostory _repostory;
        private readonly IMemoryCache _cache;

        public CourseController(ICourseRepostory repostory , IMemoryCache cache)
        {
            _repostory = repostory;
            _cache = cache;
        }

        //Index
        //Get
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> Index()
        {
            if(!_cache.TryGetValue("Courses",out List<CourseReadDTO> courses))
            {
                 courses = await _repostory.GetAllAsync();
                var cachOption = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                _cache.Set("Courses",courses,cachOption);

            }

            return View(courses);
        }
        //GET: StudentController/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var courses = await _repostory.GetAll();
            if (courses == null || courses.Count == 0)
                return Json(new { success = false, message = "No courses founded" });
            return Json(new { success = true, data = courses, message = "The courses founded successfully" });
        }
        //GET : CourseController/GetMoreCourses/10
        [HttpGet]
        public async Task<ActionResult> GetMoreCourses(int skip, int take=6)
        {
            var courses = await _repostory.GetMoreCorses(skip,take);
            if(!courses.Any())
            {
                return NotFound("Courses is not founded");
            }
            return Ok(courses);
        }
        //GET: CourseController/GetById/Id
        // Course/GetById/1
        [Authorize(Roles = "Admin")]

        [HttpGet]
        public async Task<ActionResult> GetById(int id)
        {
            var course = await _repostory.GetByIdAsync(id);
            if (course == null)
                return NotFound($"The course with id = {{id}} Not Found");
            return Ok(course);
        }
        [HttpGet]
        public async Task<ActionResult> GetMaxEnrollments(int number)
        {
            var courses = await _repostory.GetMaxEnrollmentsInCourses(number);
            if (!courses.Any())
                return NotFound();
            return Ok(courses);
        }
        //Get : Courses/GetCoursesByCategoryId/Id
        [HttpGet]
        public async Task<ActionResult> GetCoursesByCategoryId(int id)
        {
            string cacheKey = $"courses_{id}";

            if (!_cache.TryGetValue(cacheKey, out List<CourseReadDTO> c))
            {

                c = await _repostory.GetCoursesByCategoryId(id);
                if (c != null && c.Any())
                {
                    var cacheOption = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                    _cache.Set(cacheKey, c, cacheOption);
                }
            }

            if (c == null || !c.Any())
                return View();

            return View(c);
        }
        //Edit/Id
        //Get
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Edit(int id)
        {
            var course = await _repostory.GetByIdAsync(id);
            if (course == null)
            {
                return Json(new { success = false, message = "The course not found" });
            }
            return Json(new
            {
                success = true,
                data = new
                {
                    id = course.courseId,
                    title = course.Name,
                    description = course.Description,
                    instructorId = course.instructorId,
                    instructorName = course.InstructorName,
                    price = course.Price,
                    categeory = course.categeoryId,
                    startDate = course.StartDate,
                    endDate = course.EndDate,
                    duration=course.time,
                    courseImage = course.courseImage ?? "",
                }
            });

        }

        //Edit/Id
        //Post
        [Authorize(Roles = "Admin")]

        [HttpPost("/Course/ConfirmEdit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CourseCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid model state" });
            }
            if(dto.StartDate <  DateTime.Now)
                return Json(new { success = false, message = "Invalid start date state" });
            if (dto.StartDate > dto.EndDate)
                return Json(new { success = false, message = $"strat date : {dto.StartDate} can`t grater than end date: {dto.EndDate}" });

            var result = await _repostory.UpdateAsync(id,dto);
            if (result == null)
            {
                return Json(new { success = false, message = "Failed to update the course" });
            }

            return Json(new { success = true, data = result, message = "The course updated successfully" });
        }

        [Authorize(Roles = "Admin")]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWithVideos([FromForm] CourseCreateDto dto)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid model state." });

            if (dto.StartDate < DateTime.Now)
                return Json(new { success = false, message = "Invalid start date state" });
            if (dto.StartDate > dto.EndDate)
                return Json(new { success = false, message = $"strat date : {dto.StartDate} can`t grater than end date: {dto.EndDate}" });

            var result = await _repostory.AddAsync(dto);

            if (result == null)
            {
                return Json(new { success = false, message = "Error creating course." });
            }

          
            return Json(new { success = true,
                data = new CourseReadDTO()
                {
                    courseId = result.courseId,
                    Name = result.Name,
                    Description = result.Description,
                    instructorId = result.instructorId,
                    categeoryId = result.categeoryId,
                    Price = result.Price,
                    StartDate=result.StartDate,
                    EndDate=result.EndDate,
                    time=result.time,
                    courseImage=result.courseImage ?? "",
                    //Categeory = result.Categeory,
                }, message = "Course created with videos." });
        }

        //Course/Delete/Id
        //Post
        [Authorize(Roles = "Admin")]

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Delete (int id)
        {
            var course = await _repostory.GetByIdAsync(id);
            if (course == null)
                return Json(new { success = false, message = "The course not found" });
            var res=   await _repostory.DeleteAsync(id);
            return Json(new { success = res, data = course, message = "the course deleted successfully" });
        }
        [HttpGet]
        public async Task<ActionResult<List<CourseReadDTO>?>> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Ok(new List<CourseReadDTO>()); // يرجع List فاضي بدل ما يعمل query

            var result = await _repostory.Search(term);

            if (result == null || !result.Any())
                return Ok(new List<CourseReadDTO>()); // مش NotFound، بس List فاضي

            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> DeletePermanent(int id)
        {
            var result = await _repostory.ConfirmDeleted(id);
            if(result)
                return Ok(result);
            return Ok(new {success=false,message="an error on delete course"});
        }
        [HttpPost]
        public async Task<IActionResult> Restore([FromHeader]int courseId)
        {
             await _repostory.RestoreAsync(courseId);
                return Ok();
        }
    }


}

