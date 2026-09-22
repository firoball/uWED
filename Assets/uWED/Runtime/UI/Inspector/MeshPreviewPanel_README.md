# MeshPreviewPanel — Integration How-To

UIElements panel that shows a rotatable preview of a `Mesh`, styled and
positioned to sit next to the `InfoPanel` at the bottom of the level
editor window. No `UnityEditor` dependency — works identically in-editor
and in a standalone build.

## Files

- `MeshPreviewPanel.cs` — the panel (`Editor.UI.Inspector.MeshPreviewPanel`)
- `MeshPreviewPanel.uss` — its styling
- `IsolatedScene.cs` — reusable helper (`UWED.Runtime.Core.IsolatedScene`)
  that gives a consumer its own hidden Scene + rendering layer; used here
  for the preview camera/lights, but written to be reused elsewhere too
  (e.g. a future 3D edit mode) — each consumer creates its own instance.

`MeshPreviewPanel.uss` must be loadable via `Resources.Load<StyleSheet>`
at runtime, i.e. placed under a `Resources/` folder as `MeshPreviewPanel`
(`Resources/MeshPreviewPanel.uss`).

## 0. One-time project setup: reserve a layer

`IsolatedScene` needs a dedicated rendering layer to keep its objects from
being seen by, or affecting, the rest of the scene. In **Project Settings
→ Tags and Layers**, add a layer named exactly `IsolatedPreview`
(`IsolatedScene.LayerName`). Missing this throws a clear exception at
first use rather than silently rendering wrong.

## 1. Adding it to the window

No shared container is required. `MeshPreviewPanel` positions itself via
`position: absolute` (bottom right) in its own USS, same as `InfoPanel`
(bottom left) — add both as children of the same container `InfoPanel`
already uses (e.g. `editor`):

```csharp
editor?.Add(m_editorView);
editor?.Add(m_infoPanel);
editor?.Add(m_meshPreviewPanel);
```

Because it only occupies its own small rect, it does not intercept pointer
events anywhere else — clicks outside the panel reach whatever is
underneath as usual.

## 2. Size — fixed square matching InfoPanel

`MeshPreviewPanel.uss` defines `--panel-height` (currently `174px`,
matching `InfoPanel.uss`'s height) and uses it for both width and height —
the panel is a fixed square, not resized dynamically. Update that one
value if `InfoPanel`'s height changes.

## 3. Lifecycle: create once, like InfoPanel

`MeshPreviewPanel` mirrors `InfoPanel`'s lifecycle: create a single
instance once (e.g. in `EditorWindow.OnEnable`, or the standalone
equivalent), not per mode-switch. It starts hidden; `Set()` shows it,
`Clear()` hides it (next section) — use those for mode enter/exit as well
as for changing the displayed mesh.

```csharp
void OnEnable()
{
    meshPreviewPanel = new MeshPreviewPanel();
    rootVisualElement.Add(meshPreviewPanel);
    // ... wire orbit forwarding (see below) ...
}

void OnDisable()
{
    meshPreviewPanel.Dispose();
}
```

One difference from `InfoPanel`: `MeshPreviewPanel` **does** need
`Dispose()` even though it's only created once, because it owns native
resources — an `IsolatedScene`, a `Camera`, and a `RenderTexture` —
`InfoPanel` has no equivalent, which is presumably why it doesn't need
one. `Dispose()` also fires automatically if the element is ever removed
from the panel hierarchy, as a safety net, but calling it explicitly on
window-close is the main path. Resources are also rebuilt automatically
if destroyed externally (e.g. an editor domain reload tearing down the
isolated scene) — `EnsurePreviewResources()` detects this via Unity's
"fake-null" comparison and recreates everything on the next render.

## 4. Set / Clear

Mirrors `InfoPanel`'s `Set()` / `Clear()` naming. Unlike `InfoPanel`,
these also control visibility here — there's no separate `Show()`/`Hide()`:

```csharp
// Shows the panel with a new mesh, replacing whatever was shown before:
meshPreviewPanel.Set(myMesh);
meshPreviewPanel.Set(myMesh, myMaterials);

// Hides the panel and drops the current mesh:
meshPreviewPanel.Clear();
```

Use `Clear()` on mode-exit and whenever there's nothing to preview;
`Set()` on mode-enter (once a mesh is available) and whenever the
displayed mesh changes.

## 5. Orbit input (CTRL+drag) — must work anywhere in the window

A `MouseDownEvent` registered directly on `MeshPreviewPanel` would only
fire while the pointer is over its own (small) rect. Since CTRL+drag must
orbit the mesh no matter where the pointer is, `MeshPreviewPanel` does
**not** register its own mouse handlers at all. Instead it exposes:

```csharp
public void BeginOrbit(Vector2 mousePosition);
public void UpdateOrbit(Vector2 mousePosition);
public void EndOrbit();
public bool IsOrbiting { get; }
```

Forward events into these from a VisualElement that covers the whole area
where orbiting should work — typically `rootVisualElement`:

```csharp
rootVisualElement.RegisterCallback<MouseDownEvent>(evt =>
{
    if (evt.button == 0 && evt.ctrlKey)
    {
        meshPreviewPanel.BeginOrbit(evt.mousePosition);
        rootVisualElement.CaptureMouse(); // keep receiving move events past window edges
        evt.StopPropagation();
    }
});

rootVisualElement.RegisterCallback<MouseMoveEvent>(evt =>
{
    if (!meshPreviewPanel.IsOrbiting)
        return;

    if (!evt.ctrlKey)
    {
        meshPreviewPanel.EndOrbit();
        rootVisualElement.ReleaseMouse();
        return;
    }

    meshPreviewPanel.UpdateOrbit(evt.mousePosition);
    evt.StopPropagation();
});

rootVisualElement.RegisterCallback<MouseUpEvent>(evt =>
{
    if (!meshPreviewPanel.IsOrbiting)
        return;

    meshPreviewPanel.EndOrbit();
    rootVisualElement.ReleaseMouse();
    evt.StopPropagation();
});
```

`evt.StopPropagation()` while orbiting is what stops other elements (e.g.
`InfoPanel`, the viewport) from also reacting to the same drag — only call
it once `IsOrbiting` is true, so ordinary clicks/drags elsewhere in the
window are unaffected.

## 6. Behaviour notes

- **Full visibility, tightly framed**: camera distance is refit each
  render to the mesh's AABB corners at the *current* orbit orientation
  (~2% margin), not a fixed worst-case bounding-sphere distance — the
  mesh stays fully in frame at any angle without the excess empty space a
  sphere fit would leave for non-spherical shapes.
- **Fixed square size**: no dynamic width — see section 2.
- **Opaque preview**: the preview camera's `clearFlags` is `SolidColor`
  with a color matching the panel background, so the render never looks
  see-through against the panel.
- **Isolated rendering**: the preview camera/lights live in their own
  `IsolatedScene` (own layer + own ambient light), so they never appear
  in, or affect the lighting of, the user's actual scene(s).
- **Default material is single-sided**: backfaces are culled on the
  fallback material used when no material is supplied (`_Cull` forced to
  `Back`, needed because materials created via code skip the shader GUI
  validation that normally sets it). Materials passed to `Set()` are used
  as-is, with whatever `Cull` setting they already have.
- **Orbit control**: hold **CTRL** and drag with the left mouse button
  anywhere in the forwarded area (see section 5) to orbit; releasing CTRL
  mid-drag cancels the drag.
- **Default material**: if no material is supplied, a material is created
  from the first available shader in `Universal Render Pipeline/Lit` →
  `Standard` → `Diffuse` → error shader fallback.
