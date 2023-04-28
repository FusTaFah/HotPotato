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
    private Vector2 movementDirection = Vector2.zero;
    [SerializeField]
    private float movementSpeed;

    private void Start()
    {
        playerBody = gameObject.GetComponent<Rigidbody>();
        playerInput = gameObject.GetComponent<PlayerInput>();
        playerInput.enabled = true;
    }

    [ServerRpc]
    void SubmitPositionRequestServerRpc(ServerRpcParams serverRpcParams = default)
    {
        
    }

    public void OnMove(InputAction.CallbackContext callbackContext)
    {
        if (IsOwner)
        {
            InputActionPhase phs = callbackContext.phase;

            switch (phs)
            {
                case InputActionPhase.Started:
                    break;
                case InputActionPhase.Performed:
                    movementDirection = callbackContext.ReadValue<Vector2>();
                    break;
                case InputActionPhase.Canceled:
                    movementDirection = Vector2.zero;
                    break;
            }
        }
    }

    public void OnThrow(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("test throw");
    }

    void Update()
    {
        playerBody.position += new Vector3(movementDirection.x, 0.0f, movementDirection.y) * movementSpeed * Time.deltaTime;
    }
}
