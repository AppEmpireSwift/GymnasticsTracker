using System;
using AddEntry;
using Bitsplash.DatePicker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using EntryLogic;

namespace ViewEntry
{
    public class AddProgressScreen : MonoBehaviour
    {
        [SerializeField] private EntryButtonHolder _entryButtonHolder;
        [SerializeField] private Button _dateButton;
        [SerializeField] private TMP_Text _dateText;
        [SerializeField] private DatePickerSettings _datePickerSettings;
        [SerializeField] private GameObject _dateScreen;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _dateConfirmButton;

        [SerializeField] private TMP_InputField _typeInput;

        [SerializeField] private float _fadeInDuration = 0.3f;
        [SerializeField] private float _dateScreenAnimDuration = 0.5f;
        [SerializeField] private Ease _dateScreenEase = Ease.OutBack;

        private Vector3 _dateScreenStartPosition = Vector3.zero;
        private Vector3 _dateScreenOriginalPosition;

        private DateTime _selectedDate;

        public event Action<ProgressData> ProgressSaved;

        private void OnEnable()
        {
            _dateButton.onClick.AddListener(OpenDateScreen);
            _backButton.onClick.AddListener(CloseScreen);
            _saveButton.onClick.AddListener(SaveEntry);
            _dateConfirmButton.onClick.AddListener(SetDateText);
            _typeInput.onValueChanged.AddListener(OnInputValueChanged);
            _entryButtonHolder.RatingChanged += ToggleSaveButton;
        }

        private void OnDisable()
        {
            _dateButton.onClick.RemoveListener(OpenDateScreen);
            _backButton.onClick.RemoveListener(CloseScreen);
            _saveButton.onClick.RemoveListener(SaveEntry);
            _dateConfirmButton.onClick.RemoveListener(SetDateText);
            _typeInput.onValueChanged.RemoveListener(OnInputValueChanged);
            _entryButtonHolder.RatingChanged -= ToggleSaveButton;
        }

        private void OpenDateScreen()
        {
            if (_dateScreen == null) return;

            _dateScreen.SetActive(true);
            _dateScreenOriginalPosition = _dateScreen.transform.position;
            PlayDateScreenOpenAnimation();
        }

        private void CloseScreen()
        {
            gameObject.SetActive(false);
            ClearForm();
        }

        private void SaveEntry()
        {
            if (!GetSaveButtonStatus()) return;

            ProgressData newEntry = new ProgressData();
            newEntry.Date = _selectedDate;
            newEntry.Progress = _entryButtonHolder.GetCurrentRating();

            ProgressSaved?.Invoke(newEntry);
            ClearForm();
            gameObject.SetActive(false);
        }

        private void ClearForm()
        {
            _typeInput.text = string.Empty;
            _dateText.text = "Add date";
            _entryButtonHolder.ResetAllButtons();
            _selectedDate = default;
            ToggleSaveButton();
        }

        private void AnimateDateTextUpdate()
        {
            _dateText.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.2f, 1, 0.5f);
        }

        private void SetDateText()
        {
            var selection = _datePickerSettings.Content.Selection;

            if (selection.Count > 0)
            {
                _dateText.text = selection.GetItem(0).Date.ToString("dd/MM/yyyy");
                _selectedDate = selection.GetItem(0);

                AnimateDateTextUpdate();

                PlayDateScreenCloseAnimation(() =>
                {
                    _dateScreen.gameObject.SetActive(false);
                    selection.Clear();
                });
            }
            else
            {
                _dateScreen.gameObject.SetActive(false);
            }

            ToggleSaveButton();
        }

        private void OnInputValueChanged(string value)
        {
            ToggleSaveButton();

            TMP_InputField inputField = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject
                ?.GetComponent<TMP_InputField>();
            if (inputField != null)
            {
                inputField.transform.DOComplete();

                Vector3 originalScale = inputField.transform.localScale;
                inputField.transform.DOPunchScale(new Vector3(0.02f, 0.02f, 0.02f), 0.1f, 1, 0.5f)
                    .OnComplete(() => { inputField.transform.localScale = originalScale; });
            }
        }

        private bool GetSaveButtonStatus()
        {
            return !string.IsNullOrEmpty(_typeInput.text) &&
                   _entryButtonHolder.GetCurrentRating() > 0;
        }

        private void ToggleSaveButton()
        {
            bool canSave = GetSaveButtonStatus();
            _saveButton.interactable = canSave;

            _saveButton.gameObject.SetActive(true);

            if (_saveButton.transform.localScale == Vector3.zero)
            {
                _saveButton.transform.DOScale(Vector3.one, _fadeInDuration).SetEase(Ease.OutBack);
            }

            float targetScale = canSave ? 1f : 0.95f;
            _saveButton.transform.DOScale(targetScale, 0.2f);
        }

        private void PlayDateScreenOpenAnimation()
        {
            if (_dateScreen == null) return;

            if (_dateScreenStartPosition != Vector3.zero)
            {
                _dateScreen.transform.position = _dateScreenStartPosition;
            }
            else
            {
                _dateScreen.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            }

            _dateScreen.transform.DOScale(Vector3.one, _dateScreenAnimDuration).SetEase(_dateScreenEase);
            if (_dateScreenStartPosition != Vector3.zero)
            {
                _dateScreen.transform.DOMove(_dateScreenOriginalPosition, _dateScreenAnimDuration)
                    .SetEase(_dateScreenEase);
            }
        }

        private void PlayDateScreenCloseAnimation(Action onComplete = null)
        {
            if (_dateScreen == null) return;

            Sequence dateScreenClose = DOTween.Sequence();

            dateScreenClose.Append(_dateScreen.transform
                .DOScale(new Vector3(0.8f, 0.8f, 0.8f), _dateScreenAnimDuration * 0.7f).SetEase(Ease.InBack));

            if (_dateScreenStartPosition != Vector3.zero)
            {
                dateScreenClose.Join(_dateScreen.transform
                    .DOMove(_dateScreenStartPosition, _dateScreenAnimDuration * 0.7f).SetEase(Ease.InBack));
            }

            if (onComplete != null)
            {
                dateScreenClose.OnComplete(() => onComplete.Invoke());
            }
        }
    }
}