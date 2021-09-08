using System.Collections.Generic;
using Data;
using Unity.Entities;
using UnityEngine;

namespace Core.Services.Implementation
{
    [CreateAssetMenu(menuName = "Database/Surfaces")]
    public class SurfaceService : ScriptableObject, ISurfaceService, IPrefabProvider
    {
        [SerializeField]
        private SurfaceDefinition[] surfaces;
        
        public SurfaceDefinition[] Surfaces => surfaces;
        
        private SurfaceData[] surfaceData;
        
        private struct SurfaceData
        {
            public Entity SkidmarksPrefab;
        }
        
        public void DeclarePrefabs(List<GameObject> referencedPrefabs)
        {
            for (var index = 0; index < surfaces.Length; index++)
            {
                var surfaceDefinition = surfaces[index];
                surfaceDefinition.RuntimeIndex = index;
                if (surfaceDefinition.Skidmarks != null) referencedPrefabs.Add(surfaceDefinition.Skidmarks);
            }
        }
        
        public void PreparePrefabs(GameObjectConversionSystem conversionSystem)
        {
            surfaceData = new SurfaceData[surfaces.Length];
            for (var index = 0; index < surfaces.Length; index++)
            {
                var surfaceDefinition = surfaces[index];
                if (surfaceDefinition.Skidmarks != null)
                {
                    surfaceData[surfaceDefinition.RuntimeIndex].SkidmarksPrefab =
                        conversionSystem.GetPrimaryEntity(surfaceDefinition.Skidmarks);
                }
                else
                {
                    surfaceData[surfaceDefinition.RuntimeIndex].SkidmarksPrefab = Entity.Null;
                }
            }
        }
        
        public Entity GetSkidmarksPrefab(int surfaceIndex)
        {
            return surfaceData[surfaceIndex].SkidmarksPrefab;
        }
    }
}