using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AgInventory.API.Controllers
{
    public class AuditLogEntry
    {
        public int LogId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty;
        public int QtyDelta { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogController : ControllerBase
    {
        private readonly string _connectionString;

        public AuditLogController(IConfiguration configuration)
        {
            _connectionString = configuration
                .GetConnectionString("DefaultConnection")!;
        }

        // GET api/auditlog
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            using var connection = new SqlConnection(_connectionString);
            var logs = await connection.QueryAsync<AuditLogEntry>(@"
                SELECT TOP 100
                    al.LogId,
                    p.Name AS PartName,
                    p.SKU,
                    u.Username,
                    al.ChangeType,
                    al.QtyDelta,
                    al.Reason,
                    al.CreatedAt
                FROM AuditLog al
                JOIN Parts p ON al.PartId = p.PartId
                JOIN Users u ON al.UserId = u.UserId
                ORDER BY al.CreatedAt DESC");
            return Ok(logs);
        }

        // GET api/auditlog/part/5
        [HttpGet("part/{partId}")]
        public async Task<IActionResult> GetByPart(int partId)
        {
            using var connection = new SqlConnection(_connectionString);
            var logs = await connection.QueryAsync<AuditLogEntry>(@"
                SELECT
                    al.LogId,
                    p.Name AS PartName,
                    p.SKU,
                    u.Username,
                    al.ChangeType,
                    al.QtyDelta,
                    al.Reason,
                    al.CreatedAt
                FROM AuditLog al
                JOIN Parts p ON al.PartId = p.PartId
                JOIN Users u ON al.UserId = u.UserId
                WHERE al.PartId = @PartId
                ORDER BY al.CreatedAt DESC",
                new { PartId = partId });
            return Ok(logs);
        }
    }
}