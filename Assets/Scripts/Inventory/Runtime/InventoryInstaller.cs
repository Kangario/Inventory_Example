using UnityEngine;
using Zenject;

namespace TestTaskInventory.Inventory.Runtime
{
    public sealed class InventoryInstaller : MonoInstaller
    {
        [SerializeField] private InventoryConfig _config;
        [SerializeField] private InventoryItemCatalog _catalog;

        public override void InstallBindings()
        {
            Container.BindInstance(_config).IfNotBound();
            Container.BindInstance(_catalog).IfNotBound();
            Container.BindInterfacesAndSelfTo<InventoryService>().AsSingle().NonLazy();
        }
    }
}
