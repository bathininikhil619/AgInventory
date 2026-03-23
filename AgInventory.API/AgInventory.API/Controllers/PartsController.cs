using AgInventory.API.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AgInventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartsController : ControllerBase
    {
        private readonly string _connectionString;

        public PartsController(IConfiguration configuration)
        {
            _connectionString = configuration
                .GetConnectionString("DefaultConnection")!;
        }

        // GET api/parts
        [HttpGet]
        public async Task<IActionResult> GetAllParts()
        {
            using var connection = new SqlConnection(_connectionString);
            var parts = await connection.QueryAsync<Part>(@"
                SELECT 
                    p.PartId, p.SKU,
                    p.Name, c.Name AS Category,
                    s.Name AS Supplier,
                    p.UnitCost, p.ReorderPoint,
                    st.QtyOnHand,
                    l.Name AS Location,
                    p.IsActive,
                    CASE 
                        WHEN st.QtyOnHand <= p.ReorderPoint 
                        THEN 'LOW STOCK' ELSE 'OK' 
                    END AS StockStatus
                FROM Parts p
                JOIN Categories c  ON p.CategoryId  = c.CategoryId
                JOIN Suppliers s   ON p.SupplierId  = s.SupplierId
                JOIN Stock st      ON p.PartId      = st.PartId
                JOIN Locations l   ON st.LocationId = l.LocationId
                WHERE p.IsActive = 1
                ORDER BY p.Name");
            return Ok(parts);
        }

        // GET api/parts/lowstock
        [HttpGet("lowstock")]
        public async Task<IActionResult> GetLowStockParts()
        {
            using var connection = new SqlConnection(_connectionString);
            var parts = await connection.QueryAsync<StockItem>(
                "usp_GetLowStockParts",
                commandType: System.Data.CommandType.StoredProcedure);
            return Ok(parts);
        }

        // GET api/parts/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPartById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var part = await connection.QueryFirstOrDefaultAsync<Part>(@"
                SELECT 
                    p.PartId, p.SKU,
                    p.Name, c.Name AS Category,
                    s.Name AS Supplier,
                    p.UnitCost, p.ReorderPoint,
                    st.QtyOnHand,
                    l.Name AS Location,
                    p.IsActive
                FROM Parts p
                JOIN Categories c  ON p.CategoryId  = c.CategoryId
                JOIN Suppliers s   ON p.SupplierId  = s.SupplierId
                JOIN Stock st      ON p.PartId      = st.PartId
                JOIN Locations l   ON st.LocationId = l.LocationId
                WHERE p.PartId = @PartId",
                new { PartId = id });

            if (part == null) return NotFound();
            return Ok(part);
        }

        // POST api/parts/adjuststock
        [HttpPost("adjuststock")]
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