using UnityEngine;

namespace TestTaskInventory.Inventory.Runtime
{
    [CreateAssetMenu(menuName = "Inventory/Config", fileName = "InventoryConfig")]
    public sealed class InventoryConfig : ScriptableObject
    {
        [Min(1)]
        [SerializeField] private int _totalSlots = 30;

        [Min(0)]
        [SerializeField] private int _defaultUnlockedSlots = 15;

        [Min(0)]
        [SerializeField] private int _slotUnlockPrice = 100;

        [Min(1)]
        [SerializeField] private int _ammoGrantAmountPerType = 30;

        [Min(0)]
        [SerializeField] private int _coinsPerClick = 50;

        public int TotalSlots => _totalSlots;
        public int DefaultUnlockedSlots => Mathf.Clamp(_defaultUnlockedSlots, 0, _totalSlots);
        public int SlotUnlockPrice => _slotUnlockPrice;
        public int AmmoGrantAmountPerType => _ammoGrantAmountPerType;
        public int CoinsPerClick => _coinsPerClick;
    }
}
