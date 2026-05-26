using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float maxDistance = 0.5f;
    [SerializeField] private bool useAgent = false;
    public float MovementSpeed { get => movementSpeed; set => movementSpeed = value; }

    public float MaxDistance { get => maxDistance; set => maxDistance = value; }
    [SerializeField] private Transform rootVisual;
    private NavMeshAgent agent;

    Queue<Vector3> paths;
    
    Vector3 currentTargetPath;
    Vector3 direction;
    Vector3 destination;

    public bool isMoving { get; private set; }
    bool reachDestination = false;
    public bool ReachDestination => reachDestination;

    Action currentAction;
    public event Action OnArrived;
    public event Action<Vector2> ChangeMoveDirection;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!useAgent)
        {
            Destroy(agent);
        }
        else
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }
        currentAction = null;

        isMoving = false;
        reachDestination = false;
    }

    private void Update()
    {
        if (useAgent)
        {
            direction = agent.desiredVelocity.normalized;
            ChangeMoveDirection?.Invoke(direction);
            if (isMoving && agent.isStopped)
            {
                Stop();
                OnArrived?.Invoke();
            }
        }
        else
        {
            currentAction?.Invoke();
        }
    }

    public void SetPath(Vector3 destination)
    {
        if (useAgent)
        {
            agent.SetDestination(destination);
            isMoving = true;
        }
        else
        {
            Vector3 startingPos = transform.position;
            this.destination = destination;
            if (NavMeshUtils.IsOnNavMesh(startingPos))
            {
                Vector3[] paths = NavMeshUtils.GetPath(startingPos, destination);
                //Debug.Log($"Paths: {paths.Length}");
                this.paths = new Queue<Vector3>(paths);
                DequeuePath();
            }
            else
            {
                OnArrived?.Invoke();
            }
        }
    }
    private void DequeuePath()
    {
        isMoving = true;
        reachDestination = false;
        currentTargetPath = paths.Dequeue();
        
        direction = (currentTargetPath - transform.position).normalized;

        Vector3 scaleDirection = Vector3.one;
        //scaleDirection.x = direction.x > 0 ? 1 : -1;
        
        //Debug.Log($"Scale Direction: {scaleDirection}");
        rootVisual.localScale = scaleDirection;
        
        currentAction = Moving;
        ChangeMoveDirection?.Invoke(direction);
    }
    private void Moving()
    {
        if(Vector3.Distance(transform.position, destination) < maxDistance)
        {
            Stop();
            OnArrived?.Invoke();
        }
        //Set New path
        if(Vector3.Distance(transform.position, currentTargetPath) < 0.5f)
        {
            if (paths.Count > 0)
            {
                DequeuePath();
            }
            else
            {
                Stop();
                OnArrived?.Invoke();
            }
        }
        //Keep Moving
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, currentTargetPath, Time.deltaTime * movementSpeed);
        }
    }
    public void CancelWalk()
    {
        Stop();
    }
    private void Stop()
    {
        currentAction = null;
        isMoving = false;
        reachDestination = true;

        if(rootVisual != null)
            rootVisual.localScale = Vector3.one;
    }
}
