using System;
using UnityEditor;
using UnityEngine;

public class ReGoapNodeEditor
{
    public Rect Rect;
    public string Title;
    public bool IsSelected;

    public GUIStyle Style;
    public GUIStyle DefaultNodeStyle;

    public Action<ReGoapNodeEditor, Event> OnEvent;

    public ReGoapNodeEditor(Vector2 position, float width, float height, GUIStyle nodeStyle)
    {
        Rect = new Rect(position.x, position.y, width, height);
        Style = nodeStyle;
        DefaultNodeStyle = nodeStyle;
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