using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class StalkerEnemy : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;

    [Header("Movement")]
    public float speed = 3.5f;

    [Header("Timers")]
    public float updateTargetTime = 2f;
    public float teleportTime = 30f;

    [Header("Teleport")]
    public float minDistanceFromPlayer = 10f;

    private float updateTimer = 0f;
    private float teleportTimer = 0f;

    private List<Vector3> teleportPoints = new List<Vector3>();

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        if (agent != null)
            agent.speed = speed;

        updateTimer = updateTargetTime;
        teleportTimer = 0f;

        UpdateTarget();
    }

    void Update()
    {
        if (player == null || agent == null)
            return;

        updateTimer += Time.deltaTime;
        teleportTimer += Time.deltaTime;

        if (updateTimer >= updateTargetTime)
        {
            updateTimer = 0f;
            UpdateTarget();
        }

        if (teleportTimer >= teleportTime)
        {
            teleportTimer = 0f;
            TeleportToRandomPoint();
            UpdateTarget();
        }

        RotateTowardVelocity();
    }

    void UpdateTarget()
    {
        if (player == null || agent == null)
            return;

        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    void TeleportToRandomPoint()
    {
        if (teleportPoints == null || teleportPoints.Count == 0 || player == null)
            return;

        List<Vector3> validPoints = new List<Vector3>();

        for (int i = 0; i < teleportPoints.Count; i++)
        {
            float dist = Vector3.Distance(teleportPoints[i], player.position);

            if (dist >= minDistanceFromPlayer)
                validPoints.Add(teleportPoints[i]);
        }

        if (validPoints.Count == 0)
            return;

        Vector3 newPos = validPoints[Random.Range(0, validPoints.Count)];

        agent.isStopped = true;
        agent.Warp(newPos);
        agent.isStopped = false;
    }

    void RotateTowardVelocity()
    {
        Vector3 dir = agent.velocity;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
            Time.deltaTime * 5f);
    }

    public void SetTeleportPoints(List<Vector3> points)
    {
        teleportPoints = points;
    }
}