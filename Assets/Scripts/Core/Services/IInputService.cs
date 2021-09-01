using Settings;

namespace Core.Services
{
    public interface IInputService
    {
        public Controls.PlayerActions Player { get; }
    }
}