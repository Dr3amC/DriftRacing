using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LoadingView : MonoBehaviour
    {
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Animator animator;

        private bool _isVisible;
        private static readonly int Hide = Animator.StringToHash("Hide");

        public IEnumerator Show()
        {
            while (!_isVisible)
            {
                yield return null;
            }
        }

        public void Stop()
        {
            animator.SetTrigger(Hide);
        }

        public void UpdateProgress(float progress)
        {
            if (progressSlider == null)
            {
                return;
            }
            
            progressSlider.value = progress;
        }

        public void ShowCompleted()
        {
            _isVisible = true;
        }

        public void HideCompleted()
        {
            Destroy(gameObject);
        }
    }
}