namespace ReGoap.Godot.FSMExample.Tests;

#if GDUNIT4NET_API_V5
using GdUnit4;

using ReGoap.Godot.FSMExample.World;

using static GdUnit4.Assertions;

[TestSuite]
/// <summary>
/// Verifies chest resource bookkeeping primitives used by world actions.
/// </summary>
public class ChestNodeTests
{
    /// <summary>
    /// New chest instances must start with zero resources.
    /// </summary>
    [TestCase]
    [RequireGodotRuntime]
    public void StartsEmpty()
    {
        var chest = new ChestNode();
        AssertThat(chest.Wood).IsEqual(0);
        AssertThat(chest.IronOre).IsEqual(0);
        AssertThat(chest.IronIngot).IsEqual(0);
        AssertThat(chest.Swords).IsEqual(0);
    }

    /// <summary>
    /// Consume calls should fail when empty and succeed only when stock exists.
    /// </summary>
    [TestCase]
    [RequireGodotRuntime]
    public void ConsumeRequiresAvailableResources()
    {
        var chest = new ChestNode();

        AssertBool(chest.ConsumeWood(1)).IsFalse();
        AssertBool(chest.ConsumeIronOre(1)).IsFalse();
        AssertBool(chest.ConsumeIronIngot(1)).IsFalse();

        chest.AddWood(2);
        chest.AddIronOre(2);
        chest.AddIronIngot(2);

        AssertBool(chest.ConsumeWood(1)).IsTrue();
        AssertBool(chest.ConsumeIronOre(1)).IsTrue();
        AssertBool(chest.ConsumeIronIngot(1)).IsTrue();

        AssertThat(chest.Wood).IsEqual(1);
        AssertThat(chest.IronOre).IsEqual(1);
        AssertThat(chest.IronIngot).IsEqual(1);
    }
}
#endif
