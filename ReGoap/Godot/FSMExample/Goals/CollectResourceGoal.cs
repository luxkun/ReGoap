using ReGoap.Core;
using ReGoap.Godot;

namespace ReGoap.Godot.FSMExample.Goals
{
    public partial class CollectResourceGoal : ReGoapGoal<string, object>
    {
        public string ResourceName = "Wood";

        public override void _Ready()
        {
            base._Ready();
            goal.Set("collectedResource" + ResourceName, true);
        }

        public override string ToString()
        {
            return string.Format("GoapGoal('{0}', '{1}')", Name, ResourceName);
        }
    }
}
