# uWED – Folder Structure (as of: standalone preparation + prefs abstraction)

```
Runtime/
├── Core/
│   ├── Modes/
│   │   ├── BaseEditorMode.cs
│   │   ├── ObjectMode.cs
│   │   ├── RegionMode.cs
│   │   ├── SegmentMode.cs
│   │   └── WayMode.cs
│   ├── Drawers/
│   │   ├── BaseEditorDrawer.cs
│   │   ├── ObjectDrawer.cs
│   │   ├── RegionDrawer.cs
│   │   ├── SegmentDrawer.cs
│   │   └── WayDrawer.cs
│   ├── Map/
│   │   ├── Model/
│   │   │   ├── IndexedData.cs
│   │   │   ├── MapObject.cs
│   │   │   ├── Vertex.cs
│   │   │   ├── VertexComparer.cs
│   │   │   ├── Region.cs
│   │   │   ├── Segment.cs
│   │   │   └── Way.cs
│   │   ├── Container/
│   │   │   ├── MapData.cs            // Aggregate Root
│   │   │   └── MapDataSet.cs
│   │   ├── Support/
│   │   │   ├── Contour.cs
│   │   │   └── MeshManager.cs
│   │   └── IO/
│   │       ├── IMapLoader.cs
│   │       └── IMapWriter.cs
│   ├── Model/
│   │   ├── CursorInfo.cs
│   │   └── EditorStatus.cs
│   └── Utilities/
│       ├── ContourHelper.cs
│       ├── Geom2D.cs
│       ├── SegmentHelper.cs
│       └── LockAngleUtility.cs
│
├── UI/
│   ├── EventBus/
│   │   └── EditorEventBus.cs
│   ├── Binder/
│   │   ├── InspectorBinder.cs
│   │   ├── ManipulatorBinder.cs
│   │   ├── KeyBinder.cs
│   │   ├── MenuBinder.cs             // TODO(platform-abstraction): EditorUtility.OpenFilePanel instead
│   │   │                             //   of IFileDialogService; review ToolbarMenu usage
│   │   └── PrefsBinder.cs            // resolves IPrefsProvider via ServiceLocator, calls SetPrefsProvider
│   │                                 // on all held IPrefsPersistable instances, subscribes to
│   │                                 // OnSavePrefs/OnLoadPrefs on EditorEventBus (SavePrefsRequested/
│   │                                 // LoadPrefsRequested); domain classes know neither the bus nor
│   │                                 // the ServiceLocator
│   ├── Inspector/
│   │   ├── Resources/
│   │   │   ├── InfoPanel.uss
│   │   │   ├── MeshPreviewPanel.uss  // TODO(platform-abstraction): see MeshPreviewPanel.cs
│   │   │   └── StatisticsPanel.uss
│   │   ├── DetailViewBase.cs
│   │   ├── InfoEntities.cs
│   │   ├── InfoPanel.cs              // TODO(platform-abstraction): switch USS loading to Resources.Load
│   │   ├── InfoPanelStats.cs
│   │   ├── InfoPanelViews.cs
│   │   ├── MeshPreviewPanel.cs       // TODO(platform-abstraction): PreviewRenderUtility is UnityEditor-only;
│   │   │                             //   possibly remove entirely, or find a standalone alternative
│   │   │                             //   without a UnityEditor dependency
│   │   └── StatisticsPanel.cs
│   ├── Manipulator/
│   │   ├── Resources/
│   │   │   └── ManipulatorBase.uss
│   │   ├── ManipulatorBase.uxml
│   │   ├── IGenericNameProvider.cs
│   │   ├── ManipulatorInterfaces.cs
│   │   ├── ManipulatorSettings.cs    // implements IPrefsPersistable; TODO(platform-abstraction):
│   │   │                             //   Load/SavePrefs not yet switched to IPrefsProvider
│   │   ├── ManipulatorWindowBase.cs  // TODO(platform-abstraction): switch USS loading to Resources.Load
│   │   ├── MapObjectManipulator.cs
│   │   ├── NameSanitizer.cs
│   │   ├── NameTextureSlot.cs
│   │   ├── NumberStepperField.cs
│   │   ├── RegionManipulator.cs
│   │   ├── SegmentManipulator.cs
│   │   ├── SegmentManipulator.md
│   │   ├── SimpleGenericNameProvider.cs
│   │   ├── SimpleTextureProvider.cs
│   │   ├── Vector2StepperField.cs
│   │   ├── VertexManipulator.cs
│   │   └── WayManipulator.cs
│   ├── View2D/
│   │   ├── ContentDragger.cs
│   │   ├── ContentZoomer.cs
│   │   ├── EditorManipulator.cs      // implements IPrefsPersistable; TODO(platform-abstraction):
│   │   │                             //   Load/SavePrefs not yet switched to IPrefsProvider
│   │   ├── EditorView.cs             // implements IPrefsPersistable; TODO(platform-abstraction):
│   │   │                             //   Load/SavePrefs not yet switched to IPrefsProvider
│   │   ├── GridBackground.cs         // TODO(platform-abstraction): HandleUtility.ApplyWireMaterial via
│   │   │                             //   reflection (BindingFlags.NonPublic), UnityEditor-only
│   │   ├── GridManipulator.cs        // implements IPrefsPersistable; TODO(platform-abstraction):
│   │   │                             //   Load/SavePrefs not yet switched to IPrefsProvider
│   │   ├── GridView.cs
│   │   └── View2DFacade.cs           // formerly EditorInterface.cs; facade for MenuBinder access to View2D
│   ├── Help/
│   │   ├── Resources/
│   │   │   └── EditorHelp.uss
│   │   ├── EditorHelp.cs             // TODO(platform-abstraction): switch USS loading to Resources.Load
│   │   └── EditorHelp.uxml
│   └── uWED.uss / uWED.uxml
│
├── Platform/
│   ├── IFileDialogService.cs
│   ├── IMapPersistence.cs
│   ├── IUndoService.cs
│   ├── IPrefsProvider.cs             // HasKey, GetFloat/SetFloat, GetBool/SetBool
│   ├── IPrefsPersistable.cs          // SetPrefsProvider(IPrefsProvider), OnSavePrefs(), OnLoadPrefs()
│   ├── ServiceLocator.cs
│   └── Standalone/
│       ├── StandaloneFileDialogService.cs
│       ├── StandaloneMapPersistence.cs
│       ├── StandaloneUndoService.cs
│       ├── PlayerPrefsProvider.cs
│       └── FilePrefsProvider.cs
│
├── Formats/                           // moved here from top-level (see note below)
│   └── Wmp/
│       └── MapWmpLoader.cs           // Acknex base/default format; stays in uWED for now (like most
│                                      //   of the code currently), moves to uWED.Acknex/Formats/ only
│                                      //   once Acknex is split out into an external repo
│
└── Bootstrap/
    └── StandaloneEntryPoint.cs        // MonoBehaviour, UnityEngine-only

Editor/
├── Bootstrap/
│   └── EditorEntryPoint.cs           // EditorWindow (formerly UWed.cs); [MenuItem] entry point, registers
│                                     //   Editor-only providers in the ServiceLocator, builds rootVisualElement
│                                     //   from uWED.uxml/.uss, wires up View2DFacade/EventBus/Binder
├── AssetInspector/
│   ├── MapAssetCleaner.cs
│   ├── MapAssetEditor.cs
│   ├── MapAssetStatistics.cs
│   └── MapAssetViewer.cs
└── Platform/
    ├── EditorFileDialogService.cs
    ├── EditorMapPersistence.cs
    ├── EditorUndoService.cs
    ├── EditorPrefsProvider.cs        // uses EditorPrefs, necessarily Editor-only
    ├── MapAsset.cs                   // ScriptableObject wrapper
    ├── MapAssetLoader.cs
    └── MapAssetWriter.cs

ext/                                 // external third-party repos, no uWED semantics, outside Runtime/Editor
├── GenericComboBoxField/
├── Triangulator/
├── WDL2CS/
└── WMPio/
```

