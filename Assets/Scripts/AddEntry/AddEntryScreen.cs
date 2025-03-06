using System;
using Bitsplash.DatePicker;
using DG.Tweening;
using EntryLogic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AddEntry
{
    [RequireComponent(typeof(ScreenVisabilityHandler))]
    public class AddEntryScreen : MonoBehaviour
    {
        public interface IRatingProvider
        {
            event Action RatingChanged;
            int GetCurrentRating();
            void ResetAllButtons();
        }

        [SerializeField] private Button _openDateButton;
        [SerializeField] private TMP_Text _dateText;
        [SerializeField] private DatePickerSettings _datePickerSettings;
        [SerializeField] private GameObject _dateScreen;
        [SerializeField] private TMP_InputField _typeInput;
        [SerializeField] private TMP_InputField _achievementInput;
        [SerializeField] private TMP_InputField _detailsInput;
        [SerializeField] private EntryButtonHolder _entryButtonHolder;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _dateConfirmButton;

        [Header("Animation Settings")] [SerializeField]
        private float _fadeInDuration = 0.3f;

        [SerializeField] private float _fadeOutDuration = 0.2f;
        [SerializeField] private float _buttonScaleFactor = 1.1f;
        [SerializeField] private float _inputFieldHighlightDuration = 0.3f;
        [SerializeField] private Vector3 _dateScreenStartPosition;
        [SerializeField] private float _dateScreenAnimDuration = 0.5f;
        [SerializeField] private Ease _dateScreenEase = Ease.OutBack;

        private ScreenVisabilityHandler _screenVisabilityHandler;
        private CanvasGroup _canvasGroup;
        private Vector3 _dateScreenOriginalPosition;
        private Sequence _currentAnimation;
        private DateTime _selectedDate;

        public event Action<EntryData> DataSaved;
        public event Action BackButtonClicked;

        private void Awake()
        {
            _screenVisabilityHandler = GetComponent<ScreenVisabilityHandler>();
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (_dateScreen != null)
            {
                _dateScreenOriginalPosition = _dateScreen.transform.position;
            }
        }

        private void OnEnable()
        {
            _openDateButton.onClick.AddListener(OnDateButtonClicked);
            _saveButton.onClick.AddListener(OnSaveButtonClicked);
            _backButton.onClick.AddListener(OnBackButtonClicked);

            if (_dateConfirmButton != null)
            {
                _dateConfirmButton.onClick.AddListener(SetDateText);
            }

            _typeInput.onValueChanged.AddListener(OnInputValueChanged);
            _achievementInput.onValueChanged.AddListener(OnInputValueChanged);
            _detailsInput.onValueChanged.AddListener(OnInputValueChanged);

            _entryButtonHolder.RatingChanged += OnRatingChanged;

            SetupButtonAnimation(_openDateButton);
            SetupButtonAnimation(_backButton);
            SetupButtonAnimation(_dateConfirmButton);

            SetupInputFieldAnimation(_typeInput);
            SetupInputFieldAnimation(_achievementInput);
            SetupInputFieldAnimation(_detailsInput);

            ToggleSaveButton();
        }

        private void OnDisable()
        {
            _openDateButton.onClick.RemoveListener(OnDateButtonClicked);
            _saveButton.onClick.RemoveListener(OnSaveButtonClicked);
            _backButton.onClick.RemoveListener(OnBackButtonClicked);

            if (_dateConfirmButton != null)
            {
                _dateConfirmButton.onClick.RemoveListener(SetDateText);
            }

            _typeInput.onValueChanged.RemoveListener(OnInputValueChanged);
            _achievementInput.onValueChanged.RemoveListener(OnInputValueChanged);
            _detailsInput.onValueChanged.RemoveListener(OnInputValueChanged);

            _entryButtonHolder.RatingChanged -= OnRatingChanged;

            KillAllAnimations();
        }

        private void Start()
        {
            _screenVisabilityHandler.DisableScreen();

            if (_dateScreen != null)
            {
                _dateScreen.gameObject.SetActive(false);
            }

            if (_entryButtonHolder is MonoBehaviour mbEntryButtonHolder &&
                mbEntryButtonHolder is IRatingProvider == false)
            {
                Debug.LogWarning("EntryButtonHolder should implement IRatingProvider interface for rating events");
            }
        }

        public void Enable()
        {
            _screenVisabilityHandler.EnableScreen();
            ResetScreen();

            PlayFadeInAnimation();
        }

        public void Disable()
        {
            PlayFadeOutAnimation(() => _screenVisabilityHandler.DisableScreen());
        }

        private void ResetScreen()
        {
            _typeInput.text = string.Empty;
            _achievementInput.text = string.Empty;
            _detailsInput.text = string.Empty;
            _entryButtonHolder.ResetAllButtons();
            _dateText.text = "Add date";

            ToggleSaveButton();
        }

        private bool GetSaveButtonStatus()
        {
            return !string.IsNullOrEmpty(_typeInput.text) && !string.IsNullOrEmpty(_achievementInput.text) &&
                   !string.IsNullOrEmpty(_detailsInput.text) && _entryButtonHolder.GetCurrentRating() > 0;
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

        private void OnDateButtonClicked()
        {
            if (_dateScreen != null)
            {
                _dateScreen.gameObject.SetActive(true);

                PlayDateScreenOpenAnimation();
            }
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
        }

        private void OnSaveButtonClicked()
        {
            var entryData = new EntryData(_typeInput.text, _entryButtonHolder.GetCurrentRating(),
                _achievementInput.text, _detailsInput.text, _selectedDate);

            PlaySaveAnimation(() =>
            {
                DataSaved?.Invoke(entryData);
                Disable();
            });
        }

        private void OnBackButtonClicked()
        {
            PlayBackAnimation(() => Disable());
            BackButtonClicked?.Invoke();
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

        private void OnRatingChanged()
        {
            ToggleSaveButton();

            _entryButtonHolder.transform.DOKill();
            _entryButtonHolder.transform.DOPunchScale(new Vector3(0.05f, 0.05f, 0.05f), 0.2f, 2, 0.5f);
        }

        #region Animation Methods

        private void PlayFadeInAnimation()
        {
            KillAllAnimations();

            _canvasGroup.alpha = 0f;
            _currentAnimation = DOTween.Sequence();

            foreach (Transform child in transform)
            {
                if (child.gameObject.activeSelf)
                {
                    child.localScale = new Vector3(0.95f, 0.95f, 0.95f);
                    _currentAnimation.Join(child.DOScale(Vector3.one, _fadeInDuration).SetEase(Ease.OutCubic));
                }
            }

            _currentAnimation.Join(_canvasGroup.DOFade(1f, _fadeInDuration).SetEase(Ease.OutCubic));
        }

        private void PlayFadeOutAnimation(Action onComplete = null)
        {
            KillAllAnimations();

            _currentAnimation = DOTween.Sequence();

            foreach (Transform child in transform)
            {
                if (child.gameObject.activeSelf)
                {
                    _currentAnimation.Join(child.DOScale(new Vector3(0.95f, 0.95f, 0.95f), _fadeOutDuration)
                        .SetEase(Ease.InCubic));
                }
            }

            _currentAnimation.Join(_canvasGroup.DOFade(0f, _fadeOutDuration).SetEase(Ease.InCubic));

            if (onComplete != null)
            {
                _currentAnimation.OnComplete(() => onComplete.Invoke());
            }
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

        private void AnimateDateTextUpdate()
        {
            if (_dateText == null) return;

            _dateText.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f, 5, 0.5f);
        }

        private void PlaySaveAnimation(Action onComplete = null)
        {
            _saveButton.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f, 5, 0.5f);

            Sequence saveSequence = DOTween.Sequence();

            saveSequence.Append(_typeInput.transform.DOScale(1.05f, 0.15f).SetEase(Ease.OutQuad));
            saveSequence.Join(_achievementInput.transform.DOScale(1.05f, 0.15f).SetEase(Ease.OutQuad));
            saveSequence.Join(_detailsInput.transform.DOScale(1.05f, 0.15f).SetEase(Ease.OutQuad));

            saveSequence.Append(_typeInput.transform.DOScale(0.9f, 0.2f).SetEase(Ease.InQuad));
            saveSequence.Join(_achievementInput.transform.DOScale(0.9f, 0.2f).SetEase(Ease.InQuad));
            saveSequence.Join(_detailsInput.transform.DOScale(0.9f, 0.2f).SetEase(Ease.InQuad));

            if (onComplete != null)
            {
                saveSequence.OnComplete(() => onComplete.Invoke());
            }
        }

        private void PlayBackAnimation(Action onComplete = null)
        {
            _backButton.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f, 5, 0.5f);

            if (onComplete != null)
            {
                DOVirtual.DelayedCall(0.2f, () => onComplete.Invoke());
            }
        }

        private void SetupButtonAnimation(Button button)
        {
            if (button == null) return;

            button.transition = Selectable.Transition.None;

            button.onClick.AddListener(() =>
            {
                button.transform.DOComplete();
                button.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f, 5, 0.5f);
            });

            EventTrigger eventTrigger = button.gameObject.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = button.gameObject.AddComponent<EventTrigger>();
            }

            if (button == _saveButton && button.interactable)
            {
                Sequence pulseSequence = DOTween.Sequence();
                pulseSequence.Append(button.transform.DOScale(Vector3.one * _buttonScaleFactor, 0.8f)
                    .SetEase(Ease.InOutSine));
                pulseSequence.Append(button.transform.DOScale(Vector3.one, 0.8f).SetEase(Ease.InOutSine));
                pulseSequence.SetLoops(-1);
            }
            else if (button == _dateConfirmButton)
            {
                Sequence pulseSequence = DOTween.Sequence();
                pulseSequence.Append(button.transform.DOScale(Vector3.one * _buttonScaleFactor, 0.8f)
                    .SetEase(Ease.InOutSine));
                pulseSequence.Append(button.transform.DOScale(Vector3.one, 0.8f).SetEase(Ease.InOutSine));
                pulseSequence.SetLoops(-1);
            }
        }

        private void SetupInputFieldAnimation(TMP_InputField inputField)
        {
            if (inputField == null) return;

            Vector3 originalScale = inputField.transform.localScale;

            inputField.onSelect.AddListener(_ =>
            {
                inputField.transform.DOComplete();
                inputField.transform.localScale = originalScale;
                inputField.transform.DOScale(1.05f, _inputFieldHighlightDuration).SetEase(Ease.OutBack);
            });

            inputField.onDeselect.AddListener(_ =>
            {
                inputField.transform.DOComplete();
                inputField.transform.DOScale(originalScale, _inputFieldHighlightDuration).SetEase(Ease.OutBack);
            });
        }

        private void KillAllAnimations()
        {
            if (_currentAnimation != null)
            {
                _currentAnimation.Kill();
                _currentAnimation = null;
            }

            foreach (Transform child in transform)
            {
                child.DOKill();
            }

            _canvasGroup.DOKill();
            if (_dateScreen != null) _dateScreen.transform.DOKill();
        }

        #endregion
    }
}