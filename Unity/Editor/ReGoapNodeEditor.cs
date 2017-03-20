using System;
using UnityEditor;
using UnityEngine;

public struct ReGoapNodeEditor
{
    public delegate void ReGoapNodeEditorEvent(ReGoapNodeEditor node, Event e);
    public ReGoapNodeEditorEvent OnEvent;

    public Rect Rect;
    public string Title;
    public bool IsSelected;

    public GUIStyle Style;
    public GUIStyle DefaultNodeStyle;

    public ReGoapNodeEditor(string title, Vector2 position, float width, float height, GUIStyle nodeStyle, bool isSelected = false, ReGoapNodeEditorEvent onEvent = null)
    {
        Rect = new Rect(position.x, position.y, width, height);
        Style = nodeStyle;
        DefaultNodeStyle = nodeStyle;

        Title = title;
        IsSelected = isSelected;
        OnEvent = onEvent;
    }

    public void Drag(Vector2 delta)
    {
        Rect.position += delta;
    }

    public void Draw()
    {
        GUI.Box(Rect, Title, Style);
    }

    public bool ProcessEvents(Event e)
    {
        if (OnEvent != null)
        {
            OnEvent(this, e);
        }
        return false;
    }

    private void ProcessContextMenu()
    {
    }
}