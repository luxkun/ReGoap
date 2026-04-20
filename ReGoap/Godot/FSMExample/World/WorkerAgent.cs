using ReGoap.Godot;
using ReGoap.Core;
using Godot;

namespace ReGoap.Godot.FSMExample.World
{
    public partial class WorkerAgent : ReGoapAgentAdvanced<string, object>
    {
        public int PlanRevision { get; private set; }

        private bool retryPlanPending;
        private float nextWatchdogCheckAt;
        private IReGoapAction<string, object> lastObservedAction;
        private float lastObservedActionAt;
        private float idleReplanAt;
        private const float ActionStallSeconds = 8.0f;
        private const float IdleReplanDelaySeconds = 0.35f;

        public override void _Process(double delta)
        {
            base._Process(delta);
            var now = (float)(System.DateTime.UtcNow.Ticks / (double)System.TimeSpan.TicksPerSecond);
            if (now < nextWatchdogCheckAt)
                return;
            nextWatchdogCheckAt = now + 0.75f;

            if (IsPlanning)
                return;

            if (currentActionState != null)
            {
                idleReplanAt = 0f;
                var currentAction = currentActionState.Action;
                if (!ReferenceEquals(lastObservedAction, currentAction))
                {
                    lastObservedAction = currentAction;
                    lastObservedActionAt = now;
                }

                if (!currentActionState.Action.IsActive())
                {
                    WarnActionFailure(currentAction);
                    return;
                }

                if (now - lastObservedActionAt > ActionStallSeconds)
                {
                    GD.PushWarning("[WorkerAgent] Stalled action detected on " + Name + ": " + currentAction.GetName());
                    WarnActionFailure(currentAction);
                }
                return;
            }

            lastObservedAction = null;
            lastObservedActionAt = 0f;

            if (currentGoal == null)
            {
                CalculateNewGoal(true);
                return;
            }

            if (idleReplanAt <= 0f)
                idleReplanAt = now + IdleReplanDelaySeconds;

            if (now >= idleReplanAt)
            {
                idleReplanAt = 0f;
                CalculateNewGoal(true);
            }
        }

        protected override void OnDonePlanning(ReGoap.Core.IReGoapGoal<string, object> newGoal)
        {
            if (newGoal != null)
                PlanRevision++;

            base.OnDonePlanning(newGoal);
            if (newGoal != null)
            {
                retryPlanPending = false;
                return;
            }

            if (newGoal == null && !retryPlanPending)
                _ = RetryPlanLater();
        }

        public override void WarnActionFailure(ReGoap.Core.IReGoapAction<string, object> thisAction)
        {
            if (currentActionState != null && ReferenceEquals(thisAction, currentActionState.Action))
            {
                currentActionState.Action.Exit(null);
                currentActionState = null;
            }

            base.WarnActionFailure(thisAction);
        }

        public void ForceReplanNow()
        {
            idleReplanAt = 0f;
            CalculateNewGoal(true);
        }

        private async System.Threading.Tasks.Task RetryPlanLater()
        {
            retryPlanPending = true;
            var timer = GetTree().CreateTimer(0.75);
            await ToSignal(timer, global::Godot.SceneTreeTimer.SignalName.Timeout);

            if (!GodotObject.IsInstanceValid(this) || !IsInsideTree())
                return;

            retryPlanPending = false;
            CalculateNewGoal(true);
        }

        public override void _ExitTree()
        {
            PlanRevision++;
            base._ExitTree();
        }
    }
}
