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
        Vector2 movementDirection = callbackContext.ReadValue<Vector2>();
        if (IsClient)
        {

        }
        else
        {
            playerBody.position += new Vector3(movementDirection.x, 0.0f, movementDirection.y);
        }
    }

    public void OnThrow(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("test throw");
    }

    void Update()
    {
        
    }
}
