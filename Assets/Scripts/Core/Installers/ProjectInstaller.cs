using Core.Services.Implementation;
using UnityEngine;
using Zenject;

namespace Core.Installers
{
    public class ProjectInstaller : MonoInstaller
    {
        [SerializeField] private Database database;
        
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<InputService>()
                .AsSingle()
                .Lazy();

            Container.BindInterfacesTo<GameService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesTo<Database>()
                .FromInstance(database)
                .AsSingle();
        }
    }
}