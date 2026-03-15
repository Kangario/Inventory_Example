using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TestTaskInventory.Inventory.Presentation
{
    public sealed class InventoryView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _coinsLabel;
        [SerializeField] private TMP_Text _weightLabel;
        [SerializeField] private List<InventorySlotView> _slotViews = new List<InventorySlotView>();

        public IReadOnlyList<InventorySlotView> SlotViews => _slotViews;

        public void Render(Runtime.InventoryService inventoryService, Runtime.InventoryConfig config)
        {
            if (_coinsLabel != null)
            {
                _coinsLabel.text = $"Coin balance: {inventoryService.CoinsBalance}";
            }

            if (_weightLabel != null)
            {
                _weightLabel.text = $"Total weight: {inventoryService.TotalWeightKg:0.##} kg";
            }

            var slots = inventoryService.Slots;
            for (var i = 0; i < _slotViews.Count; i++)
            {
                var state = i < slots.Count ? slots[i] : null;
                _slotViews[i].Render(state, config.SlotUnlockPrice);
            }
        }
    }
}
