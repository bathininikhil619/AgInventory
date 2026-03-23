namespace AgInventory.API.Models
{
    public class AdjustStockRequest
    {
        public int PartId { get; set; }
        public int LocationId { get; set; }
        public int QtyDelta { get; set; }
        public int UserId { get; set; }
        public string ChangeType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}