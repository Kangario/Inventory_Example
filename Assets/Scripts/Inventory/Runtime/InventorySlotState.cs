namespace TestTaskInventory.Inventory.Runtime
{
    public sealed class InventorySlotState
    {
        public InventorySlotState(bool isUnlocked)
        {
            IsUnlocked = isUnlocked;
        }

        public bool IsUnlocked { get; set; }
        public InventoryEntry Entry { get; set; }
        public bool IsEmpty => Entry == null || Entry.Quantity <= 0;
    }
}
