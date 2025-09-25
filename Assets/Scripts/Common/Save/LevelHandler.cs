using UnityEngine;

namespace Common.SaveSystem
{
    public sealed class LevelHandler : SaveDataHandlerBase
    {
        //TODO if needed move later to some static class, like PlayerPrefsKeyConstants etc.
        private const string BEST_TIME = "best_time";

        /// <summary>
        /// Should be done through SaveDataManager on all data handlers initialization
        /// </summary>
        public LevelHandler()
        {
            Validate();
        }
        
        public override void Validate()
        {
            if (!PlayerPrefs.HasKey(BEST_TIME))
            {
                PlayerPrefs.SetInt(BEST_TIME, int.MaxValue);
            }
        }

        public float GetBestTime()
        {
            return PlayerPrefs.GetInt(BEST_TIME);
        }

        public void TryToSaveBestTime(float seconds)
        {
            var currentTopResult = GetBestTime();
            if (seconds < currentTopResult)
            {
                PlayerPrefs.SetInt(BEST_TIME, Mathf.FloorToInt(seconds));
            }
        }
    }
}