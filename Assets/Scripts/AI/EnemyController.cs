using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;




public class EnemyController : BaseController
{
    [Header("HordeStats")]
    [HideInInspector] public float moraleCount { get; private set; }
    private DifficultyMode.DifficultyModes mode; //EnemyController difficulty mode;

    private enum EnemyType
    {
        NormalEnemy,
        SuicideDiver,
    }
    [SerializeField] private EnemyType enemyType;

    private enum Decision
    {
        Fight,
        Flee,
        Ignore,
    }
    private Decision decision;

    [Header("Reference")]
    private NavMeshAgent agent;
    private HordeFormation hordeFormation;
    private Animator animator;

    [Header("Collection")]
    [SerializeField] private List<Transform> waypoints;
    private List<HordeFormation> threatNearby;
    private NavMeshPath path;


    [Header("Settings")]
    private HordeFormation hordeToFight;

    private float updateMethodCooldownTime = 0.2f; //10 times per sec;
    private float updateMethodElapsedTime = 0f;
    private float safeDistance = 15f;
    private float moraleThreshold = 1.25f;
    private float normalMoveSpeed = 4f;

    [Header("LayerMask")]
    public LayerMask interactableLayer;

    [Header("Animator")]
    private string MOVE = "Move";
    private int moveHash;





    private void Awake()
    {
        poolKey = ObjectPooler.PoolKey.EnemyUnit;

        threatNearby = new List<HordeFormation>();
        path = new NavMeshPath();


        hordeFormation = GetComponent<HordeFormation>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();


        moveHash = Animator.StringToHash(MOVE);
    }

    public void Start()
    {
        decision = Decision.Ignore;
        path = new NavMeshPath();
    }


    public void Update()
    {
        updateMethodElapsedTime += Time.deltaTime;
        if (updateMethodElapsedTime < updateMethodCooldownTime) return;
        updateMethodElapsedTime = 0f;

        PlayAnimation();
        //SelectStrategy();
        StrategyStateMachine();

        //if (mode == 0) Debug.LogWarning("No Mode set for the EnemyController " + this.name);
        switch (mode)
        {
            default:
            case DifficultyMode.DifficultyModes.Easy:
                NormalAgentPatrol();
                break;
            case DifficultyMode.DifficultyModes.Medium:
                NormalAgentPatrol();
                AvoidStrongEnemies();
                break;
            case DifficultyMode.DifficultyModes.Hard:
                StateMachine(); //Handle different Decisions;
                ScanThreatsNearby(); //Continuously scan for threats nearby;

                if (threatNearby.Count > 0) BotDecisionMakingProcess();
                else decision = Decision.Ignore;
                break;
        }
    }

    private void PlayAnimation()
    {
        if (IsMoving()) animator.SetBool(moveHash, true);
        else animator.SetBool(moveHash, false);
    }


    //Strategy; Not currently in use, Plan Abort;
    private enum Strategy
    {
        AddUnit,
        UpgradeStats,
    }
    private Strategy strategy = Strategy.AddUnit;

    /*private void SelectStrategy()
    {
        if (enemyControllerHordeFormation.GetUnitCount() == enemyControllerHordeFormation.GetPositions().Count)
        {
            strategy = Strategy.UpgradeStats;
        }
        else
        {
            strategy = Strategy.AddUnit;
        }
    }*/

    private void StrategyStateMachine()
    {
        switch (strategy)
        {
            default:
            case Strategy.AddUnit:
                AddUnitStrategy();
                break;
            case Strategy.UpgradeStats:
                UpgradeStatsStrategy();
                break;
        }
    }

    private void AddUnitStrategy()
    {
        hordeFormation.AddUnit();
    }

    private void UpgradeStatsStrategy()
    {
        //Upgrade Stats;
    }


