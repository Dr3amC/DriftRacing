﻿using Components;
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
                in WheelContactVelocity contactVelocity, in WheelFriction friction) =>
            {
                output.FrictionImpulse = float3.zero;
                output.SuspensionImpulse = float3.zero;

                if (!contact.IsInContact)
                {
                    return;
                }
                //Suspension
                
                var suspensionDelta = wheel.SuspensionLength - contact.Distance;
                var suspensionForceValue = suspensionDelta * suspension.Stiffness * input.SuspensionMultiplier;

                var suspensionDampingSpeed = math.dot(input.Up, contactVelocity.Value);
                var suspensionDampingForce = suspensionDampingSpeed * suspension.Damping * input.SuspensionMultiplier;

                suspensionForceValue -= suspensionDampingForce;
                var suspensionForce = suspensionForceValue * contact.Normal;
                output.SuspensionImpulse = suspensionForce * deltaTime;
                
                //Friction
                if (suspensionForceValue > 0)
                {
                    var lateralDirection = math.rotate(input.WorldTransform.rot, math.right());
                    var longitudinalDirection = math.normalize(math.cross(lateralDirection, contact.Normal));
                    var lateralSpeed = math.dot(lateralDirection, contactVelocity.Value);
                    var longitudinalSpeed = math.dot(longitudinalDirection, contactVelocity.Value);
                    var lateralSpeedSign = math.sign(lateralSpeed);
                    var longitudinalSpeedSign = math.sign(longitudinalSpeed);
                    var lateralSpeedAbs = lateralSpeed * lateralSpeedSign;
                    var longitudinalSpeedAbs = longitudinalSpeed * longitudinalSpeedSign;
                    var lateralFrictionRate = friction.Lateral.Value.Evaluate(lateralSpeedAbs);
                    var longitudinalFrictionRate = friction.Longitudinal.Value.Evaluate(longitudinalSpeedAbs);
                
                    var lateralForce = (-lateralSpeedSign * lateralFrictionRate * suspensionForceValue 
                                        * Bias(math.saturate(lateralSpeedAbs), -1)) * lateralDirection;
                    var longitudinalForce = (-longitudinalSpeedSign * longitudinalFrictionRate * suspensionForceValue 
                                             * Bias(math.saturate(longitudinalSpeedAbs), -1)) * longitudinalDirection;
                    output.FrictionImpulse = (lateralForce + longitudinalForce) * deltaTime;
                }
            }).Schedule(Dependency);

            var dep = JobHandle.CombineDependencies(Dependency, _exportPhysicsWorld.GetOutputDependency());
            
            Dependency = Entities.ForEach((ref PhysicsVelocity velocity, in Vehicle vehicle, in PhysicsMass mass, in Translation translation,
                in Rotation rotation) =>
            {
                for (var i = 0; i < vehicle.Wheels.Length; i++)
                {
                    var wheelEntity = vehicle.Wheels[i];
                    var wheelOutput = GetComponent<WheelOutput>(wheelEntity);
                    var wheelContact = GetComponent<WheelContact>(wheelEntity);
                    
                    velocity.ApplyImpulse(mass, translation, rotation, wheelOutput.SuspensionImpulse, wheelContact.Point);
                }
            }).Schedule(dep);
        }
        
        private static float Bias(float x, float bias)
        {
            var k = math.pow(1 - bias, 3);
            return (x * k) / (x * k - x + 1);
        }
    }
}