using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AgInventory.API.Controllers
{
    public class PurchaseOrder
    {
        public int POId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ReceivedAt { get; set; }
    }

    public class CreatePORequest
    {
        public int SupplierId { get; set; }
        public List<POItem> Items { get; set; } = new();
    }

    public class POItem
    {
        public int PartId { get; set; }
        public int QtyOrdered { get; set; }
        public decimal UnitCost { get; set; }
    }

    public class ReceivePORequest
    {
        public int POId { get; set; }
        public int UserId { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly string _connectionString;

        public PurchaseOrdersController(IConfiguration configuration)
        {
            _connectionString = configuration
                .GetConnectionString("DefaultConnection")!;
        }

        // GET api/purchaseorders
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            using var connection = new SqlConnection(_connectionString);
            var orders = await connection.QueryAsync<PurchaseOrder>(@"
                SELECT
                    po.POId,
                    s.Name AS SupplierName,
                    po.Status,
                    po.CreatedAt,
                    po.ReceivedAt
                FROM PurchaseOrders po
                JOIN Suppliers s ON po.SupplierId = s.SupplierId
                ORDER BY po.CreatedAt DESC");
            return Ok(orders);
        }

        // GET api/purchaseorders/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var items = await connection.QueryAsync(@"
                SELECT
                    poi.POItemId,
                    p.SKU,
                    p.Name AS PartName,
                    poi.QtyOrdered,
                    poi.UnitCost,
                    poi.QtyOrdered * poi.UnitCost AS LineTotal
                FROM PurchaseOrderItems poi
                JOIN Parts p ON poi.PartId = p.PartId
                WHERE poi.POId = @POId",
                new { POId = id });
            return Ok(items);
        }

        // POST api/purchaseorders
        [HttpPost]
        public async Task<IActionResult> CreatePO(
            [FromBody] CreatePORequest request)
        {
            using var connection = new SqlConnection(_connectionString);

            // Create the PO header
            var poId = await connection.QuerySingleAsync<int>(@"
                INSERT INTO PurchaseOrders (SupplierId, Status)
                VALUES (@SupplierId, 'Draft')
                SELECT SCOPE_IDENTITY()",
                new { request.SupplierId });

            // Add line items
            foreach (var item in request.Items)
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO PurchaseOrderItems
                        (POId, PartId, QtyOrdered, UnitCost)
                    VALUES
                        (@POId, @PartId, @QtyOrdered, @UnitCost)",
                    new
                    {
                        POId = poId,
                        item.PartId,
                        item.QtyOrdered,
                        item.UnitCost
                    });
            }

            return Ok(new { POId = poId, message = "Purchase order created" });
        }

        // POST api/purchaseorders/receive
        [HttpPost("receive")]
        public async Task<IActionResult> ReceivePO(
            [FromBody] ReceivePORequest request)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                "usp_ReceivePO",
                new { request.POId, request.UserId },
                commandType: System.Data.CommandType.StoredProcedure);
            return Ok(new { message = "Purchase order received successfully" });
        }
    }
}