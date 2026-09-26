# uWED – Folder Structure

Folder tree only — no individual files, since that list goes stale fast. Check the repo for the current file set.

## Top level

```
Runtime/
Editor/
ext/
```

- **`Runtime/`** — must not reference `UnityEditor`. Everything that isn't inherently editor-only lives here, including things that only run inside the Unity Editor today but could plausibly run standalone later.
- **`Editor/`** — anything using `UnityEditor` APIs. Unity requires editor-only code to live under a top-level `Editor/` folder, which is the one exception to keeping things under `Runtime/`.
- **`ext/`** — vendored third-party code (e.g. `GenericComboBoxField/`, `Triangulator/`, `WDL2CS/`, `WMPio/`). No uWED-specific logic goes here.

## Inside `Runtime/`

```
Runtime/
├── Bootstrap/
│   └── Resources/
├── Core/
│   ├── Drawers/
│   ├── Map/
│   │   ├── Container/
│   │   ├── IO/
│   │   ├── Model/
│   │   └── Support/
│   ├── Model/
│   ├── Modes/
│   └── Utilities/
├── Formats/
│   └── Wmp/
├── Platform/
│   └── Standalone/
└── UI/
    ├── Binder/
    ├── EventBus/
    ├── Help/
    │   └── Resources/
    ├── Inspector/
    │   └── Resources/
    ├── Manipulator/
    │   └── Resources/
    └── View2D/
```

- **`Bootstrap/`** — shared, UnityEditor-independent startup/wiring, used by both the Editor entry point and (eventually) a standalone entry point. Its `.uxml`/`.uss` live in `Resources/`.
- **`Core/`** — the domain model and logic. `Map/Model/` holds the data types themselves, `Map/IO/` how they're read/written, `Map/Container/` and `Map/Support/` the aggregate/contour/mesh support around them, `Modes/` and `Drawers/` the per-object-type editing/rendering, `Utilities/` shared geometry helpers.
- **`Formats/`** — one subfolder per map format, each holding its loader/writer pair (e.g. `Wmp/`). Lives under `Runtime/` rather than top-level to avoid an asmdef circular dependency.
- **`Platform/`** — the interfaces that abstract over anything that differs between the Unity Editor and a standalone build (file dialogs, prefs, defaults, ...), with `Standalone/` for their non-Editor implementations.
- **`UI/`** — the editor UI itself, built with UI Toolkit and free of UnityEditor dependencies. `EventBus/` for cross-cutting notifications, `Binder/` wires UI to domain/platform services, `Inspector/` is the object detail/preview panels, `Manipulator/` the per-object-type edit popups plus shared building blocks, `View2D/` the 2D viewport, `Help/` in-app help. Each UI subfolder that needs UXML/USS keeps them in its own `Resources/`.

Plus `uWED.Runtime.asmdef` at the root of `Runtime/`.

## Inside `Editor/`

```
Editor/
├── AssetInspector/
├── Bootstrap/
└── Platform/
```

- **`Bootstrap/`** — the actual Unity entry point(s): `EditorWindow`, `[MenuItem]`, menu wiring — calls into `Runtime/Bootstrap/` for the shared setup.
- **`AssetInspector/`** — custom inspectors/editors for the map asset.
- **`Platform/`** — the UnityEditor-backed implementation of each `Runtime/Platform/` interface, plus the map `ScriptableObject` wrapper and its loader/writer.

Plus `uWED.Editor.asmdef` at the root of `Editor/`.

## Format/persistence status

`IMapPersistence`, `IUndoService`, and `IPrefsPersistable` (an interface sketched at one point for save/load hooks) aren't implemented — not currently in scope. `Platform/Standalone/` is correspondingly empty for now.

---

# uWED.Acknex

Same `Editor/`/`Runtime/` split as top-level uWED, with its own asmdefs — still an in-repo folder, not split into a separate package yet.

```
uWED.Acknex/
├── Editor/
│   ├── Bootstrap/
│   └── Platform/
└── Runtime/
    ├── Model/
    │   ├── Assets/
    │   ├── Instances/
    │   └── Templates/
    └── Registry/
```

- **`Runtime/Model/Assets/`** — classic asset file formats (bitmaps, sounds, fonts, palettes, ...).
- **`Runtime/Model/Templates/`** — the Wall/Thing/Actor/Region property templates (shared "type" definitions).
- **`Runtime/Model/Instances/`** — per-map-object instance data that references a template (e.g. a texture instance).
- **`Runtime/Registry/`** — lookup/storage abstraction for templates and assets.
- **`Editor/Platform/`** — the Editor-backed asset storage implementation.
- **`Editor/Bootstrap/`** — Acknex-specific editor wiring.

Not built yet, but expected to follow the pattern above once needed: `AcknexMapObject`/`AcknexRegion`/`AcknexWay` subclasses, the WDL format (on top of WMP), and Acknex-specific Manipulator subclasses.

---

## Open items / future extension architecture

- **Engine extensions (Doom, Acknex, ...):** the interim step is an in-repo folder (`uWED.Acknex/`) inside the uWED project, not an immediate separate repo. Only once an engine is actually split out does it become its own UPM package with an `.asmdef` that references `uWED` as a dependency. Each one gets its own `Model/` (subclasses of `MapObject`/`Region`/`Vertex` etc.), `Formats/` (implementing `IMapLoader`/`IMapWriter`, possibly building on another as WDL does on WMP), and `Manipulator/` (subclasses of `ManipulatorWindowBase`). `Formats/Wmp` stays in uWED for now since it's the Acknex base/default format and most of the code currently still lives there.
- **`Binder` naming:** renaming to `Coordinator`/`Wiring` was discussed but not finally decided — currently still called `Binder`.
- All extension points (`MapObject`, `ManipulatorWindowBase`, `IMapLoader`, ...) must be `public`/`virtual`/`abstract` so external packages can cleanly inherit/implement them.
