using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AddEntry;
using EntryLogic;
using SaveSystem;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using ViewEntry;

namespace MainScreen
{
    [RequireComponent(typeof(ScreenVisabilityHandler))]
    public class MainScreenController : MonoBehaviour
    {
        [SerializeField] private Button _addEntryButton;
        [SerializeField] private List<EntryPlane> _entryPlanes;
        [SerializeField] private GameObject _emptyPlane;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private AddEntryScreen _addEntryScreen;
        [SerializeField] private CanvasGroup _mainScreenCanvasGroup;
        [SerializeField] private ViewEntryScreen _viewEntryScreen;

        [Header("Animation Settings")] [SerializeField]
        private float _animationDuration = 0.5f;

        [SerializeField] private Ease _animationEase = Ease.OutQuad;

        private ScreenVisabilityHandler _screenVisabilityHandler;
        private DataSaver _dataSaver;

        private void Awake()
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            _screenVisabilityHandler = GetComponent<ScreenVisabilityHandler>();
            _dataSaver = new DataSaver();
            LoadSavedEntries();
        }

        private void OnEnable()
        {
            _addEntryButton.onClick.AddListener(OnAddEntryClicked);
            _settingsButton.onClick.AddListener(OnSettingsClicked);
            _addEntryScreen.DataSaved += EnablePlane;
            _addEntryScreen.BackButtonClicked += Enable;
            _viewEntryScreen.DeletedPlane += DeletePlane;
            _viewEntryScreen.DataUpdated += SaveItemPlanes;
            _viewEntryScreen.BackClicked += Enable;

            foreach (var entryPlane in _entryPlanes)
            {
                entryPlane.OpenClicked += OpenEntry;
            }
        }

        private void OnDisable()
        {
            _addEntryButton.onClick.RemoveListener(OnAddEntryClicked);
            _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            _addEntryScreen.DataSaved -= EnablePlane;
            _addEntryScreen.BackButtonClicked -= Enable;
            _viewEntryScreen.DeletedPlane -= DeletePlane;
            _viewEntryScreen.DataUpdated -= SaveItemPlanes;
            _viewEntryScreen.BackClicked -= Enable;

            foreach (var entryPlane in _entryPlanes)
            {
                entryPlane.OpenClicked -= OpenEntry;
            }
        }

        private void Start()
        {
            LoadSavedEntries();
            _screenVisabilityHandler.DisableScreen();
            ToggleEmptyPlane();
        }

        private void OpenEntry(EntryPlane entryPlane)
        {
            _viewEntryScreen.Enable(entryPlane);
            Disable();
        }

        private void LoadSavedEntries()
        {
            DisableAllPlanes();

            try
            {
                List<EntryData> loadedDatas = _dataSaver.LoadData();

                if (loadedDatas == null || loadedDatas.Count == 0)
                {
                    return;
                }

                for (int i = 0; i < loadedDatas.Count; i++)
                {
                    if (i >= _entryPlanes.Count)
                    {
                        break;
                    }

                    _entryPlanes[i].Enable(loadedDatas[i]);
                    _entryPlanes[i].gameObject.transform
                        .DOScale(Vector3.one, _animationDuration)
                        .From(Vector3.zero)
                        .SetEase(_animationEase);
                }

                ToggleEmptyPlane();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private void DisableAllPlanes()
        {
            foreach (var entryPlane in _entryPlanes)
            {
                entryPlane.Disable();
            }
        }

        public void Enable()
        {
            _screenVisabilityHandler.EnableScreen();

            if (_mainScreenCanvasGroup != null)
            {
                _mainScreenCanvasGroup.alpha = 0;
                _mainScreenCanvasGroup.DOFade(1f, _animationDuration)
                    .SetEase(_animationEase);
            }
        }

        public void Disable()
        {
            if (_mainScreenCanvasGroup != null)
            {
                _mainScreenCanvasGroup.DOFade(0f, _animationDuration)
                    .SetEase(_animationEase)
                    .OnComplete(() => _screenVisabilityHandler.DisableScreen());
            }
            else
            {
                _screenVisabilityHandler.DisableScreen();
            }
        }

        private void SaveItemPlanes()
        {
            try
            {
                List<EntryData> activeDatas = _entryPlanes
                    .Where(plane => plane.IsActive)
                    .Select(plane => plane.EntryData)
                    .ToList();

                _dataSaver.SaveData(activeDatas);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private void ToggleEmptyPlane()
        {
            bool shouldShowEmptyPlane = _entryPlanes.All(plane => !plane.IsActive);

            if (_emptyPlane != null)
            {
                _emptyPlane.transform.DOScale(shouldShowEmptyPlane ? Vector3.one : Vector3.zero, _animationDuration)
                    .SetEase(_animationEase);
                _emptyPlane.SetActive(shouldShowEmptyPlane);
            }
        }

        private void EnablePlane(EntryData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Enable();

            var availablePlane = _entryPlanes.FirstOrDefault(plane => !plane.IsActive);

            if (availablePlane == null)
                return;

            availablePlane.Enable(data);

            ToggleEmptyPlane();
            SaveItemPlanes();
        }

        private void DeletePlane(EntryPlane plane)
        {
            if (plane == null)
                throw new ArgumentNullException(nameof(plane));

            Enable();

            plane.Disable();
            SaveItemPlanes();
            ToggleEmptyPlane();
        }

        private void OnAddEntryClicked()
        {
            if (_mainScreenCanvasGroup != null)
            {
                _mainScreenCanvasGroup.DOFade(0f, _animationDuration)
                    .SetEase(_animationEase)
                    .OnComplete(() =>
                    {
                        _addEntryScreen.Enable();
                        Disable();
                    });
            }
            else
            {
                _addEntryScreen.Enable();
                Disable();
            }
        }

        private void OnSettingsClicked()
        {
        }
    }
}