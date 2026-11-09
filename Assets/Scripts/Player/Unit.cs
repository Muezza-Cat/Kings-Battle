using System;
using System.Collections;
using UnityEngine;



public class Unit : MonoBehaviour
{
    #region Events
    public event Action OnPositionChanged;
    public event Action OnSpeedupConsumed;
    public event Action OnSpeedupFinished;
    public event Action OnPositionUnchanged;
    public event Action OnFightEvent;
    public event Action OnUnitDead;
    #endregion


    [Header("Reference")]
    [HideInInspector] public HordeFormation hordeFormation;
    [HideInInspector] public BaseController baseController;
    [SerializeField] private SkinnedMeshRenderer[] skinnedMeshRendererList;
    private CapsuleCollider capsuleCollider;


    [Header("Settings")]
    public Vector3 targetPosition; //World Pos;
    private float currentHealth;
    public float CurrentHealth
    {
        get
        {
            return currentHealth;
        }
        set
        {
            if (value <= 0f)
            {
                this.gameObject.layer = 0;
                currentHealth = 0f;
                StartCoroutine(TimedEliminationRoutine());
                return;
            }

            currentHealth = value;
        }
    }

    private float attackRange = 1f;

    //HitPoints
    private float damageAmount = 1f;

    //Linear Velocity
    private float crestMoveSpeed = 6f;
    private float troughMoveSpeed = 4f;
    private float currentMoveSpeed;
    private float movementThreshold = 0.05f;

    /*//Angular Velocity
    private float crestAngularVelocity = 25f;
    private float troughAngularVelocity = 15f;
    private float currentAngularVelocity;*/

    private float sqrDistanceFromTargetPosition = 0f;

    //Capsule cast
    private float radius = 0.5f;
    private float castDistance = 0.5f;

    [Header("Flag")]
    public bool isDetected = false;
    private bool canMove;



    private void Awake()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void OnEnable()
    {
        currentMoveSpeed = UnityEngine.Random.Range(troughMoveSpeed, crestMoveSpeed);

        isFighting = false;
        //currentAngularVelocity = UnityEngine.Random.Range(troughAngularVelocity, crestAngularVelocity);
    }



    public void Initialize(HordeFormation hordeFormation)
    {
        this.hordeFormation = hordeFormation;
        baseController = hordeFormation.GetBaseController();

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRendererList)
        {
            if (hordeFormation == GameplayManager.Instance.playerHorde)
                skinnedMeshRenderer.material = UnitSelectionWindowUI.Instance.GetUnitTemplateList()[UnitSelectionWindowUI.Instance.GetSelectedUnitTemplateIndex()].GetUnitTemplateSO().unitMaterial;
            else 
                skinnedMeshRenderer.material = hordeFormation.unitMaterial;
        }

