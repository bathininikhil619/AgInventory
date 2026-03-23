using AgInventory.API.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AgInventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly string _connectionString;

        public StockController(IConfiguration configuration)
        {
            _connectionString = configuration
                .GetConnectionString("DefaultConnection")!;
        }

        // GET api/stock
        [HttpGet]
        public async Task<IActionResult> GetAllStock()
        {
            using var connection = new SqlConnection(_connectionString);
            var stock = await connection.QueryAsync<StockItem>(@"
                SELECT
                    p.PartId, p.SKU,
                    p.Name AS PartName,
                    c.Name AS Category,
                    st.QtyOnHand,
                    p.ReorderPoint,
                    l.Name AS Location
                FROM Parts p
                JOIN Categories c  ON p.CategoryId  = c.CategoryId
                JOIN Stock st      ON p.PartId      = st.PartId
                JOIN Locations l   ON st.LocationId = l.LocationId
                WHERE p.IsActive = 1
                ORDER BY p.Name");
            return Ok(stock);
        }

        // GET api/stock/lowstock
        [HttpGet("lowstock")]
        public async Task<IActionResult> GetLowStock()
        {
            using var connection = new SqlConnection(_connectionString);
            var parts = await connection.QueryAsync<StockItem>(
                "usp_GetLowStockParts",
                commandType: System.Data.CommandType.StoredProcedure);
            return Ok(parts);
        }

        // POST api/stock/adjust
        [HttpPost("adjust")]
        public async Task<IActionResult> AdjustStock(
            [FromBody] AdjustStockRequest request)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                "usp_AdjustStock",
                new
                {
                    request.PartId,
                    request.LocationId,
                    request.QtyDelta,
                    request.UserId,
                    request.ChangeType,
                    request.Reason
                },
                commandType: System.Data.CommandType.StoredProcedure);
            return Ok(new { message = "Stock adjusted successfully" });
        }
    }
}