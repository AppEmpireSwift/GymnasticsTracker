using System;
using UnityEngine;
using UnityEngine.UI;

namespace AddEntry
{
    public class AddEntryButton : MonoBehaviour
    {
        [SerializeField] private Sprite _unselectedSprite;
        [SerializeField] private Sprite _selectedSprite;
        [SerializeField] private Button _button;
        [SerializeField] private Image _buttonImage;

        private int _index;
        private bool _isSelected = false;
        private Action _onClick;

        public void Initialize(int index, Action onClick)
        {
            _index = index;
            _onClick = onClick;

            if (_button == null)
            {
                _button = GetComponent<Button>();
            }

            if (_buttonImage == null && _button != null)
            {
                _buttonImage = _button.GetComponent<Image>();
            }

            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(HandleClick);
            }
            else
            {
                Debug.LogError("Button component missing on AddEntryButton");
            }

            UpdateVisualState();
        }

        private void HandleClick()
        {
            _onClick?.Invoke();
        }

        public void SetSelected(bool isSelected)
        {
            _isSelected = isSelected;
            UpdateVisualState();
        }
        
        private void UpdateVisualState()
        {
            if (_buttonImage != null)
            {
                _buttonImage.sprite = _isSelected ? _selectedSprite : _unselectedSprite;
            }
        }

        public bool IsSelected => _isSelected;
        public int Index => _index;
    }
}