> **Note — `Formats/` moved into `Runtime/`.** `Formats/` was originally planned as a top-level folder, sibling to `Runtime/` and `Editor/`. It was moved into `Runtime/Formats/` because keeping it top-level caused asmdef circular dependencies. This does not change the file's role or its future path (see below) — only where it currently sits.

---

# uWED.Acknex – current state

`uWED.Acknex` is an in-repo folder inside the uWED project (own `asmdef`s already in place,
not yet split into a separate repo). Current layout:

```
uWED.Acknex/
├── Editor/
│   ├── Bootstrap/
│   │   └── AcknexEditorBootstrap.cs
│   ├── Platform/
│   │   └── EditorAssetStorageService.cs
│   └── uWED.Acknex.Editor.asmdef
└── Runtime/
    ├── Model/
    │   ├── AcknexFlags.cs
    │   ├── Assets/
    │   │   ├── Bitmap.cs
    │   │   ├── Bmap.cs
    │   │   ├── ClassicAsset.cs
    │   │   ├── Flic.cs
    │   │   ├── Font.cs
    │   │   ├── Model.cs
    │   │   ├── Music.cs
    │   │   ├── Ovly.cs
    │   │   ├── Palette.cs
    │   │   └── Sound.cs
    │   ├── Instances/
    │   │   └── TextureInstance.cs
    │   └── Templates/
    │       ├── ActorTargetRef.cs
    │       ├── ActorTemplate.cs
    │       ├── BaseObjectTemplate.cs
    │       ├── MapObjectTemplate.cs
    │       ├── RegionTemplate.cs
    │       ├── TemplateAsset.cs
    │       ├── TextureTemplate.cs
    │       ├── ThingTemplate.cs
    │       └── WallTemplate.cs
    ├── Registry/
    │   ├── IAssetStorageService.cs
    │   └── TemplateRegistry.cs
    └── uWED.Acknex.Runtime.asmdef
```

