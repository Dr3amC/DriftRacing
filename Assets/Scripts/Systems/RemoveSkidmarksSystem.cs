using Events;
using Unity.Entities;
using Unity.Rendering;

namespace Systems
{
    public class RemoveSkidmarksSystem : EventBufferSystem<RemoveSkidmarksSystem.Request>
    {
        protected override void Handle(Request request)
        {
            var renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(request.Entity);
            UnityEngine.Object.Destroy(renderMesh.mesh);
            EntityManager.DestroyEntity(request.Entity);
        }
        
        public struct Request
        {
            public Entity Entity;
        }
    }
}