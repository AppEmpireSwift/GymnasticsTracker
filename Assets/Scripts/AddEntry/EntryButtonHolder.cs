using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace AddEntry
{
    public class EntryButtonHolder : MonoBehaviour, AddEntryScreen.IRatingProvider
    {
        [SerializeField] private List<AddEntryButton> _entryButtons = new List<AddEntryButton>();
        [SerializeField] private int _maxButtonCount = 10;

        [Header("Animation Settings")] [SerializeField]
        private float _buttonAnimationDuration = 0.2f;

        [SerializeField] private float _buttonSelectedScale = 1.15f;
        [SerializeField] private float _buttonNormalScale = 1.0f;
        [SerializeField] private float _buttonHoverScale = 1.1f;
        [SerializeField] private float _sequentialDelay = 0.03f;
        [SerializeField] private Ease _animationEase = Ease.OutBack;

        private int _enabledButtonCount = 0;
        private Sequence _currentAnimation;

        public int EnabledButtonCount => _enabledButtonCount;

        public event Action RatingChanged;

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

                SetupButtonHoverAnimation(_entryButtons[i], buttonIndex);
            }

            ResetAllButtons();
        }

        private void OnDestroy()
        {
            if (_currentAnimation != null)
            {
                _currentAnimation.Kill();
                _currentAnimation = null;
            }

            foreach (var button in _entryButtons)
            {
                if (button != null)
                {
                    button.transform.DOKill();
                }
            }
        }

        public void HandleButtonClick(int buttonIndex)
        {
            if (_currentAnimation != null)
            {
                _currentAnimation.Kill();
            }

            _currentAnimation = DOTween.Sequence();

            for (int i = 0; i < _entryButtons.Count; i++)
            {
                int index = i;
                float delay = i * _sequentialDelay;

                if (i <= buttonIndex)
                {
                    _currentAnimation.Insert(delay, _entryButtons[i].transform
                        .DOScale(_buttonSelectedScale, _buttonAnimationDuration)
                        .SetEase(_animationEase));

                    _currentAnimation.InsertCallback(delay + _buttonAnimationDuration,
                        () => { _entryButtons[index].SetSelected(true); });
                }
                else
                {
                    _currentAnimation.Insert(delay, _entryButtons[i].transform
                        .DOScale(_buttonNormalScale, _buttonAnimationDuration)
                        .SetEase(_animationEase));

                    _currentAnimation.InsertCallback(delay + _buttonAnimationDuration,
                        () => { _entryButtons[index].SetSelected(false); });
                }
            }

            int previousRating = _enabledButtonCount;
            _enabledButtonCount = buttonIndex + 1;

            if (previousRating != _enabledButtonCount)
            {
                _currentAnimation.OnComplete(() => { RatingChanged?.Invoke(); });
            }
        }

        public void ResetAllButtons()
        {
            if (_currentAnimation != null)
            {
                _currentAnimation.Kill();
            }

            _currentAnimation = DOTween.Sequence();

            for (int i = 0; i < _entryButtons.Count; i++)
            {
                float delay = i * _sequentialDelay;

                _currentAnimation.Insert(delay, _entryButtons[i].transform
                    .DOScale(_buttonNormalScale, _buttonAnimationDuration)
                    .SetEase(_animationEase));

                int index = i;
                _currentAnimation.InsertCallback(delay + _buttonAnimationDuration,
                    () => { _entryButtons[index].SetSelected(false); });
            }

            int previousRating = _enabledButtonCount;
            _enabledButtonCount = 0;

            if (previousRating != _enabledButtonCount)
            {
                _currentAnimation.OnComplete(() => { RatingChanged?.Invoke(); });
            }
        }

        public int GetCurrentRating()
        {
            return _enabledButtonCount;
        }

        private void SetupButtonHoverAnimation(AddEntryButton button, int buttonIndex)
        {
            if (button == null) return;

            UnityEngine.UI.Button uiButton = button.GetComponent<UnityEngine.UI.Button>();
            if (uiButton == null) return;

            UnityEngine.EventSystems.EventTrigger eventTrigger =
                button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }

            UnityEngine.EventSystems.EventTrigger.Entry entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
            entryEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            entryEnter.callback.AddListener((data) => { OnPointerEnterButton(buttonIndex); });
            eventTrigger.triggers.Add(entryEnter);

            UnityEngine.EventSystems.EventTrigger.Entry entryExit = new UnityEngine.EventSystems.EventTrigger.Entry();
            entryExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
            entryExit.callback.AddListener((data) => { OnPointerExitButton(buttonIndex); });
            eventTrigger.triggers.Add(entryExit);

            void OnPointerEnterButton(int index)
            {
                ShowRatingPreview(index);
            }

            void OnPointerExitButton(int index)
            {
                RestoreCurrentRating();
            }
        }

        private void ShowRatingPreview(int hoverIndex)
        {
            for (int i = 0; i < _entryButtons.Count; i++)
            {
                if (i <= hoverIndex)
                {
                    if (!_entryButtons[i].IsSelected)
                    {
                        _entryButtons[i].transform.DOScale(_buttonHoverScale, _buttonAnimationDuration * 0.5f)
                            .SetEase(Ease.OutQuad);
                    }
                }
            }
        }

        private void RestoreCurrentRating()
        {
            for (int i = 0; i < _entryButtons.Count; i++)
            {
                if (i < _enabledButtonCount)
                {
                    _entryButtons[i].transform.DOScale(_buttonSelectedScale, _buttonAnimationDuration * 0.5f)
                        .SetEase(Ease.OutQuad);
                }
                else
                {
                    _entryButtons[i].transform.DOScale(_buttonNormalScale, _buttonAnimationDuration * 0.5f)
                        .SetEase(Ease.OutQuad);
                }
            }
        }

        public void AnimateShake()
        {
            Sequence shakeSequence = DOTween.Sequence();

            for (int i = 0; i < _entryButtons.Count; i++)
            {
                float delay = i * 0.05f;
                shakeSequence.Insert(delay, _entryButtons[i].transform.DOShakeScale(0.3f, 0.2f, 10, 90f));
            }
        }

        public void AnimateWave()
        {
            Sequence waveSequence = DOTween.Sequence();

            for (int i = 0; i < _entryButtons.Count; i++)
            {
                float delay = i * 0.08f;
                float duration = 0.3f;

                waveSequence.Insert(delay, _entryButtons[i].transform
                    .DOScale(_buttonSelectedScale * 1.1f, duration * 0.5f)
                    .SetEase(Ease.OutQuad));

                waveSequence.Insert(delay + duration * 0.5f, _entryButtons[i].transform
                    .DOScale(i < _enabledButtonCount ? _buttonSelectedScale : _buttonNormalScale, duration * 0.5f)
                    .SetEase(Ease.InOutQuad));
            }
        }
    }
}