    //Easy Mode;
    private void NormalAgentPatrol()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.SetDestination(GetRandomWaypointTransform().position);
        }
    } //Needs changing;


    private Transform GetRandomWaypointTransform()
    {
        Transform randomTransform = waypoints[UnityEngine.Random.Range(0, waypoints.Count)];
        return randomTransform;
    }


    //Medium Mode;

    private void AvoidStrongEnemies()
    {
        foreach (HordeFormation enemyHordeFormation in GameplayManager.Instance.GetHordeFormation())
        {
            if (enemyHordeFormation == hordeFormation) continue; //Except this controller;
            if (Vector3.Distance(transform.position, enemyHordeFormation.transform.position) > safeDistance) continue; //Enemy very close;

            if (enemyHordeFormation.hordeMorale > hordeFormation.hordeMorale * moraleThreshold) //If enemy is 1.25x or more stronger compared to us.
            {
                float minumumDotValue = 1f;
                Transform waypointAwayFromEnemy = null;
                if (Vector3.Dot((enemyHordeFormation.transform.position - transform.position), enemyHordeFormation.transform.forward) >= 0f) return; //Is not moving towards us;

                foreach (Transform waypoint in waypoints)
                {
                    float dot = Vector3.Dot((waypoint.position - transform.position), (enemyHordeFormation.transform.position - transform.position));
                    if (dot < minumumDotValue)
                    {
                        minumumDotValue = dot;
                        waypointAwayFromEnemy = waypoint;
                    }
                    if (dot < 0f && Vector3.Distance(transform.position, waypoint.position) > 5f)
                    {
                        agent.SetDestination(waypoint.position);
                        break;
                    }
                }
                if (minumumDotValue < 0.5f && Vector3.Distance(waypointAwayFromEnemy.position, transform.position) > 5f) agent.SetDestination(waypointAwayFromEnemy.position);
            }
        }
    }



    //Hard Mode;
    private void StateMachine()
    {
        switch (decision)
        {
            case Decision.Fight:
                FightThreat(hordeToFight);
                break;
            case Decision.Flee:
                AvoidStrongEnemies();
                break;
            case Decision.Ignore:
                if (enemyType == EnemyType.SuicideDiver && hordeFormation.GetUnitCount() >= 15) agent.SetDestination(GameplayManager.Instance.playerHorde.transform.position);
                else NormalAgentPatrol();

                break;
        }
    }


    private void BotDecisionMakingProcess()
    {
        foreach (HordeFormation hordeFomration in threatNearby)
        {
            float dot = Vector3.Dot((hordeFomration.transform.position - transform.position), hordeFomration.transform.forward);

            if (dot < -0.5f) //Coming towards us;
            {
                if (IsEnemyVeryStrong())
                {
                    decision = Decision.Flee;
                }
                else
                {
                    decision = Decision.Fight;
                    hordeToFight = hordeFomration;
                }
            }
            else //Some other direction
            {
                decision = Decision.Ignore;
            }
        }
    }

    private void FightThreat(HordeFormation hordeToFight)
    {
        if (hordeToFight == null || hordeToFight.GetUnitCount() == 0)
        {
            decision = Decision.Ignore;
            agent.ResetPath();
            return;
        }
        agent.SetDestination(hordeToFight.transform.position);
    }



    private void ScanThreatsNearby() //needs optimization;
    {
        threatNearby.Clear();
        foreach (HordeFormation enemyHordeFormation in GameplayManager.Instance.GetHordeFormation())
        {
            if (enemyHordeFormation == hordeFormation) continue;
            if (IsInThreatRadii(enemyHordeFormation))
            {
                threatNearby.Add(enemyHordeFormation);
            }
        }
    }

    private bool IsInThreatRadii(HordeFormation targetHorde)
    {
        bool isInThreatRadius = false;
        if (agent.CalculatePath(targetHorde.transform.position, path))
        {
            float completeDistance = 0f;
            for (int i = 1; i < path.corners.Length; i++)
            {
                completeDistance += (path.corners[i] - path.corners[i - 1]).sqrMagnitude;
            }
            if (completeDistance < safeDistance * safeDistance)
            {
                isInThreatRadius = true;
            }
            else
            {
                isInThreatRadius = false;
            }
        }
        return isInThreatRadius;
    }

    private bool IsEnemyVeryStrong()
    {
        foreach (HordeFormation enemyHordeFormation in threatNearby)
        {
            if (enemyHordeFormation.hordeMorale > hordeFormation.hordeMorale * moraleThreshold) //Enemy moraleThreshold..x stronger than us;
            {
                return true;
            }
        }
        return false;
    }


    public void SetDifficultyMode(DifficultyMode.DifficultyModes mode)
    {
        this.mode = mode;
    }


    public override Vector3 GetRotation()
    {
        //return agent.transform.eulerAngles;

        Quaternion rotation = Quaternion.LookRotation((agent.destination - agent.transform.position).normalized);
        return rotation.eulerAngles;
    }

    public override float GetNormalMoveSpeed()
    {
        return normalMoveSpeed;
    }
    
    
    public override void SetCurrentMoveSpeed(float newMoveSpeed)
    {
        agent.speed = newMoveSpeed;
    }

    public override LayerMask GetInteractableLayers()
    {
        return interactableLayer;
    }

    public override void RemoveDeadEnemyLayer(int layer)
    {
        interactableLayer &= ~(1 << layer);
    }

    public override bool IsMoving()
    {
        return agent.velocity.sqrMagnitude > 0.01f * 0.01f;
    }

    public override void UpdateMoveSpeed(float updatedMoveSpeed)
    {
        agent.speed = updatedMoveSpeed;
    }

    public override void AddHomeAsWaypoint(Transform homeTransform)
    {
        waypoints.Add(homeTransform);
    }
}