using Components;
using Core.Services;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Zenject;

namespace Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
    public class VehicleInputSystem : SystemBase
    {
        private IInputService _input;

        protected override void OnUpdate()
        {
            var movementInput = _input.Player.Movement.ReadValue<float>();
            var steeringInput = _input.Player.Steering.ReadValue<float>();
            var handbrakeInput = _input.Player.Handbrake.ReadValue<float>();
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

        [Inject]
        private void Inject(IInputService inputService)
        {
            _input = inputService;
        }
    }
}