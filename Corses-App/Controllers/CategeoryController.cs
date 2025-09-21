using Corses_App.Data.Repostory;
using Courses_App.Core.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Corses_App.Controllers
{
    public class CategeoryController : Controller
    {
        private readonly ICategeoryRepostory _repostory;

        public CategeoryController(ICategeoryRepostory repostory)
        {
            _repostory = repostory;
        }
        public async Task<ActionResult> Index()
        {
            var categories = await _repostory.GetCategeories();
            return View(categories);
        }
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var categeories = await _repostory.GetCategeories();
            if (categeories == null)
            {
                return NotFound();
            }
            return Ok(categeories);
        }
        [HttpPost]
        public async Task<ActionResult> Add(CategeoryCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var categeory = await _repostory.AddCategeoryAsync(dto);
            if (categeory != null)
                return Ok(categeory);

            return BadRequest();
        }
        [HttpPost]
        public async Task<ActionResult> Update ([FromRoute]int id ,[FromForm] CategeoryCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var cat = await _repostory.UpdateCategory( dto , id);
            if (cat == null)
                return NotFound($"error in {cat}");
            return Ok(cat);

        }
        [HttpPost]
        public async Task<ActionResult> Delete (int id)
        {
            if(id == 0 || string.IsNullOrEmpty(id.ToString()))
                return BadRequest();
            var cat = await _repostory.GetCategeoryById(id);
            var result = await _repostory.Delete(id);
            if (result == null)
                return Ok($"No Course found with category Id =  {id}");
            if (result == false)
                return Ok("The Caegory have one or more courses you can`t delete it");
            return Ok(result);
        }
    }
}
