using System.Collections.Generic;
using API.Data;
using API.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers
{
    public class ProductController : BaseController
    {
        private readonly IUnitOfWork _uow;

        public ProductController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] Dictionary<string, string> queryParams)
        {
            var result = await _uow.SearchRepository.Search(queryParams);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            var userId = HttpContext.User.GetUserId();
            var product = await _uow.ProductRepository.GetProduct(id, userId);
            if (product == null)
                return BadRequest("Product not found!");
            return Ok(product);
        }

        [HttpGet("home")]
        public async Task<ActionResult> GetHomePage()
        {
            var result = await _uow.SearchRepository.GetHomePage();
            return Ok(result);
        }
        [HttpPost("favorite/{id}")]
        public async Task<IActionResult> AddFavorite(int id)
        {
            var userId = HttpContext.User.ToString();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _uow.ProductFavoriteRepository.AddFavorite(id, userId);
            return Ok(new { success = true });
        }


        [HttpDelete("favorite/{id}")]
        public async Task<IActionResult> RemoveFavorite(int id)
        {
            var userId = HttpContext.User.ToString();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _uow.ProductFavoriteRepository.RemoveFavorite(id, userId);
            return Ok(new { success = true });
        }

        [HttpGet("favorites")]
        public async Task<IActionResult> GetFavorites()
        {
            var userId = HttpContext.User.ToString();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var favorites = await _uow.ProductFavoriteRepository.GetUserFavorites(userId);
            return Ok(favorites);
        }

    }
}
