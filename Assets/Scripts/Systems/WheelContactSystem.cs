using Components;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Collider = Unity.Physics.Collider;

namespace Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(VehicleToWheelsSystem))]
    [UpdateAfter(typeof(StepPhysicsWorld))]
    public class WheelContactSystem : SystemBase
    {
        private StepPhysicsWorld _stepPhysicsWorld;
        private BuildPhysicsWorld _buildPhysicsWorld;

        protected override void OnCreate()
        {
            base.OnCreate();
            _stepPhysicsWorld = World.GetExistingSystem<StepPhysicsWorld>();
            _buildPhysicsWorld = World.GetExistingSystem<BuildPhysicsWorld>();
        }

        protected override unsafe void OnUpdate()
        {
            var dep = JobHandle.CombineDependencies(Dependency, _stepPhysicsWorld.GetOutputDependency());
            var physicsWorld = _buildPhysicsWorld.PhysicsWorld;
            
            Dependency = Entities.WithReadOnly(physicsWorld).ForEach((ref WheelContact contact, in Wheel wheel, in WheelInput input) =>
            {
                var colliderCastInput = new ColliderCastInput
                {
                    Collider = (Collider*)wheel.Collider.GetUnsafePtr(),
                    Start = input.WorldTransform.pos,
                    End = input.WorldTransform.pos - input.Up * wheel.SuspensionLength,
                    Orientation = input.WorldTransform.rot
                };

                if (!physicsWorld.CastCollider(colliderCastInput, out var hit))
                {
                    contact.IsInContact = false;
                    contact.Distance = wheel.SuspensionLength;
                    return;
                }

                contact.IsInContact = true;
                contact.Point = hit.Position;
                contact.Normal = hit.SurfaceNormal;
                contact.Distance = hit.Fraction * wheel.SuspensionLength;
            }).Schedule(dep);
            
            _buildPhysicsWorld.AddInputDependencyToComplete(Dependency);
        }
    }
}