using UnityEngine;

public class AllInOne : MonoBehaviour
{
    enum WorkerState {
        GoToForestAndGetWood,
        GoToWarehouse,
        GoToWellAndDrink,
        GoHomeAndRest
    }

    [SerializeField] private WorkerState state;

    private Worker worker;

    void Awake()
    {
        worker = GetComponent<Worker>();
    }

    void ChangeState(WorkerState state)
    {
        this.state = state;
        worker.StopCurrent();
    }

    void Update()
    {
        switch(state)
        {
            case WorkerState.GoToForestAndGetWood:
                worker.activeTarget = worker.FindFirst(LocationType.Forest);
                if (worker.CloseTo(worker.activeTarget))
                {
                    worker.Stop();
                    worker.GatherWood();
                    if (worker.thirst >= 1)
                        ChangeState(WorkerState.GoToWellAndDrink);
                    else if (worker.wood == 10)
                        ChangeState(WorkerState.GoToWarehouse);
                } else
                    worker.GoTowards(worker.activeTarget);
                break;
            case WorkerState.GoToWarehouse:
                worker.activeTarget = worker.FindFirst(LocationType.Warehouse);
                if (worker.CloseTo(worker.activeTarget))
                {
                    worker.Stop();
                    worker.DeliverResources();
                    if (worker.wood == 0)
                    {
                        if (worker.energy <= 0)
                            ChangeState(WorkerState.GoHomeAndRest);
                        else
                            ChangeState(WorkerState.GoToForestAndGetWood);
                    }
                } else
                    worker.GoTowards(worker.activeTarget);
                break;
            case WorkerState.GoHomeAndRest:
                worker.activeTarget = worker.FindFirst(LocationType.House);
                if (worker.CloseTo(worker.activeTarget))
                {
                    worker.Stop();
                    worker.DrinkAndSleep();
                    if (worker.energy >= 1)
                        ChangeState(WorkerState.GoToForestAndGetWood);
                }
                else
                    worker.GoTowards(worker.activeTarget);
                break;
            case WorkerState.GoToWellAndDrink:
                worker.activeTarget = worker.FindFirst(LocationType.Well);
                if (worker.CloseTo(worker.activeTarget))
                {
                    worker.Stop();
                    worker.DrinkFromWell();
                    if (worker.thirst <= 0)
                        ChangeState(WorkerState.GoToForestAndGetWood);
                }
                else
                    worker.GoTowards(worker.activeTarget);
                break;
        }
    }
}
