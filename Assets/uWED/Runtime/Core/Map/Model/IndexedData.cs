using System;
using UnityEngine;

namespace uWED.Runtime.Core.Map.Model
{
    [Serializable]
    public abstract class IndexedData
    {
        private int m_index;
        [SerializeField]
        private int m_typeId;
    
        public int Index
        {
            get => m_index;
            set => m_index = value;
        }

        public int TypeId
        {
            get => m_typeId;
            set => m_typeId = value;
        }
    }
}
