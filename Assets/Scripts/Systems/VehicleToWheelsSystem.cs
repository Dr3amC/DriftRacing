using Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public class VehicleToWheelsSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((in Vehicle vehicle, in Translation translation, in Rotation rotation, in PhysicsMass mass) =>
            {
                for (int i = 0; i < vehicle.Wheels.Length; i++)
                {
                    var wheelEntity = vehicle.Wheels[i];
                    var origin = GetComponent<WheelOrigin>(wheelEntity);

                    var wheelTransform = math.mul(new RigidTransform(rotation.Value, translation.Value), origin.Value);
                    
                    var wheelInput = new WheelInput
                    {
                        WorldTransform = wheelTransform,
                        Up = math.rotate(wheelTransform.rot, math.up()),
                        SuspensionMultiplier = 1.0f / mass.InverseMass
                    };
                    SetComponent(wheelEntity, wheelInput);
                }
            }).Schedule();
        }
    }
}