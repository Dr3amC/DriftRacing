using Core.Services.Implementation;
using UnityEngine;
using Zenject;

namespace Core.Installers
{
    public class LevelSceneInstaller : MonoInstaller
    {
        [SerializeField] private SurfaceService surfaces;
        
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SurfaceService>()
                .FromInstance(surfaces)
                .AsSingle();
        }
    }
}