using TestTaskInventory.Inventory.Runtime;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TestTaskInventory.Inventory.Presentation
{
    public sealed class InventoryScreenController : MonoBehaviour
    {
        [SerializeField] private InventoryView _view;
        [SerializeField] private Button _shootButton;
        [SerializeField] private Button _addAmmoButton;
        [SerializeField] private Button _addItemButton;
        [SerializeField] private Button _removeItemButton;
        [SerializeField] private Button _addCoinsButton;

        private InventoryService _inventoryService;
        private InventoryConfig _config;

        [Inject]
        public void Construct(InventoryService inventoryService, InventoryConfig config)
        {
            _inventoryService = inventoryService;
            _config = config;
        }

        private void Awake()
        {
            BindButtons();
            BindSlots();
        }

        private void OnEnable()
        {
            if (_inventoryService == null)
            {
                return;
            }

            _inventoryService.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (_inventoryService == null)
            {
                return;
            }

            _inventoryService.Changed -= Refresh;
        }

        private void BindButtons()
        {
            BindButton(_shootButton,() => _inventoryService?.ShootRandom());
            BindButton(_addAmmoButton,() => _inventoryService?.AddAmmo());
            BindButton(_addItemButton,() => _inventoryService?.AddRandomItem());
            BindButton(_removeItemButton,() => _inventoryService?.RemoveRandomItem());
            BindButton(_addCoinsButton,() => _inventoryService?.AddCoins());
        }

        private void BindSlots()
        {
            if (_view == null)
            {
                return;
            }

            var slotViews = _view.SlotViews;
            for (var i = 0; i < slotViews.Count; i++)
            {
                slotViews[i].Bind(i, OnUnlockRequested);
            }
        }

        private void Refresh()
        {
            if (_view == null || _inventoryService == null || _config == null)
            {
                return;
            }

            _view.Render(_inventoryService, _config);
        }

        private void OnUnlockRequested(int slotIndex)
        {
            _inventoryService.TryUnlockSlot(slotIndex);
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null || action == null)
            {
                return;
            }

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }
    }
}
