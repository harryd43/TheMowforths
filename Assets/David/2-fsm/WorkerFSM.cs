using System.Collections.Generic;
using UnityEngine;
using static FSM<WorkerAI.State, Worker>;

public class WorkerFSM : MonoBehaviour
{
    [SerializeField] private FSM<WorkerAI.State, Worker> fsm = WorkerAI.FSMInstance();
    [SerializeField] private WorkerAI.State state = WorkerAI.State.GoToForestAndGetWood;
    private Worker worker;

    void Start()
    {
        worker = GetComponent<Worker>();
        fsm.Enter(worker, state);
    }

    void Update()
    {
        state = fsm.Update(worker, state);
    }
}

static class WorkerAI {

    public enum State {
        Idle,
        GoToForestAndGetWood,
        GoToWarehouse,
        GoToWellAndDrink,
        GoHomeAndRest
    }

    private static FSM<State, Worker> instance;
    public static FSM<State, Worker> FSMInstance()
    {
        if (instance == null)
        {
            instance = new FSM<State, Worker>
            {
                StateDelegatesTable = WorkerAI.StateDelegatesTable,
                StateTransitionTable = WorkerAI.StateTransitionTable
            };
        }
        return instance;
    } 

    public static Dictionary<State, StateMethods> StateDelegatesTable = new()
    {
        [State.Idle] = new StateMethods { Enter = SetActiveTarget  },
        [State.GoHomeAndRest] = new StateMethods { Enter = SetActiveTarget, Update = GoHomeAndRest, Leave = StopCurrent },
        [State.GoToForestAndGetWood] = new StateMethods { Enter = SetActiveTarget, Update = GoToForestAndGetWood, Leave = StopCurrent },
        [State.GoToWarehouse] = new StateMethods { Enter = SetActiveTarget, Update = GoToWarehouse, Leave = StopCurrent },
        [State.GoToWellAndDrink] = new StateMethods { Enter = SetActiveTarget, Update = GoToWellAndDrink, Leave = StopCurrent }
    };

    public static Dictionary<State, Transition[]>  StateTransitionTable = new()
        {
            [State.Idle] = new Transition[] { new Transition
                {
                    trigger = (Worker worker, State state) => worker.energy <= 0,
                    goTo = State.GoHomeAndRest
                }, new Transition
                {
                    trigger = (Worker worker, State state) => worker.thirst > 1 && worker.FindFirstWithResource(LocationType.Well, Resource.Water, 1),
                    goTo = State.GoToWellAndDrink
                }, new Transition
                {
                    trigger = (Worker worker, State state) => worker.wood < 10 && worker.FindFirstWithResource(LocationType.Forest, Resource.Wood, 1),
                    goTo = State.GoToForestAndGetWood
                }, new Transition
                {
                    trigger = (Worker worker, State state) => worker.wood > 0,
                    goTo = State.GoToWarehouse
                }
            },
            [State.GoToWellAndDrink] = new Transition[] { new Transition
                {
                    trigger = (Worker worker, State state) => worker.thirst <= 0,
                    goTo = State.GoToForestAndGetWood
                }, new Transition
                {
                    trigger = (Worker worker, State state) => worker.activeTarget == null,
                    goTo = State.Idle
                }, new Transition
                {
                    trigger = (Worker worker, State state) => worker.activeTarget?.Available(Resource.Water) == 0,
                    goTo = State.GoToWellAndDrink
                }},
            [State.GoHomeAndRest] = new Transition[] { new Transition
                {
                    trigger = (Worker worker, State state) => worker.energy >= 1,
                    goTo = State.GoToForestAndGetWood
                }, new Transition
                {
                    trigger = (Worker worker, State state) => worker.activeTarget == null,
                    goTo = State.Idle
                }},
            [State.GoToWarehouse] = new Transition[] { new Transition
                {
                    trigger = (Worker worker, State state) => worker.wood == 0 && worker.energy <= 0,
                    goTo = State.GoHomeAndRest
                }, new Transition
                {
                    trigger = (Worker worker, State state) => worker.wood == 0 && worker.energy > 0,
                    goTo = State.GoToForestAndGetWood
                }, new Transition
                {
                    trigger = (Worker worker, State state) => worker.activeTarget == null,
                    goTo = State.Idle
                }},
            [State.GoToForestAndGetWood] = new Transition[] { new Transition
                {
                    trigger = (Worker worker, State state) => worker.thirst > 1,
                    goTo = State.GoToWellAndDrink
                }, new Transition
                {
                    trigger = (Worker worker, State state) => worker.wood >= 10,
                    goTo = State.GoToWarehouse
                }, new Transition
                {
                    trigger = (Worker worker, State state) => worker.activeTarget == null,
                    goTo = State.Idle
                }, new Transition
                {
                    trigger = (Worker worker, State state) => worker.activeTarget?.Available(Resource.Wood) == 0,
                    goTo = State.GoToForestAndGetWood
                }
            }
        };


    static void SetActiveTarget(Worker worker, State state)
    {
        if (state == State.Idle)
            worker.activeTarget = null;
        else if (state == State.GoToForestAndGetWood)
            worker.activeTarget = worker.FindFirstWithResource(LocationType.Forest, Resource.Wood, 1);
        else if (state == State.GoToWarehouse)
            worker.activeTarget = worker.FindFirst(LocationType.Warehouse);
        else if (state == State.GoHomeAndRest)
            worker.activeTarget = worker.FindFirstWithResource(LocationType.House, Resource.Water, 1);
        else if (state == State.GoToWellAndDrink)
            worker.activeTarget = worker.FindFirstWithResource(LocationType.Well, Resource.Water, 1);
    }

    static void StopCurrent(Worker worker, State state)
    {
        worker.StopCurrent();
    }

    static void GoToForestAndGetWood(Worker worker, State state)
    {
        if (worker.CloseTo(worker.activeTarget))
        {
            worker.Stop();
            worker.GatherWood();
        }
        else
            worker.GoTowards(worker.activeTarget);
    }

    static void GoToWarehouse(Worker worker, State state)
    {
        if (worker.CloseTo(worker.activeTarget))
        {
            worker.Stop();
            worker.DeliverResources();
        }
        else
            worker.GoTowards(worker.activeTarget);
    }

    static void GoHomeAndRest(Worker worker, State state)
    {
        if (worker.CloseTo(worker.activeTarget))
        {
            worker.Stop();
            worker.DrinkAndSleep();
        }
        else
            worker.GoTowards(worker.activeTarget);
    }

    static void GoToWellAndDrink(Worker worker, State state)
    {
        if (worker.CloseTo(worker.activeTarget))
        {
            worker.Stop();
            worker.DrinkFromWell();
        }
        else
            worker.GoTowards(worker.activeTarget);
    }
}