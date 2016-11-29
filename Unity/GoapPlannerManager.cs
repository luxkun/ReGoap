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
            planner.Plan(work.agent, work.blacklistGoal, work.actions,
                (newGoal) => onDonePlan(this, work, newGoal));
        }
    }
}

// behaviour that should be added once (and only once) to a gameobject in your unity's scene
public class GoapPlannerManager : MonoBehaviour
{
    public static GoapPlannerManager instance;

    public int threadsCount = 4;
    private GoapPlannerThread[] planners;

    private AutoResetEvent threadEvents;
    private volatile Queue<PlanWork> worksQueue;
    private volatile List<PlanWork> doneWorks;
    private Thread[] threads;

    public bool workInFixedUpdate = true;
    public ReGoapPlannerSettings plannerSettings;

    #region UnityFunctions
    protected virtual void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            var errorString =
                "[GoapPlannerManager] Trying to instantiate a new manager but there can be only one per scene.";
            ReGoapLogger.LogError(errorString);
            throw new UnityException(errorString);
        }
        instance = this;

        doneWorks = new List<PlanWork>();
        worksQueue = new Queue<PlanWork>();
        planners = new GoapPlannerThread[threadsCount];
        threads = new Thread[threadsCount];
        threadEvents = new AutoResetEvent(false);
        if (threadsCount > 1)
        {
            ReGoapLogger.Log("[GoapPlannerManager] Running in multi-thread mode.");
            for (int i = 0; i < threadsCount; i++)
            {
                planners[i] = new GoapPlannerThread(threadEvents, plannerSettings, worksQueue, OnDonePlan);
                var thread = new Thread(planners[i].MainLoop) {IsBackground = true};
                thread.Start();
                threads[i] = thread;
            }
        } // no threads run
        else
        {
            ReGoapLogger.Log("[GoapPlannerManager] Running in single-thread mode.");
            planners[0] = new GoapPlannerThread(threadEvents, plannerSettings, worksQueue, OnDonePlan);
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
        if (workInFixedUpdate) return;
        Tick();
    }

    protected virtual void FixedUpdate()
    {
        if (!workInFixedUpdate) return;
        Tick();
    }

    // check all threads for done work
    protected virtual void Tick()
    {
        lock (doneWorks)
        {
            if (doneWorks.Count > 0)
            {
                var doneWorksCopy = doneWorks.ToArray();
                doneWorks.Clear();
                foreach (var work in doneWorksCopy)
                {
                    work.callback(work.newGoal);
                }
            }
        }
        if (threadsCount <= 1)
        {
            planners[0].CheckWorkers();
        }
    }
    #endregion

    // called in another thread
    private void OnDonePlan(GoapPlannerThread plannerThread, PlanWork work, IReGoapGoal newGoal)
    {
        work.newGoal = newGoal;
        lock (doneWorks)
        {
            doneWorks.Add(work);
#if DEBUG
            if (work.newGoal != null)
            {
                ReGoapLogger.Log("[GoapPlannerManager] Done calculating plan, actions list:");
                var i = 0;
                foreach (var action in work.newGoal.GetPlan())
                {
                    ReGoapLogger.Log(string.Format("{0}: {1}", i++, action));
                }
            }
#endif
        }
    }

    public PlanWork Plan(IReGoapAgent agent, IReGoapGoal blacklistGoal, Queue<IReGoapAction> currentPlan, Action<IReGoapGoal> callback)
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
    public readonly IReGoapAgent agent;
    public readonly IReGoapGoal blacklistGoal;
    public readonly Queue<IReGoapAction> actions;
    public readonly Action<IReGoapGoal> callback;

    public IReGoapGoal newGoal;

    public PlanWork(IReGoapAgent agent, IReGoapGoal blacklistGoal, Queue<IReGoapAction> actions, Action<IReGoapGoal> callback) : this()
    {
        this.agent = agent;
        this.blacklistGoal = blacklistGoal;
        this.actions = actions;
        this.callback = callback;
    }
}
