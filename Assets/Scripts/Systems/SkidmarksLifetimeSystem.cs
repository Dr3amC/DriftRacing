using Components;
using Components.Shared;
using Unity.Entities;
using Unity.Rendering;

namespace Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(WheelSkidmarksSystem))]
    public class SkidmarksLifetimeSystem : SystemBase
    {
        private RemoveSkidmarksSystem _removeSkidmarksSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _removeSkidmarksSystem = World.GetOrCreateSystem<RemoveSkidmarksSystem>();
        }

        protected override void OnUpdate()
        {
            var request = _removeSkidmarksSystem.CreateBuffer();
            var deltaTime = Time.DeltaTime;

            Entities.ForEach((Entity entity, ref Lifetime lifetime) =>
            {
                lifetime.Time -= deltaTime;

                if (lifetime.Time < 0)
                {
                    request.Enqueue(new RemoveSkidmarksSystem.Request
                    {
                        Entity = entity
                    });
                }
            }).Schedule();
            
            _removeSkidmarksSystem.AddProducerJob(Dependency);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Entities.ForEach((in Skidmarks skidmarks, in RenderMesh renderMesh) =>
            {
                UnityEngine.Object.Destroy(renderMesh.mesh);
            }).WithoutBurst().Run();
        }
    }
}