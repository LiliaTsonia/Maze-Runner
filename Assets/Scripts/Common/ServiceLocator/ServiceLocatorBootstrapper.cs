using UnityEngine;

namespace Common.ServiceLocator
{
    public static class ServiceLocatorBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void Initialise() 
        { 
            if (ServiceLocator.Instance != null)
            {
                ServiceLocator.Instance.OnBootstrapped();
            }
        }
    }
}