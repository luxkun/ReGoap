using System;
using System.Collections.Generic;
using System.Reflection;
using ReGoap.Core;

namespace ReGoap.Godot
{
    public partial class ReGoapDebugger : global::Godot.CanvasLayer
    {
        public bool LockAgent;
        public bool StartVisible = false;
        public bool FollowFirstAgentWhenUnlocked = false;
        public float RefreshInterval = 0.25f;
        public global::Godot.Key ToggleKey = global::Godot.Key.F3;
        public bool UseSeparateWindow = true;
        public global::Godot.Vector2I SeparateWindowSize = new global::Godot.Vector2I(1500, 980);
        public int UiFontSize = 24;

        public bool ShowTextInspector = true;
        public bool ShowGraphInspector = true;

        private readonly List<IReGoapAgentHelper> agentHelpers = new List<IReGoapAgentHelper>();
        private readonly Dictionary<string, MethodInfo> typedSnapshotMethods = new Dictionary<string, MethodInfo>();

        private int selectedAgentIndex;
        private string selectedAgentId = "";
        private float refreshCooldown;

        private global::Godot.PanelContainer panel;
        private global::Godot.Button lockButton;
        private global::Godot.Button pauseButton;
        private global::Godot.LineEdit filterInput;
        private global::Godot.OptionButton snapshotSelector;
        private global::Godot.AcceptDialog exportDialog;

        private global::Godot.TabContainer tabs;
        private global::Godot.RichTextLabel headerLabel;
        private global::Godot.RichTextLabel goalsLabel;
        private global::Godot.RichTextLabel actionsLabel;
        private global::Godot.RichTextLabel planLabel;
        private global::Godot.RichTextLabel worldStateLabel;

        private global::Godot.GraphEdit graph;
        private global::Godot.Label graphLegend;
        private readonly Dictionary<string, global::Godot.GraphNode> graphVisualNodes = new Dictionary<string, global::Godot.GraphNode>();
        private readonly Dictionary<string, global::Godot.RichTextLabel> graphNodeLabels = new Dictionary<string, global::Godot.RichTextLabel>();
        private readonly HashSet<string> graphConnections = new HashSet<string>();

        private bool paused;
        private readonly List<SnapshotRecord> snapshotHistory = new List<SnapshotRecord>();
        private int selectedSnapshotIndex = -1;
        private string filterText = "";
        private bool uiReady;
        private global::Godot.Window detachedWindow;

        private float UiScale => Math.Max(1.0f, UiFontSize / 16.0f);
        private int UiSpacing => Math.Max(6, (int)Math.Round(8 * UiScale));
        private int UiMargin => Math.Max(8, (int)Math.Round(12 * UiScale));

        public override void _Ready()
        {
            Visible = StartVisible;
            if (StartVisible)
            {
                EnsureUiIfNeeded();
                if (detachedWindow != null)
                    detachedWindow.Visible = true;
                RefreshAgents(forceResetSelection: true);
                CaptureAndRender();
            }
        }

        public override void _UnhandledInput(global::Godot.InputEvent @event)
        {
            if (@event is global::Godot.InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo && keyEvent.Keycode == ToggleKey)
            {
                Visible = !Visible;
                if (Visible)
                {
                    EnsureUiIfNeeded();
                    if (detachedWindow != null)
                        detachedWindow.Visible = true;
                    RefreshAgents(forceResetSelection: true);
                    CaptureAndRender();
                }
                else if (detachedWindow != null)
                {
                    detachedWindow.Visible = false;
                }
                GetViewport().SetInputAsHandled();
            }
        }

        public override void _Process(double delta)
        {
            if (!Visible)
                return;

            refreshCooldown -= (float)delta;
            if (refreshCooldown <= 0f)
            {
                refreshCooldown = RefreshInterval;
                RefreshAgents(forceResetSelection: false);
                if (!paused)
                    CaptureAndRender();
            }
        }

