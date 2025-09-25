using System.Collections.Generic;
using System.Linq;
using Common.ServiceLocator;

namespace Game.UI.PopUp
{
    /// <summary>
    /// Light version without loading/unloading resources
    /// </summary>
    public class PopUpManager : ILocatableService
    {
        private List<PopUp> _popupStack = new();

        public PopUp PushWithIntent<TPopUp, TIntent>(PopUp popup, TIntent intent) where TPopUp : PopupWithIntent<TIntent>
        {
            if (_popupStack.Count > 0)
            {
                _popupStack.Last().Hide();
            }

            TPopUp popupWithIntent = popup as TPopUp;
            popupWithIntent.SetIntent(intent);
            _popupStack.Add(popupWithIntent);
            popupWithIntent.Show();

            return popupWithIntent;
        }
    
        public void PopLast()
        {
            if (_popupStack.Count > 0)
            {
                _popupStack.RemoveAt(_popupStack.Count - 1);
                if (_popupStack.Count > 0)
                {
                    var popup = _popupStack.Last();
                    popup.Show();
                }
            }
        }
        
        public void PopAll()
        {
            for (var i = 0; i < _popupStack.Count; i++)
            {
                var popup = _popupStack[i];
                popup.Hide();
            }

            _popupStack.Clear();
        }
    
        public bool IsAnyPopUpOpened()
        {
            return _popupStack.Count > 0;
        }
    }
}
