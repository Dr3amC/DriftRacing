using Shared;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;
using Collider = Unity.Physics.Collider;

namespace Components
{
    public class WheelAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Header("Collider")]
        public float radius;
        public float width;
        public PhysicsCategoryTags belongsTo;
        public PhysicsCategoryTags collidesWith;

        [Header("Suspension")] 
        public float suspensionLength;
        public float stiffness;
        public float damping;

        [Header("Friction")]
        public AnimationCurve longitudinal;
        public AnimationCurve lateral;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponents(entity, new ComponentTypes(new ComponentType[]
            {
                typeof(Wheel),
                typeof(WheelOrigin),
                typeof(WheelInput),
                typeof(WheelContact),
                typeof(WheelSuspension),
                typeof(WheelContactVelocity),
                typeof(WheelOutput),
                typeof(WheelFriction)
            }));
            
            var wheelCollider = CylinderCollider.Create(new CylinderGeometry
            {
                Center = float3.zero,
                Height = width,
                Radius = radius,
                BevelRadius = 0.1f,
                SideCount = 12,
                Orientation = quaternion.AxisAngle(math.up(), math.PI * 0.5f)
            }, new CollisionFilter
            {
                BelongsTo = belongsTo.Value,
                CollidesWith = collidesWith.Value
            });

            conversionSystem.BlobAssetStore.AddUniqueBlobAsset(ref wheelCollider);
            
            dstManager.SetComponentData(entity, new Wheel
            {
                Radius = radius,
                Width = width,
                SuspensionLength = suspensionLength,
                Collider = wheelCollider
            });
            
            dstManager.SetComponentData(entity, new WheelOrigin
            {
                Value = new RigidTransform(transform.localRotation, transform.localPosition)
            });
            
            dstManager.SetComponentData(entity, new WheelSuspension
            {
                Stiffness = stiffness,
                Damping = damping
            });

            var longitud = AnimationCurveBlob.Build(longitudinal, 128, Allocator.Persistent);
            var later = AnimationCurveBlob.Build(lateral, 128, Allocator.Persistent);
            
            conversionSystem.BlobAssetStore.AddUniqueBlobAsset(ref longitud);
            conversionSystem.BlobAssetStore.AddUniqueBlobAsset(ref later);
            
            dstManager.SetComponentData(entity, new WheelFriction
            {
                Longitudinal = longitud,
                Lateral = later
            });
        }
    }

    public struct Wheel : IComponentData
    {
        public float Radius;
        public float Width;
        public float SuspensionLength;
        public BlobAssetReference<Collider> Collider;
    }

    public struct WheelOrigin : IComponentData
    {
        public RigidTransform Value;
    }

    public struct WheelInput : IComponentData
    {
        public float3 Up;
        public RigidTransform WorldTransform;
        public float SuspensionMultiplier;
    }

    public struct WheelContact : IComponentData
    {
        public bool IsInContact;
        public float3 Point;
        public float3 Normal;
        public float Distance;
    }

    public struct WheelSuspension : IComponentData
    {
        public float Stiffness;
        public float Damping;
    }

    public struct WheelContactVelocity : IComponentData
    {
        public float3 Value;
    }

    public struct WheelOutput : IComponentData
    {
        public float3 SuspensionImpulse;
        public float3 FrictionImpulse;
    }

    public struct WheelFriction : IComponentData
    {
        public BlobAssetReference<AnimationCurveBlob> Longitudinal;
        public BlobAssetReference<AnimationCurveBlob> Lateral;
    }
}