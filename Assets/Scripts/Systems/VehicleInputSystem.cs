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

            Entities.ForEach((ref VehicleInput vehicleInput) =>
            {
                var throttleInput = math.clamp(movementInput, 0, 1);
                var brakeInput = math.clamp(movementInput, -1, 0);
                vehicleInput.Steering = Mathf.MoveTowards(vehicleInput.Steering, steeringInput, deltaTime * 4);
                vehicleInput.Throttle = Mathf.MoveTowards(vehicleInput.Throttle, throttleInput, deltaTime * 4);
                vehicleInput.Break = Mathf.MoveTowards(vehicleInput.Break, brakeInput, deltaTime * 4);
                vehicleInput.Handbrake = Mathf.MoveTowards(vehicleInput.Handbrake, handbrakeInput, deltaTime * 4);
            }).Schedule();
        }
    }
}