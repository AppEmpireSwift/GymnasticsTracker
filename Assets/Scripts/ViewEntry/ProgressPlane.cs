using System;
using AddEntry;
using EntryLogic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ViewEntry
{
    public class ProgressPlane : MonoBehaviour
    {
        [SerializeField] private TMP_Text _dateText;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private AddEntryButton[] _addEntryButtons;
        [SerializeField] private Button _deleteButton;

        public event Action Updated;
        
        public bool IsActive { get; private set; }
        public ProgressData ProgressData { get; private set; }

        private void OnEnable()
        {
            _deleteButton.onClick.AddListener(OnDeleteClicked);
        }

        private void OnDisable()
        {
            _deleteButton.onClick.RemoveListener(OnDeleteClicked);
        }

        public void Enable(ProgressData data)
        {
            ProgressData = data ?? throw new ArgumentNullException(nameof(data));

            IsActive = true;
            gameObject.SetActive(IsActive);
            DiactivateAllButtons();

            _dateText.text = ProgressData.Date.ToString("dd/MM/yyyy");
            _nameText.text = ProgressData.Details;

            for (int i = 0; i < ProgressData.Progress; i++)
            {
                _addEntryButtons[i].SetSelected(true);
            }
        }

        public void Disable()
        {
            IsActive = false;
            gameObject.SetActive(IsActive);
            OnReset();
        }

        private void OnReset()
        {
            _dateText.text = string.Empty;
            _nameText.text = string.Empty;
            ProgressData = null;
        }

        private void OnDeleteClicked()
        {
            Updated?.Invoke();
            Disable();
        }

        private void DiactivateAllButtons()
        {
            foreach (var addEntryButton in _addEntryButtons)
            {
                addEntryButton.SetSelected(false);
            }
        }
    }
}