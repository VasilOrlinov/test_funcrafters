using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace VasilVasilev.Data
{
    [Serializable]
    public struct DataItemsPackage 
    {
        public List<DataItem> items;
        public int startIndex;
        public int totalItemsOnServer;
    }
}
