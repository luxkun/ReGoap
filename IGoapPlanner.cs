using System;
using System.Collections.Generic;

public interface IGoapPlanner
{
    IReGoapGoal Plan(IReGoapAgent goapAgent, IReGoapGoal blacklistGoal, Queue<ReGoapActionState> currentPlan, Action<IReGoapGoal> callback);
    IReGoapGoal GetCurrentGoal();
    IReGoapAgent GetCurrentAgent();
    bool IsPlanning();
    ReGoapPlannerSettings GetSettings();
}