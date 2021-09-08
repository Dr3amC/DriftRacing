using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Database/Surface")]
    public class SurfaceDefinition : ScriptableObject
    {
        public int RuntimeIndex;
        public GameObject Skidmarks;
    }
}