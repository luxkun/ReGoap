using System;
using System.Collections.Generic;

public interface IGoapPlanner<T, W>
{
    IReGoapGoal<T, W> Plan(IReGoapAgent<T, W> goapAgent, IReGoapGoal<T, W> blacklistGoal, Queue<ReGoapActionState<T, W>> currentPlan, Action<IReGoapGoal<T, W>> callback);
    IReGoapGoal<T, W> GetCurrentGoal();
    IReGoapAgent<T, W> GetCurrentAgent();
    bool IsPlanning();
    ReGoapPlannerSettings GetSettings();
}