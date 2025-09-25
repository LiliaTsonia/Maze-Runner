using UnityEngine;

namespace Game.UI.PopUp
{
    public class PopUp : MonoBehaviour, IPopUp
    {
        [SerializeField] 
        private bool _animated;

        protected bool ClicksEnabled { get; private set; }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            SwitchClicksAvailability();
        }

        public virtual void Hide()
        {
            ClicksEnabled = false;
            gameObject.SetActive(false);
        }

        public void SwitchClicksAvailability()
        {
            ClicksEnabled = !ClicksEnabled;
        }
    }
}