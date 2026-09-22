namespace uWED.Runtime.Core.Map.Model
{
    public abstract class IndexedData
    {
        private int m_index;
    
        public int Index
        {
            get => m_index;
            set => m_index = value;
        }

    }
}
