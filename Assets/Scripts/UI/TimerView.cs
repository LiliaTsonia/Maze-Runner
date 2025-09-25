using System;
using TMPro;
using UnityEngine;

namespace Game.UI.Timer
{
    public class TimerView : MonoBehaviour
    {
        [SerializeField] 
        public TextMeshProUGUI _timeText;

        public void UpdateTimeDisplay(ref float time)
        {
            var timeSpan = TimeSpan.FromSeconds(time);
            _timeText.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }

        public void EnableTimer()
        {
            _timeText.gameObject.SetActive(true);
        }

        public void DisableTimer()
        {
            _timeText.gameObject.SetActive(false);
        }
    }
}