using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Zenject;

namespace TestTaskInventory.Inventory.Runtime
{
    public sealed class InventoryService : IInitializable
    {
        private readonly InventoryConfig _config;
        private readonly InventoryItemCatalog _catalog;
        private readonly List<InventorySlotState> _slots = new List<InventorySlotState>();

        public InventoryService(InventoryConfig config, InventoryItemCatalog catalog)
        {
            _config = config;
            _catalog = catalog;
        }

        public event System.Action Changed;

        public IReadOnlyList<InventorySlotState> Slots => _slots;
        public int CoinsBalance { get; private set; }
        public float TotalWeightKg => _slots.Where(slot => !slot.IsEmpty).Sum(slot => slot.Entry.TotalWeightKg);

        public void Initialize()
        {
            _slots.Clear();

            for (var i = 0; i < _config.TotalSlots; i++)
            {
                _slots.Add(new InventorySlotState(i < _config.DefaultUnlockedSlots));
            }

            ValidateCatalog();
            NotifyChanged();
        }

        public void AddCoins()
        {
            CoinsBalance += _config.CoinsPerClick;
            Debug.Log($"[Inventory] Добавлено монет: {_config.CoinsPerClick}. Баланс: {CoinsBalance}.");
            NotifyChanged();
        }

        public bool TryUnlockSlot(int slotIndex)
        {
            if (!IsValidIndex(slotIndex))
            {
                Debug.LogError($"[Inventory] Невозможно разблокировать слот {slotIndex + 1}: некорректный индекс.");
                return false;
            }

            var slot = _slots[slotIndex];
            if (slot.IsUnlocked)
            {
                return true;
            }

            if (CoinsBalance < _config.SlotUnlockPrice)
            {
                Debug.LogError($"[Inventory] Недостаточно монет для разблокировки слота {slotIndex + 1}. Нужно {_config.SlotUnlockPrice}, есть {CoinsBalance}.");
                return false;
            }

            CoinsBalance -= _config.SlotUnlockPrice;
            slot.IsUnlocked = true;
            Debug.Log($"[Inventory] Слот {slotIndex + 1} разблокирован за {_config.SlotUnlockPrice}. Баланс: {CoinsBalance}.");
            NotifyChanged();
            return true;
        }

        public void ShootRandom()
        {
            var availableAmmoTypes = GetAvailableShootAmmoTypes();
            if (availableAmmoTypes.Count == 0)
            {
                Debug.LogError("[Inventory] Выстрел невозможен: нет пары оружие + подходящие патроны.");
                return;
            }

            var ammoType = availableAmmoTypes[UnityEngine.Random.Range(0, availableAmmoTypes.Count)];
            var ammoSlots = GetOccupiedSlots(slot => slot.Entry.Definition.Category == ItemCategory.Ammo && slot.Entry.Definition.AmmoType == ammoType);
            var weaponSlots = GetOccupiedSlots(slot => slot.Entry.Definition.Category == ItemCategory.Weapon && slot.Entry.Definition.AmmoType == ammoType);

            var ammoSlotIndex = ammoSlots[UnityEngine.Random.Range(0, ammoSlots.Count)];
            var weaponSlotIndex = weaponSlots[UnityEngine.Random.Range(0, weaponSlots.Count)];

            var ammoEntry = _slots[ammoSlotIndex].Entry;
            var weaponEntry = _slots[weaponSlotIndex].Entry;

            ammoEntry.Quantity -= 1;
            if (ammoEntry.Quantity <= 0)
            {
                _slots[ammoSlotIndex].Entry = null;
            }

            Debug.Log($"[Inventory] Выстрел из {weaponEntry.Definition.DisplayName} патронами {ammoEntry.Definition.DisplayName}. Нанесено урона: {weaponEntry.Definition.Damage}.");
            NotifyChanged();
        }

        public void AddAmmo()
        {
            var ammoItems = _catalog.AmmoItems;
            if (ammoItems.Count == 0)
            {
                Debug.LogError("[Inventory] В каталоге нет патронов для добавления.");
                return;
            }

            foreach (var ammoDefinition in ammoItems)
            {
                TryAddAmmoDefinition(ammoDefinition, _config.AmmoGrantAmountPerType);
            }

            NotifyChanged();
        }

        public void AddRandomItem()
        {
            var definition = _catalog.GetRandomEquipment();
            if (definition == null)
            {
                Debug.LogError("[Inventory] В каталоге нет предметов для случайного добавления.");
                return;
            }

            var emptySlotIndex = FindFirstEmptyUnlockedSlot();
            if (emptySlotIndex < 0)
            {
                Debug.LogError($"[Inventory] Не удалось добавить предмет {definition.DisplayName}: нет свободных слотов.");
                return;
            }

            _slots[emptySlotIndex].Entry = new InventoryEntry(definition, 1);
            Debug.Log($"[Inventory] Добавлен предмет {definition.DisplayName} в слот {emptySlotIndex + 1}.");
            NotifyChanged();
        }

