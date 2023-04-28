using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class ChefBehaviour : NetworkBehaviour
{
    //[SerializeField]
    //private InputAction MoveVerticalAction;
    private Rigidbody playerBody;
    private PlayerInput playerInput;
    [SerializeField]
    private float movementSpeed;

    private NetworkVariable<Vector2> desiredMovementDirection = new();

    private void Start()
    {
        playerBody = gameObject.GetComponent<Rigidbody>();
        playerInput = gameObject.GetComponent<PlayerInput>();
        playerInput.enabled = true;
    }

    [ServerRpc(RequireOwnership=false)]
    void SubmitPositionRequestServerRpc(Vector2 movementDirection, ServerRpcParams serverRpcParams = default)
    {
        desiredMovementDirection.Value = movementDirection;
    }

    public void OnMove(InputAction.CallbackContext callbackContext)
    {
        InputActionPhase phs = callbackContext.phase;

        switch (phs)
        {
            case InputActionPhase.Started:
                break;
            case InputActionPhase.Performed:
                if (IsServer)
                {
                    desiredMovementDirection.Value = callbackContext.ReadValue<Vector2>();
                }
                else
                {
                    SubmitPositionRequestServerRpc(callbackContext.ReadValue<Vector2>());
                }
                break;
            case InputActionPhase.Canceled:
                if (IsServer)
                {
                    desiredMovementDirection.Value = Vector2.zero;
                }
                else
                {
                    SubmitPositionRequestServerRpc(Vector2.zero);
                }
                break;
        }
    }

    public void OnThrow(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("test throw");
    }

    void Update()
    {
        playerBody.position += new Vector3(desiredMovementDirection.Value.x, 0.0f, desiredMovementDirection.Value.y) * movementSpeed * Time.deltaTime;
    }
}
