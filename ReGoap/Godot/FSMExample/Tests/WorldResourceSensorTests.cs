namespace ReGoap.Godot.FSMExample.Tests;

#if GDUNIT4NET_API_V5
using GdUnit4;

using GD = global::Godot.GD;
using PackedScene = global::Godot.PackedScene;
using SceneTree = global::Godot.SceneTree;
using Engine = global::Godot.Engine;

using ReGoap.Godot.FSMExample.World;

using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
/// <summary>
/// Validates that the world sensor mirrors exact chest counts into memory.
/// </summary>
public class WorldResourceSensorTests
{
    /// <summary>
    /// Sensor update must copy all four chest counters without boolean approximation.
    /// </summary>
    [TestCase]
    public void UpdateSensorCopiesExactChestCountsIntoWorldState()
    {
        var mapScene = GD.Load<PackedScene>("res://Scenes/TestMap.tscn");
        AssertThat(mapScene).IsNotNull();

        var map = mapScene.Instantiate();
        AssertThat(map).IsNotNull();

        var tree = Engine.GetMainLoop() as SceneTree;
        AssertThat(tree).IsNotNull();
        tree.Root.AddChild(map);

        var chest = map.GetNode<ChestNode>("Chest");
        var sensor = map.GetNode<WorldResourceSensor>("Workers/Worker1/Memory/WorldSensor");
        var memory = map.GetNode<WorkerMemory>("Workers/Worker1/Memory");

        AssertThat(chest).IsNotNull();
        AssertThat(sensor).IsNotNull();
        AssertThat(memory).IsNotNull();

        chest.AddWood(3);
        chest.AddIronOre(2);
        chest.AddIronIngot(1);
        chest.AddSwords(4);

        sensor.UpdateSensor();

        var state = memory.GetWorldState();
        AssertThat(state.Get("chestWoodCount")).IsEqual(3);
        AssertThat(state.Get("chestOreCount")).IsEqual(2);
        AssertThat(state.Get("chestIngotCount")).IsEqual(1);
        AssertThat(state.Get("chestSwordCount")).IsEqual(4);

        map.Free();
    }
}
#endif
