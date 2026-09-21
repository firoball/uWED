    using UnityEngine.UIElements;

    public interface IFileDialog
    {
        public void New(DropdownMenuAction item);
        public void Load(DropdownMenuAction item);
        public void Save(DropdownMenuAction item);
        public void SaveAs(DropdownMenuAction item);
    }
