namespace Common.SaveSystem
{
    /// <summary>
    /// //Here has to be implemented Save method,
    /// like delegate control to some SaveDataManager,
    /// can be reworked when needed
    /// </summary>
    public abstract class SaveDataHandlerBase : ISaveDataHandler
    {
        public abstract void Validate();
    }
}