using Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public class WheelsTransformSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref Translation translation, ref Rotation rotation, in WheelOrigin origin,
                in WheelContact contact) =>
            {
                translation.Value = origin.Value.pos - math.rotate(origin.Value.rot, math.up()) * contact.Distance;
                rotation.Value = origin.Value.rot;
            }).Schedule();
        }
    }
}