using System;
using EntryLogic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainScreen
{
    public class EntryPlane : MonoBehaviour
    {
        [SerializeField] private Image _progressImage;
        [SerializeField] private TMP_Text _percentageText;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private Button _openButton;

        public event Action<EntryPlane> OpenClicked;

        public EntryData EntryData { get; private set; }
        public bool IsActive { get; private set; }

        private void OnEnable()
        {
            _openButton.onClick.AddListener(OnOpenClicked);
        }

        private void OnDisable()
        {
            _openButton.onClick.AddListener(OnOpenClicked);
        }

        public void Enable(EntryData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            IsActive = true;
            gameObject.SetActive(IsActive);
            EntryData = data;

            _progressImage.fillAmount = EntryData.Progress / 10f;
            _percentageText.text = $"{EntryData.Progress * 10}%";
            _nameText.text = EntryData.Name;
        }

        public void Disable()
        {
            IsActive = false;
            gameObject.SetActive(IsActive);
            OnReset();
        }

        public void OnReset()
        {
            EntryData = null;
            _progressImage.fillAmount = 0f;
            _percentageText.text = string.Empty;
            _nameText.text = string.Empty;
            _openButton.interactable = false;
        }

        private void OnOpenClicked()
        {
            OpenClicked?.Invoke(this);
        }
    }
}