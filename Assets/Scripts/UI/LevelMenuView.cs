using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class LevelMenuView : MonoBehaviour
    {
        public event UnityAction MainMenu
        {
            add => mainMenuButton.onClick.AddListener(value);
            remove => mainMenuButton.onClick.RemoveListener(value);
        }
        
        [SerializeField] private Button mainMenuButton;
    }
}