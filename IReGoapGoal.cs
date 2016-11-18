using System;
using System.Collections.Generic;

public interface IReGoapGoal
{
    void Run(Action<IReGoapGoal> callback);
    // THREAD SAFE METHODS (cannot use any unity library!)
    Queue<IReGoapAction> GetPlan();
    string GetName();
    void Precalculations(IGoapPlanner goapPlanner);
    bool IsGoalPossible();
    ReGoapState GetGoalState();
    int GetPriority();
    void SetPlan(Queue<IReGoapAction> path);
    float GetErrorDelay();
}