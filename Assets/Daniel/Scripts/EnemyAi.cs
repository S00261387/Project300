using UnityEngine;
using UnityEngine.AI;

public class EnemyAi : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform player;

    public LayerMask whatIsGround, whatIsPlayer;



  

    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet = false;
    public float walkPointRange;
    public float MoveCd;
    private float MoveCdTimer;
    Vector3 Home;
    public float HomeRadius;

    //States
    public float sightRange;
    public bool playerInSightRange;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        Home = transform.position;
    }

    private void Update()
    {
        playerInSightRange = Vector3.Distance(transform.position, player.transform.position) <= sightRange;

        if (!playerInSightRange) Patrolling();
        if (playerInSightRange) ChasePlayer();

        if (MoveCdTimer > 0)
        {
            MoveCdTimer -= Time.deltaTime;
        }
    }

    private void Patrolling()
    {
        if (Vector3.Distance(transform.position, Home) > HomeRadius)
        {
            agent.SetDestination(Home);
        }


        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
        {
            if (MoveCdTimer > 0)
            {
                return;
            }
            else
            {
                MoveCdTimer = MoveCd;

                agent.SetDestination(walkPoint);

                Vector3 distanceToWalkPoint = transform.position - walkPoint;

                if (distanceToWalkPoint.magnitude < 1f)
                    walkPointSet = false;
            }
        }
    }


        private void SearchWalkPoint()
        {
            float randomZ = Random.Range(-walkPointRange, walkPointRange);
            float randomX = Random.Range(-walkPointRange, walkPointRange);

            walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

            if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround) && Vector3.Distance(walkPoint, Home) <= HomeRadius)
                walkPointSet = true;

        }

        private void ChasePlayer()
        {
            agent.SetDestination(player.position);
        }
    
}
