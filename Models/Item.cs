using System;

namespace SalesInventorySystem_WAM1.Models
{
    /// <summary>
    /// Represents an item in the inventory.
    /// </summary>
    public class Item
    {
        public int Id { get; }
        public string Name { get; set; }
        public string Category { get; set; }

        public double UnitPrice { get; set; }
        public int Stock { get; set; }

        public DateTime DateAdded { get; }
        public DateTime DateModified { get; set; }
    };
}