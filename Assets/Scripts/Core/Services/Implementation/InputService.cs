using System;
using Settings;

namespace Core.Services.Implementation
{
    public class InputService : IInputService, IDisposable
    {
        public Controls.PlayerActions Player => _controls.Player;
        
        private readonly Controls _controls;

        public InputService()
        {
            _controls = new Controls();
            _controls.Enable();
        }

        public void Dispose()
        {
            _controls?.Dispose();
        }
    }
}