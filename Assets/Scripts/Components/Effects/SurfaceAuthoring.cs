using Data;
using Unity.Entities;
using UnityEngine;

namespace Components.Effects
{
    public class SurfaceAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public SurfaceDefinition surface;
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Surface
            {
                SurfaceIndex = surface.RuntimeIndex
            });
        }
    }
    
    public struct Surface : IComponentData
    {
        public int SurfaceIndex;
    }
}