using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VasilVasilev.Data;
using VasilVasilev.Events;
using EventType = VasilVasilev.Events.EventType;

namespace VasilVasilev.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Transform _rootDataItemViews;
        [SerializeField] private GameObject _loadingCircleGameObject;
        [SerializeField] private Button _buttonPrevious;
        [SerializeField] private Button _buttonNext;
        [Header("Prefabs")]
        [SerializeField] private GameObject _dataItemViewPrefab;

        private List<DataItemView> _activeItemViews = new List<DataItemView>();

        private void OnEnable()
        {
            EventManager.Instance.AddListener<DataItemsPackage>(EventType.ShowDataItems, OnShowDataItemsEvent);
            EventManager.Instance.AddListener(EventType.ShowLoadingCircle, OnShowLoadingCircleEvent);
            _buttonPrevious.onClick.AddListener(OnPreviousButtonClick);
            _buttonNext.onClick.AddListener(OnNextButtonClick);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener<DataItemsPackage>(EventType.ShowDataItems, OnShowDataItemsEvent);
            EventManager.Instance.RemoveListener(EventType.ShowLoadingCircle, OnShowLoadingCircleEvent);
            _buttonPrevious.onClick.RemoveListener(OnPreviousButtonClick);
            _buttonNext.onClick.RemoveListener(OnNextButtonClick);
        }

        private void OnNextButtonClick()
        {
            EventManager.Instance.TriggerEvent(EventType.NextDataItemsRequested);
        }

        private void OnPreviousButtonClick()
        {
            EventManager.Instance.TriggerEvent(EventType.PreviousDataItemsRequested);
        }

        private void OnShowLoadingCircleEvent()
        {
            _loadingCircleGameObject.SetActive(true);
            _rootDataItemViews.gameObject.SetActive(false);
            _buttonPrevious.interactable = false;
            _buttonNext.interactable = false;
        }

        private void HideLoadingCircle()
        {
            _loadingCircleGameObject.SetActive(false);
        }
        
        private void OnShowDataItemsEvent(DataItemsPackage package)
        {
            HideLoadingCircle();
            _rootDataItemViews.gameObject.SetActive(true);

            UpdateUI(package.items,package.startIndex, package.totalItemsOnServer);
        }

        private void UpdateUI(List<DataItem> items, int firstItemIndex,int totalItems)
        {
            UpdateDataItemViewsCount(items.Count);
            int currentItemIndex = firstItemIndex;
            for (int i = 0; i < items.Count; i++)
            {
                _activeItemViews[i].Setup(items[i], currentItemIndex + i + 1);
            }
            
            _buttonPrevious.interactable = firstItemIndex > 0;
            _buttonNext.interactable = totalItems - (firstItemIndex + items.Count) > 0;
        }

        private void UpdateDataItemViewsCount(int desiredCount)
        {
            while (_activeItemViews.Count < desiredCount)
            {
                _activeItemViews.Add(Instantiate(_dataItemViewPrefab, _rootDataItemViews).GetComponent<DataItemView>());
            }

            for (int i = 0; i < _activeItemViews.Count; i++)
            {
                _activeItemViews[i].gameObject.SetActive(i < desiredCount);
            }
        }
    }
}