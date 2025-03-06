using System;
using DG.Tweening;
using EntryLogic;
using MainScreen;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EditEntry
{
    [RequireComponent(typeof(ScreenVisabilityHandler))]
    public class EditEntryScreen : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _typeInput;
        [SerializeField] private TMP_InputField _achievementInput;
        [SerializeField] private TMP_InputField _detailsInput;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _saveButton;

        [Header("Animation Settings")] [SerializeField]
        private float _fadeInDuration = 0.3f;

        [SerializeField] private float _fadeOutDuration = 0.2f;
        [SerializeField] private float _buttonScaleFactor = 1.1f;
        [SerializeField] private float _inputFieldHighlightDuration = 0.3f;

        private ScreenVisabilityHandler _screenVisabilityHandler;
        private CanvasGroup _canvasGroup;
        private Sequence _currentAnimation;
        private EntryPlane _currentEntryPlane;
        private DateTime _entryDate;

        public event Action<EntryPlane> DataSaved;
        public event Action BackButtonClicked;

        private void Awake()
        {
            _screenVisabilityHandler = GetComponent<ScreenVisabilityHandler>();
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void OnEnable()
        {
            _saveButton.onClick.AddListener(OnSaveButtonClicked);
            _backButton.onClick.AddListener(OnBackButtonClicked);

            _typeInput.onValueChanged.AddListener(OnInputValueChanged);
            _achievementInput.onValueChanged.AddListener(OnInputValueChanged);
            _detailsInput.onValueChanged.AddListener(OnInputValueChanged);

            SetupButtonAnimation(_backButton);

            SetupInputFieldAnimation(_typeInput);
            SetupInputFieldAnimation(_achievementInput);
            SetupInputFieldAnimation(_detailsInput);

            ToggleSaveButton();
        }

        private void OnDisable()
        {
            _saveButton.onClick.RemoveListener(OnSaveButtonClicked);
            _backButton.onClick.RemoveListener(OnBackButtonClicked);

            _typeInput.onValueChanged.RemoveListener(OnInputValueChanged);
            _achievementInput.onValueChanged.RemoveListener(OnInputValueChanged);
            _detailsInput.onValueChanged.RemoveListener(OnInputValueChanged);
        }

        private void Start()
        {
            _screenVisabilityHandler.DisableScreen();
        }

        public void Enable(EntryPlane entryPlane)
        {
            _currentEntryPlane = entryPlane;
            _screenVisabilityHandler.EnableScreen();
            PopulateFields(entryPlane.EntryData);

            PlayFadeInAnimation();
        }

        public void Disable()
        {
            PlayFadeOutAnimation(() => _screenVisabilityHandler.DisableScreen());
        }

        private void PopulateFields(EntryData entryData)
        {
            if (entryData == null)
            {
                Debug.LogError("EntryData is null");
                return;
            }

            _typeInput.text = entryData.Goal;
            _achievementInput.text = entryData.Goal;
            _detailsInput.text = entryData.Details;
            _entryDate = entryData.Date;

            ToggleSaveButton();
        }

        private bool GetSaveButtonStatus()
        {
            return !string.IsNullOrEmpty(_typeInput.text) && !string.IsNullOrEmpty(_achievementInput.text) &&
                   !string.IsNullOrEmpty(_detailsInput.text);
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

        private void OnSaveButtonClicked()
        {
            var entryData = new EntryData(_typeInput.text, _currentEntryPlane.EntryData.Progress,
                _achievementInput.text, _detailsInput.text, _entryDate);

            PlaySaveAnimation(() =>
            {
                _currentEntryPlane.Enable(entryData);
                DataSaved?.Invoke(_currentEntryPlane);
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

        #region Animation Methods

        private void PlayFadeInAnimation()
        {
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

        #endregion
    }
}