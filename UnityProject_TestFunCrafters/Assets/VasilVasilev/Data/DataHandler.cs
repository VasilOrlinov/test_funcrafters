using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace VasilVasilev.Data
{
    public class DataHandler 
    {
        private IDataServer _dataServer;
        private List<DataItem> _cachedDataItemsList = new List<DataItem>(); 
        private int? _cachedDataCount = null;
        private int _itemsToPreload;
        private CancellationTokenSource _preloadCancellationTokenSource = new CancellationTokenSource();

        public DataHandler(IDataServer dataServer, int itemsToPreload = 5)
        {
            _dataServer = dataServer;
            _itemsToPreload = itemsToPreload;
        }

        /// <summary>
        /// Returns the total data items count on the server
        /// </summary>
        public async Task<int> GetDataCount(CancellationToken cancellationToken)
        {
            if (_cachedDataCount.HasValue)
                return _cachedDataCount.Value;

            try
            {
                _cachedDataCount = await _dataServer.DataAvailable(cancellationToken);
            }
            catch (TaskCanceledException)
            {
            }

            return _cachedDataCount ?? 0;
        }

        /// <summary>
        /// Returns requested DataItems as a List. Checks the cache first before fetching from server. Returns null if out of range. 
        /// </summary>
        public async Task<List<DataItem>> GetItemsAsync(int startIndex, int itemCount, CancellationToken cancellationToken)
        {
            int totalItems = await GetDataCount(cancellationToken);
            if (startIndex >= totalItems)
            {
                return null;
            }

            int fetchCount = Mathf.Min(itemCount, totalItems - startIndex);
            bool isDataCached = startIndex + fetchCount <= _cachedDataItemsList.Count;

            if (isDataCached)
            {
                if (startIndex + fetchCount + _itemsToPreload > _cachedDataItemsList.Count)
                {
                    _ = PreloadData(_cachedDataItemsList.Count);
                }
                return _cachedDataItemsList.Skip(startIndex).Take(fetchCount).ToList();
            }

            CancelPreloading();

            try
            {
                var newItems = await _dataServer.RequestData(startIndex, fetchCount, cancellationToken);
                _cachedDataItemsList.AddRange(newItems);

                if (_cachedDataItemsList.Count < totalItems)
                {
                    _ = PreloadData(_cachedDataItemsList.Count);
                }

                return newItems.ToList();
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }

        private async Task PreloadData(int startIndex)
        {
            int totalItems = await GetDataCount(CancellationToken.None);

            if (startIndex >= totalItems)
            {
                return;
            }

            int fetchCount = Mathf.Min(_itemsToPreload, totalItems - startIndex);

            _preloadCancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = _preloadCancellationTokenSource.Token;

            try
            {
                var nextItems = await _dataServer.RequestData(startIndex, fetchCount, token);
                if (!token.IsCancellationRequested)
                {
                    _cachedDataItemsList.AddRange(nextItems);
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        private void CancelPreloading()
        {
            if (_preloadCancellationTokenSource != null)
            {
                _preloadCancellationTokenSource.Cancel();
                _preloadCancellationTokenSource.Dispose();
                _preloadCancellationTokenSource = new CancellationTokenSource();
            }
        }
    }
}