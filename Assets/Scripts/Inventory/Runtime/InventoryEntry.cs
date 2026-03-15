namespace TestTaskInventory.Inventory.Runtime
{
    public sealed class InventoryEntry
    {
        public InventoryEntry(InventoryItemDefinition definition, int quantity)
        {
            Definition = definition;
            Quantity = quantity;
        }

        public InventoryItemDefinition Definition { get; }
        public int Quantity { get; set; }
        public float TotalWeightKg => Definition == null ? 0f : Definition.WeightKg * Quantity;
    }
}
