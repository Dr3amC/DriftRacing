﻿using Components.Effects;
using Unity.Entities;
using UnityEngine;

namespace Components
{
    public class WheelSkidmarksAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponents(entity, new ComponentTypes(
                typeof(Surface)
            ));
        }
    }

    public struct WheelSkidmarks : IComponentData
    {
        public int SurfaceIndex;
        public Entity ActiveSkidmarks;
    }
}