namespace AgInventory.API.Models
{
    public class StockItem
    {
        public int PartId { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int QtyOnHand { get; set; }
        public int ReorderPoint { get; set; }
        public string Location { get; set; } = string.Empty;
    }
}