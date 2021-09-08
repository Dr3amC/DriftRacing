using Data;
using Unity.Entities;

namespace Core.Services
{
    public interface ISurfaceService
    {
        SurfaceDefinition[] Surfaces { get; }
        
        Entity GetSkidmarksPrefab(int index);
    }
}