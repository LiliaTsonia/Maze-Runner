using System;

namespace Game.UI.PopUp
{
    public class PausePopup : PopupWithIntent<PausePopup.Intent>
    {
        public class Intent
        {
            public Action OnExit;
            public Action OnContinue;

            public Intent(Action onExit, Action onContinue)
            {
                OnExit = onExit;
                OnContinue = onContinue;
            }
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
                intent?.OnContinue.Invoke();
                base.OnRightButtonClick();
            }
        }

        protected override void UnSubscribe()
        {
            intent.OnExit = null;
            intent.OnContinue = null;
            base.UnSubscribe();
        }
    }
}
