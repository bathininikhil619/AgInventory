using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AgInventory.API.Controllers
{
    public class Supplier
    {
        public int SupplierId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly string _connectionString;

        public SuppliersController(IConfiguration configuration)
        {
            _connectionString = configuration
                .GetConnectionString("DefaultConnection")!;
        }

        // GET api/suppliers
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            using var connection = new SqlConnection(_connectionString);
            var suppliers = await connection.QueryAsync<Supplier>(
                "SELECT SupplierId, Name, ContactName, Phone, Email FROM Suppliers WHERE IsActive = 1 ORDER BY Name");
            return Ok(suppliers);
        }
    }
}