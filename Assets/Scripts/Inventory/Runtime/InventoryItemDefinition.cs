using UnityEngine;

namespace TestTaskInventory.Inventory.Runtime
{
    [CreateAssetMenu(menuName = "Inventory/Item Definition", fileName = "InventoryItemDefinition")]
    public sealed class InventoryItemDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private ItemCategory _category;
        [SerializeField] private AmmoType _ammoType = AmmoType.None;
        [SerializeField] private float _weightKg;
        [SerializeField] private int _maxStack = 1;
        [SerializeField] private int _damage;
        [SerializeField] private int _defense;

        public string Id => _id;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public ItemCategory Category => _category;
        public AmmoType AmmoType => _ammoType;
        public float WeightKg => _weightKg;
        public int MaxStack => Mathf.Max(1, _maxStack);
        public int Damage => _damage;
        public int Defense => _defense;
        public bool IsStackable => _category == ItemCategory.Ammo && MaxStack > 1;

        private void OnValidate()
        {
            if (_category != ItemCategory.Ammo)
            {
                _maxStack = 1;
            }

            if (_category != ItemCategory.Weapon && _category != ItemCategory.Ammo)
            {
                _ammoType = AmmoType.None;
            }

            if (_weightKg < 0f)
            {
                _weightKg = 0f;
            }
        }
    }
}
