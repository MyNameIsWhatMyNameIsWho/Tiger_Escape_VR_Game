using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using UnityEditor.Search;

public class Mock_Controller : MonoBehaviour
{
    //private enum ControllerHand { Left, Right };
    public enum CollisionState { NO_COLLISION, FROM_BACK, FROM_FRONT};

    [SerializeField] private float lookStep = 1.0f;
    [SerializeField] private float yRotationClamp = 15.0f;
    [SerializeField] private float continuousMovementSpeed = 0.1f;
    [SerializeField] private float maxMoveSpeed = 2.0f;

    //private UnityEngine.XR.InputDevice _controller;
    //private ControllerHand _hand;

    private bool turned = false;
    private GameObject _collidedObject;
    private GameObject _remoteObject;

    /// <summary>
    /// CAMERA ROTATION
    /// </summary>
    private Camera _camera;
    private Vector2 _screenCenter = Vector2.zero;
    private Vector2 _mousePos = Vector2.zero;
    private Vector2 _rotation = Vector2.zero;

    /// <summary>
    /// CONTINOUS MOVEMENT
    /// </summary>
    private float _curMovement;
    private CollisionState _collisionState;

    /// <summary>
    /// INPUT
    /// </summary>
    private MockInputHandler _inputHandler;

    /// <summary>
    /// TELEPORT
    /// </summary>
    private Teleport_Manager _teleportManager;
    //Simplistic movement so this is to allow reset in case you get stuck.
    private Vector3 _originalPos;
    private Quaternion _originalRotation;

    private Vector3 _previousRemotePosition;


    //private Dictionary<InputFeatureUsage<bool>, bool> _lastState = new Dictionary<InputFeatureUsage<bool>, bool>();

    #region Input


    private void Start()
    {
        _camera = Camera.main;
        _collisionState = CollisionState.NO_COLLISION;
        _screenCenter.x = Screen.width * 0.5f;
        _screenCenter.y = Screen.height * 0.5f;
        _mousePos = Mouse.current.position.ReadValue();
        _teleportManager = GetComponent<Teleport_Manager>();
        _inputHandler = GetComponent<MockInputHandler>();
        _originalPos = CameraOffset.Instance.transform.position;
        _originalRotation = CameraOffset.Instance.transform.rotation;

    }


    #endregion

    private void HandleContinousTurn()
    {

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 difference = screenPos - _mousePos;
        if (Mathf.Abs(difference.x) > 1.0f)
        {
            if (difference.x > 0)
            {
                CameraOffset.Instance.transform.rotation *= Quaternion.Euler(0, lookStep, 0);
                _rotation.x += lookStep;
            }
            else
            {
                CameraOffset.Instance.transform.rotation *= Quaternion.Euler(0, -lookStep, 0);
                _rotation.x -= lookStep;
            }
        }
        _mousePos = screenPos;
        if ((_inputHandler.UpState == MockInputHandler.ButtonState.Held 
            || _inputHandler.UpState == MockInputHandler.ButtonState.Down)
            && _rotation.y - lookStep >= -yRotationClamp)
        {
            CameraOffset.Instance.transform.rotation *= Quaternion.Euler(-lookStep, 0, 0);
            _rotation.y -= lookStep;
        }
        if ((_inputHandler.DownState == MockInputHandler.ButtonState.Held
            || _inputHandler.DownState == MockInputHandler.ButtonState.Down)
            && _rotation.y + lookStep <= yRotationClamp)
        {
            CameraOffset.Instance.transform.rotation *= Quaternion.Euler(lookStep, 0, 0);
            _rotation.y += lookStep;
        }
    }

    private void HandleContinousMovement()
    {
        // If forward is held accumulate speed going forward
        //print(_inputHandler.ForwardState);
        if ((_inputHandler.ForwardState == MockInputHandler.ButtonState.Held 
            || _inputHandler.ForwardState == MockInputHandler.ButtonState.Down) 
            && _collisionState != CollisionState.FROM_FRONT) { 
            _curMovement = Mathf.Min(_curMovement + continuousMovementSpeed, maxMoveSpeed);
        }
        else
        {
            // Otherwise remove the positive speed contribution from the movement
            _curMovement = Mathf.Min(_curMovement, 0);
        }
        // Analogically for backward button
        if ((_inputHandler.BackwardState == MockInputHandler.ButtonState.Held 
               || _inputHandler.BackwardState == MockInputHandler.ButtonState.Down)
               && _collisionState != CollisionState.FROM_BACK)
        {
            _curMovement = Mathf.Max(_curMovement - continuousMovementSpeed, -maxMoveSpeed);
        }
        else
        {
            // Otherwise remove the negative speed contribution from the movement
            _curMovement = Mathf.Max(_curMovement, 0);
        }
        // To make sure
        _curMovement = Mathf.Clamp(_curMovement, -maxMoveSpeed, maxMoveSpeed);
        var direction = CameraOffset.Instance.transform.forward;
        direction.y = 0;
        CameraOffset.Instance.transform.position += _curMovement * direction;
    }
    private void HandleReset()
    {
        var resetState = _inputHandler.ResetState;
        if(resetState == MockInputHandler.ButtonState.Down || resetState == MockInputHandler.ButtonState.Held)
        {
            CameraOffset.Instance.transform.position = _originalPos;
            CameraOffset.Instance.transform.rotation = _originalRotation;
            _rotation = Vector2.zero;
        }
    }
    void Update()
    {
        HandleReset();
        HandleContinousTurn();
        HandleContinousMovement();
        HandleTeleportation();
        SnapToRotate();
        if (_collidedObject)
            HandleCloseInteraction();
        else
            HandleRemoteInteraction();
    }

