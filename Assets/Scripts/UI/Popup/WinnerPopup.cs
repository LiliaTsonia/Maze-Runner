using System;
using TMPro;
using UnityEngine;

namespace Game.UI.PopUp
{
    public class WinnerPopup : PopupWithIntent<WinnerPopup.Intent>
    {
        public class Intent
        {
            public float BestTime;
            public float CurrentTime;
            public Action OnExit;
            public Action OnNext;

            public Intent(float bestTime, float currentTime, Action onExit, Action onNext)
            {
                BestTime = bestTime;
                CurrentTime = currentTime;
                OnExit = onExit;
                OnNext = onNext;
            }
        }

        /// <summary>
        /// Can be used in future for localization keys
        /// </summary>
        private const string CURRENT_TIME = "Current time";
        private const string BEST_TIME = "Best time";

        [SerializeField] 
        private TextMeshProUGUI _currentTimeText;
        [SerializeField] 
        private TextMeshProUGUI _bestTimeText;

        public override void Show()
        {
            var timeSpan = TimeSpan.FromSeconds(intent.CurrentTime);
            _currentTimeText.text = $"{CURRENT_TIME}: {timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            timeSpan = TimeSpan.FromSeconds(intent.BestTime);
            _bestTimeText.text = $"{BEST_TIME}: {timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";

            base.Show();
        }

        protected override void OnLeftButtonClick()
        {
            if (ClicksEnabled)
            {
                SwitchClicksAvailability();
                intent.OnExit?.Invoke();
                base.OnLeftButtonClick();
            }
        }

        protected override void OnRightButtonClick()
        {
            if (ClicksEnabled)
            {
                SwitchClicksAvailability();
                intent.OnNext?.Invoke();
                base.OnRightButtonClick();
            }
        }

        protected override void UnSubscribe()
        {
            intent.OnExit = null;
            intent.OnNext = null;

            base.UnSubscribe();
        }
    }
}
