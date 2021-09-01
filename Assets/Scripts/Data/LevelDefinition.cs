using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Database/Level")]
    public class LevelDefinition : ScriptableObject
    {
        public string Name;
        public string Scene;
    }
}