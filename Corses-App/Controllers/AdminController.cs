using Corses_App.Data.Repostory;
using Corses_App.Repostory;
using Courses_App.Core.DTO;
using Courses_App.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.Reflection.Metadata;
using System.Threading.Tasks;


namespace Corses_App.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {
        private readonly SignInManager<User> _signIn;
        private readonly UserManager<User> _userManager;
        private readonly ICourseRepostory courseRepostory;
        private readonly IInstructorRepostory instructorRepostory;
        private readonly IStudentRepostory studentRepostory;
        private readonly IMemoryCache _cache;
        private readonly ICategeoryRepostory categeoryRepostory;

        public AdminController(SignInManager<User> signIn , UserManager<User> userManager
            , ICourseRepostory course
            ,IInstructorRepostory repostory
            ,IStudentRepostory student
            ,IMemoryCache cache
            ,ICategeoryRepostory categeory )
        {
            
            _signIn=signIn;
            _userManager = userManager;
            courseRepostory =course;
            instructorRepostory = repostory;
            studentRepostory =student;
            _cache = cache;
            categeoryRepostory = categeory;

        }
        public async Task<IActionResult> Index()
        {
            var model = new List<HistoryDTO>();
            var courses = await courseRepostory.GetDeletedCoursesAsync();
            var instructors = await instructorRepostory.GetInstructorsDeleted();
            var students = await studentRepostory.GetDeletedStudents();
            if(!_cache.TryGetValue("studentsCount", out int sCount))
            {
                sCount = await studentRepostory.GetStudentsCountAsync();
                var cacheOption = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                _cache.Set("studentsCount", sCount, cacheOption);

            }
            if (!_cache.TryGetValue("CoursesCount", out int cCount))
            {
                cCount = await courseRepostory.GetCourseCountAsync() ;
                var cacheOption = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                _cache.Set("CoursesCount", cCount, cacheOption);

            }
            if (!_cache.TryGetValue("InstructorsCount", out int iCount))
            {
                iCount = await instructorRepostory.GetInstructorsCountAsync();
                var cacheOption = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                _cache.Set("InstructorsCount", iCount, cacheOption);

            }
            if (!_cache.TryGetValue("CategoriesCount", out int catCount))
            {
                catCount = await categeoryRepostory.GetCategoriesCountAsync();
                var cacheOption = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                _cache.Set("CategoriesCount", catCount, cacheOption);

            }
            ViewBag.iCount = iCount;
            ViewBag.sCount = sCount;
            ViewBag.cCount = cCount;
            ViewBag.catCount = catCount;
           if(courses != null)
            {
                model.AddRange(courses);
            }
           if(instructors != null)
            {
                model.AddRange(instructors);
            }
            if (students != null) 
            {
                model.AddRange(students);
            }
            model.OrderByDescending(c => c.DeletedAt);

            if(model!=null)
            return View(model);
            return View(new List<HistoryDTO>());
        }
        
    }
}
