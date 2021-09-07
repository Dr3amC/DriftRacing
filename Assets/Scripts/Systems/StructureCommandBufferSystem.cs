using Unity.Entities;
using Unity.Transforms;

namespace Systems
{
    public class StructureCommandBufferSystem : SystemBase
    {
        private EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _entityCommandBufferSystem = World.GetExistingSystem<EndInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var commands = _entityCommandBufferSystem.CreateCommandBuffer();

            Dependency = Entities.ForEach((Entity entity, in Translation translation) =>
            {
                if (translation.Value.y < 0)
                {
                    commands.DestroyEntity(entity);
                }
            }).Schedule(Dependency);
            
            _entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}