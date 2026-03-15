using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TestTaskInventory.Inventory.Runtime
{
    [CreateAssetMenu(menuName = "Inventory/Item Catalog", fileName = "InventoryItemCatalog")]
    public sealed class InventoryItemCatalog : ScriptableObject
    {
        [SerializeField] private List<InventoryItemDefinition> _items = new List<InventoryItemDefinition>();

        public IReadOnlyList<InventoryItemDefinition> AllItems => _items;
        public IReadOnlyList<InventoryItemDefinition> AmmoItems => _items.Where(item => item != null && item.Category == ItemCategory.Ammo).ToList();
        public IReadOnlyList<InventoryItemDefinition> RandomEquipmentPool => _items.Where(item =>
            item != null &&
            (item.Category == ItemCategory.Weapon || item.Category == ItemCategory.HeadArmor || item.Category == ItemCategory.TorsoArmor)).ToList();

        public InventoryItemDefinition GetAmmoDefinition(AmmoType ammoType)
        {
            return _items.FirstOrDefault(item => item != null && item.Category == ItemCategory.Ammo && item.AmmoType == ammoType);
        }

        public InventoryItemDefinition GetRandomEquipment()
        {
            var pool = RandomEquipmentPool;
            if (pool.Count == 0)
            {
                return null;
            }

            var index = UnityEngine.Random.Range(0, pool.Count);
            return pool[index];
        }

        public bool IsValid(out string error)
        {
            var nullItem = _items.Any(item => item == null);
            if (nullItem)
            {
                error = "В каталоге есть пустые ссылки на предметы.";
                return false;
            }

            var duplicateId = _items
                .GroupBy(item => item.Id)
                .FirstOrDefault(group => !string.IsNullOrWhiteSpace(group.Key) && group.Count() > 1);

            if (duplicateId != null)
            {
                error = $"Повторяющийся Id предмета: {duplicateId.Key}";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}
