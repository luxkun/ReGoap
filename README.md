# ReGoap
Generic C# GOAP (Goal Oriented Action Planning) library with Unity3d examples and helpers classes.

This library is very generic, if you don't include the Unity folder you can use it in any game engine.

1. [Get Started, fast version](#get-started-fast-version)
2. [Get Started, long version](#get-started-long-version)
    1. [Explaining GOAP](#explaining-goap)
    2. [How to use ReGoap in Unity3D](#how-to-use-regoap-in-unity3d)
        1. [How to implement your own GoapAction](#how-to-implement-your-own-goapaction)
        2. [How to implement your own GoapGoal](#how-to-implement-your-own-goapgoal)
        3. [How to implement your own GoapSensor](#how-to-implement-your-own-goapsensor)
3. [Debugging](#debugging)
4. [Pull Requests](#pull-requests)

---

## Get Started, fast version
Start by checking out the [Unity FSM example here](https://github.com/luxkun/ReGoap/tree/master/Unity/FSMExample).

This example uses the ReGoap library in Unity with a simple FSM (Finite State Machine) to handle the macro behaviours (in most games three FSM states should suffice: idle, goto, animate).

To use it create a new Unity project, open the shell, go to the Assets directory and clone the whole repository in there:
```bash
git clone https://github.com/luxkun/ReGoap.git
```
(in windows you can do the same with command line or any git client, also you can just click on "Clone or Download" and then "Download ZIP")

Also you can just download the last release's unity package, which probably won't have latest changes, [here](https://github.com/luxkun/ReGoap/releases/) or on the [unity asset store](https://www.assetstore.unity3d.com/en/#!/content/77376).

---

## Get Started, long version
###Explaining GOAP
(if you just want to use the library and want an explained example skip to **[How to use ReGoap](#how-to-use-regoap-in-unity3d)**)

Before explaining how to use this library in your game let me explain how does a Goap system work, starting with a quote of [Jeff Orkin](http://alumni.media.mit.edu/~jorkin/goap.html)
```
Goal-Oriented Action Planning (aka GOAP, rhymes with soap) refers to a simplfied STRIPS-like planning architecture specifically designed for real-time control of autonomous character behavior in games.
```
Basically all it does is find a plan (a list of actions) that will fulfill the choosen goal's objectives.

The main concept you need to understand are: [States](#state), [Action](#action), [Goal](#goal), [Memory](#memory) and [Sensors](#sensor)

####State
is a definition of the world, in this library they are handled as a Dictionary of string to object (Dictionary<string, object>).

Check out ReGoapState class in this file: https://github.com/luxkun/ReGoap/blob/master/ReGoapPlanner.cs

Example: *'isAt': 'enemy', 'isWarned': true, 'hasWeapon': true*


####Action
can be defined as a list of preconditions and effects, these are the actions that the Agent (AI pawn, will be called Agent from now on) can do.

The preconditions are the requirement that the Action requires to be ran, described as a State, the effects, as the name implies, are the effects of the Action, as well described as a State.

Examples: 

* *'Open door': [pre: {'nearDoor': true, 'doorUnlocked': true}, effects: {'doorOpened': true}]*
* *'Close combat attack': [pre: {'weaponEquipped': true, 'isAt': 'enemy'}, effects: {'hurtEnemy' true}]*
* *'Go to enemy': [pre: {'enemyInLoS': true, 'canMove': true}, effects: {'isAt': 'enemy'}]*
* *'Equip weapon': [pre: {'hasWeapon': true}, effects: {'weaponEquipped': true}]*
* *'Patrol': [pre: {'canMove': true}, effects: {'isPatrolling': true}]*

*IMPORTANT*: false preconditions are NOT supported
*IMPORTANT*: the action effects aren't written in the memory when the action is done, this is a wanted behaviour because in most of the games you will want to set these variables from the memory or from the sensors.
If you want you can override Exit in your GoapAction and set the effects to the memory, example following.


####Goal
can be defined as a list of requisites, described as a State, this is basically what the Agent should do.

Examples:

* *'Kill Enemy': {'hurtEnemy': true}*
* *'Patrol': {'isPatrolling': true}*


####Memory
is the memory of the Agent, everything the Agent knows and feel should be inserted here. A memory also can have many sensors, in this library, which are a memory helper. Basically the job of the Memory is to create and keep updated a 'World' State.

####Sensor
is a memory helper, it should handle a specific scope.

Example: 
* *EyeSensor (check if an enemy is in line of sight)*
* *EarsSensor (check if an enemy has been heard, you could make a single EnemySensor which has EyeSensor and EarsSensor of course)*


Now you should understand what is a GOAP library for and what you should use it for, if still having questions or want to know more about this field I advise you to read Jeff Orkin's papers here: http://alumni.media.mit.edu/~jorkin/

###How to use ReGoap in Unity3D
1. Clone this repository in your Unity project.
Command line:
```bash
git clone https://github.com/luxkun/ReGoap.git
```
2. Create a GameObject for your Agent
3. Add a GoapAgent component, choose a name (it is advised to create your own class that inherit GoapAgent, or implements IReGoapAgent)
4. Add a GoapMemory component, choose a name (it is advised to create your own class that inherit GoapMemory, or implements IReGoapMemory)
5. [optional | repeat as needed] Add your own sensor class that inherit GoapSensor or implements IReGoapSensor
6. [repeat as needed] Add your own class that inherit GoapAction or implements IReGoapAction (choose wisely what preconditions and effects should this action have) and implement the action logic by overriding the Run function, this function will be called by the GoapAgent.
7. [repeat as needed] Add your own class that inherit GoapGoal or implements IReGoapGoal (choose wisely what goal state the goal has)
8. Add ONE GoapPlannerManager to any GameObject (not the agent!), this will handle all the planning in multiple-threads.
9. Play the game :-)

What's more? nothing really, the library will handle all the planning, choose the actions to complete a goal and run the first one until it's done, then the second one and so on, all you need to do is implement your own actions and goals.

In the next paragraphs I'll explain how to create your own classes (but for most of behaviours all you need to implement is GoapAction and GoapGoal).

####How to implement your own GoapAction
Check out the actions in this example: https://github.com/luxkun/ReGoap/tree/master/Unity/FSMExample/Actions

Check out GoapAction implementation, to see what functions you can override: https://github.com/luxkun/ReGoap/blob/master/Unity/GoapAction.cs

You can also implement your own GoapAction by implementing IReGoapAction interface, not advised except you know what you are doing!

For a simple implementation all you have to do is this:
```C#
public class MyGoapAction : GoapAction
{
    protected override void Awake()
    {
        base.Awake();
        preconditions.Set("myPrecondition", myValue); // myValue can be anything, it's an object internally
        effects.Set("myEffects", myValue);
    }
    public override void Run(IReGoapAction previous, IReGoapAction next, IReGoapActionSettings settings, ReGoapState goalState, Action<IReGoapAction> done, Action<IReGoapAction> fail)
    {
        base.Run(previous, next, goalState, done, fail);
        // do your own game logic here
        // when done, in this function or outside this function, call the done or fail callback, automatically saved to doneCallback and failCallback by GoapAction
        doneCallback(this); // this will tell the GoapAgent that the action is succerfully done and go ahead in the action plan
        // if the action has failed then run failCallback(this), the GoapAgent will automatically invalidate the whole plan and ask the GoapPlannerManager to create a new plan
    }
}
```

As written before the GoapAction does not, by default, write the effects on the memory, but the memory should check out if the effects are effectively done, if for any reason you want to set the effects at the end of the action you can add this code to your GoapAction implementation:
```C#
    public override void Exit(IReGoapAction next)
    {
        base.Exit(next);

        var worldState = agent.GetMemory().GetWorldState();
        foreach (var pair in effects) {
            worldState.Set(pair.Key, pair.Value);
        }
    }
```

You can also have preconditions and effects that are dynamically changed based on the next action's preconditions/effects, for example this how you can handle a GoTo action in your agent.

Check out FSMExample to see how to do this:
https://github.com/luxkun/ReGoap/blob/master/Unity/FSMExample/Actions/GenericGoToAction.cs

####How to implement your own GoapGoal
This is less tricky, most of the goal will only override the Awake function to add your own goal state (objectives).

Anyway check out GoapGoal, like everything you can implement your own class from scratch by implementing IReGoapGoal interface: https://github.com/luxkun/ReGoap/blob/master/Unity/GoapGoal.cs

Also check out the goals in this example: https://github.com/luxkun/ReGoap/tree/master/Unity/FSMExample/Goals

```C#
public class MyGoapGoal : GoapGoal
{
    protected override void Awake()
    {
        base.Awake();
        goal.Set("myRequirement", myValue); // like any State myValue is an object, so can be anything
    }
}
```

####How to implement your own GoapSensor
Check out GoapSensor basic class here: https://github.com/luxkun/ReGoap/blob/master/Unity/GoapSensor.cs

Check out examples here: https://github.com/luxkun/ReGoap/tree/master/Unity/FSMExample/Sensors

As always you can implement your own class by implementing IReGoapSensor interface.

```C#
public class MySensor : GoapSensor
{
    void FixedUpdate()
    {
        var worldState = memory.GetWorldState();
        worldState.Set("mySensorValue", myValue); // like always myValue can be anything... it's a GoapState :)
    }
}
```

---

##Debugging
To debug your own agent you can, of course, debug on your own, with your favourite editor.

But ReGoap has a very userful debugger for agents in Unity (https://github.com/luxkun/ReGoap/blob/master/Unity/Editor/ReGoapNodeEditor.cs and https://github.com/luxkun/ReGoap/blob/master/Unity/Editor/ReGoapNodeBaseEditor.cs).

To use it just click on the Unity's menu **Window** and then **ReGoap Debugger**, an Unity Window will open, this is the agent debugger.

Now if you click on any agent in your scene (while playing, works only on running agents) the window will automatically update letting you know the agent's "thoughts" (current world state, choosen goal and current plan, possibile goals, possible actions, what can be done and what not, try it!).

---

##Pull Requests
Any pull request is appreciated, just make sure to check Unity Tests (menu **Window** -> **Editor Tests Runner** -> **Run All**) before committing and to keep the same style of code.
