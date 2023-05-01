using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Controls;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ChefBehaviour : NetworkBehaviour
{
    private bool screenPressed = false;
    private bool initialScreenPositionRecorded = false;
    private bool debugOverride = false;
    //[SerializeField]
    //private InputAction MoveVerticalAction;
    private Rigidbody playerBody;
    private PlayerInput playerInput;
    [SerializeField]
    private float movementSpeed;

    private TouchControl touchControls = new();

    private NetworkVariable<Vector2> desiredMovementDirection = new();

    Vector2 touchScreenInitialPointTouch = new();
    Vector2 touchScreenCurrentPointTouch = new();

    private void Start()
    {
        debugOverride = SceneManager.GetActiveScene().name == "BetweenScene";
        playerBody = gameObject.GetComponent<Rigidbody>();
        playerInput = gameObject.GetComponent<PlayerInput>();
        EnhancedTouchSupport.Enable();
        if (!IsLocalPlayer && !debugOverride)
        {
            playerInput.enabled = false;
        }
    }

    [ServerRpc(RequireOwnership=false)]
    void SubmitPositionRequestServerRpc(Vector2 movementDirection, ServerRpcParams serverRpcParams = default)
    {
        desiredMovementDirection.Value = movementDirection;
    }

    public void OnTouchPressed(InputAction.CallbackContext callbackContext)
    {
        screenPressed = callbackContext.ReadValueAsButton();
        if (!screenPressed)
        {
            touchScreenInitialPointTouch = Vector2.zero;
            touchScreenCurrentPointTouch = Vector2.zero;
            initialScreenPositionRecorded = false;

            if (IsServer || debugOverride)
            {
                desiredMovementDirection.Value = Vector2.zero;
            }
            else
            {
                SubmitPositionRequestServerRpc(Vector2.zero);
            }
        }
    }

    public void OnPointerMove(InputAction.CallbackContext callbackContext)
    {
        if (screenPressed)
        {
            if(!initialScreenPositionRecorded)
            {
                touchScreenInitialPointTouch = callbackContext.ReadValue<Vector2>();
                initialScreenPositionRecorded = true;
            }

            touchScreenCurrentPointTouch = callbackContext.ReadValue<Vector2>();
            Vector2 touchDirection = (touchScreenCurrentPointTouch - touchScreenInitialPointTouch).normalized;

            if (IsServer || debugOverride)
            {
                desiredMovementDirection.Value = touchDirection;
            }
            else
            {
                SubmitPositionRequestServerRpc(touchDirection);
            }
        }
    }

    public void OnMove(InputAction.CallbackContext callbackContext)
    {
        InputActionPhase phs = callbackContext.phase;
        switch (phs)
        {
            case InputActionPhase.Started:
                Debug.Log("Started!");
                break;
            case InputActionPhase.Performed:
                if (IsServer || debugOverride)
                {
                    desiredMovementDirection.Value = callbackContext.ReadValue<Vector2>();
                }
                else
                {
                    SubmitPositionRequestServerRpc(callbackContext.ReadValue<Vector2>());
                }
                Debug.Log("Performed!");
                break;
            case InputActionPhase.Waiting:
                Debug.Log("Waiting!");
                break;
            case InputActionPhase.Disabled:
                Debug.Log("Disabled!");
                break;
            case InputActionPhase.Canceled:
                if (IsServer || debugOverride)
                {
                    desiredMovementDirection.Value = Vector2.zero;
                }
                else
                {
                    SubmitPositionRequestServerRpc(Vector2.zero);
                }
                Debug.Log("Cancelled!");
                break;
        }
    }

    public void OnThrow(InputAction.CallbackContext callbackContext)
    {
        //Debug.Log("test throw");
    }

    void Update()
    {
        //Debug.Log($"i:{touchScreenInitialPointTouch} c:{touchScreenCurrentPointTouch}");
        playerBody.position += new Vector3(desiredMovementDirection.Value.x, 0.0f, desiredMovementDirection.Value.y) * movementSpeed * Time.deltaTime;
        //may need this in the future Vector3.Cross(Camera.main.transform.right, Vector3.up)
    }
}
