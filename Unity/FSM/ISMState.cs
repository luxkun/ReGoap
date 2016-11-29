using System;
using System.Collections.Generic;

public interface ISmState
{
    List<ISmTransistion> Transistions { get; set; }

    void Enter();
    void Exit();
    void Init(StateMachine stateMachine);
    bool IsActive();

    int GetPriority();
}

public interface ISmTransistion
{
    Type TransistionCheck(ISmState state);
    int GetPriority();
}

// you can inherit your FSM's transistion from this, but feel free to implement your own (note: must implement ISmTransistion and IComparable<ISmTransistion>)
public class SmTransistion : ISmTransistion, IComparable<ISmTransistion>
{
    private readonly int priority;
    private readonly Func<ISmState, Type> checkFunc;

    public SmTransistion(int priority, Func<ISmState, Type> checkFunc)
    {
        this.priority = priority;
        this.checkFunc = checkFunc;
    }

    public Type TransistionCheck(ISmState state)
    {
        return checkFunc(state);
    }

    public int GetPriority()
    {
        return priority;
    }

    public int CompareTo(ISmTransistion other)
    {
        return -GetPriority().CompareTo(other.GetPriority());
    }
}