        private void EnsureUiIfNeeded()
        {
            if (uiReady)
                return;

            if (ShouldUseDetachedWindow())
                detachedWindow = TryCreateDetachedWindow();

            panel = new global::Godot.PanelContainer();
            panel.Name = "ReGoapDebuggerPanel";
            panel.SetAnchorsPreset(global::Godot.Control.LayoutPreset.FullRect);
            panel.OffsetLeft = UiMargin;
            panel.OffsetTop = UiMargin;
            panel.OffsetRight = -UiMargin;
            panel.OffsetBottom = -UiMargin;
            if (detachedWindow != null)
                detachedWindow.AddChild(panel);
            else
                AddChild(panel);

            var margin = new global::Godot.MarginContainer();
            margin.AddThemeConstantOverride("margin_left", UiMargin);
            margin.AddThemeConstantOverride("margin_top", UiMargin);
            margin.AddThemeConstantOverride("margin_right", UiMargin);
            margin.AddThemeConstantOverride("margin_bottom", UiMargin);
            panel.AddChild(margin);

            var root = new global::Godot.VBoxContainer();
            root.AddThemeConstantOverride("separation", UiSpacing);
            margin.AddChild(root);

            root.AddChild(BuildToolbar());

            tabs = new global::Godot.TabContainer();
            tabs.SizeFlagsVertical = global::Godot.Control.SizeFlags.ExpandFill;
            root.AddChild(tabs);

            if (ShowTextInspector)
                tabs.AddChild(BuildTextInspector());

            if (ShowGraphInspector)
                tabs.AddChild(BuildGraphInspector());

            tabs.AddThemeFontSizeOverride("font_size", UiFontSize);
            var tabBar = tabs.GetTabBar();
            if (tabBar != null)
                tabBar.AddThemeFontSizeOverride("font_size", UiFontSize);

            uiReady = true;

            if (detachedWindow != null)
                detachedWindow.Visible = Visible;

            exportDialog = new global::Godot.AcceptDialog();
            exportDialog.Title = "Snapshot Export";
            if (detachedWindow != null)
                detachedWindow.AddChild(exportDialog);
            else
                AddChild(exportDialog);
        }

        private global::Godot.Window TryCreateDetachedWindow()
        {
            try
            {
                var root = GetTree().Root;
                if (root != null)
                    root.GuiEmbedSubwindows = false;
                var viewport = GetViewport();
                if (viewport != null)
                    viewport.GuiEmbedSubwindows = false;

                var window = new global::Godot.Window();
                window.Title = "ReGoap Debugger";
                window.Size = SeparateWindowSize;
                window.Unresizable = false;
                window.Transient = false;
                window.Exclusive = false;
                window.CloseRequested += () =>
                {
                    Visible = false;
                    window.Visible = false;
                };

                if (root != null)
                    root.AddChild(window);
                else
                    AddChild(window);

                return window;
            }
            catch (Exception e)
            {
                global::Godot.GD.PushWarning("[ReGoapDebugger] Detached window unavailable, using inline mode. " + e.Message);
                return null;
            }
        }

        private bool ShouldUseDetachedWindow()
        {
            return UseSeparateWindow && !ReGoapLaunchContext.IsEditorHostedRun();
        }

        private global::Godot.Control BuildToolbar()
        {
            var toolbar = new global::Godot.HBoxContainer();
            toolbar.AddThemeConstantOverride("separation", UiSpacing);

            var prevButton = new global::Godot.Button { Text = "<" };
            prevButton.AddThemeFontSizeOverride("font_size", UiFontSize);
            prevButton.Pressed += SelectPreviousAgent;
            toolbar.AddChild(prevButton);

            var nextButton = new global::Godot.Button { Text = ">" };
            nextButton.AddThemeFontSizeOverride("font_size", UiFontSize);
            nextButton.Pressed += SelectNextAgent;
            toolbar.AddChild(nextButton);

            lockButton = new global::Godot.Button { Text = "Lock Agent", ToggleMode = true, ButtonPressed = LockAgent };
            lockButton.AddThemeFontSizeOverride("font_size", UiFontSize);
            lockButton.Toggled += pressed => LockAgent = pressed;
            toolbar.AddChild(lockButton);

            pauseButton = new global::Godot.Button { Text = "Pause", ToggleMode = true };
            pauseButton.AddThemeFontSizeOverride("font_size", UiFontSize);
            pauseButton.Toggled += OnPauseToggled;
            toolbar.AddChild(pauseButton);

            var exportButton = new global::Godot.Button { Text = "Export JSON" };
            exportButton.AddThemeFontSizeOverride("font_size", UiFontSize);
            exportButton.Pressed += ExportCurrentSnapshot;
            toolbar.AddChild(exportButton);

            var filterLabel = new global::Godot.Label { Text = "Filter:" };
            filterLabel.AddThemeFontSizeOverride("font_size", UiFontSize);
            toolbar.AddChild(filterLabel);
            filterInput = new global::Godot.LineEdit { PlaceholderText = "goal/action/state key..." };
            filterInput.AddThemeFontSizeOverride("font_size", UiFontSize);
            filterInput.SizeFlagsHorizontal = global::Godot.Control.SizeFlags.ExpandFill;
            filterInput.TextChanged += text =>
            {
                filterText = text ?? "";
                RenderCurrentSnapshot();
            };
            toolbar.AddChild(filterInput);

            var snapshotLabel = new global::Godot.Label { Text = "Snapshot:" };
            snapshotLabel.AddThemeFontSizeOverride("font_size", UiFontSize);
            toolbar.AddChild(snapshotLabel);
            snapshotSelector = new global::Godot.OptionButton();
            snapshotSelector.AddThemeFontSizeOverride("font_size", UiFontSize);
            var snapshotPopup = snapshotSelector.GetPopup();
            if (snapshotPopup != null)
                snapshotPopup.AddThemeFontSizeOverride("font_size", UiFontSize);
            snapshotSelector.ItemSelected += index =>
            {
                selectedSnapshotIndex = (int)index;
                RenderCurrentSnapshot();
            };
            toolbar.AddChild(snapshotSelector);

            return toolbar;
        }

