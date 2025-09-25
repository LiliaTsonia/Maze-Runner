using System;

namespace Common.ServiceLocator
{
    public class ServiceQuery
    {
        public EServiceSearchMode SearchMode;
        public ServiceAddress Address;
        public Action<ILocatableService> CallbackFn;
    }
}