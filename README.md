# uWED

A level editor for a classic 2.5D game engine, built as a Unity Editor extension on Unity 6 LTS (6.3.1) with UI Toolkit (UXML/USS) — no IMGUI.

uWED started as a project to give the old Acknex3 engine a modern map editor. It's built generically enough that other formats can be added later rather than being locked to Acknex from the ground up — Acknex (WDL/WMP) is just the first one.

This README is for people working on the editor itself. A section for actual map-makers will get added once there's something worth documenting for them.

## What it does

You edit level geometry — vertices, segments, regions, ways, objects — through a set of panels and per-object "Manipulator" popups. 2D editing works; 3D is in progress.

Longer term the goal is to run standalone, outside the Unity Editor, mainly to support an Acknex (WDL/WMP) format extension. That's why there's a hard line between UnityEditor-only code and portable runtime code — see below.

## Requirements

Unity 6.3 LTS (or close enough). Nothing else beyond what's already vendored in `ext/`.

## Layout

```
Runtime/   editor-independent code, has to stay UnityEditor-free — this includes Formats/
Editor/    UnityEditor-only code (Unity forces this to be top-level)
ext/       vendored third-party stuff, not part of uWED
```

Inside `Runtime/`:

- `Core/` — the domain model (`Vertex`, `Segment`, `Region`, `MapObject`, `Way`, all off `IndexedData`), editor modes/drawers, map I/O interfaces, geometry helpers
- `UI/` — the editor UI itself: the 2D view, the inspector, the Manipulator popups, the event bus, and the "Binder" classes that wire everything together
- `Platform/` — interfaces like `IFileDialogService`, `IMapPersistence`, `IUndoService`, `IPrefsProvider`, plus `Standalone/` implementations with no UnityEditor dependency
- `Formats/` — map format loaders/writers (WMP for now). Was originally meant to be top-level, moved in here to get rid of an asmdef circular dependency
- `Bootstrap/` — the standalone entry point

`Editor/` has the UnityEditor-backed side of those `Platform/` interfaces (`EditorPrefsProvider`, `EditorFileDialogService`, ...), the asset inspector stuff, and `EditorEntryPoint`, which is the actual `EditorWindow` that ties it all together.

There's also a `uWED.Acknex/` folder living in-repo for now (own asmdefs already, not split into a separate package yet) — Acknex asset/template model, asset storage, and eventually the Acknex-specific manipulators and WDL format on top of WMP.

Full tree with current TODOs: [`uWED-FolderStructure.md`](uWED-FolderStructure.md).

## A few rules the codebase follows

- Anything touching `UnityEditor` lives under `Editor/`, full stop — no matter how editor-y something in `Runtime/` feels. Where that's not true yet, it's marked `TODO(platform-abstraction)`, not worked around.
- Only `EditorEntryPoint` is allowed to construct the UnityEditor-backed implementations and wire them up.
- New interfaces/events should look like the ones already there (`IPrefsProvider`, `BusEvent<T>`) rather than inventing a new shape.
- No half-way folder structures — an explicit TODO beats a "temporary" compromise that sticks around.
- UI Toolkit event phase matters: bubble-phase handlers on children can eat events before a parent-level handler sees them, so system-level input goes on the capture phase.
- Domain reload destroys `PreviewRenderUtility` instances and code-created materials, even though the C# objects survive — needs an `AssemblyReloadEvents` hook.
- Fix layout in USS (e.g. `height` vs `max-height`), not with hardcoded pixels in code.
- UI elements get built once in the constructor and updated via `Bind()`/`Set()`/`Clear()` afterwards — some of this runs on every mouse-move, so no rebuilding.
- The object-type Manipulators should all expose the same method shapes — no picking and choosing which methods a given one has.

## Where things stand

- Domain model, `EditorEventBus`, and the Manipulator window base are done.
- `VertexManipulator` is finished. `SegmentManipulator` works for what's confirmed so far — a second name/texture slot has UI but isn't wired up yet.
- Still in progress: Region/Way/Object manipulators, the 3D mode, and moving the remaining `EditorPrefs` calls behind `Platform/` interfaces.
- The reusable `ComboBoxField` control is a separate piece under `ext/GenericComboBoxField/`. WDL/Acknex support is underway in `uWED.Acknex/` but the format loader and Acknex-specific manipulators aren't built yet.

## Contributing

- Comments and any `.md` docs are in English, describe the code as it is (not how it got there), kept short.
- Target Unity 6.3 LTS.
- No `EditorPrefs`/UnityEditor calls outside `Editor/` — go through `Platform/`.

## License

CC BY-NC 4.0 — see [`LICENSE.md`](LICENSE.md).
