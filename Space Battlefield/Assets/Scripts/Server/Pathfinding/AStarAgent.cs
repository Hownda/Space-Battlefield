using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AStarAgentStatus
{
    Invalid,
    InProgress,
    Finished,
    RePath
}

public class AStarAgent : MonoBehaviour
{
    public float Speed;
    public float TurnSpeed;
    [HideInInspector] public int Priority { get; private set; }
    Point _start;
    Point _end;
    Vector3 _startPosition;
    Vector3 _endPosition;
    [HideInInspector] public List<Point> TotalPath;
    [HideInInspector] public List<Point> CornerPoints;

    public bool CurvePath;

    [HideInInspector] public AStarAgentStatus Status = AStarAgentStatus.Finished;

    private void Awake()
    {
        AssignPriority();

    }

    private void Start()
    {
        SetStationaryPoint();
    }

    private void AssignPriority()
    {
        AStarAgent[] agents = FindObjectsOfType<AStarAgent>();
        //Sort by speed
        for (int i = 0; i < agents.Length; i++)
        {
            for (int j = i; j < agents.Length; j++)
            {
                if (agents[i].Speed > agents[j].Speed)
                {
                    AStarAgent pom = agents[i];
                    agents[i] = agents[j];
                    agents[j] = agents[i];
                }
            }
        }

        for (int i = 0; i < agents.Length; i++)
        {
            agents[i].Priority = i;
        }
    }

    private float HeuristicFunction(Vector3 p1, Vector3 p2)
    {
        return (p2 - p1).sqrMagnitude;
    }

    private List<Point> ReconstructPath(PointData start, PointData current, PointData[][][] dataSet)
    {
        CornerPoints = new List<Point>();
        List<Point> totalPath = new List<Point>();

        PointData currentPointData = dataSet[current.Coords.x][current.Coords.y][current.Coords.z];
        Point currentPoint = WorldManager.Instance.Grid[current.Coords.x][current.Coords.y][current.Coords.z];

        currentPoint.AddMovingData(this, currentPointData.TimeToReach);
        totalPath.Add(currentPoint);

        Point cameFromPoint = WorldManager.Instance.Grid[current.CameFrom.x][current.CameFrom.y][current.CameFrom.z];

        Vector3 direction = (currentPoint.Coords - cameFromPoint.Coords);
        direction = direction.normalized;

        CornerPoints.Add(currentPoint);

        int count = 0;
        while (current.CameFrom.x != -1 && count < 10000)
        {

            currentPoint = WorldManager.Instance.Grid[current.Coords.x][current.Coords.y][current.Coords.z];
            PointData cameFromPointData = dataSet[current.CameFrom.x][current.CameFrom.y][current.CameFrom.z];
            cameFromPoint = WorldManager.Instance.Grid[current.CameFrom.x][current.CameFrom.y][current.CameFrom.z];

            Vector3 dir = (currentPoint.Coords - cameFromPoint.Coords);
            if (dir != direction)
            {
                CornerPoints.Add(currentPoint);
                direction = dir;
            }

            cameFromPoint.AddMovingData(this, cameFromPointData.TimeToReach);
            totalPath.Add(cameFromPoint);
            current = dataSet[current.CameFrom.x][current.CameFrom.y][current.CameFrom.z];
        }

        currentPoint = WorldManager.Instance.Grid[current.Coords.x][current.Coords.y][current.Coords.z];
        CornerPoints.Add(currentPoint);

        for (int i = 0; i < totalPath.Count; i++)
        {
            totalPath[i].CheckForIntersections();
        }

        return totalPath;
    }

    private void Heapify(List<PointData> list, int i)
    {
        int parent = (i - 1) / 2;
        if (parent > -1)
        {
            if (list[i].FScore < list[parent].FScore)
            {
                PointData pom = list[i];
                list[i] = list[parent];
                list[parent] = pom;
                Heapify(list, parent);
            }
        }
    }

    private void HeapifyDeletion(List<PointData> list, int i)
    {
        int smallest = i;
        int l = 2 * i + 1;
        int r = 2 * i + 2;

        if (l < list.Count && list[l].FScore < list[smallest].FScore)
        {
            smallest = l;
        }
        if (r < list.Count && list[r].FScore < list[smallest].FScore)
        {
            smallest = r;
        }
        if (smallest != i)
        {
            PointData pom = list[i];
            list[i] = list[smallest];
            list[smallest] = pom;

            // Recursively heapify the affected sub-tree
            HeapifyDeletion(list, smallest);
        }
    }

