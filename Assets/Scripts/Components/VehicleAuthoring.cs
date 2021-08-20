using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Components
{
    public class VehicleAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var vehicle = new Vehicle();

            foreach (var wheelAuthoring in GetComponentsInChildren<WheelAuthoring>(true))
            {
                vehicle.Wheels.Add(conversionSystem.GetPrimaryEntity(wheelAuthoring));
            }
            
            dstManager.AddComponentData(entity, vehicle);
        }
    }

    public struct Vehicle : IComponentData
    {
        public FixedList64<Entity> Wheels;
    }
}