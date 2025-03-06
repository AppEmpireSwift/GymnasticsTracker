using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using EditEntry;
using EntryLogic;
using MainScreen;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ViewEntry
{
    [RequireComponent(typeof(ScreenVisabilityHandler))]
    public class ViewEntryScreen : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private Image _progressImage;
        [SerializeField] private TMP_Text _percentageText;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _goalDetailsText;
        [SerializeField] private List<ProgressPlane> _progressPlanes;
        [SerializeField] private Button _addProgressButton;
        [SerializeField] private Button _editPlaneButton;
        [SerializeField] private AddProgressScreen _addProgressScreen;
        [SerializeField] private CanvasGroup _screenCanvasGroup;
        [SerializeField] private EditEntryScreen _editEntryScreen;

        [Header("Animation Settings")] [SerializeField]
        private float _animationDuration = 0.3f;

        [SerializeField] private Ease _animationEase = Ease.OutQuad;

        private ScreenVisabilityHandler _screenVisabilityHandler;
        private EntryPlane _entryPlane;

        public event Action<EntryPlane> DeletedPlane;
        public event Action<EntryPlane> EditPlane;
        public event Action BackClicked;
        public event Action DataUpdated;

        private void Awake()
        {
            _screenVisabilityHandler = GetComponent<ScreenVisabilityHandler>();
            _deleteButton.onClick.AddListener(OnDeleteButtonClicked);
        }

        private void OnEnable()
        {
            _backButton.onClick.AddListener(OnBackClicked);
            _editPlaneButton.onClick.AddListener(OnEditClicked);
            _addProgressButton.onClick.AddListener(OnAddProgressClicked);
            _addProgressScreen.ProgressSaved += EnableProgressPlane;
            _editEntryScreen.DataSaved += Enable;
            _editEntryScreen.BackButtonClicked += _screenVisabilityHandler.EnableScreen;

            foreach (var progressPlane in _progressPlanes)
            {
                progressPlane.Updated += OnDataUpdated;
            }
        }

        private void OnDisable()
        {
            _backButton.onClick.RemoveListener(OnBackClicked);
            _editPlaneButton.onClick.RemoveListener(OnEditClicked);
            _addProgressButton.onClick.RemoveListener(OnAddProgressClicked);
            _addProgressScreen.ProgressSaved -= EnableProgressPlane;
            _editEntryScreen.DataSaved -= Enable;
            _editEntryScreen.BackButtonClicked -= _screenVisabilityHandler.EnableScreen;

            foreach (var progressPlane in _progressPlanes)
            {
                progressPlane.Updated -= OnDataUpdated;
            }
        }

        private void Start()
        {
            _screenVisabilityHandler.DisableScreen();
            _addProgressScreen.gameObject.SetActive(false);
        }

        public void Enable(EntryPlane entryPlane)
        {
            if (entryPlane == null)
                throw new ArgumentNullException(nameof(entryPlane));

            DOTween.Kill(_screenCanvasGroup);
            DOTween.Kill(transform);
            DOTween.Kill(_progressImage);
            DOTween.Kill(_goalDetailsText);

            DisableAllProgressPlanes();

            _entryPlane = entryPlane;
            _screenVisabilityHandler.EnableScreen();

            _addProgressScreen.gameObject.SetActive(false);

            UpdatePercentages();

            _nameText.text = _entryPlane.EntryData.Name;

            _goalDetailsText.text =
                $"Achievement goal\n\n{_entryPlane.EntryData.Goal}\n\nDetails\n\n{_entryPlane.EntryData.Details}";
            _goalDetailsText.DOFade(0f, 0f);
            _goalDetailsText.DOFade(1f, _animationDuration);

            AnimateProgressPlanes();

            _screenCanvasGroup.alpha = 0f;
            _screenCanvasGroup.DOFade(1f, _animationDuration)
                .SetEase(_animationEase);

            transform.localScale = Vector3.zero;
            transform.DOScale(1f, _animationDuration)
                .SetEase(_animationEase);
        }

        public void Disable()
        {
            _screenCanvasGroup.DOFade(0f, _animationDuration)
                .SetEase(_animationEase);

            transform.DOScale(0.8f, _animationDuration)
                .SetEase(_animationEase)
                .OnComplete(() =>
                {
                    _screenVisabilityHandler.DisableScreen();
                    transform.localScale = Vector3.one;
                });
        }

        private void AnimateProgressPlanes()
        {
            for (int i = 0; i < _entryPlane.EntryData.ProgressDatas.Count; i++)
            {
                var progressData = _entryPlane.EntryData.ProgressDatas[i];
                var availablePlane = _progressPlanes.FirstOrDefault(p => !p.IsActive);
                if (availablePlane != null)
                {
                    availablePlane.Enable(progressData);
                }
            }
        }

        private void UpdatePercentages()
        {
            float maxProgressDataPercentage = _entryPlane.EntryData.ProgressDatas.Any()
                ? _entryPlane.EntryData.ProgressDatas.Max(plane => plane.Progress)
                : 0f;

            float finalPercentage = Mathf.Max(maxProgressDataPercentage, _entryPlane.EntryData.Progress);

            _progressImage.fillAmount = 0f;
            _progressImage.DOFillAmount(finalPercentage / 10f, _animationDuration)
                .SetEase(_animationEase);

            _percentageText.text = "0%";
            DOTween.To(() => 0f, x => _percentageText.text = $"{x * 10:0}%",
                    finalPercentage, _animationDuration)
                .SetEase(_animationEase);
        }

        private void EnableProgressPlane(ProgressData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (!_entryPlane.EntryData.ProgressDatas.Contains(data))
            {
                _entryPlane.EntryData.ProgressDatas.Add(data);
                DataUpdated?.Invoke();
            }

            var availablePlane = _progressPlanes.FirstOrDefault(p => !p.IsActive);
            if (availablePlane != null)
            {
                availablePlane.Enable(data);
            }

            _entryPlane.UpdatePercentages();
            UpdatePercentages();
        }

        private void OnDataUpdated()
        {
            DataUpdated?.Invoke();
        }

        private void OnAddProgressClicked()
        {
            _addProgressScreen.gameObject.SetActive(true);
            _addProgressScreen.transform.localScale = Vector3.zero;
            _addProgressScreen.transform.DOScale(1f, _animationDuration)
                .SetEase(_animationEase);
        }

        private void OnDeleteButtonClicked()
        {
            transform.DOShakeScale(0.5f, 0.2f, 10)
                .OnComplete(() =>
                {
                    DeletedPlane?.Invoke(_entryPlane);
                    Disable();
                });
        }

        private void OnBackClicked()
        {
            _backButton.transform.DOPunchScale(new Vector3(1.2f, 1.2f, 1.2f), 0.2f)
                .OnComplete(() =>
                {
                    BackClicked?.Invoke();
                    Disable();
                });
        }

        private void OnEditClicked()
        {
            _editPlaneButton.transform.DOPunchRotation(new Vector3(0, 0, 15f), 0.2f)
                .OnComplete(() =>
                {
                    EditPlane?.Invoke(_entryPlane);
                    _editEntryScreen.Enable(_entryPlane);
                    Disable();
                });
        }

        private void DisableAllProgressPlanes()
        {
            foreach (var progressPlane in _progressPlanes)
            {
                progressPlane.Disable();
            }
        }
    }
}