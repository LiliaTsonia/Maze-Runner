using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.PopUp
{
    public class PopupWithIntent<TIntent> : PopUp
    {
        protected TIntent intent;

        [SerializeField] 
        protected Button _leftButton;
        [SerializeField] 
        protected Button _rightButton;

        public void SetIntent(TIntent intent)
        {
            this.intent = intent;
        }
        
        public override void Show()
        {
            _leftButton.onClick.AddListener(OnLeftButtonClick);
            _rightButton.onClick.AddListener(OnRightButtonClick);

            base.Show();
        }

        protected virtual void OnLeftButtonClick()
        {
            Hide();
        }

        protected virtual void OnRightButtonClick()
        {
            Hide();
        }

        protected virtual void UnSubscribe()
        {
            _leftButton.onClick.RemoveListener(OnLeftButtonClick);
            _rightButton.onClick.RemoveListener(OnRightButtonClick);
        }

        private void OnDestroy()
        {
            UnSubscribe();
        }
    }
}
