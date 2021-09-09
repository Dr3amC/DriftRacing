using Components;
using Components.Effects;
using Unity.Entities;

namespace Systems
{
    public class WheelSkidmarksCheckSystem : SystemBase
    {
        private SwitchSkidmarksSystem _switchSkidmarksSystem;
        
        protected override void OnCreate()
        {
            base.OnCreate();
            _switchSkidmarksSystem = World.GetOrCreateSystem<SwitchSkidmarksSystem>();
        }

        protected override void OnUpdate()
        {
            var switchRequests = _switchSkidmarksSystem.CreateBuffer();
            Dependency = Entities.WithAll<Wheel>().ForEach((Entity entity, in Surface surface) =>
            {
                var switchSkidmarks = false;

                if (HasComponent<WheelSkidmarks>(entity))
                {
                    var wheelSkidmarks = GetComponent<WheelSkidmarks>(entity);
                    if (wheelSkidmarks.SurfaceIndex == surface.SurfaceIndex)
                    {
                        var skidmarks = GetComponent<Skidmarks>(wheelSkidmarks.ActiveSkidmarks);
                        var buffer = GetBuffer<SkidmarksPoint>(wheelSkidmarks.ActiveSkidmarks);

                        if (buffer.Length >= skidmarks.Capacity)
                        {
                            switchSkidmarks = true;
                        }
                        else if (surface.SurfaceIndex >= 0)
                        {
                            switchSkidmarks = true;
                        }
                    }
                }
                else
                {
                    switchSkidmarks = surface.SurfaceIndex >= 0;
                }

                if (switchSkidmarks)
                {
                    switchRequests.Enqueue(new SwitchSkidmarksSystem.Request
                    {
                        Wheel = entity,
                        SurfaceIndex = surface.SurfaceIndex
                    });
                }
            }).Schedule(Dependency);
            
            _switchSkidmarksSystem.AddProducerJob(Dependency);
        }
    }
}