        private global::Godot.Control BuildTextInspector()
        {
            var scroll = new global::Godot.ScrollContainer { Name = "Inspector" };
            scroll.SizeFlagsVertical = global::Godot.Control.SizeFlags.ExpandFill;

            var vbox = new global::Godot.VBoxContainer();
            vbox.CustomMinimumSize = new global::Godot.Vector2(920 * UiScale, 980 * UiScale);
            vbox.AddThemeConstantOverride("separation", UiSpacing);
            scroll.AddChild(vbox);

            headerLabel = MakeLabel(120);
            goalsLabel = MakeLabel(180);
            actionsLabel = MakeLabel(220);
            planLabel = MakeLabel(120);
            worldStateLabel = MakeLabel(220);

            vbox.AddChild(headerLabel);
            vbox.AddChild(goalsLabel);
            vbox.AddChild(actionsLabel);
            vbox.AddChild(planLabel);
            vbox.AddChild(worldStateLabel);

            return scroll;
        }

        private global::Godot.Control BuildGraphInspector()
        {
            var vbox = new global::Godot.VBoxContainer { Name = "Graph" };
            vbox.SizeFlagsVertical = global::Godot.Control.SizeFlags.ExpandFill;

            graphLegend = new global::Godot.Label
            {
                Text = "Legend: Goals=Green | Possible Actions=Orange | Current Plan=Blue | Selected Plan Step=Bright Cyan + Selected | World State=Deep Blue | Active=Brighter Node | Missing Preconditions=Red"
            };
            graphLegend.AutowrapMode = global::Godot.TextServer.AutowrapMode.WordSmart;
            graphLegend.AddThemeFontSizeOverride("font_size", UiFontSize);
            vbox.AddChild(graphLegend);

            graph = new global::Godot.GraphEdit();
            graph.SizeFlagsVertical = global::Godot.Control.SizeFlags.ExpandFill;
            graph.MinimapEnabled = true;
            graph.ShowZoomLabel = true;
            graph.AddThemeFontSizeOverride("font_size", UiFontSize);
            vbox.AddChild(graph);

            return vbox;
        }

        private global::Godot.RichTextLabel MakeLabel(float minY)
        {
            var label = new global::Godot.RichTextLabel
            {
                BbcodeEnabled = true,
                FitContent = true,
                ScrollActive = true,
                CustomMinimumSize = new global::Godot.Vector2(880 * UiScale, minY * UiScale),
                AutowrapMode = global::Godot.TextServer.AutowrapMode.WordSmart
            };
            label.AddThemeFontSizeOverride("normal_font_size", UiFontSize);
            return label;
        }

        private void SelectPreviousAgent()
        {
            if (agentHelpers.Count == 0)
                return;
            selectedAgentIndex = (selectedAgentIndex - 1 + agentHelpers.Count) % agentHelpers.Count;
            selectedAgentId = GetHelperId(GetCurrentHelper());
            CaptureAndRender();
        }

        private void SelectNextAgent()
        {
            if (agentHelpers.Count == 0)
                return;
            selectedAgentIndex = (selectedAgentIndex + 1) % agentHelpers.Count;
            selectedAgentId = GetHelperId(GetCurrentHelper());
            CaptureAndRender();
        }

        private void OnPauseToggled(bool isPaused)
        {
            paused = isPaused;
            pauseButton.Text = isPaused ? "Paused" : "Pause";
            if (!paused)
            {
                selectedSnapshotIndex = -1;
                CaptureAndRender();
            }
            else
            {
                PopulateSnapshotSelector();
            }
        }

        private void RefreshAgents(bool forceResetSelection)
        {
            agentHelpers.Clear();

            var currentScene = GetTree().CurrentScene;
            if (currentScene == null)
                return;

            var queue = new Queue<global::Godot.Node>();
            queue.Enqueue(currentScene);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (node is IReGoapAgentHelper helper)
                    agentHelpers.Add(helper);

                foreach (var childObj in node.GetChildren())
                    if (childObj is global::Godot.Node child)
                        queue.Enqueue(child);
            }

