#if UNITY_EDITOR
using Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;

namespace Systems.Utils
{
    public class VehicleDebugSystem : SystemBase
    {
        private static readonly float _updateInterval = 0.05f;
        private VehicleDebugWindow _vehicleDebugWindow;
        private float _timeSincePreviousUpdate;

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            _vehicleDebugWindow = EditorWindow.GetWindow<VehicleDebugWindow>();
        }

        protected override void OnUpdate()
        {
            _timeSincePreviousUpdate += Time.DeltaTime;
            if (_timeSincePreviousUpdate >= _updateInterval)
            {
                _timeSincePreviousUpdate %= _updateInterval;
                Entities.ForEach((in Vehicle vehicle) =>
                {
                    for (var i = 0; i < vehicle.Wheels.Length; i++)
                    {
                        var wheelEntity = vehicle.Wheels[i];
                        var wheelInput = GetComponent<WheelInput>(wheelEntity);
                        var wheelOutput = GetComponent<WheelOutput>(wheelEntity);
                    
                        _vehicleDebugWindow.SetWheel(i, new VehicleDebugWindow.WheelData
                        {
                            Impulse = wheelOutput.FrictionImpulse,
                            Torque = wheelInput.Torque,
                            RotationSpeed = wheelOutput.RotationSpeed,
                            SuspensionImpulse = math.length(wheelOutput.SuspensionImpulse),
                            Slip = wheelOutput.Slip
                        });
                    }
                }).WithoutBurst().Run();
                _vehicleDebugWindow.Repaint();
            }
        }
    }
}
#endif