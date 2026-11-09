using UnityEngine;


public class PlayerController : BaseController
{
    [Header("Reference")]
    private CharacterController controller;
    [HideInInspector] public HordeFormation hordeFormation;
    private Animator animator;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    private float normalMoveSpeed;

    [Header("Settings")]
    private Vector3 moveDirection;
    private float radius = 0.5f;
    private float castDistance = 0.1f;

    [Header("LayerMask")]
    public LayerMask interactableLayer;

    [Header("Flag")]
    private bool canMove;

    [Header("Animator")]
    private string MOVE = "Move";
    private int moveHash;

    private void Awake()
    {
        poolKey = ObjectPooler.PoolKey.PlayerUnit;

        controller = GetComponent<CharacterController>();
        hordeFormation = GetComponent<HordeFormation>();
        animator = GetComponentInChildren<Animator>();

        normalMoveSpeed = moveSpeed;

        moveHash = Animator.StringToHash(MOVE);
    }



    private void Update()
    {
        MovementHandler();
        RotationHandler();
    }



    private void MovementHandler()
    {
        if (InputManager.Instance.GetMoveDirection() == Vector2.zero)
        {
            animator.SetBool(moveHash, false);
            return;
        }
        animator.SetBool(moveHash, true);

        moveDirection = new Vector3(InputManager.Instance.GetMoveDirection().x, 0f, InputManager.Instance.GetMoveDirection().y).normalized;

        Vector3 point1 = transform.position + Vector3.up * +0.5f;
        Vector3 point2 = transform.position + Vector3.up * -0.5f;
        canMove = !Physics.CapsuleCast(point1, point2, radius, moveDirection, castDistance, interactableLayer, QueryTriggerInteraction.Ignore);

        if (canMove)
        {
            transform.position += moveDirection * Time.deltaTime * moveSpeed;
        }
        else if (!canMove)
        {
            Vector3 moveDirectionX = new Vector3(moveDirection.x, 0f, 0f);
            canMove = (moveDirection.x > 0.1f || moveDirection.x < -0.1f) && !Physics.CapsuleCast(point1, point2, radius, moveDirectionX, castDistance, interactableLayer, QueryTriggerInteraction.Ignore);

            if (canMove)
            {
                transform.position += moveDirectionX * Time.deltaTime * moveSpeed;
            }
            else
            {
                Vector3 moveDirectionZ = new Vector3(0f, 0f, moveDirection.z);
                canMove = (moveDirection.z > 0.1f || moveDirection.z < -0.1f) && !Physics.CapsuleCast(point1, point2, radius, moveDirectionZ, castDistance, interactableLayer, QueryTriggerInteraction.Ignore);

                if (canMove)
                {
                    transform.position += moveDirectionZ * Time.deltaTime * moveSpeed;
                }
                else
                {
                    //No Movement;
                }
            }
        }
    }

    private void RotationHandler()
    {
        if (InputManager.Instance.GetMoveDirection().sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(InputManager.Instance.GetMoveDirection().x, 0f, InputManager.Instance.GetMoveDirection().y));
        transform.rotation = targetRotation;
    }


    public override Vector3 GetRotation()
    {
        return new Vector3(0f, Mathf.Atan2(InputManager.Instance.GetMoveDirection().x, InputManager.Instance.GetMoveDirection().y) * Mathf.Rad2Deg, 0f);
    }

    public override float GetNormalMoveSpeed()
    {
        return normalMoveSpeed;
    }

    public override void SetCurrentMoveSpeed(float newMoveSpeed)
    {
        moveSpeed = newMoveSpeed;
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
        //return InputManager.Instance.GetMoveDirection().sqrMagnitude > 0.2f * 0.2f;
        return InputManager.Instance.GetMoveDirection().sqrMagnitude > 0.01f * 0.01f;
    }

    public override void UpdateMoveSpeed(float updatedMoveSpeed)
    {
        moveSpeed = updatedMoveSpeed;
    }
    public override void AddHomeAsWaypoint(Transform homeTransform) { }
}