            if (agentHelpers.Count == 0)
            {
                selectedAgentIndex = 0;
                selectedAgentId = "";
                return;
            }

            if (!string.IsNullOrEmpty(selectedAgentId))
            {
                for (var i = 0; i < agentHelpers.Count; i++)
                {
                    if (GetHelperId(agentHelpers[i]) == selectedAgentId)
                    {
                        selectedAgentIndex = i;
                        return;
                    }
                }
            }

            if (!LockAgent && FollowFirstAgentWhenUnlocked && forceResetSelection)
            {
                selectedAgentIndex = 0;
                selectedAgentId = GetHelperId(agentHelpers[selectedAgentIndex]);
                return;
            }

            if (forceResetSelection && selectedAgentIndex >= agentHelpers.Count)
                selectedAgentIndex = 0;

            selectedAgentIndex = Math.Clamp(selectedAgentIndex, 0, agentHelpers.Count - 1);
            selectedAgentId = GetHelperId(agentHelpers[selectedAgentIndex]);
        }

        private IReGoapAgentHelper GetCurrentHelper()
        {
            if (agentHelpers.Count == 0)
                return null;
            if (selectedAgentIndex < 0 || selectedAgentIndex >= agentHelpers.Count)
                selectedAgentIndex = 0;
            return agentHelpers[selectedAgentIndex];
        }

        private void CaptureAndRender()
        {
            var helper = GetCurrentHelper();
            if (helper == null)
            {
                RenderNoAgent();
                return;
            }

            selectedAgentId = GetHelperId(helper);

            var snapshot = BuildSnapshot(helper);
            snapshotHistory.Add(new SnapshotRecord
            {
                TimestampIso = DateTime.UtcNow.ToString("o"),
                AgentNodeName = (helper as global::Godot.Node)?.Name ?? "<unnamed>",
                Data = snapshot
            });

            if (snapshotHistory.Count > 200)
                snapshotHistory.RemoveAt(0);

            if (!paused)
                selectedSnapshotIndex = snapshotHistory.Count - 1;

            PopulateSnapshotSelector();
            RenderCurrentSnapshot();
        }

        private void PopulateSnapshotSelector()
        {
            if (snapshotSelector == null)
                return;

            snapshotSelector.Clear();
            for (var i = 0; i < snapshotHistory.Count; i++)
            {
                var s = snapshotHistory[i];
                snapshotSelector.AddItem((i + 1) + " " + s.AgentNodeName + " @ " + ShortTime(s.TimestampIso));
            }

            if (snapshotHistory.Count == 0)
            {
                snapshotSelector.Disabled = true;
                return;
            }

            snapshotSelector.Disabled = !paused;
            var index = Math.Clamp(selectedSnapshotIndex, 0, snapshotHistory.Count - 1);
            snapshotSelector.Select(index);
            selectedSnapshotIndex = index;
        }

        private void RenderCurrentSnapshot()
        {
            if (snapshotHistory.Count == 0)
            {
                RenderNoAgent();
                return;
            }

            var index = Math.Clamp(selectedSnapshotIndex, 0, snapshotHistory.Count - 1);
            selectedSnapshotIndex = index;
            var record = snapshotHistory[index];
            var snapshot = record.Data;

            headerLabel.Text =
                "[b]ReGoap Debugger[/b]\n" +
                "Agent [color=#7fd1ff]" + record.AgentNodeName + "[/color]  " +
                "([color=#9aa6b2]" + (selectedAgentIndex + 1) + "/" + agentHelpers.Count + "[/color])\n" +
                "Snapshot: [color=#9aa6b2]" + (index + 1) + "/" + snapshotHistory.Count + " @ " + ShortTime(record.TimestampIso) + "[/color]\n" +
                "Mode: " + (paused ? "[color=#ffd27f]paused[/color]" : "[color=#6bff95]live[/color]");

            goalsLabel.Text = ApplyFilter(snapshot.GoalsText);
            actionsLabel.Text = ApplyFilter(snapshot.ActionsText);
            planLabel.Text = ApplyFilter(snapshot.PlanText);
            worldStateLabel.Text = ApplyFilter(snapshot.WorldStateText);

            RenderGraph(snapshot);
        }

