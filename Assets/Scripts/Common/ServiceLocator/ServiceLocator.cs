using System;
using System.Collections.Generic;

namespace Common.ServiceLocator
{
    public class ServiceLocator : StandaloneSingleton<ServiceLocator>
    {
        private Dictionary<ServiceAddress, ILocatableService> _serviceRegistry;
        private List<ServiceQuery> _pendingQueries;
        
        protected override void OnInitialise()
        {
            base.OnInitialise();

            _serviceRegistry = new();
            _pendingQueries = new();
        }

#if UNITY_EDITOR
        protected override void OnPlayInEditorStopped()
        {
            base.OnPlayInEditorStopped();

            _serviceRegistry = null;
            _pendingQueries = null;
        }
#endif
        
        public static void RegisterService<T>(T inService, Object inContext = null) where T : class, ILocatableService
        {
            if (Instance != null)
            {
                Instance.RegisterServiceInternal(inService, inContext);
            }
        }
        
        public static T LocateService<T>(object InContext = null, EServiceSearchMode InSearchMode = EServiceSearchMode.GlobalFirst)
            where T : class, ILocatableService
        {
            if (Instance != null)
            {
                return Instance.LocateServiceInternal<T>(InContext, InSearchMode);
            }

            return null;
        }
        
        private void RegisterServiceInternal<T>(T inService, Object inContext = null) where T : class, ILocatableService
        {
            var address = FindOrCreateAddress<T>(inService, inContext);
            _serviceRegistry[address] = inService;
            AttemptToFlushPendingQueries();
        }

        private T LocateServiceInternal<T>(object InContext, EServiceSearchMode InSearchMode) where T : class, ILocatableService
        {
            return AttemptToFindService(typeof(T), InContext, InSearchMode) as T;
        }

        
        // public static void AsyncLocateService<T>(Action<ILocatableService> inCallback, Object inContext = null, 
        //     EServiceSearchMode InSearchMode = EServiceSearchMode.GlobalFirst) where T : class, ILocatableService
        // {
        //     if (Instance != null)
        //     {
        //         Instance.AsyncLocateServiceInternal<T>(inCallback, inContext, InSearchMode);
        //     }
        // }
        
        // private void AsyncLocateServiceInternal<T>(Action<ILocatableService> inCallback, Object inContext,
        //     EServiceSearchMode inSearchMode) where T : class, ILocatableService
        // {
        //     // see if we already have the service
        //     var result = AttemptToFindService(typeof(T), inContext, inSearchMode);
        //     if (result != null)
        //     {
        //         inCallback(result);
        //         return;
        //     }
        //
        //     _pendingQueries.Add(new ServiceQuery
        //     { 
        //         Address = new ServiceAddress
        //         {
        //             ServiceType = typeof(T), 
        //             Context = inContext
        //         },
        //         CallbackFn = inCallback, 
        //         SearchMode = inSearchMode
        //         
        //     });
        // }
        
        private ServiceAddress FindOrCreateAddress<T>(T inService, Object inContext) where T : class, ILocatableService
        {
            foreach (var address in _serviceRegistry.Keys)
            {
                if ((address.Context == inContext) && (address.ServiceType == typeof(T)))
                {
                    return address;
                }
            }

            return new ServiceAddress
            {
                ServiceType = typeof(T), 
                Context = inContext
            };
        }
        
        private void AttemptToFlushPendingQueries()
        {
            for (var index = _pendingQueries.Count - 1; index >= 0; index--) 
            { 
                var query = _pendingQueries[index];

                var result = AttemptToFindService(query.Address.ServiceType, query.Address.Context, query.SearchMode);
                if (result != null) 
                { 
                    query.CallbackFn(result);
                    _pendingQueries.RemoveAt(index);
                }
            }
        }
        
        //TODO simplify the method
        ILocatableService AttemptToFindService(Type inType, Object inContext, EServiceSearchMode inSearchMode)
        {
            Func<Type, Object, ILocatableService> performSearch = (inType, inContext) =>
            {
                foreach(var registryEntry in _serviceRegistry)
                {
                    var address = registryEntry.Key;

                    if ((address.Context == inContext) && (address.ServiceType == inType))
                    {
                        return registryEntry.Value;
                    }
                }

                return null;
            };

            if ((inSearchMode == EServiceSearchMode.GlobalOnly) || (inSearchMode == EServiceSearchMode.GlobalFirst))
            {
                var result = performSearch(inType, null);
                if (result != null)
                {
                    return result;
                }

                // global only or global first but no context to perform a local search
                if ((inSearchMode == EServiceSearchMode.GlobalOnly) || (inContext == null))
                {
                    return null;
                }

                // try a local search
                return performSearch(inType, inContext);
            }

            // can only do a local search if we have a context
            if (inContext != null)
            {
                var result = performSearch(inType, inContext);
                if (result != null)
                {
                    return result;
                }

                // if we are looking only locally then exit
                if (inSearchMode == EServiceSearchMode.LocalOnly)
                {
                    return null;
                }

                // try a global search
                return performSearch(inType, null);
            }

            return null;
        }
    }
}
