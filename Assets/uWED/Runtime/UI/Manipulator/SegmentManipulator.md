# SegmentManipulator extension

Derive a subclass. Manual `is`/`as` checks against your derived `Segment` type, not generics on `SegmentManipulator<TSegment>` - avoids cascading constraints through `ManipulatorWindowBase<T>` for what's likely one or two extra types.

## Wire an unwired property

Override, call `base.LoadValues`/`base.WriteBack` first if you still want the base behavior:

- `LoadTextureInfo(NameTextureSlot slot, Segment copy)` - sets `slot.TextureNameValue.text`, `slot.ScaleValue.text`, `slot.TextureHintValue.text`.
- `LoadSlot1(NameTextureSlot slot1, Segment copy)` / `WriteBackSlot1(Segment target, NameTextureSlot slot1)` - default reads/writes `Segment.Name`/`.Offset`.
- `LoadSlot2(NameTextureSlot slot2, Segment copy)` / `WriteBackSlot2(Segment target, NameTextureSlot slot2)` - default no-op until Segment exposes a second Name/Offset.

```csharp
public class MySegmentManipulator : SegmentManipulator
{
    protected override void LoadSlot2(NameTextureSlot slot2, Segment copy)
    {
        if (copy is MySegment s)
        {
            slot2.NameDropdown.SetValueWithoutNotify(s.Name2);
            slot2.OffsetStepper.Value = s.Offset2;
        }
    }

    protected override void WriteBackSlot2(Segment target, NameTextureSlot slot2)
    {
        if (target is MySegment s)
        {
            s.Name2 = slot2.NameDropdown.value;
            s.Offset2 = slot2.OffsetStepper.Value;
        }
    }
}
```

## Add a new property

Read-only: `AddReadonlyRow(block, "Label")` in an overridden `PopulateContent` (call `base.PopulateContent` first), set `.text` in an overridden `LoadValues`.

Editable: build the field in `PopulateContent`, wire its change callback to a field on your derived `Segment`, set its value in `LoadValues`, write it back in an overridden `WriteBack`.

## Add a Tab

`TabView` (protected, on `ManipulatorWindowBase<T>`) is available once `base(...)` has run:

```csharp
public MySegmentManipulator(VisualTreeAsset baseUxml, IManipulatorSettings settings)
    : base(baseUxml, settings)
{
    var tab = new Tab { label = "Advanced" };
    var scroll = new ScrollView { verticalScrollerVisibility = ScrollerVisibility.Auto };
    scroll.AddToClassList("manip-scroll");
    // build fields, scroll.Add(...)
    tab.Add(scroll);
    TabView.Add(tab);
}
```
