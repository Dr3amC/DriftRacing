using System.Collections.Generic;
using Unity.Entities;
using Zenject;
using Core.Services;
using UnityEngine;

namespace Core.Installers
{
    public class WorldInstaller : MonoInstaller, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<World>()
                .FromMethod(CreateWorld)
                .AsSingle()
                .NonLazy();
        }

        private World CreateWorld(InjectContext context)
        {
            var world = new World("LevelWorld");
            World.DefaultGameObjectInjectionWorld = world;

            var container = context.Container;

            var systemTypes = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default, false);
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, systemTypes);

            foreach (var system in world.Systems)
            {
                container.Inject(system);
            }

            return world;
        }
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var prefabProviders = Container.ResolveAll<Services.IPrefabProvider>();
            foreach (var prefabProvider in prefabProviders)
            {
                prefabProvider.PreparePrefabs(conversionSystem);
            }
        }
        
        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            var prefabProviders = Container.ResolveAll<Services.IPrefabProvider>();
            foreach (var prefabProvider in prefabProviders)
            {
                prefabProvider.DeclarePrefabs(referencedPrefabs);
            }
        }
    }
}