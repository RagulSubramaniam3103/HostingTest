using System;

namespace EWOMS_ClassLibrary.DataIntegration
{
    public class ConstructionExpense
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime SpentOn { get; set; } = DateTime.UtcNow;
        public string Category { get; set; } = "Materials"; // Materials, Labor, Equipment, Permits, Other
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
