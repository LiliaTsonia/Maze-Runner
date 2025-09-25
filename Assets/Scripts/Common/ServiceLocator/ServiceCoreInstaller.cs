using Common.EventsSystem;
using Common.SaveSystem;
using Common.SceneManagement;
using Game.UI.PopUp;
using UnityEngine;

namespace Common.ServiceLocator
{
    public class ServiceCoreInstaller : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);
            InstallBindings();
        }

        private void InstallBindings()
        {
            ServiceLocator.RegisterService<EventManager>(new EventManager());
            ServiceLocator.RegisterService<LevelHandler>(new LevelHandler());
            ServiceLocator.RegisterService<SceneLoader>(new SceneLoader());
            ServiceLocator.RegisterService<PopUpManager>(new PopUpManager());
        }
    }
}