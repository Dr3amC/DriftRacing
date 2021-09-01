using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.States
{
    public class LoadSceneStateBase
    {
        protected async UniTask LoadScene(string sceneName)
        {
            var loadingView = Object.Instantiate((LoadingView) await Resources.LoadAsync<LoadingView>("LoadingView"));
            Object.DontDestroyOnLoad(loadingView);

            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single)
                .ToUniTask(Progress.CreateOnlyValueChanged(progress => loadingView.UpdateProgress(progress), EqualityComparer<float>.Default));

            loadingView.Stop();
        }
    }
}