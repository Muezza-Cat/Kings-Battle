using System;
using UnityEngine;
using UnityEngine.InputSystem;



[DefaultExecutionOrder(-10)]
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }


    #region Event
    public event Action OnInteract;
    public event Action OnAddUnits;
    #endregion

    private TouchInputSystem touchInputSystem;




    private void Awake()
    {
        Instance = this;

        touchInputSystem = new TouchInputSystem();
        touchInputSystem.Enable();
    }


    private void OnEnable()
    {
        touchInputSystem.Keyboard.Interact.performed += Interact_performed;
        touchInputSystem.Keyboard.AddUnits.performed += AddUnits_performed;
    }

    private void AddUnits_performed(InputAction.CallbackContext context)
    {
        OnAddUnits?.Invoke();
    }

    private void Interact_performed(InputAction.CallbackContext context)
    {
        OnInteract?.Invoke();
    }


    public Vector2 GetMoveDirection()
    {
        return touchInputSystem.Keyboard.Move.ReadValue<Vector2>();
    }


    private void OnDisable()
    {
        touchInputSystem.Keyboard.Interact.performed -= Interact_performed;
        touchInputSystem.Keyboard.AddUnits.performed -= AddUnits_performed;
    }

    /*private TouchInputSystem touchInputSystem;

    //public delegate void OnTouchEnd(Vector2 position, float time);
    //public event OnTouchEnd OnTouchEndEvent;

    public float swipeValueNormalized { get; private set; }
    public bool isTouchValid = true;

    private Vector2 moveDirection;



    private void Awake()
    {
        Instance = this;

        touchInputSystem = new TouchInputSystem();
        touchInputSystem.Enable();
    }

    private void Start()
    {
        //touchInputSystem.Touch.Touch.started += Touch_Started;
        //touchInputSystem.Touch.Touch.canceled += Touch_Canceled;
    }

    


    //private void Touch_Started(InputAction.CallbackContext context)
    //{
    //    //Touchable area calc;
    //    if (TouchPosition().y > Screen.height * 0.7f)
    //    {
    //        isTouchValid = false;
    //    }
    //    else
    //    {
    //        isTouchValid = true;
    //        OnTouchStartEvent?.Invoke(TouchPosition(), (float)context.startTime);
    //    }
    //}

    //private void Touch_Canceled(InputAction.CallbackContext context)
    //{
    //    OnTouchEndEvent?.Invoke(TouchPosition(), (float)context.time);
    //}

    //public Vector2 TouchPosition()
    //{
    //    return touchInputSystem.Touch.TouchPosition.ReadValue<Vector2>();
    //}


    private void Update()
    {
        moveDirection = touchInputSystem.Touch.Move.ReadValue<Vector2>();
    }

    public Vector2 GetMoveDirection()
    {
        return moveDirection.normalized;
    }


    private void OnDestroy()
    {
        touchInputSystem.Touch.Disable();
        touchInputSystem.Dispose();
    }*/
}
