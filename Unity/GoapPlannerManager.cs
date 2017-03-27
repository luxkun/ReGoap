using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

// every thread runs on one of these classes
public class GoapPlannerThread
{
    private volatile ReGoapPlanner planner;
    private volatile Queue<PlanWork> worksQueue;
    private bool isRunning;
    private readonly Action<GoapPlannerThread, PlanWork, IReGoapGoal> onDonePlan;
    private readonly AutoResetEvent threadEvents;

    public GoapPlannerThread(AutoResetEvent threadEvents, ReGoapPlannerSettings plannerSettings, Queue<PlanWork> worksQueue, Action<GoapPlannerThread, PlanWork, IReGoapGoal> onDonePlan)
    {
        this.threadEvents = threadEvents;
        planner = new ReGoapPlanner(plannerSettings);
        this.worksQueue = worksQueue;
        isRunning = true;
        this.onDonePlan = onDonePlan;
    }

    public void Stop()
    {
        isRunning = false;
    }

    public void MainLoop()
    {
        while (isRunning)
        {
            threadEvents.WaitOne();
            CheckWorkers();
        }
    }

    public void CheckWorkers()
    {
        PlanWork? checkWork = null;
        lock (worksQueue)
        {
            if (worksQueue.Count > 0)
            {
                checkWork = worksQueue.Dequeue();
            }
        }
        if (checkWork != null)
        {
            var work = checkWork.Value;
            planner.Plan(work.Agent, work.BlacklistGoal, work.Actions,
                (newGoal) => onDonePlan(this, work, newGoal));
        }
    }
}

// behaviour that should be added once (and only once) to a gameobject in your unity's scene
public class GoapPlannerManager : MonoBehaviour
{
    public static GoapPlannerManager Instance;

    public int ThreadsCount = 4;
    private GoapPlannerThread[] planners;

    private AutoResetEvent threadEvents;
    private volatile Queue<PlanWork> worksQueue;
    private volatile List<PlanWork> doneWorks;
    private Thread[] threads;

    public bool WorkInFixedUpdate = true;
    public ReGoapPlannerSettings PlannerSettings;

    public ReGoapLogger.DebugLevel LogLevel = ReGoapLogger.DebugLevel.Full;

    public int NodeWarmupCount = 1000;
    public int StatesWarmupCount = 10000;

    #region UnityFunctions
    protected virtual void Awake()
    {
        ReGoapNode.Warmup(NodeWarmupCount);
        ReGoapState.Warmup(StatesWarmupCount);

        ReGoapLogger.Instance.Level = LogLevel;
        if (Instance != null)
        {
            Destroy(this);
            var errorString =
                "[GoapPlannerManager] Trying to instantiate a new manager but there can be only one per scene.";
            ReGoapLogger.LogError(errorString);
            throw new UnityException(errorString);
        }
        Instance = this;

        doneWorks = new List<PlanWork>();
        worksQueue = new Queue<PlanWork>();
        planners = new GoapPlannerThread[ThreadsCount];
        threads = new Thread[ThreadsCount];
        threadEvents = new AutoResetEvent(false);
        if (ThreadsCount > 1)
        {
            ReGoapLogger.Log("[GoapPlannerManager] Running in multi-thread mode.");
            for (int i = 0; i < ThreadsCount; i++)
            {
                planners[i] = new GoapPlannerThread(threadEvents, PlannerSettings, worksQueue, OnDonePlan);
                var thread = new Thread(planners[i].MainLoop) { IsBackground = true };
                thread.Start();
                threads[i] = thread;
            }
        } // no threads run
        else
        {
            ReGoapLogger.Log("[GoapPlannerManager] Running in single-thread mode.");
            planners[0] = new GoapPlannerThread(threadEvents, PlannerSettings, worksQueue, OnDonePlan);
        }
    }

    protected virtual void Start()
    {
    }

    void OnDestroy()
    {
        foreach (var planner in planners)
        {
            planner.Stop();
        }
        // should wait here?
        foreach (var thread in threads)
        {
            if (thread != null)
                thread.Abort();
        }
    }

    protected virtual void Update()
    {
        ReGoapLogger.Instance.Level = LogLevel;
        if (WorkInFixedUpdate) return;
        Tick();
    }

    protected virtual void FixedUpdate()
    {
        if (!WorkInFixedUpdate) return;
        Tick();
    }

    // check all threads for done work
    protected virtual void Tick()
    {
        if (doneWorks.Count > 0)
        {
            lock (doneWorks)
            {
                foreach (var work in doneWorks)
                {
                    work.Callback(work.NewGoal);
                }
                doneWorks.Clear();
            }
        }
        if (ThreadsCount <= 1)
        {
            planners[0].CheckWorkers();
        }
    }
    #endregion

    // called in another thread
    private void OnDonePlan(GoapPlannerThread plannerThread, PlanWork work, IReGoapGoal newGoal)
    {
        work.NewGoal = newGoal;
        lock (doneWorks)
        {
            doneWorks.Add(work);
            if (work.NewGoal != null && ReGoapLogger.Instance.Level == ReGoapLogger.DebugLevel.Full)
            {
                ReGoapLogger.Log("[GoapPlannerManager] Done calculating plan, actions list:");
                var i = 0;
                foreach (var action in work.NewGoal.GetPlan())
                {
                    ReGoapLogger.Log(string.Format("{0}: {1}", i++, action.Action));
                }
            }
        }
    }

    public PlanWork Plan(IReGoapAgent agent, IReGoapGoal blacklistGoal, Queue<ReGoapActionState> currentPlan, Action<IReGoapGoal> callback)
    {
        var work = new PlanWork(agent, blacklistGoal, currentPlan, callback);
        lock (worksQueue)
        {
            worksQueue.Enqueue(work);
        }
        threadEvents.Set();
        return work;
    }
}

public struct PlanWork
{
    public readonly IReGoapAgent Agent;
    public readonly IReGoapGoal BlacklistGoal;
    public readonly Queue<ReGoapActionState> Actions;
    public readonly Action<IReGoapGoal> Callback;

    public IReGoapGoal NewGoal;

    public PlanWork(IReGoapAgent agent, IReGoapGoal blacklistGoal, Queue<ReGoapActionState> actions, Action<IReGoapGoal> callback) : this()
    {
        Agent = agent;
        BlacklistGoal = blacklistGoal;
        Actions = actions;
        Callback = callback;
    }
}
