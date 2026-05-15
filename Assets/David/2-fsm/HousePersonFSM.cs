using System.Collections.Generic;
using UnityEngine;
using static FSM<HousePersonAI.State, Worker>;

public class HousePersonFSM : MonoBehaviour
{
    [SerializeField] private FSM<HousePersonAI.State, Worker> fsm = HousePersonAI.FSMInstance();
    [SerializeField] private HousePersonAI.State state = HousePersonAI.State.Sleep;
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

static class HousePersonAI {

    public enum State {
        Idle,
        Sleep,
        Housework,
        GetWaterFromWell,
        CarryWaterHome,
    }


    private static FSM<State, Worker> instance;
    public static FSM<State, Worker> FSMInstance()
    {
        if (instance == null)
        {
            instance = new FSM<State, Worker>
            {
                StateDelegatesTable = HousePersonAI.StateDelegatesTable,
                StateTransitionTable = HousePersonAI.StateTransitionTable
            };
        }
        return instance;
    } 

    public static Dictionary<State, StateMethods> StateDelegatesTable = new()
    {
        [State.Idle] = new StateMethods { Enter = SetActiveTarget },
        [State.Sleep] = new StateMethods { Enter = SetActiveTarget, Update = Sleep },
        [State.Housework] = new StateMethods { Enter = SetActiveTarget, Update = Housework },
        [State.GetWaterFromWell] = new StateMethods { Enter = SetActiveTarget, Update = GetWaterFromWell, Leave = StopCurrent },
        [State.CarryWaterHome] = new StateMethods { Enter = SetActiveTarget, Update = CarryWaterHome, Leave = StopCurrent },
    };

    public static Dictionary<State, Transition[]>  StateTransitionTable = new()
        {
            // Transitions from State.Idle
            [State.Idle] = new Transition[] { 
                new() {
                    trigger = (w, s)=> w.FindFirst(LocationType.House) != null,
                    goTo = State.Housework
                }},

            // Transitions from State.Sleep
            [State.Sleep] = new Transition[] { 
                new() {
                    trigger = (w, s)=> w.energy >= 1,
                    goTo = State.Idle
                },
                new() {
                    trigger = (w, s)=> w.activeTarget == null,
                    goTo = State.Idle
                }, 
                new() {
                    trigger = (w, s)=> w.activeTarget?.Available(Resource.Water) == 0,
                    goTo = State.GetWaterFromWell
                }},

            // Transitions from State.Housework
            [State.Housework]  = new Transition[] { 
                new() {
                    trigger = (w, s)=> w.activeTarget?.Available(Resource.Water) == 0,
                    goTo = State.GetWaterFromWell
                },
                new() {
                    trigger = (w, s)=> w.energy <= 0,
                    goTo = State.Sleep
                }},

            // Transitions from State.GetWaterFromWell
            [State.GetWaterFromWell] = new Transition[] { 
                new() {
                    trigger = (w, s)=> w.water >= 1,
                    goTo = State.CarryWaterHome
                }, new() {
                    trigger = (w, s)=> w.activeTarget == null,
                    goTo = State.Idle
                },
                new() {
                    trigger = (w, s)=> w.activeTarget?.Available(Resource.Water) == 0,
                    goTo = State.GetWaterFromWell
                }},

            // Transitions from State.CarryWaterHome
            [State.CarryWaterHome] = new Transition[] { 
                new() {
                    trigger = (w, s) => w.water == 0 && w.energy <= 0,
                    goTo = State.Sleep
                },
                new() {
                    trigger = (w, s) => w.water == 0 && w.energy > 0,
                    goTo = State.Idle
                },
                new() {
                    trigger = (w, s) => w.activeTarget == null,
                    goTo = State.Idle
                }},
        };


    static void SetActiveTarget(Worker worker, State state)
    {
        if (state == State.Idle)
            worker.activeTarget = null;
        else if (state == State.Sleep || state == State.CarryWaterHome || state == State.Housework)
            worker.activeTarget = worker.FindFirst(LocationType.House);
        else if (state == State.GetWaterFromWell)
            worker.activeTarget = worker.FindFirstWithResource(LocationType.Well, Resource.Water, 1);
    }

    static void StopCurrent(Worker worker, State state)
    {
        worker.StopCurrent();
    }

    static void Sleep(Worker worker, State state)
    {
        worker.DrinkAndSleep();
    }

    static void Housework(Worker worker, State state)
    {
        if (worker.CloseTo(worker.activeTarget))
        {
            worker.Stop();
            worker.energy -= Time.deltaTime * 0.05f;
        }
        else
            worker.GoTowards(worker.activeTarget);
    }

    static void CarryWaterHome(Worker worker, State state)
    {
        if (worker.CloseTo(worker.activeTarget))
        {
            worker.Stop();
            worker.DeliverResources();
        }
        else
            worker.GoTowards(worker.activeTarget);
    }

    static void GetWaterFromWell(Worker worker, State state)
    {
        if (worker.CloseTo(worker.activeTarget))
        {
            worker.Stop();
            worker.CollectWaterFromWell();
        }
        else
            worker.GoTowards(worker.activeTarget);
    }
}