Notes:
- Split into `Editor/`/`Runtime/` with their own asmdefs, mirroring the top-level uWED split —
  superseding the earlier assumption that no `Editor/` subfolder would be needed here.
- `AcknexMapObject`/`AcknexRegion`/`AcknexWay` (subclasses of `MapObject`/`Region`/`Way`) and the
  WDL format (`Formats/Wdl/WdlMapLoader.cs`/`WdlMapWriter.cs`) described in earlier planning are
  not present yet — current work is the classic-asset model (`Model/Assets`), the
  Template/Instance property system (`Model/Templates`, `Model/Instances`), and asset storage
  (`Registry/IAssetStorageService`, `Editor/Platform/EditorAssetStorageService`).
- `AcknexRegionManipulator`/`AcknexWayManipulator` (planned `Manipulator/` subclasses) not present yet.
- Later split into a real repo: `uWED.Acknex/` becomes 1:1 the `Runtime/`+`Editor/` of a new
  package repo; `Runtime/Formats/Wmp` moves out of uWED to join it at that point (once WDL depends
  on it), plus asmdef references to `uWED.Runtime`/`uWED.Editor`.
- The same pattern (in-repo folder first, separate repo later) applies to a future `uWED.Doom`.

---

## Open items / future extension architecture

- **Engine extensions (Doom, Acknex, ...):** the interim step is an in-repo folder
  (`uWED.Acknex/`) inside the uWED project, not an immediate separate repo. Only once
  an engine is actually split out does it become its own UPM package with an
  `.asmdef` that references `uWED` as a dependency. Each one gets its own `Map/`
  (subclasses of `MapObject`/`Region`/`Vertex` etc.), `Formats/` (implementing
  `IMapLoader`/`IMapWriter`, possibly building on another as WDL does on WMP), and
  `Manipulator/` (subclasses of `ManipulatorWindowBase`). `Formats/Wmp` stays in uWED
  for now since it's the Acknex base/default format and most of the code currently
  still lives there.
- **`Binder` naming:** renaming to `Coordinator`/`Wiring` was discussed but not
  finally decided — currently still called `Binder`.
- **`MenuBinder`:** still contains `UnityEditor` references that should eventually
  go away (goal: fully standalone-capable, like `KeyBinder`).
- All extension points (`MapObject`, `ManipulatorWindowBase`, `IMapLoader`, ...)
  must be `public`/`virtual`/`abstract` so external packages can cleanly inherit/implement them.
