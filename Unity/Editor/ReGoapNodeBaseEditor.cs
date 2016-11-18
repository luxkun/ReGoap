using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ReGoapNodeBaseEditor : EditorWindow
{
    private List<ReGoapNodeEditor> nodes;

    private GUIStyle nodeStyle;
    private GUIStyle selectedNodeStyle;

    private Vector2 offset;
    private Vector2 drag;
    private GUIStyle activeStyle;
    private GUIStyle actionNodeStyle;
    private GUIStyle activeActionNodeStyle;
    private GUIStyle worldStateStyle;
    private Vector2 totalDrag;
    private IReGoapAgent agent;
    private GUIStyle possibleActionStyle;

    private float repaintCooldown;
    private float repaintDelay = 0.33f;

    [MenuItem("Window/Node Based Editor")]
    private static void OpenWindow()
    {
        ReGoapNodeBaseEditor window = GetWindow<ReGoapNodeBaseEditor>();
        window.titleContent = new GUIContent("ReGoap Debug Editor");
        EditorApplication.update += window.Update;
    }

    private void OnEnable()
    {
        var textOffset = new RectOffset(12, 0, 10, 0);//new Vector2(9f, 7f);

        nodeStyle = new GUIStyle();
        nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        nodeStyle.border = new RectOffset(12, 12, 12, 12);
        nodeStyle.richText = true;
        nodeStyle.padding = textOffset;

        worldStateStyle = new GUIStyle();
        worldStateStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node4.png") as Texture2D;
        worldStateStyle.border = new RectOffset(12, 12, 12, 12);
        worldStateStyle.richText = true;
        worldStateStyle.padding = textOffset;

        activeStyle = new GUIStyle();
        activeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        activeStyle.border = new RectOffset(12, 12, 12, 12);
        activeStyle.richText = true;
        activeStyle.padding = textOffset;

        actionNodeStyle = new GUIStyle();
        actionNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node3.png") as Texture2D;
        actionNodeStyle.border = new RectOffset(12, 12, 12, 12);
        actionNodeStyle.richText = true;
        actionNodeStyle.padding = textOffset;

        activeActionNodeStyle = new GUIStyle();
        activeActionNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node3 on.png") as Texture2D;
        activeActionNodeStyle.border = new RectOffset(12, 12, 12, 12);
        activeActionNodeStyle.richText = true;
        activeActionNodeStyle.padding = textOffset;

        possibleActionStyle = new GUIStyle();
        possibleActionStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node4.png") as Texture2D;
        possibleActionStyle.border = new RectOffset(12, 12, 12, 12);
        possibleActionStyle.richText = true;
        possibleActionStyle.padding = textOffset;

        selectedNodeStyle = new GUIStyle();
        selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);
        selectedNodeStyle.richText = true;
        selectedNodeStyle.padding = textOffset;
    }

    private void OnGUI()
    {
        if (Selection.activeGameObject != null)
        {
            agent = Selection.activeGameObject.GetComponent<IReGoapAgent>();
            if (agent == null)
                return;
        }
        else
            return;

        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);

        UpdateGoapNodes(Selection.activeGameObject);
        DrawNodes();

        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);

        if (GUI.changed) Repaint();
    }

    void Update()
    {
        if (Time.time < repaintCooldown)
            return;
        repaintCooldown = repaintDelay + Time.time;
        UpdateGoapNodes(Selection.activeGameObject);
        Repaint();
    }

    #region GOAP
    ReGoapNodeEditor DrawGenericNode(string text, float width, float height, GUIStyle style, ref Vector2 nodePosition)
    {
        var node = new ReGoapNodeEditor(nodePosition + totalDrag, width, height, style, selectedNodeStyle)
        {
            title = text
        };
        nodes.Add(node);
        nodePosition += new Vector2(width, 0f);
        return node;
    }

    private void UpdateGoapNodes(GameObject gameObj)
    {
        if (agent == null || !agent.IsActive() || agent.GetMemory() == null)
            return;

        if (nodes == null)
        {
            nodes = new List<ReGoapNodeEditor>();
        }
        nodes.Clear();
        var width = 250f;
        var height = 70f;
        var nodePosition = Vector2.zero;

        foreach (var goal in gameObj.GetComponents<IReGoapGoal>())
        {
            if (goal.GetGoalState() == null)
                continue;
            var text = string.Format("<b>GOAL</b> <i>{0}</i>\n", goal);
            foreach (var keyValue in goal.GetGoalState().GetValues())
            {
                text += string.Format("<b>'{0}'</b> = <i>'{1}'</i>", keyValue.Key, keyValue.Value);
            }
            var style = nodeStyle;
            if (agent.IsActive() && agent.GetCurrentGoal() == goal)
            {
                style = activeStyle;
            }
            DrawGenericNode(text, width, height, style, ref nodePosition);
        }
        
        nodePosition = new Vector2(0f, nodePosition.y + height + 10);
        height = 66;
        var maxHeight = height;
        var worldState = agent.GetMemory().GetWorldState();
        foreach (var action in agent.GetActionsSet())
        {
            var curHeight = height;
            var text = string.Format("<b>POSS.ACTION</b> <i>{0}</i>\n", action.GetName());
            text += "-<b>preconditions</b>-\n";
            ReGoapState preconditionsDifferences = new ReGoapState();
            worldState.MissingDifference(action.GetPreconditions(null), ref preconditionsDifferences);
            foreach (var preconditionPair in action.GetPreconditions(null).GetValues())
            {
                curHeight += 13;
                var color = "#004d00";
                if (preconditionsDifferences.GetValues().ContainsKey(preconditionPair.Key))
                {
                    color = "#800000";
                }
                text += string.Format("<color={2}>'<b>{0}</b>' = '<i>{1}</i>'</color>\n", preconditionPair.Key, preconditionPair.Value, color);
            }
            text += "-<b>effects</b>-\n";
            foreach (var effectPair in action.GetEffects(null).GetValues())
            {
                curHeight += 13;
                text += string.Format("'<b>{0}</b>' = '<i>{1}</i>'\n", effectPair.Key, effectPair.Value);
            }
            maxHeight = Mathf.Max(maxHeight, curHeight);
            DrawGenericNode(text, width, curHeight, possibleActionStyle, ref nodePosition);
        }
        nodePosition.x = 0;
        nodePosition.y += maxHeight + 10;
        height = 40;
        if (agent.GetCurrentGoal() != null)
        {
            foreach (var action in agent.GetStartingPlan().ToArray())
            {
                var style = actionNodeStyle;
                if (action.IsActive())
                {
                    style = activeActionNodeStyle;
                }
                var text = string.Format("<b>ACTION</b> <i>{0}</i>\n", action.GetName());
                DrawGenericNode(text, width, height, style, ref nodePosition);
            }
        }

        if (agent.GetMemory() != null)
        {
            nodePosition = new Vector2(0, nodePosition.y + height + 10);
            width = 500;
            height = 40;
            var nodeText = "<b>WORLD STATE</b>\n";
            foreach (var pair in agent.GetMemory().GetWorldState().GetValues())
            {
                nodeText += string.Format("'<b>{0}</b>' = '<i>{1}</i>'\n", pair.Key, pair.Value);
                height += 13;
            }
            DrawGenericNode(nodeText, width, height, worldStateStyle, ref nodePosition);
        }
    }
#endregion

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        offset += drag * 0.5f;
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++)
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);

        for (int j = 0; j < heightDivs; j++)
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void DrawNodes()
    {
        if (nodes != null)
            foreach (var node in nodes)
                node.Draw();
    }

    private void ProcessEvents(Event e)
    {
        drag = Vector2.zero;

        switch (e.type)
        {
            case EventType.MouseDrag:
                if (e.button == 0)
                {
                    OnDrag(e.delta);
                }
                break;
        }
    }

    private void ProcessNodeEvents(Event e)
    {
        if (nodes == null) return;
        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            bool guiChanged = nodes[i].ProcessEvents(e);

            if (guiChanged)
            {
                GUI.changed = true;
            }
        }
    }

    private void ProcessContextMenu(Vector2 mousePosition)
    {
    }

    private void OnDrag(Vector2 delta)
    {
        totalDrag += delta;
        drag = delta;

        if (nodes != null)
        {
            foreach (ReGoapNodeEditor node in nodes)
            {
                node.Drag(delta);
            }
        }

        GUI.changed = true;
    }
}