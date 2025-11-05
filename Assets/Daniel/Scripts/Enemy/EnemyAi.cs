using UnityEngine;
using UnityEngine.AI;

public class EnemyAi: MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform player;

    public LayerMask whatIsGround, whatIsPlayer;





    //Patroling
    //public Vector3[] walkPoints;
    //private Vector3 currentWalkPoint;
    //public Vector3 nextWalkPoint;
    //bool walkPointsSet = false;
    //private int currentPoint = 0;
    public Vector3 walkPoint;
    bool walkPointSet = false;
    public float walkPointRange;
    public float MoveCd;
    private float MoveCdTimer;
    Vector3 Home;
    public float HomeRadius;

    //Searching

    [SerializeField] float fullAwareness;
    [SerializeField] private float halfAwareness;
    [SerializeField] private float awareness;
    public bool searchingStarted;
    

    //Animation
    public Animator animator;
    public bool Chasing;
    public bool Attacking;

    //States
    [SerializeField] private bool Chase;
    [SerializeField] private bool Search;
    [SerializeField] private bool Patrol = true;

    //Audio
    [SerializeField] private AudioClip[] IdleAudios;
    [SerializeField] private AudioClip ChasingAudio;
    [SerializeField] private float SoundDelay = 10;
    private float SoundDelayTimer;
    private bool PlayedChaseSound;

    //FOV
    public FieldOfView fov;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        fov = GetComponent<FieldOfView>();
        Home = transform.position;
        halfAwareness = fullAwareness / 2;
        //walkPoints = new Vector3[Random.Range(3, 6)];
    }

    private void Update()
    {
        //playerInSightRange = Vector3.Distance(transform.position, player.transform.position) <= sightRange;
        fov.FindVisibleTargets();
        //if (!playerInSightRange)
        //{
        //    Chasing = false;
        //    Patrolling();
        //}

        if (MoveCdTimer > 0)
        {
            MoveCdTimer -= Time.deltaTime;
        }

        if (SoundDelayTimer > 0)
        {
            SoundDelayTimer -= Time.deltaTime;
        }

        animator.SetFloat("Speed", agent.desiredVelocity.sqrMagnitude);
        animator.SetBool("Chasing", Chasing);
        animator.SetBool("Attacking", Attacking);

        //RaycastHit hit;
        //if (Physics.Raycast(transform.position, (player.transform.position - transform.position), out hit, Mathf.Infinity))
        //{
        //    if(hit.transform == player.transform) 
        //    { 
        //    PlayerHidden = false;
        //    }
        //    else
        //    {
        //        PlayerHidden = true;
        //    }
        //}

        //side1 = player.transform.position - transform.position;
        //side2 = transform.forward;
        //float angle = Vector3.SignedAngle(side1, side2, Vector3.up);
        if (fov.visible && !Chase)
        {
            Patrol = false;
           Search = true;
        }

        if (Chase)
        {
            Chasing = true;
            ChasePlayer();
        }
        if (Search)
        {
            Chasing = false;
            Searching();
        }
        if (Patrol)
        {
            Chasing = false;
            Patrolling();
        }
        //else
        //{
        //    Chasing = false;
        //    Patrolling();
        //}

        
    }

    private void Searching()
    {

        //Quaternion.LookRotation(player.transform.position);
        if (!searchingStarted)
        {
            awareness = halfAwareness;
            searchingStarted = true;
        }
       
        if (fov.visible)
        {
            awareness -= Time.deltaTime;
        }
        else
        {
            awareness += Time.deltaTime;
        }

        if (awareness <= 0)
        {
            Search = false;
            searchingStarted = false;
            Chase = true;
        }

        if (awareness >= fullAwareness)
        {
            Search = false;
            searchingStarted = false;
            Patrol = true;
        }

    }

    private void Patrolling()
    {
        if (Vector3.Distance(transform.position, Home) > HomeRadius)
        {
            agent.SetDestination(Home);
        }

        if (SoundDelayTimer <= 0)
        {
            SoundDelayTimer = SoundDelay;
            SoundFXManager.Instance.PlayRandomSoundFXClip(IdleAudios, transform);
        }

        if (PlayedChaseSound)
        {
            PlayedChaseSound = false;
        }

        if (!walkPointSet) SearchWalkPoint();


        //if (MoveCdTimer > 0)
        //{
        //    return;
        //}
        //else
        //{
        //    MoveCdTimer = MoveCd;
        //    agent.SetDestination(walkPoints[currentPoint]);
        //   if (currentPoint == walkPoints.Length - 1)
        //   {
        //       currentPoint = 0;
        //   }
        //   else
        //   {
        //       currentPoint++;
        //   }

        //}

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

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround) && Vector3.Distance(walkPoint, Home) <= HomeRadius && Vector3.Distance(transform.position, walkPoint) >= walkPointRange / 2)
            walkPointSet = true;

    }


    //private Vector3[] SearchWalkPoint(Vector3[] WalkPoints)
    //{
    //    bool walkPointValid;
    //    for (int i = 0; i < WalkPoints.Length; i++)
    //    {
    //        walkPointValid = false;
    //        do
    //        {
    //            if (i == 0)
    //            {
    //                WalkPoints[i] = transform.position;
    //                walkPointValid = true;
    //            }
    //            else 
    //            { 

    //                float randomZ = Random.Range(-walkPointRange, walkPointRange);
    //                float randomX = Random.Range(-walkPointRange, walkPointRange);

    //                Vector3 walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);             

    //                if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround) && Vector3.Distance(transform.position, walkPoint) >= walkPointRange - 2)
    //                {
    //                    WalkPoints[i] = walkPoint;
    //                    walkPointValid = true;
    //                }
    //                //Vector3.Distance(walkPoint, Home) <= HomeRadius && && Vector3.Distance(WalkPoints[i - 1], walkPoint) <= walkPointRange 

    //            }
    //        }
    //        while (!walkPointValid);
    //    }
    //    return WalkPoints;
    //}

    private void ChasePlayer()
    {
        if (!PlayedChaseSound)
        {
            SoundFXManager.Instance.PlaySoundFXClip(ChasingAudio, transform);
            PlayedChaseSound = true;
        }

        agent.SetDestination(player.position);

        if (!fov.visible)
        {
            Chase = false;
            Search = true;
        }
    }
    
}
