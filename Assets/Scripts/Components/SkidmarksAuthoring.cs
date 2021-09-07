using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Components
{
    public class SkidmarksAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int capacity = 512;
        public float lifetime;
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponents(entity, new ComponentTypes(
                typeof(Skidmarks),
                typeof(SkidmarksPoint),
                typeof(SkidmarksSequence)
                ));

            var points = dstManager.GetBuffer<SkidmarksPoint>(entity);
            points.Capacity = capacity;
            dstManager.SetComponentData(entity, new Skidmarks
            {
                Lifetime = lifetime,
                Capacity = capacity
            });
        }
    }

    public struct Skidmarks : IComponentData
    {
        public float Lifetime;
        public int Capacity;
    }

    public struct SkidmarksPoint : IBufferElementData
    {
        public float3 Position;
        public float3 Normal;
        public float3 Right;
        public float Width;
        public float Intensity;
        public float CreationTime;
    }

    public struct SkidmarksSequence : IBufferElementData
    {
        public int End;
    }
}