using Common.ServiceLocator;
using UnityEngine;

namespace Common.SaveSystem
{
    public interface ISaveDataHandler : ILocatableService
    {
        void Validate();
    }
}
