using System.Collections.Generic;

public interface IReGoapAgent
{
    IReGoapMemory GetMemory();
    IReGoapGoal GetCurrentGoal();
    void WarnPossibleGoal(IReGoapGoal goal);
    bool IsActive();
    List<ReGoapActionState> GetStartingPlan();
    T GetPlanValue<T>(string key);
    void SetPlanValue<T>(string key, T value);
    bool HasPlanValue(string target);
    // THREAD SAFE
    List<IReGoapGoal> GetGoalsSet();
    List<IReGoapAction> GetActionsSet();
}
