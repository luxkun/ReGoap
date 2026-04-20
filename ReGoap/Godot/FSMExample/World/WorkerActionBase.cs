using ReGoap.Core;
using ReGoap.Godot;
using Godot;

namespace ReGoap.Godot.FSMExample.World
{
    public abstract partial class WorkerActionBase : ReGoapAction<string, object>
    {
        private ReGoapState<string, object> expectedGoalState;
        private int expectedPlanRevision;

        public override void Run(
            IReGoapAction<string, object> previous,
            IReGoapAction<string, object> next,
            ReGoapState<string, object> actionSettings,
            ReGoapState<string, object> goalState,
            System.Action<IReGoapAction<string, object>> done,
            System.Action<IReGoapAction<string, object>> fail)
        {
            base.Run(previous, next, actionSettings, goalState, done, fail);
            expectedGoalState = goalState;
            if (agent is WorkerAgent workerAgent)
                expectedPlanRevision = workerAgent.PlanRevision;
        }

        protected WorkerAgentBindings GetBindings()
        {
            var node = GetNodeOrNull<WorkerAgentBindings>("../Bindings");
            if (node != null)
                return node;
            node = GetParent()?.GetNodeOrNull<WorkerAgentBindings>("Bindings");
            if (node != null)
                return node;
            return GetNodeOrNull<WorkerAgentBindings>("..");
        }

        protected ReGoapState<string, object> GetWorldState()
        {
            return agent.GetMemory().GetWorldState();
        }

        protected bool IsRunningStalePlan()
        {
            if (agent is WorkerAgent workerAgent && workerAgent.PlanRevision != expectedPlanRevision)
                return true;

            var activeGoal = agent.GetCurrentGoal();
            if (activeGoal == null)
                return false;
            return !ReferenceEquals(activeGoal.GetGoalState(), expectedGoalState);
        }

        protected bool IsContextValid(WorkerAgentBindings bindings)
        {
            if (!GodotObject.IsInstanceValid(this) || !IsInsideTree())
                return false;

            if (agent is Node agentNode)
            {
                if (!GodotObject.IsInstanceValid(agentNode) || !agentNode.IsInsideTree())
                    return false;
            }

            if (bindings == null || !GodotObject.IsInstanceValid(bindings) || !bindings.IsInsideTree())
                return false;
            if (bindings.Pawn == null || !GodotObject.IsInstanceValid(bindings.Pawn) || !bindings.Pawn.IsInsideTree())
                return false;
            if (bindings.World == null || !GodotObject.IsInstanceValid(bindings.World) || !bindings.World.IsInsideTree())
                return false;
            return true;
        }

        protected bool FailIfInvalid(WorkerAgentBindings bindings)
        {
            if (IsContextValid(bindings))
                return false;
            SafeFail();
            return true;
        }

        protected void SafeDone()
        {
            doneCallback?.Invoke(this);
        }

        protected void SafeFail()
        {
            failCallback?.Invoke(this);
        }
    }
}
