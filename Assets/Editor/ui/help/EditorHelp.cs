using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Ui.Help
{
    public class EditorHelp : VisualElement
    {
        //private VisualElement m_helpRoot;
        private TabView m_helpTabView;
        private int m_index = -1;
        
        const int GeneralTabIndex = 0;
        const int ModeTabOffset = 1;


        public EditorHelp(VisualElement parent, VisualTreeAsset uxml)
        {
            this.StretchToParentSize();
            pickingMode = PickingMode.Position; // explicit: acts as the click barrier
            focusable = true;                   // needed so it can receive KeyDownEvent
            style.display = DisplayStyle.None;
            
            VisualElement helpInstance = uxml.Instantiate();
            helpInstance.StretchToParentSize();
            helpInstance.pickingMode = PickingMode.Ignore; // must not block clicks itself
            Add(helpInstance); // added LAST -> renders above everything else, blocks clicks below it

            Button closeButton = helpInstance.Q<Button>("help-close-button");
            m_helpTabView = helpInstance.Q<TabView>("help-tabview");

            closeButton.clicked += CloseHelp;

            //TODO: unregister on close menu??
            RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Escape)
                {
                    CloseHelp();
                    evt.StopPropagation(); // don't let ESC also trigger "Exit Construction Mode" underneath
                }
            });            
        }

        public void OnSetMode(ChangeEvent<string> evt)
        {
            PopupField<string> field = evt.target as PopupField<string>;
            if (field != null && field.index >= 0 && field.index < (int)EditorStatus.Mode.Count)
            {
                m_index = field.index;
            }
        }
        
        public void OnOpenHelp(ClickEvent clickEvent)
        {
            style.display = DisplayStyle.Flex;

            m_helpTabView.selectedTabIndex = m_index >= 0
                ? m_index + ModeTabOffset
                : GeneralTabIndex;

            Focus(); // so ESC below actually reaches us
        }

        private void CloseHelp()
        {
            style.display = DisplayStyle.None;
        }
        
    }
}