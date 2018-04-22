using System;

namespace ReGoap.Planner
{
    [Serializable]
    public class ReGoapPlannerSettings
    {
        public bool PlanningEarlyExit = false;
        // increase both if your agent has a lot of actions
        public int MaxIterations = 1000;
        public int MaxNodesToExpand = 10000;
        // set this to true if using dynamic actions, such as GenericGoTo or GatherResourceAction
        // a dynamic action is an action that has dynamic preconditions or effects (changed in runtime/precalcultions)
        public bool UsingDynamicActions = false;

        public bool DebugPlan = false;
    }
}