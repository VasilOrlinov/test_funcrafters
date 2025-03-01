using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using VasilVasilev.Data;
using VasilVasilev.Events;
using EventType = VasilVasilev.Events.EventType;

namespace VasilVasilev.Core
{
    public class App : Singleton<App>
    {        
        [Header("Configuration")]
        [SerializeField] int _dataItemsPerPage = 5;
        [Header("Optimization")]
        [SerializeField] private int _pagesToPreload = 2;
        [Header("Mockup Server Config")]
        [SerializeField] bool _useMockupServer = true;
        [SerializeField] int _mockupMinDelay = 200;
        [SerializeField] int _mockupMaxDelay = 1200;
        [SerializeField] int _mockupMinItems = 15;
        [SerializeField] int _mockupMaxItems = 30;

        private int _currentDataItemStartIndex;

        private DataHandler _dataHandler;
        CancellationTokenSource _cancellationTokenSource;
        
        private void Start()
        {
            IDataServer dataServer = _useMockupServer ? new DataServerMock(_mockupMinDelay, _mockupMaxDelay, _mockupMinItems, _mockupMaxItems) : new DataServerMock();
            if (!_useMockupServer)
            {
                Debug.LogWarning("No Mockup Server is configured, creating a new one with default values. No other classes implementing IDataServer exists yet.");
            }
            
            _dataHandler = new DataHandler(dataServer,_pagesToPreload * _dataItemsPerPage );
            
            EventManager.Instance.AddListener(EventType.NextDataItemsRequested, OnNextDataItemsRequested);
            EventManager.Instance.AddListener(EventType.PreviousDataItemsRequested, OnPreviousDataItemsRequested);

            _ = RequestDataItems(0, _dataItemsPerPage);
        }
 
        private void OnDestroy()
        {
            EventManager.Instance.RemoveListener(EventType.NextDataItemsRequested, OnNextDataItemsRequested);
            EventManager.Instance.RemoveListener(EventType.PreviousDataItemsRequested, OnPreviousDataItemsRequested);
           
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose(); 
        }

        private async Task RequestDataItems(int startIndex, int desiredCount)
        {
            EventManager.Instance.TriggerEvent(EventType.ShowLoadingCircle);
            CancelAndAssignCancellationTokenSource();
            int totalItems = await _dataHandler.GetDataCount(_cancellationTokenSource.Token);
            int remainingItemsCountAfterIndex = totalItems - startIndex;
            if (desiredCount + startIndex > totalItems)
            {
                if (remainingItemsCountAfterIndex <= 0)
                {
                    Debug.LogWarning("Requested count is less than total items count");
                    return;
                }
                
            }
            List<DataItem> items = await _dataHandler.GetItemsAsync(startIndex, math.min(desiredCount,remainingItemsCountAfterIndex), _cancellationTokenSource.Token);
            if (items == null)
            {
                Debug.LogWarning("Getting items failed");
                return;
            }
            
            if (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                _currentDataItemStartIndex = startIndex;
                DataItemsPackage package = new DataItemsPackage(){items = items, startIndex = _currentDataItemStartIndex, totalItemsOnServer = totalItems };
                EventManager.Instance.TriggerEvent<DataItemsPackage>(EventType.ShowDataItems, package);
            }
        }
        
        private void OnNextDataItemsRequested()
        {
            _ = RequestDataItems(_currentDataItemStartIndex + _dataItemsPerPage,_dataItemsPerPage);
        }
        
        private void OnPreviousDataItemsRequested()
        {
            _ = RequestDataItems(_currentDataItemStartIndex - _dataItemsPerPage,_dataItemsPerPage);
        }

        void CancelAndAssignCancellationTokenSource()
        {
            CancellationTokenSource oldToken = _cancellationTokenSource;
            _cancellationTokenSource = new CancellationTokenSource();
            oldToken?.Cancel();
            oldToken?.Dispose();
        }
    }
}
