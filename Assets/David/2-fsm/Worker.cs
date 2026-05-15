using System.Collections;
using UnityEngine;

public class Worker : MonoBehaviour
{
    public int water = 0;
    public int wood = 0;
    public float thirst = 0.0f;
    public float energy = 1.0f;
    public Location activeTarget;

    private enum WorkerState {
        Idle,
        Walking,
        GatheringWood,
        CollectingWater,
        DeliveringResources,
        Drinking,
        Sleeping,
    }
    [SerializeField] private WorkerState state;
    private Steering steering;

    private Coroutine coroutine;
    private Worker[] objectsToAvoid;

    void Awake()
    {
        steering = GetComponent<Steering>();
    }

    void Start()
    {
        objectsToAvoid = FindObjectsByType<Worker>(FindObjectsSortMode.None);
    }

    public bool CloseTo(Location target)
    {
        if (target == null)
            return false;
        return (target.transform.position - transform.position).sqrMagnitude < 5;
    }

    public void GoTowards(Location target)
    {
        StopCurrent();
        if (target != null)
        {
            state = WorkerState.Walking;
            Vector3 separation = steering.Separation(objectsToAvoid, 3, 5f);
            Vector3 arrive = steering.Arrive(target.transform.position);
            Vector3 steeringForce = separation.sqrMagnitude > 0 ? separation : arrive;
            steering.ApplySteeringForce(steeringForce, Steering.FacingMode.Forward);
        }
    }

    public void Stop()
    {
        steering.ApplySteeringForce(steering.Stop(), Steering.FacingMode.Forward);
    }

    public void StopCurrent()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        state = WorkerState.Idle;
    }

    public void GatherWood()
    {
        if (state != WorkerState.GatheringWood)
        {
            StopCurrent();
            coroutine = StartCoroutine(GatherCoroutine());
        }
    }

    private IEnumerator GatherCoroutine()
    {
        state = WorkerState.GatheringWood;
        while (wood < 10)
        {
            thirst += Random.value * 0.1f;
            energy -= Random.value * 0.05f;
            int gained = 0;
            if (activeTarget != null)
                gained = activeTarget.LoseResource(Resource.Wood, 1);
            wood += gained;
            if (gained > 0)
                yield return new WaitForSeconds(1);
            else
                break;
        }
        StopCurrent();
    }

    public void DeliverResources()
    {
        if (state != WorkerState.DeliveringResources)
        {
            StopCurrent();
            coroutine = StartCoroutine(DeliverCoroutine());
        }
    }

    private IEnumerator DeliverCoroutine()
    {
        state = WorkerState.DeliveringResources;
        yield return new WaitForSeconds(0.2f);
        if (activeTarget != null)
        {
            wood = activeTarget.ReceiveResource(Resource.Wood, wood);
            water = activeTarget.ReceiveResource(Resource.Water, water);
        }
        StopCurrent();
    }

    public void DrinkFromWell()
    {
        if (state != WorkerState.Drinking)
        {
            StopCurrent();
            coroutine = StartCoroutine(DrinkCoroutine());
        }
    }

    private IEnumerator DrinkCoroutine()
    {
        state = WorkerState.Drinking;
        yield return new WaitForSeconds(1);
        if (activeTarget?.LoseResource(Resource.Water, 1) > 0)
            thirst = 0;
        StopCurrent();
    }

    public void DrinkAndSleep()
    {
        if (state != WorkerState.Sleeping)
        {
            StopCurrent();
            coroutine = StartCoroutine(DrinkAndSleepCoroutine());
        }
    }

    private IEnumerator DrinkAndSleepCoroutine()
    {
        state = WorkerState.Sleeping;
        do
        {
            yield return new WaitForSeconds(1);
        } while (activeTarget.LoseResource(Resource.Water, 1) <= 0);
        energy = 1.0f;
        StopCurrent();
    }

    public void CollectWaterFromWell()
    {
        if (state != WorkerState.CollectingWater)
        {
            StopCurrent();
            coroutine = StartCoroutine(CollectWaterCoroutine());
        }
    }

    private IEnumerator CollectWaterCoroutine()
    {
        state = WorkerState.CollectingWater;
        yield return new WaitForSeconds(1);
        if (activeTarget != null)
            water = activeTarget.LoseResource(Resource.Water, 1);
        StopCurrent();
    }

    public Location FindFirst(LocationType type)
    {
        Location[] targets = FindObjectsByType<Location>(FindObjectsSortMode.None);
        for (int i = 0; i < targets.Length;i++)
        {
            if (targets[i].type == type)
                return targets[i];
        }
        return null;
    }

    public Location FindFirstWithResource(LocationType type, Resource resource, int count)
    {
        Location[] targets = FindObjectsByType<Location>(FindObjectsSortMode.None);
        for (int i = 0; i < targets.Length;i++)
        {
            if (targets[i].type == type && targets[i].Available(resource) >= count)
                return targets[i];
        }
        return null;
    }

}
