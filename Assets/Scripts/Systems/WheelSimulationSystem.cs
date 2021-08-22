using Components;
using Extensions;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(ExportPhysicsWorld))]
    public class WheelSimulationSystem : SystemBase
    {
        private ExportPhysicsWorld _exportPhysicsWorld;

        protected override void OnCreate()
        {
            base.OnCreate();
            _exportPhysicsWorld = World.GetExistingSystem<ExportPhysicsWorld>();
        }

        protected override void OnUpdate()
        {
            Dependency = Entities.ForEach((in Vehicle vehicle, in PhysicsVelocity velocity, in PhysicsMass mass,
                in Translation translation, in Rotation rotation) =>
            {
                for (var i = 0; i < vehicle.Wheels.Length; i++)
                {
                    var wheelEntity = vehicle.Wheels[i];
                    var wheelContact = GetComponent<WheelContact>(wheelEntity);

                    if (wheelContact.IsInContact)
                    {
                        var contactVelocity =
                            velocity.GetLinearVelocity(mass, translation, rotation, wheelContact.Point);
                        SetComponent(wheelEntity, new WheelContactVelocity
                        {
                            Value = contactVelocity
                        });
                    }
                }
            }).Schedule(Dependency);

            var deltaTime = Time.DeltaTime;
            
            Dependency = Entities.ForEach((ref WheelOutput output, in Wheel wheel, in WheelContact contact, in WheelInput input, in WheelSuspension suspension, 
                in WheelContactVelocity contactVelocity, in WheelFriction friction, in WheelBrakes brakes) =>
            {
                output.FrictionImpulse = float3.zero;
                output.SuspensionImpulse = float3.zero;

                var brakeTorque = (brakes.BrakeTorque * input.Brake + brakes.HandbrakeTorque * input.Handbrake) *
                                  input.MassMultiplier;
                var engineTorque = input.Torque;

                if (brakeTorque > 0)
                {
                    var torqueAbs = math.abs(input.Torque);

                    if (torqueAbs < brakeTorque)
                    {
                        brakeTorque -= torqueAbs;
                        engineTorque = 0f;
                    }
                    else
                    {
                        engineTorque -= brakeTorque * math.sign(engineTorque);
                        brakeTorque = 0f;
                    }
                }

                output.RotationSpeed += (engineTorque / wheel.Inertia) * deltaTime;

                if (!contact.IsInContact)
                {
                    ApplyBrakeTorque(ref output, wheel.Inertia, brakeTorque, deltaTime);
                    output.Rotation += output.RotationSpeed * deltaTime;
                    return;
                }
                //Suspension
                
                var suspensionDelta = wheel.SuspensionLength - contact.Distance;
                var suspensionForceValue = suspensionDelta * suspension.Stiffness * input.MassMultiplier;

                var suspensionDampingSpeed = math.dot(input.Up, contactVelocity.Value);
                var suspensionDampingForce = suspensionDampingSpeed * suspension.Damping * input.MassMultiplier;

                suspensionForceValue -= suspensionDampingForce;
                if (suspensionForceValue < 0)
                {
                    suspensionForceValue = 0;
                }
                
                var suspensionForce = suspensionForceValue * contact.Normal;
                output.SuspensionImpulse = suspensionForce * deltaTime;
                
                //Friction
                if (suspensionForceValue > 0)
                {
                    var lateralDirection = math.rotate(input.WorldTransform.rot, math.right());
                    var longitudinalDirection = math.normalizesafe(math.cross(lateralDirection, contact.Normal));

                    var lateralSpeed = math.dot(lateralDirection, contactVelocity.Value);
                    var longitudinalSpeed = math.dot(longitudinalDirection, contactVelocity.Value);

                    var wheelDeltaSpeed = longitudinalSpeed - output.RotationSpeed.RotationToLinearSpeed(wheel.Radius);
                    var wheelDeltaSpeedSign = math.sign(wheelDeltaSpeed);
                    var wheelDeltaSpeedAbs = wheelDeltaSpeedSign * wheelDeltaSpeed;

                    var lateralSpeedSign = math.sign(lateralSpeed);
                    var lateralSpeedAbs = lateralSpeedSign * lateralSpeed;

                    var longitudinalTimeRange = friction.Longitudinal.Value.TimeRange;
                    var longitudinalSlip = math.saturate(math.unlerp(longitudinalTimeRange.x,
                        longitudinalTimeRange.y, wheelDeltaSpeedAbs));
                    var lateralTimeRange = friction.Lateral.Value.TimeRange;
                    var lateralSlip = math.saturate(math.unlerp(lateralTimeRange.x,
                        lateralTimeRange.y, lateralSpeedAbs));

                    var combinedSlip = math.max(longitudinalSlip, lateralSlip);
                    
                    var lateralFrictionRate = friction.Lateral.Value.Evaluate(
                        math.lerp(lateralTimeRange.x, lateralTimeRange.y, combinedSlip));
                    var longitudinalFrictionRate = friction.Longitudinal.Value.Evaluate(
                        math.lerp(longitudinalTimeRange.x, longitudinalTimeRange.y, combinedSlip));

                    var lateralForce = (-lateralSpeedSign * lateralFrictionRate * suspensionForceValue 
                                        * Bias(math.saturate(lateralSpeedAbs), -1)) * lateralDirection;

                    var longitudinalBias = Bias(math.saturate(wheelDeltaSpeedAbs), -1);

                    var longitudinalFrictionForceValue = -wheelDeltaSpeedSign * longitudinalFrictionRate *
                                                         longitudinalBias * suspensionForceValue;


                    var toNeutralForce =
                        (-wheelDeltaSpeed.LinearToRotationSpeed(wheel.Radius) * wheel.Inertia / deltaTime)
                        .TorqueToForce(wheel.Radius);

                    var usedForceValue = math.abs(toNeutralForce) > math.abs(longitudinalFrictionForceValue)
                        ? longitudinalFrictionForceValue
                        : toNeutralForce;
                    output.RotationSpeed -= usedForceValue.ForceToTorque(wheel.Radius) / wheel.Inertia * deltaTime;

                    ApplyBrakeTorque(ref output, wheel.Inertia, brakeTorque, deltaTime);

                    output.Rotation += output.RotationSpeed * deltaTime;
                    
                    var longitudinalForce = longitudinalFrictionForceValue * longitudinalDirection;

                    output.FrictionImpulse = (lateralForce + longitudinalForce) * deltaTime;
                }
            }).Schedule(Dependency);

            var dep = JobHandle.CombineDependencies(Dependency, _exportPhysicsWorld.GetOutputDependency());
            
            Dependency = Entities.ForEach((ref PhysicsVelocity velocity, ref VehicleOutput output, in Vehicle vehicle, in PhysicsMass mass, in Translation translation,
                in Rotation rotation) =>
            {
                output.MaxWheelRotationSpeed = 0.0f;
                
                for (var i = 0; i < vehicle.Wheels.Length; i++)
                {
                    var wheelEntity = vehicle.Wheels[i];
                    var wheelOutput = GetComponent<WheelOutput>(wheelEntity);
                    var wheelContact = GetComponent<WheelContact>(wheelEntity);
                    var wheelControllable = GetComponent<WheelControllable>(wheelEntity);

                    if (wheelContact.IsInContact)
                    {
                        velocity.ApplyImpulse(mass, translation, rotation, wheelOutput.SuspensionImpulse + wheelOutput.FrictionImpulse, wheelContact.Point);
                    }

                    if (wheelControllable.DriveRate > 0)
                    {
                        output.MaxWheelRotationSpeed =
                            math.max(output.MaxWheelRotationSpeed, wheelOutput.RotationSpeed);
                    }

                    output.LocalVelocity = math.rotate(math.inverse(rotation.Value), velocity.Linear);
                }
            }).Schedule(dep);
        }

        private static void ApplyBrakeTorque(ref WheelOutput output, float inertia, float brakeTorque, float deltaTime)
        {
            if (brakeTorque <= 0)
            {
                return;
            }
            var toZeroTorque = -output.RotationSpeed * inertia / deltaTime;
            var toZeroTorqueAbs = math.abs(toZeroTorque);

            var usedBrakeTorque = toZeroTorqueAbs < brakeTorque ? toZeroTorqueAbs : brakeTorque;

            output.RotationSpeed += math.sign(toZeroTorque) * usedBrakeTorque / inertia * deltaTime;
        }
        
        private static float Bias(float x, float bias)
        {
            var k = math.pow(1 - bias, 3);
            return (x * k) / (x * k - x + 1);
        }
    }
}