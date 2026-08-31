using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A bus event that keeps no history - each Raise only reaches
/// subscribers that are already registered at that point in time.
/// Use for one-off occurrences (something happened) rather than state
/// (something currently is). For state that new subscribers must be
/// caught up on, use ReplayEvent&lt;T&gt; instead.
/// </summary>
public sealed class BusEvent<T>
{
    private event Action<T> m_handlers;

    public void Subscribe(Action<T> handler) => m_handlers += handler;
    public void Unsubscribe(Action<T> handler) => m_handlers -= handler;
    public void Raise(T value) => m_handlers?.Invoke(value);
}

/// <summary>Parameterless variant of <see cref="BusEvent{T}"/>.</summary>
public sealed class BusEvent
{
    private event Action m_handlers;

    public void Subscribe(Action handler) => m_handlers += handler;
    public void Unsubscribe(Action handler) => m_handlers -= handler;
    public void Raise() => m_handlers?.Invoke();
}

/// <summary>Two-parameter variant of <see cref="BusEvent{T}"/>.</summary>
public sealed class BusEvent<T1, T2>
{
    private event Action<T1, T2> m_handlers;

    public void Subscribe(Action<T1, T2> handler) => m_handlers += handler;
    public void Unsubscribe(Action<T1, T2> handler) => m_handlers -= handler;
    public void Raise(T1 value1, T2 value2) => m_handlers?.Invoke(value1, value2);
}

/// <summary>
/// An event that remembers its last raised value and immediately invokes
/// any new subscriber with it. Use this for state a control needs to be
/// in sync with, regardless of when it subscribes relative to when the
/// value was first set.
/// </summary>
public sealed class ReplayEvent<T>
{
    private T m_lastValue;
    private bool m_hasValue;
    private event Action<T> m_handlers;

    /// <summary>
    /// Subscribes to future changes. If a value has already been raised,
    /// the handler is invoked immediately with that value.
    /// </summary>
    public void Subscribe(Action<T> handler)
    {
        m_handlers += handler;
        if (m_hasValue)
            handler?.Invoke(m_lastValue);
    }

    public void Unsubscribe(Action<T> handler)
    {
        m_handlers -= handler;
    }

    /// <summary>
    /// Sets the current value and notifies all current subscribers.
    /// </summary>
    public void Raise(T value)
    {
        m_lastValue = value;
        m_hasValue = true;
        m_handlers?.Invoke(value);
    }
}

/// <summary>
/// Central event bus for editor-wide communication. Access via
/// EditorEventBus.Instance.
///
/// Buffered events (ReplayEvent&lt;T&gt;) remember their last value and
/// replay it to new subscribers. Transient events (BusEvent&lt;T&gt; /
/// BusEvent) are one-off occurrences with no meaningful "current value"
/// and are not replayed. All events use Subscribe/Unsubscribe/Raise.
/// </summary>
public sealed class EditorEventBus
{
    public static EditorEventBus Instance { get; } = new EditorEventBus();

    private EditorEventBus() { }

    #region buffered events (state, replayed to new subscribers)

    public readonly ReplayEvent<bool?> ToggleSnapping = new ReplayEvent<bool?>();
    public readonly ReplayEvent<float> ScaleGrid = new ReplayEvent<float>();
    public readonly ReplayEvent<float> LockAngle = new ReplayEvent<float>();
    public readonly ReplayEvent<bool?> ToggleGrid = new ReplayEvent<bool?>();
    public readonly ReplayEvent<EditorStatus.Mode> ModeChanged = new ReplayEvent<EditorStatus.Mode>();
    public readonly ReplayEvent<EditorStatus.Construct> ConstructionModeChanged = new ReplayEvent<EditorStatus.Construct>();

    #endregion

    #region transient events (one-off occurrences, not replayed)

    public readonly BusEvent<Vector2> MouseMoved = new BusEvent<Vector2>();
    public readonly BusEvent<CursorInfo> CursorInfoChanged = new BusEvent<CursorInfo>();
    public readonly BusEvent<Mesh> RegionMeshChanged = new BusEvent<Mesh>();
    public readonly BusEvent<MapObject, List<string>> EditObject = new BusEvent<MapObject, List<string>>();
    public readonly BusEvent<Vertex> EditVertex = new BusEvent<Vertex>();
    public readonly BusEvent<Segment, List<string>> EditSegment = new BusEvent<Segment, List<string>>();
    public readonly BusEvent<Region, List<string>> EditRegion = new BusEvent<Region, List<string>>();
    public readonly BusEvent<Way, List<string>> EditWay = new BusEvent<Way, List<string>>();
    public readonly BusEvent FitViewToWindow = new BusEvent();
    public readonly BusEvent CenterView = new BusEvent();
    public readonly BusEvent<bool> ZoomChanged = new BusEvent<bool>();
    public readonly BusEvent<IMapLoader, string> LoadMap = new BusEvent<IMapLoader, string>();
    public readonly BusEvent<IMapWriter, string> WriteMap = new BusEvent<IMapWriter, string>();

    #endregion
}