        //Update Health and Damage of Units;
        if (hordeFormation.GetBaseController() is PlayerController)
        {
            CurrentHealth = UnitSelectionWindowUI.Instance.GetUnitTemplateList()[UnitSelectionWindowUI.Instance.GetSelectedUnitTemplateIndex()].GetTemplateDataSO().hp;
            damageAmount = UnitSelectionWindowUI.Instance.GetUnitTemplateList()[UnitSelectionWindowUI.Instance.GetSelectedUnitTemplateIndex()].GetTemplateDataSO().damage;
        }
        else
        {
            CurrentHealth = EnemyManager.Instance.GetEnemyTemplateDataSO().hp;
            damageAmount = EnemyManager.Instance.GetEnemyTemplateDataSO().damage;
        }
    }



    private void Update()
    {
        //IsMoving();
        //FightOnlyAtSight();
        FightAtSightChatGPTRemake();

        //IndependentMovementHandler();
        IndependentMovementHandlerRemake();
        IndependentRotationHandler();
        UnitSpeedCalculator();
        //UpdateLinearAndAngularVelocity();
    }


    //Old code, works for gameobjects that are perfectly aligned to the fixed axes.
    /*private void IndependentMovementHandler()
    {
        sqrDistanceFromTargetPosition = (targetPosition - transform.position).sqrMagnitude;

        PlayAnimation(sqrDistanceFromTargetPosition);

        if (sqrDistanceFromTargetPosition > movementThreshold * movementThreshold)
        {
            Vector3 moveDirection = (targetPosition - transform.position).normalized;
            Vector3 p1 = transform.position + Vector3.up * +0.5f;
            Vector3 p2 = transform.position + Vector3.up * -0.5f;
            canMove = !Physics.CapsuleCast(p1, p2, radius, moveDirection, castDistance, baseController.GetInteractableLayers(), QueryTriggerInteraction.Ignore);

            if (canMove)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * currentMoveSpeed);
                return;
            }
            else if (!canMove)
            {
                Vector3 moveDirectionX = new Vector3(moveDirection.x, 0f, 0f).normalized;
                canMove = !Physics.CapsuleCast(p1, p2, radius, moveDirectionX, castDistance, baseController.GetInteractableLayers(), QueryTriggerInteraction.Ignore);

                if (canMove)
                {
                    transform.position += moveDirectionX * Time.deltaTime * currentMoveSpeed;
                }
                else
                {
                    Vector3 moveDirectionZ = new Vector3(0f, 0f, moveDirection.z).normalized;
                    canMove = !Physics.CapsuleCast(p1, p2, radius, moveDirectionZ, castDistance, baseController.GetInteractableLayers(), QueryTriggerInteraction.Ignore);

                    if (canMove)
                    {
                        transform.position += moveDirectionZ * Time.deltaTime * currentMoveSpeed;
                    }
                }
            }
        }
    }*/


    private float speedOffsetWithSpeedup = 4f;

    private float speedCalculationCooldownTime = 0.4f;
    private float speedCalculationElapsedTime = 0f;
    private void UnitSpeedCalculator()
    {
        speedCalculationElapsedTime += Time.deltaTime;
        if (speedCalculationElapsedTime < speedCalculationCooldownTime) return;
        speedCalculationElapsedTime = 0f;


        if (hordeFormation.IsOnSpeedUp())
        {
            currentMoveSpeed = UnityEngine.Random.Range(troughMoveSpeed + speedOffsetWithSpeedup, crestMoveSpeed + speedOffsetWithSpeedup);
            OnSpeedupConsumed?.Invoke();
        }
        else
        {
            currentMoveSpeed = UnityEngine.Random.Range(troughMoveSpeed, crestMoveSpeed);
            OnSpeedupFinished?.Invoke();
        }
    }


    //Solution #1
    // => find the way with vectors instead of rigid axes.
    public bool isFighting = false;
    //private float teleportationStartTime = 2f;
    //private float elapsedTimeForTeleportation = 0f;
    private void IndependentMovementHandlerRemake()
    {
        if (isFighting) return;
        
        sqrDistanceFromTargetPosition = (targetPosition - transform.position).sqrMagnitude;

        PlayAnimation(sqrDistanceFromTargetPosition);
        if (sqrDistanceFromTargetPosition > movementThreshold * movementThreshold)
        {
            Vector3 moveDirection = (targetPosition - transform.position).normalized;
            Vector3 p1 = transform.position + Vector3.up * +0.5f;
            Vector3 p2 = transform.position + Vector3.up * -0.5f;

            canMove = !Physics.CapsuleCast(p1, p2, radius, moveDirection, out RaycastHit hit, castDistance, baseController.GetInteractableLayers(), QueryTriggerInteraction.Ignore);

            if (canMove)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * currentMoveSpeed);
                capsuleCollider.enabled = true;
            }
            //Only if the way is not clear;
            else
            {
                Vector3 rightToNormal = Vector3.Cross(hit.normal, Vector3.up).normalized;
                Vector3 leftToNormal = -rightToNormal;

                if (Vector3.Dot(rightToNormal, moveDirection) >= 0f
                && !Physics.CapsuleCast(p1, p2, radius - 0.2f, rightToNormal, castDistance, baseController.GetInteractableLayers(), QueryTriggerInteraction.Ignore))
                {
                    transform.position = Vector3.MoveTowards(transform.position, transform.position + rightToNormal, Time.deltaTime * currentMoveSpeed);
                }
                else if (Vector3.Dot(leftToNormal, moveDirection) >= 0f
                && !Physics.CapsuleCast(p1, p2, radius - 0.2f, leftToNormal, castDistance, baseController.GetInteractableLayers(), QueryTriggerInteraction.Ignore))
                {
                    transform.position = Vector3.MoveTowards(transform.position, transform.position + leftToNormal, Time.deltaTime * currentMoveSpeed);
                }
                /*else
                {
                    elapsedTimeForTeleportation += Time.deltaTime;
                    if (elapsedTimeForTeleportation < teleportationStartTime) return;
                    elapsedTimeForTeleportation = 0f;

                    capsuleCollider.enabled = false;
                }*/
            }
        }
    }


    private float maxSelfRotationDistance = 3f;
    private void IndependentRotationHandler()
    {
        if (sqrDistanceFromTargetPosition > maxSelfRotationDistance * maxSelfRotationDistance)
        {
            transform.rotation = Quaternion.LookRotation((targetPosition - transform.position).normalized);
        }
        else
        {
            if (baseController != null && baseController.GetRotation() != Vector3.zero) 
            {
                transform.eulerAngles = new Vector3(0f, baseController.GetRotation().y, 0f);
            } 
        }
    }


    /*
    private float velocityUpdateCooldown = 0.5f;
    private float velocityUpdateElapsedTime = 0f;
    private void UpdateLinearAndAngularVelocity()
    {
        velocityUpdateElapsedTime += Time.deltaTime;
        if (velocityUpdateElapsedTime < velocityUpdateCooldown) return;

        currentMoveSpeed = UnityEngine.Random.Range(troughMoveSpeed, crestMoveSpeed);
        currentAngularVelocity = UnityEngine.Random.Range(troughAngularVelocity, crestAngularVelocity);
    }*/



    private float hitCooldownTime = 0.5f;
    private float hitElapsedTime = 0.5f;



    private readonly Collider[] colliders = new Collider[4]; //buffer length 4;
    private float enemyDetectionCooldownTime = 0.1f;
    private float enemyDetectionElapsedTime = 0f;
    private void FightAtSightChatGPTRemake()
    {
        enemyDetectionElapsedTime += Time.deltaTime;
        hitElapsedTime += Time.deltaTime;
        if (enemyDetectionElapsedTime < enemyDetectionCooldownTime) return;
        enemyDetectionElapsedTime = 0f;


        int nearbyEnemyCount = Physics.OverlapSphereNonAlloc(transform.position, attackRange, colliders, hordeFormation.enemyLayer, QueryTriggerInteraction.Ignore);

        if (nearbyEnemyCount == 0)
        {
            isFighting = false;
            return;
        }

        Unit enemyUnit = null;
        for (int i = 0; i < nearbyEnemyCount; i++)
        {
            if (colliders[i].TryGetComponent<Unit>(out enemyUnit))
            {
                break;
            }
        }

        if (enemyUnit == null)
        {
            isFighting = false;
            return;
        }
        isFighting = true;

        OnFightEvent?.Invoke(); //Animation player event;
        //hitElapsedTime += Time.deltaTime;

        Vector3 lookDirection = (enemyUnit.transform.position - transform.position).normalized;
        if (lookDirection != Vector3.zero) transform.forward = lookDirection;

        if (hitElapsedTime < hitCooldownTime) return;
        hitElapsedTime = 0f;

        enemyUnit.TakeDamage(damageAmount);
        if (hordeFormation == GameplayManager.Instance.playerHorde) AudioManager.Instance.PlaySwordClashSound();
    }

    /*private void FightAtSight()
    {

        Collider[] collider = Physics.OverlapSphere(transform.position, attackRange, hordeFormation.enemyLayer, QueryTriggerInteraction.Ignore);
        if (collider == null || collider.Length == 0)
        {
            isFighting = false;
            return;
        }

        OnFightEvent?.Invoke(); //Animation Playing;
        hitElapsedTime += Time.deltaTime;

        //Rotation and Animation;
        Unit unit = collider[0].GetComponent<Unit>();
        Vector3 lookDirection = (unit.transform.position - transform.position).normalized;
        if(lookDirection != Vector3.zero) transform.forward = lookDirection;


        if (hitElapsedTime < hitCooldownTime) return;
        hitElapsedTime = 0f;

        unit.TakeDamage(damageAmount);
        if(hordeFormation == GameplayManager.Instance.playerHorde) AudioManager.Instance.PlaySwordClashSound();
    }*/

    /*private void FightOnlyAtSight()
    {
        Collider[] collider = Physics.OverlapSphere(transform.position, attackRange, hordeFormation.enemyLayer, QueryTriggerInteraction.Ignore);

        if (collider == null || collider.Length == 0) return;

        //First check the first unit is either already detected or not;
        //If not;
        Unit unit = collider[0].GetComponent<Unit>();
        if (unit != null && !unit.isDetected)
        {
            unit.isDetected = true;
            unit.TakeDamage(damageAmount);

            //Animation and Rotation
            transform.rotation = Quaternion.LookRotation((unit.transform.position - transform.position).normalized);
            OnFightEvent?.Invoke();

            if (!isDetected)
            {
                TakeDamage(unit.damageAmount);
                isDetected = false;
            }
            else
            {
                isDetected = false;
            }
        }
        //If yes;
        else
        {
            foreach (Collider enemyCollider in collider)
            {
                Unit otherUnit = enemyCollider.GetComponent<Unit>();
                if (otherUnit == null) continue;
                if (otherUnit.isDetected) continue;
                else
                {
                    otherUnit.isDetected = true;
                    otherUnit.TakeDamage(damageAmount);

                    //Animation and Rotation
                    transform.rotation = Quaternion.LookRotation((otherUnit.transform.position - transform.position).normalized);
                    OnFightEvent?.Invoke();

                    if (!isDetected)
                    {
                        TakeDamage(unit.damageAmount);
                        isDetected = false;
                        return;
                    }
                    else
                    {
                        isDetected = false;
                    }
                }
            }
        }
    }*/


    public void TakeDamage(float dmgAmt)
    {
        CurrentHealth -= dmgAmt;
    }


    private IEnumerator TimedEliminationRoutine()
    {
        yield return new WaitForSeconds(1f);
        OnUnitDead?.Invoke();
        yield return new WaitForSeconds(0.5f);
        hordeFormation.RemoveUnit(this.transform);
    }

    //Movement Animation;
    private void PlayAnimation(float sqrDistanceFromTargetPosition)
    {
        if (baseController.IsMoving())
        {
            if (sqrDistanceFromTargetPosition > movementThreshold * movementThreshold) OnPositionChanged?.Invoke();
        }
        else
        {
            if (sqrDistanceFromTargetPosition < 2f * 2f) OnPositionUnchanged?.Invoke();
            else OnPositionChanged?.Invoke();
        }
    }
}
