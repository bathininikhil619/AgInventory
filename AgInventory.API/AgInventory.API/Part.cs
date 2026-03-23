namespace AgInventory.API.Models
{
    public class Part
    {
        public int PartId { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Supplier { get; set; } = string.Empty;
        public decimal UnitCost { get; set; }
        public int ReorderPoint { get; set; }
        public int QtyOnHand { get; set; }
        public string Location { get; set; } = string.Empty;
        public string StockStatus { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}