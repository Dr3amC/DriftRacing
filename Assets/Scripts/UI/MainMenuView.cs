using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuView : MonoBehaviour
    {
        public event UnityAction Play
        {
            add => playButton.onClick.AddListener(value);
            remove => playButton.onClick.RemoveListener(value);
        }
        
        [SerializeField] private Button playButton;
    }
}