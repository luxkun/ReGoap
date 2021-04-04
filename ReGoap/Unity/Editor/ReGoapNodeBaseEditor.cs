using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ReGoap.Core;

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
    private IReGoapAgentHelper agentHelper;
    private GUIStyle possibleActionStyle;
    private GUIStyle menuNodeStyle;
    private GUIStyle selectedMenuNodeStyle;
    private bool agentLocked;

    private MethodInfo updateGoapNodesMethodInfo;

    [MenuItem("Window/ReGoap Debugger")]
    private static void OpenWindow()
    {
        ReGoapNodeBaseEditor window = GetWindow<ReGoapNodeBaseEditor>();
        window.titleContent = new GUIContent("ReGoap Debugger");
    }

    private void OnEnable()
    {
        foreach (var m in GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if (m.Name.StartsWith("UpdateGoapNodes"))
            {
                updateGoapNodesMethodInfo = m;
                break;
            }
        }

        agentLocked = false;

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

        menuNodeStyle = new GUIStyle();
        menuNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node5.png") as Texture2D;
        menuNodeStyle.border = new RectOffset(10, 10, 10, 10);
        menuNodeStyle.richText = true;
        menuNodeStyle.padding = textOffset;

        selectedMenuNodeStyle = new GUIStyle();
        selectedMenuNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node6 on.png") as Texture2D;
        selectedMenuNodeStyle.border = new RectOffset(10, 10, 10, 10);
        selectedMenuNodeStyle.richText = true;
        selectedMenuNodeStyle.padding = textOffset;

        // menu

    }

    private void OnGUI()
    {
        if (Selection.activeGameObject != null)
        {
            if (agentHelper == null || !agentLocked)
            {
                agentHelper = Selection.activeGameObject.GetComponent<IReGoapAgentHelper>();
                if (agentHelper == null)
                    return;
            }
        }
        else
            return;

        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);

        // create generic method for the current agent
        MethodInfo generic = updateGoapNodesMethodInfo.MakeGenericMethod(agentHelper.GetGenericArguments());
        generic.Invoke(this, new object[] { agentHelper });

        UpdateMenuNodes();
        DrawNodes();

        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);

        Repaint();
    }

    #region GOAP
    ReGoapNodeEditor DrawGenericNode(string title, float width, float height, GUIStyle style, ref Vector2 nodePosition, bool isSelected = false, ReGoapNodeEditor.ReGoapNodeEditorEvent onEvent = null)
    {
        var node = new ReGoapNodeEditor(title, nodePosition + totalDrag, width, height, style, isSelected, onEvent);
        nodes.Add(node);
        nodePosition += new Vector2(width, 0f);
        return node;
    }

    private void UpdateGoapNodes<T, W>(IReGoapAgent<T, W> agent)
    {
        if (nodes == null)
        {
            nodes = new List<ReGoapNodeEditor>();
        }
        if (agentHelper == null || agent == null || !agent.IsActive() || agent.GetMemory() == null)
            return;

        nodes.Clear();
        var width = 300f;
        var height = 70f;
        var nodePosition = new Vector2(0f, 60f);
        var nodeMiddleY = new Vector2(0f, height * 0.5f);

        ReGoapNodeEditor? previousNode = null;
        foreach (var goal in agent.GetGoalsSet())
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
            var newNode = DrawGenericNode(text, width, height, style, ref nodePosition);
            if (previousNode.HasValue)
            {
                var startPosition = previousNode.Value.Rect.max - nodeMiddleY - new Vector2(10f, 0f);
                var endPosition = newNode.Rect.min + nodeMiddleY + new Vector2(10f, 0f);
                Handles.DrawLine(startPosition, endPosition);
            }
            previousNode = newNode;
        }
        previousNode = null;
        
        nodePosition = new Vector2(0f, nodePosition.y + height + 10);
        height = 66;
        var maxHeight = height;

        var emptyGoal = agent.InstantiateNewState();
        GoapActionStackData<T, W> stackData;
        stackData.agent = agent;
        stackData.currentState = agent.GetMemory().GetWorldState();
        stackData.goalState = emptyGoal;
        stackData.next = null;
        stackData.settings = ReGoapState<T, W>.Instantiate();

        foreach (var action in agent.GetActionsSet())
        {
            var curHeight = height;
            var text = string.Format("<b>POSS.ACTION</b> <i>{0}</i>\n", action.GetName());
            text += "-<b>preconditions</b>-\n";
            var preconditionsDifferences = agent.InstantiateNewState();
            var preconditions = action.GetPreconditions(stackData);
            if (preconditions == null)
                continue;
            preconditions.MissingDifference(stackData.currentState, ref preconditionsDifferences);
            foreach (var preconditionPair in preconditions.GetValues())
            {
                curHeight += 13;
                var color = "#004d00";
                if (preconditionsDifferences.GetValues().ContainsKey(preconditionPair.Key))
                {
                    color = "#800000";
                }
                text += string.Format("<color={2}>'<b>{0}</b>' = '<i>{1}</i>'</color>\n", preconditionPair.Key, preconditionPair.Value, color);
            }
            preconditionsDifferences.Recycle();

            text += "-<b>effects</b>-\n";
            foreach (var effectPair in action.GetEffects(stackData).GetValues())
            {
                curHeight += 13;
                text += string.Format("'<b>{0}</b>' = '<i>{1}</i>'\n", effectPair.Key, effectPair.Value);
            }
            curHeight += 13;
            var proceduralCheck = action.CheckProceduralCondition(stackData);
            text += string.Format("<color={0}>-<b>proceduralCondition</b>: {1}</color>\n", proceduralCheck ? "#004d00" : "#800000", proceduralCheck);

            maxHeight = Mathf.Max(maxHeight, curHeight);

            nodeMiddleY = new Vector2(0f, curHeight * 0.5f);
            var newNode = DrawGenericNode(text, width, curHeight, possibleActionStyle, ref nodePosition);
            if (previousNode.HasValue)
            {
                var startPosition = previousNode.Value.Rect.max - nodeMiddleY - new Vector2(10f, 0f);
                var endPosition = newNode.Rect.min + nodeMiddleY + new Vector2(10f, 0f);
                Handles.DrawLine(startPosition, endPosition);
            }
            previousNode = newNode;
        }
        previousNode = null;

        nodePosition.x = 0;
        nodePosition.y += maxHeight + 10;
        height = 40;
        nodeMiddleY = new Vector2(0f, height * 0.5f);
        if (agent.GetCurrentGoal() != null)
        {
            foreach (var action in agent.GetStartingPlan().ToArray())
            {
                var style = actionNodeStyle;
                if (action.Action.IsActive())
                {
                    style = activeActionNodeStyle;
                }
                var text = string.Format("<b>ACTION</b> <i>{0}</i>\n", action.Action.GetName());
                
                var newNode = DrawGenericNode(text, width, height, style, ref nodePosition);
                if (previousNode.HasValue)
                {
                    var startPosition = previousNode.Value.Rect.max - nodeMiddleY - new Vector2(10f, 0f);
                    var endPosition = newNode.Rect.min + nodeMiddleY + new Vector2(10f, 0f);
                    Handles.DrawLine(startPosition, endPosition);
                }
                previousNode = newNode;
            }
        }

        if (agent.GetMemory() != null)
        {
            nodePosition = new Vector2(0, nodePosition.y + height + 10);
            width = 500;
            height = 40;
            nodeMiddleY = new Vector2(0f, height * 0.5f);
            var nodeText = "<b>WORLD STATE</b>\n";
            foreach (var pair in agent.GetMemory().GetWorldState().GetValues())
            {
                nodeText += string.Format("'<b>{0}</b>' = '<i>{1}</i>'\n", pair.Key, pair.Value);
                height += 13;
            }
            DrawGenericNode(nodeText, width, height, worldStateStyle, ref nodePosition);
        }
    }
    private void UpdateMenuNodes()
    {
        if (agentHelper == null)
            return;

        var lockNodePosition = new Vector2(0f, 0f);

        var lockNode = DrawGenericNode("<b>LOCK AGENT</b>", 110f, 40f, agentLocked ? selectedMenuNodeStyle : menuNodeStyle, ref lockNodePosition, false, OnLockEvent);

        var agentInfoTitle = string.Format("<b>Selected agent:</b> {0}: {1}", agentHelper, ((MonoBehaviour)agentHelper).name);
        var agentInfoNode = DrawGenericNode(agentInfoTitle, agentInfoTitle.Length * 6f, 40f, menuNodeStyle, ref lockNodePosition);
    }

    private void OnLockEvent(ReGoapNodeEditor node, Event e)
    {
        if (e.isMouse && e.type == EventType.MouseDown && e.button == 0 && node.Rect.Contains(e.mousePosition))
        {
            agentLocked = !agentLocked;
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