    public AStarAgentStatus Pathfinding(Vector3 goal,bool supressMovement=false)
    {
        _startPosition = transform.position;
        _endPosition = goal;
        _start = WorldManager.Instance.GetClosestPointWorldSpace(transform.position);
        _end = WorldManager.Instance.GetClosestPointWorldSpace(goal);
        if (_start == _end || _start.Invalid || _end.Invalid)
        {
            Status = AStarAgentStatus.Invalid;
            return Status;
        }

        if (TotalPath != null)
        {
            for (int i = 0; i < TotalPath.Count; i++)
            {
                TotalPath[i].MovingData.Remove(TotalPath[i].MovingData.Find(x => x.MovingObj == this));
            }
        }

        PointData[][][] dataSet = new PointData[WorldManager.Instance.Grid.Length][][];
        for (int i = 0; i < dataSet.Length; i++)
        {
            dataSet[i] = new PointData[WorldManager.Instance.Grid[i].Length][];
            for (int j = 0; j < dataSet[i].Length; j++)
            {
                dataSet[i][j] = new PointData[WorldManager.Instance.Grid[i][j].Length];
            }
        }

        List<PointData> openSet = new List<PointData>();

        PointData startPoint = new PointData(_start);
        dataSet[_start.Coords.x][_start.Coords.y][_start.Coords.z] = startPoint;
        startPoint.GScore = 0;

        startPoint.TimeToReach = 0;

        openSet.Add(startPoint);



        while (openSet.Count > 0)
        {
            PointData current = openSet[0];


            if (current.Coords == _end.Coords)
            {
                TotalPath = ReconstructPath(startPoint, current, dataSet);
                if (!supressMovement)
                {
                    Status = AStarAgentStatus.InProgress;
                    StartMoving();
                }
                return Status;
            }

            openSet.RemoveAt(0);
            HeapifyDeletion(openSet, 0);

            Point currentPoint = WorldManager.Instance.Grid[current.Coords.x][current.Coords.y][current.Coords.z];

            for (int i = 0; i < currentPoint.Neighbours.Count; i++)
            {
                Vector3Int indexes = currentPoint.Neighbours[i];
                Point neighbour = WorldManager.Instance.Grid[indexes.x][indexes.y][indexes.z];
                PointData neighbourData = dataSet[indexes.x][indexes.y][indexes.z];

                bool neighbourPassed = true;
                if (neighbourData == null)
                {
                    neighbourData = new PointData(neighbour);
                    dataSet[indexes.x][indexes.y][indexes.z] = neighbourData;
                    neighbourPassed = false;
                }


                float distance = (currentPoint.WorldPosition - neighbour.WorldPosition).magnitude;
                float timeToReach = current.TimeToReach + distance / Speed;
                bool neighbourAvailable = neighbour.CheckPointAvailability(timeToReach, Priority);
                if (neighbour == _end)
                {
                    if (neighbourAvailable == false)
                    {
                        Status = AStarAgentStatus.Invalid;
                        return Status;
                    }
                }
                if (!neighbour.Invalid && neighbourAvailable)
                {
                    float tenativeScore = current.GScore + WorldManager.Instance.PointDistance;
                    if (tenativeScore < neighbourData.GScore)
                    {
                        neighbourData.CameFrom = current.Coords;
                        neighbourData.GScore = tenativeScore;
                        neighbourData.FScore = neighbourData.GScore + HeuristicFunction(neighbour.WorldPosition, _end.WorldPosition);
                        neighbourData.TimeToReach = timeToReach;
                        if (!neighbourPassed)
                        {
                            openSet.Add(neighbourData);
                            Heapify(openSet, openSet.Count - 1);
                        }
                    }
                }
            }
        }
        Status = AStarAgentStatus.Invalid;
        return Status;

    }

    public void RePath()
    {
        if (Status!=AStarAgentStatus.RePath)
        {
            StopAllCoroutines();
            StartCoroutine(Coroutine_RePath());
        }
    }

    IEnumerator Coroutine_RePath()
    {
        Status = AStarAgentStatus.RePath;

        Point p = WorldManager.Instance.GetClosestPointWorldSpace(transform.position);
        p.AddMovingData(this, 0,true);

        while (Status == AStarAgentStatus.RePath)
        {
            Status = Pathfinding(_endPosition);
            if (Status == AStarAgentStatus.Invalid)
            {
                Status = AStarAgentStatus.RePath;
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    private void StartMoving()
    {
        StopAllCoroutines();
        StartCoroutine(Coroutine_CharacterFollowPath());
    }

    IEnumerator Coroutine_CharacterFollowPath()
    {
        Status = AStarAgentStatus.InProgress;
        for (int i = TotalPath.Count - 1; i >= 0; i--)
        {
            float length = (transform.position - TotalPath[i].WorldPosition).magnitude;
            float l = 0;
            while (l<length)
            {
                Vector3 forwardDirection = (TotalPath[i].WorldPosition - transform.position).normalized;
                if (CurvePath)
                {
                    transform.position += transform.forward * Time.deltaTime * Speed;
                    transform.forward = Vector3.Slerp(transform.forward, forwardDirection, Time.deltaTime * TurnSpeed);
                }
                else
                {
                    transform.forward = forwardDirection;
                    transform.position = Vector3.MoveTowards(transform.position, TotalPath[i].WorldPosition, Time.deltaTime * Speed);
                }
                l += Time.deltaTime * Speed;
                yield return new WaitForFixedUpdate();
            }
        }
        SetStationaryPoint();
        Status = AStarAgentStatus.Finished;
    }


    private void SetStationaryPoint()
    {
        Point p = WorldManager.Instance.GetClosestPointWorldSpace(transform.position);
        p.AddMovingData(this, 0, true);
        p.CheckForIntersections();
    }
}
