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
            ////is this the one we control
            //if (!IsServer && IsLocalPlayer)
            //{
            //    fakeChefInGame.gameObject.transform.position += new Vector3(intendedMovementDirection.x, 0.0f, intendedMovementDirection.y) * movementSpeed * Time.deltaTime;
            //}
            ////is this the one we do not control
            //else if (!IsLocalPlayer)
            //{
            //    //the objective of the MusTerpolation is to fulfill a certain amount of distance between the reported poisition of the last tick and the
            //    //position that the real chef moved

            //    //assess the current real position status
            //    Vector3 currentRealPosition = gameObject.transform.position;
            //    Vector3 fromPreviousToCurrentPosition = currentRealPosition - realChefPreviousPosition;

            //    float distanceBetweenSquared = (fromPreviousToCurrentPosition).sqrMagnitude;
            //    if (distanceBetweenSquared > 0.000001f)
            //    {
            //        // sqrt(distanceBetweenSquared) was travelled by ChefBehaviour in Time.deltaTime seconds
            //        // therefore, the velocity of the fakeChefInGame must be sqrt(distanceBetweenSquared) / Time.deltaTime
            //        float timeSinceLastFrame = Time.deltaTime;
            //        //how much do we need to go?
            //        float distanceToProgress = timeSinceLastFrame / secondsPerTick;
            //        fakeChefInGame.gameObject.transform.position += fromPreviousToCurrentPosition * distanceToProgress;
            //        ////if we are still within the same real chef position, we need to work towards translating the fake chef along the "track" of movement that the real chef took
            //        //if (currentRealPosition == realChefPreviousPosition)
            //        //{
            //        //    // sqrt(distanceBetweenSquared) was travelled by ChefBehaviour in Time.deltaTime seconds
            //        //    // therefore, the velocity of the fakeChefInGame must be sqrt(distanceBetweenSquared) / Time.deltaTime
            //        //    float timeSinceLastFrame = Time.deltaTime;
            //        //    //how much do we need to go?
            //        //    float distanceToProgress = timeSinceLastFrame / secondsPerTick;
            //        //    fakeChefInGame.gameObject.transform.position += fromPreviousToCurrentPosition * distanceToProgress;
            //        //}
            //        ////if we have finally advanced a server tick, recalculate our bounds based on new origin point
            //        //else
            //        //{

            //        //}
            //    }
            //    realChefPreviousPosition = currentRealPosition;
            //}
        }
        //timeSinceLastTick += Time.deltaTime;
        //may need this in the future Vector3.Cross(Camera.main.transform.right, Vector3.up)
    }
}