        private string ApplyFilter(string section)
        {
            if (string.IsNullOrWhiteSpace(filterText))
                return section;

            var lines = section.Split('\n');
            var outLines = new List<string>();
            if (lines.Length > 0)
                outLines.Add(lines[0]);

            var f = filterText.Trim();
            for (var i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0)
                    outLines.Add(line);
            }
            return string.Join("\n", outLines);
        }

        private void RenderNoAgent()
        {
            if (headerLabel != null)
                headerLabel.Text = "[b]ReGoap Debugger[/b]\nNo active ReGoap agents found.";
            if (goalsLabel != null)
                goalsLabel.Text = "";
            if (actionsLabel != null)
                actionsLabel.Text = "";
            if (planLabel != null)
                planLabel.Text = "";
            if (worldStateLabel != null)
                worldStateLabel.Text = "";

            if (graph != null)
            {
                graph.ClearConnections();
                foreach (var node in graphVisualNodes.Values)
                {
                    if (global::Godot.GodotObject.IsInstanceValid(node))
                        node.QueueFree();
                }
                graphVisualNodes.Clear();
                graphNodeLabels.Clear();
                graphConnections.Clear();
            }
        }

        private Snapshot BuildSnapshot(IReGoapAgentHelper helper)
        {
            var fallback = new Snapshot
            {
                GoalsText = "[b]Goals[/b]\nNo active agent data.",
                ActionsText = "[b]Possible Actions[/b]\nNo active agent data.",
                PlanText = "[b]Current Plan[/b]\nNo active plan.",
                WorldStateText = "[b]World State[/b]\nNo world state."
            };

            if (helper == null)
                return fallback;

            try
            {
                var genericArgs = helper.GetGenericArguments();
                if (genericArgs == null || genericArgs.Length < 2 || genericArgs[0] == null || genericArgs[1] == null)
                    return fallback;

                var key = genericArgs[0].FullName + ":" + genericArgs[1].FullName;
                if (!typedSnapshotMethods.TryGetValue(key, out var typedMethod) || typedMethod == null)
                {
                    var method = typeof(ReGoapDebugger).GetMethod(nameof(BuildSnapshotTyped), BindingFlags.NonPublic | BindingFlags.Instance);
                    if (method == null)
                        return fallback;
                    typedMethod = method.MakeGenericMethod(genericArgs);
                    typedSnapshotMethods[key] = typedMethod;
                }

                var result = typedMethod.Invoke(this, new object[] { helper }) as Snapshot;
                return result ?? fallback;
            }
            catch
            {
                // Keep debugger UI alive even if typed reflection fails for a specific agent.
                // Fallback snapshot lets users continue inspecting other agents/state.
                return fallback;
            }
        }

        private string GetHelperId(IReGoapAgentHelper helper)
        {
            if (helper is global::Godot.Node node)
                return node.GetPath().ToString();
            return helper?.ToString() ?? "";
        }

        private Snapshot BuildSnapshotTyped<T, W>(IReGoapAgent<T, W> agent)
        {
            var snapshot = new Snapshot();
            if (agent == null || !agent.IsActive() || agent.GetMemory() == null)
            {
                snapshot.GoalsText = "[b]Goals[/b]\nNo active agent data.";
                snapshot.ActionsText = "[b]Possible Actions[/b]\nNo active agent data.";
                snapshot.PlanText = "[b]Current Plan[/b]\nNo active plan.";
                snapshot.WorldStateText = "[b]World State[/b]\nNo world state.";
                return snapshot;
            }

            var currentGoal = agent.GetCurrentGoal();
            var currentAction = ResolveCurrentAction(agent);
            var worldState = agent.GetMemory().GetWorldState();

            snapshot.GoalsText = BuildGoalsText(agent, currentGoal, snapshot);
            snapshot.ActionsText = BuildActionsText(agent, worldState, snapshot);
            snapshot.PlanText = BuildPlanText(agent, currentAction, snapshot);
            snapshot.WorldStateText = BuildWorldStateText(worldState, snapshot);

            return snapshot;
        }

        private static IReGoapAction<T, W> ResolveCurrentAction<T, W>(IReGoapAgent<T, W> agent)
        {
            if (agent is ReGoapAgent<T, W> concreteAgent)
                return concreteAgent.GetCurrentAction();
            return null;
        }

        private string BuildGoalsText<T, W>(IReGoapAgent<T, W> agent, IReGoapGoal<T, W> currentGoal, Snapshot snapshot)
        {
            var lines = new List<string> { "[b]Goals[/b]" };
            var goals = agent.GetGoalsSet();
            for (var i = 0; i < goals.Count; i++)
            {
                var goal = goals[i];
                var isCurrent = ReferenceEquals(goal, currentGoal);
                var titleColor = isCurrent ? "#6bff95" : "#d0d7de";
                var goalId = "goal_" + i;

                snapshot.GraphNodes.Add(new GraphNode
                {
                    Id = goalId,
                    Title = goal.GetName(),
                    Body = "prio=" + goal.GetPriority().ToString("0.##"),
                    Kind = isCurrent ? GraphNodeKind.ActiveGoal : GraphNodeKind.Goal
                });

                lines.Add("[color=" + titleColor + "]" + goal.GetName() + "  (prio " + goal.GetPriority().ToString("0.##") + ")[/color]");

                var goalState = goal.GetGoalState();
                if (goalState == null || goalState.Count == 0)
                {
                    lines.Add("  [color=#7d8590](empty goal state)[/color]");
                }
                else
                {
                    foreach (var pair in goalState.GetValues())
                        lines.Add("  - " + pair.Key + " = " + pair.Value);
                }
            }
            return string.Join("\n", lines);
        }

        private string BuildActionsText<T, W>(IReGoapAgent<T, W> agent, ReGoapState<T, W> worldState, Snapshot snapshot)
        {
            var lines = new List<string> { "[b]Possible Actions[/b]" };
            var emptyGoal = agent.InstantiateNewState();
            var stackData = new GoapActionStackData<T, W>
            {
                agent = agent,
                currentState = worldState,
                goalState = emptyGoal,
                next = null,
                settings = ReGoapState<T, W>.Instantiate()
            };

            var actions = agent.GetActionsSet();
            for (var i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                action.Precalculations(stackData);

                var settingsList = action.GetSettings(stackData);
                stackData.settings = settingsList != null && settingsList.Count > 0 ? settingsList[0] : stackData.settings;

                var preconditions = action.GetPreconditions(stackData);
                var effects = action.GetEffects(stackData);
                var procedural = action.CheckProceduralCondition(stackData);
                if (preconditions == null || effects == null)
                    continue;

                var preconditionDiff = agent.InstantiateNewState();
                preconditions.MissingDifference(worldState, ref preconditionDiff);
                var preconditionOk = preconditionDiff.Count == 0;
                var active = action.IsActive();
                var titleColor = active ? "#ffd27f" : (procedural && preconditionOk ? "#6bff95" : "#ffb86b");

                var actionId = "action_" + i;
                snapshot.GraphNodes.Add(new GraphNode
                {
                    Id = actionId,
                    Title = action.GetName(),
                    Body = "cost=" + action.GetCost(stackData).ToString("0.##") + "\nprocedural=" + procedural + "\npreconditions_ok=" + preconditionOk,
                    Kind = active ? GraphNodeKind.ActiveAction : GraphNodeKind.Action
                });

                lines.Add("[color=" + titleColor + "]" + action.GetName() + "[/color]  cost=" + action.GetCost(stackData).ToString("0.##"));
                lines.Add("  proceduralCondition: " + (procedural ? "[color=#6bff95]true[/color]" : "[color=#ff7b72]false[/color]"));
                lines.Add("  preconditions:");

                var missingValues = preconditionDiff.GetValues();
                foreach (var pair in preconditions.GetValues())
                {
                    var missing = missingValues.ContainsKey(pair.Key);
                    var lineColor = missing ? "#ff7b72" : "#6bff95";
                    lines.Add("    [color=" + lineColor + "]" + pair.Key + " = " + pair.Value + "[/color]");
                }

                lines.Add("  effects:");
                foreach (var pair in effects.GetValues())
                    lines.Add("    " + pair.Key + " = " + pair.Value);
                lines.Add("");

                preconditionDiff.Recycle();
            }

            stackData.settings.Recycle();
            emptyGoal.Recycle();
            return string.Join("\n", lines);
        }

        private string BuildPlanText<T, W>(IReGoapAgent<T, W> agent, IReGoapAction<T, W> currentAction, Snapshot snapshot)
        {
            var lines = new List<string> { "[b]Current Plan[/b]" };
            var plan = agent.GetStartingPlan();
            if (plan == null || plan.Count == 0)
            {
                lines.Add("[color=#7d8590]No running plan.[/color]");
                return string.Join("\n", lines);
            }

            string previousId = null;
            for (var i = 0; i < plan.Count; i++)
            {
                var action = plan[i].Action;
                var isSelectedStep = ReferenceEquals(action, currentAction) || action.IsActive();
                var color = isSelectedStep ? "#7fe7ff" : "#d0d7de";
                lines.Add((i + 1) + ") [color=" + color + "]" + action.GetName() + "[/color]");

                var planId = "plan_" + i;
                snapshot.GraphNodes.Add(new GraphNode
                {
                    Id = planId,
                    Title = (i + 1) + ". " + action.GetName(),
                    Body = "active=" + action.IsActive() + "\nselected=" + isSelectedStep,
                    Kind = isSelectedStep ? GraphNodeKind.SelectedPlanAction : (action.IsActive() ? GraphNodeKind.ActivePlanAction : GraphNodeKind.PlanAction),
                    IsSelected = isSelectedStep
                });

                if (previousId != null)
                    snapshot.GraphConnections.Add(new GraphConnection(previousId, planId));
                previousId = planId;
            }
            return string.Join("\n", lines);
        }

        private string BuildWorldStateText<T, W>(ReGoapState<T, W> worldState, Snapshot snapshot)
        {
            var lines = new List<string> { "[b]World State[/b]" };
            if (worldState == null || worldState.Count == 0)
            {
                lines.Add("[color=#7d8590]World state is empty.[/color]");
                return string.Join("\n", lines);
            }

            var body = "";
            foreach (var pair in worldState.GetValues())
            {
                var line = "- " + pair.Key + " = " + pair.Value;
                lines.Add(line);
                body += pair.Key + "=" + pair.Value + "\n";
            }
            snapshot.GraphNodes.Add(new GraphNode
            {
                Id = "world_state",
                Title = "World State",
                Body = body,
                Kind = GraphNodeKind.WorldState
            });

            return string.Join("\n", lines);
        }

        private void RenderGraph(Snapshot snapshot)
        {
            if (graph == null)
                return;

            var columns = 4;
            var xSpacing = 360 * UiScale;
            var ySpacing = 220 * UiScale;

            var desiredNodeIds = new HashSet<string>();

            for (var i = 0; i < snapshot.GraphNodes.Count; i++)
            {
                var model = snapshot.GraphNodes[i];
                desiredNodeIds.Add(model.Id);

                if (!graphVisualNodes.TryGetValue(model.Id, out var node) || !global::Godot.GodotObject.IsInstanceValid(node))
                {
                    node = new global::Godot.GraphNode();
                    node.Name = model.Id;
                    node.Size = new global::Godot.Vector2(340 * UiScale, 210 * UiScale);
                    node.SetSlot(0, true, 0, global::Godot.Colors.White, true, 0, global::Godot.Colors.White);
                    node.AddThemeFontSizeOverride("title_font_size", UiFontSize);

                    var label = new global::Godot.RichTextLabel
                    {
                        BbcodeEnabled = false,
                        FitContent = true,
                        ScrollActive = true,
                        CustomMinimumSize = new global::Godot.Vector2(300 * UiScale, 150 * UiScale)
                    };
                    label.AddThemeFontSizeOverride("normal_font_size", UiFontSize);
                    node.AddChild(label);

                    graph.AddChild(node);
                    graphVisualNodes[model.Id] = node;
                    graphNodeLabels[model.Id] = label;

                    node.PositionOffset = new global::Godot.Vector2((40 * UiScale) + (i % columns) * xSpacing, (60 * UiScale) + (i / columns) * ySpacing);
                }

                node.Title = model.Title;
                node.Modulate = ColorForKind(model.Kind);
                node.Selected = model.IsSelected;
                if (graphNodeLabels.TryGetValue(model.Id, out var nodeLabel) && global::Godot.GodotObject.IsInstanceValid(nodeLabel))
                    nodeLabel.Text = model.Body;
            }

            var toRemove = new List<string>();
            foreach (var pair in graphVisualNodes)
            {
                if (desiredNodeIds.Contains(pair.Key))
                    continue;
                if (global::Godot.GodotObject.IsInstanceValid(pair.Value))
                    pair.Value.QueueFree();
                toRemove.Add(pair.Key);
            }
            foreach (var id in toRemove)
            {
                graphVisualNodes.Remove(id);
                graphNodeLabels.Remove(id);
            }

            var desiredConnections = new HashSet<string>();
            foreach (var connection in snapshot.GraphConnections)
            {
                desiredConnections.Add(MakeConnectionKey(connection.FromNodeName, connection.ToNodeName));
            }

            foreach (var key in graphConnections)
            {
                if (desiredConnections.Contains(key))
                    continue;
                var sep = key.IndexOf("->", StringComparison.Ordinal);
                if (sep <= 0)
                    continue;
                var from = key.Substring(0, sep);
                var to = key.Substring(sep + 2);
                if (graph.HasNode(from) && graph.HasNode(to))
                    graph.DisconnectNode(from, 0, to, 0);
            }

            foreach (var connection in snapshot.GraphConnections)
            {
                if (graph.HasNode(connection.FromNodeName) && graph.HasNode(connection.ToNodeName))
                {
                    var key = MakeConnectionKey(connection.FromNodeName, connection.ToNodeName);
                    if (!graphConnections.Contains(key))
                        graph.ConnectNode(connection.FromNodeName, 0, connection.ToNodeName, 0);
                }
            }

            graphConnections.Clear();
            foreach (var key in desiredConnections)
                graphConnections.Add(key);
        }

        private static string MakeConnectionKey(string fromNodeName, string toNodeName)
        {
            return fromNodeName + "->" + toNodeName;
        }

        private static global::Godot.Color ColorForKind(GraphNodeKind kind)
        {
            return kind switch
            {
                GraphNodeKind.Goal => new global::Godot.Color(0.30f, 0.54f, 0.30f),
                GraphNodeKind.ActiveGoal => new global::Godot.Color(0.42f, 0.75f, 0.42f),
                GraphNodeKind.Action => new global::Godot.Color(0.62f, 0.46f, 0.26f),
                GraphNodeKind.ActiveAction => new global::Godot.Color(0.80f, 0.62f, 0.35f),
                GraphNodeKind.PlanAction => new global::Godot.Color(0.35f, 0.38f, 0.62f),
                GraphNodeKind.ActivePlanAction => new global::Godot.Color(0.48f, 0.54f, 0.87f),
                GraphNodeKind.SelectedPlanAction => new global::Godot.Color(0.36f, 0.82f, 0.98f),
                GraphNodeKind.WorldState => new global::Godot.Color(0.24f, 0.42f, 0.62f),
                _ => global::Godot.Colors.White
            };
        }

        private void ExportCurrentSnapshot()
        {
            if (snapshotHistory.Count == 0)
                return;

            var index = Math.Clamp(selectedSnapshotIndex, 0, snapshotHistory.Count - 1);
            var record = snapshotHistory[index];
            var path = "user://regoap_debug_snapshot_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".json";

            var payload = "{\n" +
                          "  \"agent\": \"" + Escape(record.AgentNodeName) + "\",\n" +
                          "  \"timestamp\": \"" + Escape(record.TimestampIso) + "\",\n" +
                          "  \"goals\": \"" + Escape(record.Data.GoalsText) + "\",\n" +
                          "  \"actions\": \"" + Escape(record.Data.ActionsText) + "\",\n" +
                          "  \"plan\": \"" + Escape(record.Data.PlanText) + "\",\n" +
                          "  \"worldState\": \"" + Escape(record.Data.WorldStateText) + "\"\n" +
                          "}\n";

            using var file = global::Godot.FileAccess.Open(path, global::Godot.FileAccess.ModeFlags.Write);
            if (file == null)
            {
                ShowExportNotice("Could not write snapshot file.");
                return;
            }

            file.StoreString(payload);

            var globalFilePath = global::Godot.ProjectSettings.GlobalizePath(path);
            var globalFolderPath = global::Godot.ProjectSettings.GlobalizePath("user://");
            var shellResult = global::Godot.OS.ShellOpen(globalFolderPath);

            if (shellResult == global::Godot.Error.Ok)
                ShowExportNotice("Snapshot saved:\n" + globalFilePath + "\n\nOpened folder:\n" + globalFolderPath);
            else
                ShowExportNotice("Snapshot saved:\n" + globalFilePath + "\n\nCould not open folder automatically.");
        }

        private void ShowExportNotice(string message)
        {
            if (exportDialog == null)
                return;

            exportDialog.DialogText = message;
            exportDialog.PopupCentered(new global::Godot.Vector2I((int)(700 * UiScale), (int)(260 * UiScale)));
        }

        private static string Escape(string s)
        {
            if (s == null)
                return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
        }

        private static string ShortTime(string iso)
        {
            if (DateTime.TryParse(iso, out var dt))
                return dt.ToLocalTime().ToString("HH:mm:ss");
            return iso;
        }

        private sealed class SnapshotRecord
        {
            public string TimestampIso;
            public string AgentNodeName;
            public Snapshot Data;
        }

        private sealed class Snapshot
        {
            public string GoalsText;
            public string ActionsText;
            public string PlanText;
            public string WorldStateText;
            public List<GraphNode> GraphNodes = new List<GraphNode>();
            public List<GraphConnection> GraphConnections = new List<GraphConnection>();
        }

        private sealed class GraphNode
        {
            public string Id;
            public string Title;
            public string Body;
            public GraphNodeKind Kind;
            public bool IsSelected;
        }

        private readonly struct GraphConnection
        {
            public readonly string FromNodeName;
            public readonly string ToNodeName;

            public GraphConnection(string fromNodeName, string toNodeName)
            {
                FromNodeName = fromNodeName;
                ToNodeName = toNodeName;
            }
        }

        private enum GraphNodeKind
        {
            Goal,
            ActiveGoal,
            Action,
            ActiveAction,
            PlanAction,
            ActivePlanAction,
            SelectedPlanAction,
            WorldState
        }
    }
}
