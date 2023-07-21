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
    //[SerializeField]
    //private InputAction MoveVerticalAction;
    private PlayerInput playerInput;
    [SerializeField]
    private float movementSpeed;

    private TouchControl touchControls = new();

    private NetworkVariable<Vector2> desiredMovementDirection = new();
    private Vector2 intendedMovementDirection = new();

    Vector2 touchScreenInitialPointTouch = new();
    Vector2 touchScreenCurrentPointTouch = new();

    [SerializeField]
    private GameObject fakeChefOriginal;
    private FakeChefBehaviour fakeChefInGame;
    private Vector3 realChefPreviousPosition = new();
    private float timeSinceLastTick = 0.0f;
    private float frameProgressThroughTick = 0.0f;
    private uint tickRate = 0;
    private float secondsPerTick = 0.0f;

    private void Start()
    {
        playerInput = gameObject.GetComponent<PlayerInput>();
        playerInput.enabled = true;
        EnhancedTouchSupport.Enable();

        if (!IsServer && IsLocalPlayer)
        {
            //enable the fake player
            fakeChefInGame = Instantiate(fakeChefOriginal, gameObject.transform.position, Quaternion.identity).GetComponent<FakeChefBehaviour>();
            gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
        if (!IsLocalPlayer)
        {
            //disable input system and create a minime
            //fakeChefInGame = Instantiate(fakeChefOriginal, gameObject.transform.position, Quaternion.identity).GetComponent<FakeChefBehaviour>();
            //fakeChefInGame.gameObject.tag = "PlayerLocalOther";
            playerInput.enabled = false;
            //gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
        realChefPreviousPosition = gameObject.transform.position;
        NetworkManager.NetworkTickSystem.Tick += OnTick;
        tickRate = NetworkManager.NetworkTickSystem.TickRate;
        secondsPerTick = 1.0f / tickRate;
    }

    void OnTick()
    {
        timeSinceLastTick = 0.0f;
        frameProgressThroughTick = 0.0f;
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

            intendedMovementDirection = Vector2.zero;

            if (IsServer)
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

            intendedMovementDirection = touchDirection;

            if (IsServer)
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
                intendedMovementDirection = callbackContext.ReadValue<Vector2>();

                if (IsServer)
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
                intendedMovementDirection = callbackContext.ReadValue<Vector2>();

                if (IsServer)
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
        gameObject.transform.position += new Vector3(desiredMovementDirection.Value.x, 0.0f, desiredMovementDirection.Value.y) * movementSpeed * Time.deltaTime;
        if(fakeChefInGame != null)
        {
            fakeChefInGame.gameObject.transform.position += new Vector3(intendedMovementDirection.x, 0.0f, intendedMovementDirection.y) * movementSpeed * Time.deltaTime;
        }
        //may need this in the future Vector3.Cross(Camera.main.transform.right, Vector3.up)
    }
}
