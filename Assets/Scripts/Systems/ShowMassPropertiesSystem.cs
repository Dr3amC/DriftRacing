using Components.Utils;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

namespace Systems
{
    public class ShowMassPropertiesSystem : SystemBase
    {
        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            Entities.WithAll<ShowMassProperties>().ForEach((in PhysicsCollider collider) =>
            {
                var massProperties = collider.MassProperties;
                Debug.Log($"Position {massProperties.MassDistribution.Transform.pos}");
                Debug.Log($"Rotation {((Quaternion)massProperties.MassDistribution.Transform.rot).eulerAngles}");
                Debug.Log($"InertiaTensor {massProperties.MassDistribution.InertiaTensor}");
                Debug.Log("-------------");
            }).WithoutBurst().Run();
        }

        protected override void OnUpdate()
        {
            
        }
    }
}