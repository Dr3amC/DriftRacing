using System.Collections.Generic;
using UnityEngine;

namespace Core.Services
{
    public interface IPrefabProvider
    {
        void DeclarePrefabs(List<GameObject> referencedPrefabs);
        
        void PreparePrefabs(GameObjectConversionSystem conversionSystem);
    }
}