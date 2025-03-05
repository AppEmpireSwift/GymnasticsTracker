using System.Collections.Generic;
using UnityEngine;

namespace AddEntry
{
    public class EntryButtonHolder : MonoBehaviour
    {
        [SerializeField] private List<AddEntryButton> _entryButtons = new List<AddEntryButton>();
        [SerializeField] private int _maxButtonCount = 10;

        private int _enabledButtonCount = 0;

        public int EnabledButtonCount => _enabledButtonCount;

        private void Awake()
        {
            if (_entryButtons.Count != _maxButtonCount)
            {
                Debug.LogWarning(
                    $"EntryButtonHolder should have exactly {_maxButtonCount} buttons. Current count: {_entryButtons.Count}");
            }

            for (int i = 0; i < _entryButtons.Count; i++)
            {
                int buttonIndex = i;
                _entryButtons[i].Initialize(
                    buttonIndex,
                    () => HandleButtonClick(buttonIndex)
                );
            }

            ResetAllButtons();
        }

        public void HandleButtonClick(int buttonIndex)
        {
            for (int i = 0; i < _entryButtons.Count; i++)
            {
                if (i <= buttonIndex)
                {
                    _entryButtons[i].SetSelected(true);
                }
                else
                {
                    _entryButtons[i].SetSelected(false);
                }
            }

            _enabledButtonCount = buttonIndex + 1;
        }

        public void ResetAllButtons()
        {
            foreach (var button in _entryButtons)
            {
                button.SetSelected(false);
            }

            _enabledButtonCount = 0;
        }

        public int GetCurrentRating()
        {
            return _enabledButtonCount;
        }
    }
}