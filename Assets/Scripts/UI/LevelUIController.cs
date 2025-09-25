using Common.Enums;
using Common.EventsSystem;
using Common.ServiceLocator;
using Game.UI.Timer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class LevelUIController : MonoBehaviour
    {
        [SerializeField] 
        private TimerView _timerView;

        [SerializeField] 
        private Button _pauseButton;

        [SerializeField] 
        private TextMeshProUGUI _distanceText;
        
        private EventManager _eventManager;

        public void Init()
        {
            _eventManager = ServiceLocator.LocateService<EventManager>();
            _pauseButton.onClick.AddListener(OnPauseClick);
        }

        public void EnableTimer()
        {
            _timerView.EnableTimer();
        }

        public void UpdateTimeDisplay(ref float time)
        {
            _timerView.UpdateTimeDisplay(ref time);
        }
        
        public void UpdateDistance(ref int distance)
        {
            _distanceText.SetText($"Distance: {distance}");
        }

        private void OnPauseClick()
        {
            _eventManager.GetEvent(EGameEvent.Pause).Invoke();
        }

        private void OnDestroy()
        {
            _pauseButton.onClick.RemoveListener(OnPauseClick);
        }
    }
}