using Unity.Entities;
using Zenject;

namespace Core.Installers
{
    public class WorldInstaller : MonoInstaller
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
    }
}