        public void RemoveRandomItem()
        {
            var occupiedSlots = GetOccupiedSlots(slot => true);
            if (occupiedSlots.Count == 0)
            {
                Debug.LogError("[Inventory] Удаление невозможно: все слоты пустые.");
                return;
            }

            var slotIndex = occupiedSlots[UnityEngine.Random.Range(0, occupiedSlots.Count)];
            var entry = _slots[slotIndex].Entry;
            var removedName = entry.Definition.DisplayName;
            var removedQuantity = entry.Quantity;

            _slots[slotIndex].Entry = null;
            Debug.Log($"[Inventory] Удален предмет {removedName} x{removedQuantity} из слота {slotIndex + 1}.");
            NotifyChanged();
        }

        private void TryAddAmmoDefinition(InventoryItemDefinition ammoDefinition, int amount)
        {
            var placementPlan = BuildPlacementPlan(ammoDefinition, amount);
            if (placementPlan == null)
            {
                Debug.LogError($"[Inventory] Не удалось добавить {amount} патронов типа {ammoDefinition.DisplayName}: нет свободных слотов.");
                return;
            }

            foreach (var placement in placementPlan)
            {
                var slot = _slots[placement.SlotIndex];
                if (slot.Entry == null)
                {
                    slot.Entry = new InventoryEntry(ammoDefinition, placement.Amount);
                }
                else
                {
                    slot.Entry.Quantity += placement.Amount;
                }
            }

            var slotList = string.Join(", ", placementPlan.Select(item => (item.SlotIndex + 1).ToString(CultureInfo.InvariantCulture)));
            Debug.Log($"[Inventory] Добавлены патроны {ammoDefinition.DisplayName}: {amount} шт. Слоты: {slotList}.");
        }

        private List<AmmoPlacement> BuildPlacementPlan(InventoryItemDefinition definition, int amount)
        {
            var remaining = amount;
            var plan = new List<AmmoPlacement>();

            foreach (var slotIndex in GetOccupiedSlots(slot =>
                         slot.Entry.Definition == definition &&
                         slot.Entry.Quantity < definition.MaxStack))
            {
                var slot = _slots[slotIndex];
                var freeSpace = definition.MaxStack - slot.Entry.Quantity;
                if (freeSpace <= 0)
                {
                    continue;
                }

                var toAdd = Mathf.Min(freeSpace, remaining);
                if (toAdd > 0)
                {
                    plan.Add(new AmmoPlacement(slotIndex, toAdd));
                    remaining -= toAdd;
                }

                if (remaining <= 0)
                {
                    return plan;
                }
            }

            foreach (var slotIndex in GetUnlockedEmptySlots())
            {
                var toAdd = Mathf.Min(definition.MaxStack, remaining);
                plan.Add(new AmmoPlacement(slotIndex, toAdd));
                remaining -= toAdd;

                if (remaining <= 0)
                {
                    return plan;
                }
            }

            return null;
        }

        private List<AmmoType> GetAvailableShootAmmoTypes()
        {
            var ammoTypes = new List<AmmoType>();

            foreach (AmmoType ammoType in System.Enum.GetValues(typeof(AmmoType)))
            {
                if (ammoType == AmmoType.None)
                {
                    continue;
                }

                if (HasWeaponForAmmoType(ammoType) && CountAmmo(ammoType) > 0)
                {
                    ammoTypes.Add(ammoType);
                }
            }

            return ammoTypes;
        }

        private bool HasWeaponForAmmoType(AmmoType ammoType)
        {
            return _slots.Any(slot =>
                slot.IsUnlocked &&
                !slot.IsEmpty &&
                slot.Entry.Definition.Category == ItemCategory.Weapon &&
                slot.Entry.Definition.AmmoType == ammoType);
        }

        private int CountAmmo(AmmoType ammoType)
        {
            return _slots.Where(slot =>
                    slot.IsUnlocked &&
                    !slot.IsEmpty &&
                    slot.Entry.Definition.Category == ItemCategory.Ammo &&
                    slot.Entry.Definition.AmmoType == ammoType)
                .Sum(slot => slot.Entry.Quantity);
        }

        private List<int> GetOccupiedSlots(System.Predicate<InventorySlotState> predicate)
        {
            var result = new List<int>();

            for (var i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (!slot.IsUnlocked || slot.IsEmpty)
                {
                    continue;
                }

                if (predicate(slot))
                {
                    result.Add(i);
                }
            }

            return result;
        }

        private IEnumerable<int> GetUnlockedEmptySlots()
        {
            for (var i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot.IsUnlocked && slot.IsEmpty)
                {
                    yield return i;
                }
            }
        }

        private int FindFirstEmptyUnlockedSlot()
        {
            for (var i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot.IsUnlocked && slot.IsEmpty)
                {
                    return i;
                }
            }

            return -1;
        }

        private bool IsValidIndex(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < _slots.Count;
        }

        private void NotifyChanged()
        {
            Changed?.Invoke();
        }

        private void ValidateCatalog()
        {
            if (_catalog == null)
            {
                Debug.LogError("[Inventory] Не назначен InventoryItemCatalog.");
                return;
            }

            if (_catalog.IsValid(out var error))
            {
                return;
            }

            Debug.LogError($"[Inventory] Ошибка каталога: {error}");
        }

        private readonly struct AmmoPlacement
        {
            public AmmoPlacement(int slotIndex, int amount)
            {
                SlotIndex = slotIndex;
                Amount = amount;
            }

            public int SlotIndex { get; }
            public int Amount { get; }
        }
    }
}
