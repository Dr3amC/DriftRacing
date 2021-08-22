using Components;
using Settings;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
    public class VehicleInputSystem : SystemBase
    {
        private Controls _controls;

        protected override void OnCreate()
        {
            base.OnCreate();
            _controls = new Controls();
            _controls.Enable();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _controls.Dispose();
        }

        protected override void OnUpdate()
        {
            var movementInput = _controls.Player.Movement.ReadValue<float>();
            var steeringInput = _controls.Player.Steering.ReadValue<float>();
            var handbrakeInput = _controls.Player.Handbrake.ReadValue<float>();
            var deltaTime = Time.DeltaTime;

            Entities.ForEach((ref VehicleInput vehicleInput, in VehicleOutput output) =>
            {
                var throttleInput = movementInput;
                var brakeInput = 0f;

                switch (vehicleInput.ThrottleMode)
                {
                    case ThrottleMode.AccelerationForward:
                        if (throttleInput < 0)
                        {
                            vehicleInput.ThrottleMode = ThrottleMode.Braking;
                        }
                        break;
                    case ThrottleMode.AccelerationBackward:
                        if (throttleInput > 0)
                        {
                            vehicleInput.ThrottleMode = ThrottleMode.Braking;
                        }
                        break;
                    case ThrottleMode.Braking:
                        if (output.LocalVelocity.z * movementInput > 0 || math.abs(output.LocalVelocity.z) < 0.1f)
                        {
                            vehicleInput.ThrottleMode = movementInput > 0
                                ? ThrottleMode.AccelerationForward
                                : ThrottleMode.AccelerationBackward;
                        }

                        throttleInput = 0f;
                        brakeInput = math.abs(movementInput);
                        break;
                }
                
                vehicleInput.Steering = Mathf.MoveTowards(vehicleInput.Steering, steeringInput, deltaTime * 4);
                vehicleInput.Throttle = Mathf.MoveTowards(vehicleInput.Throttle, throttleInput, deltaTime * 4);
                vehicleInput.Brake = Mathf.MoveTowards(vehicleInput.Brake, brakeInput, deltaTime * 4);
                vehicleInput.Handbrake = Mathf.MoveTowards(vehicleInput.Handbrake, handbrakeInput, deltaTime * 10);
                
            }).Schedule();
        }
    }
}