    private void SnapToRotate()
    {
        //var analogPos = GetAnalogPosition();
        var analogPos = new Vector2(0, 0);

        if (!turned && Mathf.Abs(analogPos.x) > 0.3f)
        {
            if (analogPos.x < 0)
            {
                CameraOffset.Instance.transform.rotation *= Quaternion.Euler(0, -30, 0);
                /* turned left */
            }
            else
            {
                CameraOffset.Instance.transform.rotation *= Quaternion.Euler(0, 30, 0);
                /* turned right */
            }
            turned = true;
        }
        else if (turned && Mathf.Abs(analogPos.x) < 0.3f)
        {
            turned = false;
        }
    }

    private void HandleTeleportation()
    {
        RaycastHit[] hits;
        if(_inputHandler.TeleportationState != MockInputHandler.ButtonState.Held)
        {
            _teleportManager.DestroyTeleport();
        }
        bool hit_found = false;
        bool no_block = true;
        hits = Physics.RaycastAll(CameraOffset.Instance.transform.position, CameraOffset.Instance.transform.forward, float.MaxValue, LayerMask.GetMask("Ground", "Block Teleport"));
        RaycastHit ground_hit = new RaycastHit();
        foreach (var hit in hits)
        {
            if(hit.transform.gameObject.layer == LayerMask.NameToLayer("Block Teleport"))
            {
                no_block = false; 
            }
            else
            {
                hit_found = true;
                ground_hit = hit;
            }
        }
            if (hit_found)
            {
            switch (_inputHandler.TeleportationState)
            {
                case MockInputHandler.ButtonState.Down:
                    _teleportManager.Instantiate_Teleport(no_block, ground_hit.point, Quaternion.identity);
                    break;
                case MockInputHandler.ButtonState.Up:
                    if(no_block)
                    {
                        var offset = ground_hit.point - CameraOffset.Instance.transform.position;
                        offset.y = 0;
                        CameraOffset.Instance.transform.position += offset;
                    }
                    break;
                case MockInputHandler.ButtonState.Held:
                    _teleportManager.MoveTeleport(no_block, ground_hit.point);
                    break;
            }
        }
        if(_inputHandler.TeleportationState == MockInputHandler.ButtonState.Up)
        {
            _inputHandler.TeleportationState = MockInputHandler.ButtonState.Released;
        }
    }

    private void HandleCloseInteraction()
    {
        //var gripState = GetButton(CommonUsages.gripButton);
        var gripState = MockInputHandler.ButtonState.Released;

        switch (gripState)
        {
            case MockInputHandler.ButtonState.Down:
                _collidedObject.transform.parent = transform;
                _collidedObject.GetComponent<Rigidbody>().isKinematic = true;
                break;
            case MockInputHandler.ButtonState.Up:
                _collidedObject.transform.parent = null;
                _collidedObject.GetComponent<Rigidbody>().isKinematic = false;
                break;
        }
    }

    private void HandleRemoteInteraction()
    {
        RaycastHit hit;
        //var triggerState = GetButton(CommonUsages.triggerButton);
        var triggerState = MockInputHandler.ButtonState.Released;

        if (Physics.Raycast(transform.position, transform.forward, out hit, float.MaxValue, LayerMask.GetMask("Interactable")))
        {
            _remoteObject = hit.transform.gameObject;
            
            if (triggerState == MockInputHandler.ButtonState.Down)
                _remoteObject.GetComponent<Rigidbody>().isKinematic = true;
        }

        switch (triggerState)
        {
            case MockInputHandler.ButtonState.Up:
                if (_remoteObject)
                    _remoteObject.GetComponent<Rigidbody>().isKinematic = false;
                _remoteObject = null;
                break;
            case MockInputHandler.ButtonState.Held:
                if(_remoteObject)
                {
                    var deltaPosition = transform.position - _previousRemotePosition;
                    var multiplier = Vector3.Distance(transform.position, _remoteObject.transform.position) * 1f;
                    _remoteObject.transform.position += deltaPosition * multiplier;
                }
                break;
        }

        _previousRemotePosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Interactable"))
            _collidedObject = other.gameObject;
        if (other.gameObject.CompareTag("Block Teleport"))
        {
            var collisionPoint = other.ClosestPoint(transform.position);
            var directionToObject = (collisionPoint - CameraOffset.Instance.transform.position).normalized;
            // Get the cosine between the direction to the collision and forward
            // the vectors are normalized so dot product will suffice
            var angleCos = Vector3.Dot(CameraOffset.Instance.transform.forward, directionToObject);
            // If the cosine is greater then zero then the angle is less than PI/2
            // and collision from the front happened. The same happens for
            // collision from backward if the angle is < 0
            if (angleCos < 0)
            {
                _collisionState = CollisionState.FROM_BACK;
            }
            else if (angleCos > 0) 
            {
                _collisionState = CollisionState.FROM_FRONT;            
            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
        _collidedObject = null;
        _collisionState = CollisionState.NO_COLLISION;
    }
}
