#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Common.ServiceLocator
{
    public abstract class StandaloneSingleton<T> : StandaloneSingleton where T : class, new()
    {
        private static T _instance;
        private static bool _bInitialising;
        private static readonly object _instanceLock = new();

        public static T Instance
        {
            get
            {
                lock (_instanceLock)
                {
                    if (_instance != null)
                    {
                        return _instance;
                    }

                    _bInitialising = true;

                    _instance = new T();
                    (_instance as StandaloneSingleton).Initialise();

                    _bInitialising = false;
                    return _instance;
                }
            }
        }

        private static void ConstructIfNeeded(StandaloneSingleton<T> inInstance)
        {
            lock (_instanceLock)
            {
                // only construct if the instance is null and is not being initialised
                if (_instance == null && !_bInitialising)
                {
                    _instance = inInstance as T;
                }
                else if (_instance != null && !_bInitialising)
                {
                    Debug.LogError($"Found duplicate {typeof(T)}");                    
                }
            }
        }

        internal override void Initialise()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif

            OnInitialise();
        }

        protected virtual void OnInitialise()
        {
        }

#if UNITY_EDITOR
        protected virtual void OnPlayInEditorStopped()
        {
        }

        void OnPlayModeChanged(PlayModeStateChange inChange)
        {
            if ((inChange == PlayModeStateChange.ExitingPlayMode) && (_instance != null))
            {
                EditorApplication.playModeStateChanged -= OnPlayModeChanged;
                OnPlayInEditorStopped();
                _instance = null;
            }
        }
#endif
    }

    public abstract class StandaloneSingleton
    {
        internal abstract void Initialise();

        public virtual void OnBootstrapped()
        {
        }
    }
}
