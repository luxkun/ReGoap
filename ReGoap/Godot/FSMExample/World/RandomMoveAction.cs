using ReGoap.Core;

namespace ReGoap.Godot.FSMExample.World
{
    /// <summary>
    /// Executes ambient wandering by moving to random points with pauses between hops.
    /// </summary>
    public partial class RandomMoveAction : WorkerActionBase
    {
        private readonly System.Random rng = new System.Random(1337);
        private const int WanderLoops = 4;
        private const double WaitSecondsPerLoop = 5.0;

        public override void _Ready()
        {
            base._Ready();
            Name = "Random Move Around";
            Cost = 3.5f;
        }

        public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData)
        {
            var moveCountObj = stackData.currentState.Get("randomMoveCount");
            var moveCount = moveCountObj is int value ? value : 0;
            effects.Set("randomMoveCount", moveCount + 1);
            return effects;
        }

        public override bool CheckProceduralCondition(GoapActionStackData<string, object> stackData)
        {
            var bindings = GetBindings();
            return bindings != null && bindings.Pawn != null && bindings.World != null;
        }

        public override async void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next,
            ReGoapState<string, object> actionSettings, ReGoapState<string, object> goalState,
            System.Action<IReGoapAction<string, object>> done, System.Action<IReGoapAction<string, object>> fail)
        {
            base.Run(previous, next, actionSettings, goalState, done, fail);
            var bindings = GetBindings();
            if (FailIfInvalid(bindings))
                return;
            if (IsRunningStalePlan())
            {
                SafeFail();
                return;
            }

            for (var i = 0; i < WanderLoops; i++)
            {
                var origin = bindings.Pawn.GlobalPosition;
                var dx = (float)(rng.NextDouble() * 320.0 - 160.0);
                var dy = (float)(rng.NextDouble() * 200.0 - 100.0);
                var wanderTarget = origin + new global::Godot.Vector2(dx, dy);
                wanderTarget.X = global::Godot.Mathf.Clamp(wanderTarget.X, 60f, 1220f);
                wanderTarget.Y = global::Godot.Mathf.Clamp(wanderTarget.Y, 120f, 680f);

                await bindings.Pawn.MoveToGlobal(wanderTarget);
                if (FailIfInvalid(bindings))
                    return;
                if (IsRunningStalePlan())
                {
                    SafeFail();
                    return;
                }

                await ToSignal(GetTree().CreateTimer(WaitSecondsPerLoop), global::Godot.SceneTreeTimer.SignalName.Timeout);
                if (FailIfInvalid(bindings))
                    return;
                if (IsRunningStalePlan())
                {
                    SafeFail();
                    return;
                }
            }

            var worldState = GetWorldState();
            var moveCountObj = worldState.Get("randomMoveCount");
            var moveCount = moveCountObj is int value ? value : 0;
            worldState.Set("randomMoveCount", moveCount + 1);
            SafeDone();
        }
    }
}
