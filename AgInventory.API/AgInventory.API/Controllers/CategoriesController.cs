using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AgInventory.API.Controllers
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly string _connectionString;

        public CategoriesController(IConfiguration configuration)
        {
            _connectionString = configuration
                .GetConnectionString("DefaultConnection")!;
        }

        // GET api/categories
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            using var connection = new SqlConnection(_connectionString);
            var categories = await connection.QueryAsync<Category>(
                "SELECT CategoryId, Name FROM Categories WHERE IsActive = 1 ORDER BY Name");
            return Ok(categories);
        }
    }
}