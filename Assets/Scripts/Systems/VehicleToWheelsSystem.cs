using Components;
using Extensions;
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
            Entities.ForEach((in Vehicle vehicle, in Translation translation, in Rotation rotation, in PhysicsMass mass,
                in VehicleInput input, in VehicleEngine engine, in VehicleOutput output) =>
            {
                var engineTorque = engine.EvaluateTorque(engine.TransmitWheelRotationSpeedToEngineRpm(output.MaxWheelRotationSpeed)) * input.Throttle;
                var wheelsTorque = engine.TransmitEngineToTorqueToWheelTorque(engineTorque);
                
                for (int i = 0; i < vehicle.Wheels.Length; i++)
                {
                    var wheelEntity = vehicle.Wheels[i];
                    var origin = GetComponent<WheelOrigin>(wheelEntity);
                    var controllable = GetComponent<WheelControllable>(wheelEntity);

                    var localTransform = origin.Value;
                    localTransform.rot = math.mul(localTransform.rot,
                        quaternion.AxisAngle(math.up(), input.Steering * controllable.MaxSteerAngle));
                    
                    var wheelTransform = math.mul(new RigidTransform(rotation.Value, translation.Value), origin.Value);
                    
                    var wheelInput = new WheelInput
                    {
                        LocalTransform = localTransform,
                        WorldTransform = wheelTransform,
                        Up = math.rotate(wheelTransform.rot, math.up()),
                        SuspensionMultiplier = 1.0f / mass.InverseMass,
                        Torque = wheelsTorque * controllable.DriveRate
                    };
                    SetComponent(wheelEntity, wheelInput);
                }
            }).Schedule();
        }
    }
}