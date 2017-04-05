using System;
using System.Collections.Generic;
using ReGoap.Planner;

namespace ReGoap.Core
{
    public interface IReGoapGoal<T, W>
    {
        void Run(Action<IReGoapGoal<T, W>> callback);
        // THREAD SAFE METHODS (cannot use any unity library!)
        Queue<ReGoapActionState<T, W>> GetPlan();
        string GetName();
        void Precalculations(IGoapPlanner<T, W> goapPlanner);
        bool IsGoalPossible();
        ReGoapState<T, W> GetGoalState();
        float GetPriority();
        void SetPlan(Queue<ReGoapActionState<T, W>> path);
        float GetErrorDelay();
    }
}