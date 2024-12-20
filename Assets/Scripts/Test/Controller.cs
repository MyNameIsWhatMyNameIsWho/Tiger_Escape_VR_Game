using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SpatialTracking;
using UnityEngine.InputSystem.LowLevel;
using System.Security.Cryptography;
using NavKeypad;

[RequireComponent(typeof(TrackedPoseDriver))]
public class Controller : MonoBehaviour
{
    private enum ControllerHand { Left, Right };
    private enum ButtonState { Released, Down, Held, Up }
    public enum CollisionState { NO_COLLISION, FROM_BACK, FROM_FRONT };

    [SerializeField] private float continuousMovementSpeed;
    [SerializeField] private int framesAccountedForWhenThrowing = 50;
    [SerializeField] private float throwForce = 150f;
    [SerializeField] private float throwThreshold;


    private InputDevice _controller;
    private ControllerHand _hand;

    private bool turned = false;
    private GameObject _collidedObject;
    private Outline _collidedObjectOutline;
    private GameObject _collidedKeypadButton;
    private Outline _collidedKeypadButtonOutline;
    private bool _interactingWithObject = false;
    private GameObject _remoteObject;

    private Vector3 _previousRemotePosition;

    /// <summary>
    /// TELEPORT
    /// </summary>
    private Teleport_Manager _teleportManager;

    /// <summary>
    /// CONTINOUS MOVEMENT
    /// </summary>
    private CollisionState _collisionState;

    private Dictionary<InputFeatureUsage<bool>, bool> _lastState = new Dictionary<InputFeatureUsage<bool>, bool>();

    /// <summary>
    /// GRAB
    /// </summary>
    private GameObject _grabbableObject;
    private List<GameObject> _collidedObjectList;
    private Outline _grabbableObjectOutline;

    /// <summary>
    /// THROW
    /// </summary>
    private List<Vector3> _lastControllerPositions = new List<Vector3>();


    #region Input


    private void Start()
    {
        _collisionState = CollisionState.NO_COLLISION;
        _teleportManager = GetComponent<Teleport_Manager>();
        var trackedPoseDriver = GetComponent<TrackedPoseDriver>();
        _hand = trackedPoseDriver.poseSource == TrackedPoseDriver.TrackedPose.LeftPose
            ? ControllerHand.Left : ControllerHand.Right;

        InputDevices.deviceConnected += OnInputDeviceConnected;

        _collidedObjectList = new List<GameObject>();


        var devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);

        devices.ForEach(OnInputDeviceConnected);
    }

    private void OnInputDeviceConnected(InputDevice device)
    {
        Debug.Log($"{device.name} {device.characteristics}");
        if ((device.characteristics.HasFlag(InputDeviceCharacteristics.Left) && _hand == ControllerHand.Left)
            || (device.characteristics.HasFlag(InputDeviceCharacteristics.Right) && _hand == ControllerHand.Right))
        {
            Debug.Log($"{device.name} connected");
            _controller = device;
        }
    }

    private Vector2 GetAnalogPosition()
    {
        if (_controller.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 analogPosition))
            return analogPosition;
        else
            return Vector2.zero;
    }

    private ButtonState GetButton(InputFeatureUsage<bool> button)
    {
        if (!_lastState.ContainsKey(button))
            _lastState.Add(button, false);

        var state = ButtonState.Released;

        if (_controller.TryGetFeatureValue(button, out bool pressed))
        {
            if (pressed)
            {
                if (_lastState[button] == false) {
                    state = ButtonState.Down;
                    print("invoking by input");
                    EventManager.Instance.InvokeByInput(button);
                }
                else {
                    state = ButtonState.Held;
                }
            }
            else
            {
                if (_lastState[button] == true)
                    state = ButtonState.Up;
                else
                    state = ButtonState.Released;
            }
            _lastState[button] = pressed;
        }
        return state;
    }

    #endregion

    void Update()
    {
        HandleTeleportation();
        HandleContinuousMovement();
        SnapToRotate();
        if (_collidedObject)
            HandleCloseInteraction();
        else if (_grabbableObject || _collidedKeypadButton)
            HandleGrab();
        else
            HandleRemoteInteraction();
    }

    private void HandleGrab()
    {
        var triggerState = GetButton(CommonUsages.gripButton);
        switch (triggerState)
        {
            case ButtonState.Down:
                if(_grabbableObject) {
                    print("handle grab object");
                    _grabbableObject.GetComponent<Interactable>().Grabbed(gameObject, _grabbableObject);
                } else {
                    print("no grabbable objecto");
                }
                if(_collidedKeypadButton) {
                    KeypadButton keypadButton = _collidedKeypadButton.GetComponent<KeypadButton>();
                    if(keypadButton) {
                        Debug.Log("pressing button");
                        keypadButton.PressButton();
                    } else {
                        Debug.Log("not pressing button");
                    }
                }  else {
                        Debug.Log("no button");
                    }
                break;
            case ButtonState.Up:
                if(_grabbableObject) {
                    _grabbableObject.GetComponent<Interactable>().Dropped();
                    _grabbableObject = null;
                }
                break;
        }
    }


    private void SnapToRotate()
    {
        var analogPos = GetAnalogPosition();

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
        var actionButtonState = GetButton(CommonUsages.triggerButton);
        RaycastHit[] hits;
        if (actionButtonState != ButtonState.Held)
        {
            _teleportManager.DestroyTeleport();
        }
        bool hit_found = false;
        bool no_block = true;
        hits = Physics.RaycastAll(transform.position, transform.forward, float.MaxValue, LayerMask.GetMask("Ground", "Block Teleport"));
        RaycastHit ground_hit = new RaycastHit();
        foreach (var hit in hits)
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Block Teleport"))
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
            switch (actionButtonState)
            {
                case ButtonState.Down:
                    _teleportManager.Instantiate_Teleport(no_block, ground_hit.point, Quaternion.identity);
                    break;
                case ButtonState.Up:
                    if (no_block)
                    {
                        var offset = ground_hit.point - CameraOffset.Instance.transform.position;
                        offset.y = 0;
                        CameraOffset.Instance.transform.position += offset;
                    }
                    break;
                case ButtonState.Held:
                    _teleportManager.MoveTeleport(no_block, ground_hit.point);
                    break;
            }
        }
        //if (_teleportationState == ButtonState.Up)
        //{
        //    _teleportationState = ButtonState.Released;
        //}
    }

    private void HandleCloseInteraction()
    {
        var gripState = GetButton(CommonUsages.gripButton);

        switch (gripState)
        {
            case ButtonState.Down:
                _collidedObject.transform.parent = transform;
                _collidedObject.GetComponent<Rigidbody>().isKinematic = true;
                _lastControllerPositions.Clear();
                break;
            case ButtonState.Up:
                _collidedObject.transform.parent = null;
                _collidedObject.GetComponent<Rigidbody>().isKinematic = false;

                Vector3 firstPosition = transform.position;
                if (_lastControllerPositions.Count > 0)
                {
                    firstPosition = _lastControllerPositions[_lastControllerPositions.Count - 1];
                }

                Debug.Log("throwthrs: "+Vector3.Distance(transform.position, firstPosition));
                if(Vector3.Distance(transform.position, firstPosition) > throwThreshold) {

                    Vector3 diff = Vector3.Normalize(transform.position - firstPosition);
                    _collidedObject.GetComponent<Rigidbody>().AddForce(throwForce * diff, ForceMode.VelocityChange);
                }
                _collidedObject = null;
                if (_collidedObjectOutline)
                {
                    _collidedObjectOutline.enabled = false;
                }
                _interactingWithObject = false;
                break;
            case ButtonState.Held:
                //Debug.Log(_lastControllerPositions.Count);
                _lastControllerPositions.Add(transform.position);
                if (_lastControllerPositions.Count > framesAccountedForWhenThrowing)
                    _lastControllerPositions.RemoveAt(0);
                break;
        }

    }

    private void HandleRemoteInteraction()
    {
        RaycastHit hit;
        var triggerState = GetButton(CommonUsages.triggerButton);

        if (Physics.Raycast(transform.position, transform.forward, out hit, float.MaxValue, LayerMask.GetMask("Interactable")))
        {
            _remoteObject = hit.transform.gameObject;

            if (triggerState == ButtonState.Down)
                _remoteObject.GetComponent<Rigidbody>().isKinematic = true;
        }

        switch (triggerState)
        {
            case ButtonState.Up:
                if (_remoteObject)
                    _remoteObject.GetComponent<Rigidbody>().isKinematic = false;
                _remoteObject = null;
                break;
            case ButtonState.Held:
                if (_remoteObject)
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
        GameObject o = other.gameObject;
        if (other.gameObject.CompareTag("Interactable"))
        {
            if (!_interactingWithObject)
            {
                _interactingWithObject = true;
                _collidedObject = o;
                _collidedObjectOutline = _collidedObject.GetComponent<Outline>();
                if (_collidedObjectOutline)
                {
                    _collidedObjectOutline.enabled = true;
                }
            }
            else
            {
                _collidedObjectList.Add(o);
            }
        }
        else if (o.CompareTag("Grabbable"))
        {
            _grabbableObject = other.gameObject;
            _grabbableObjectOutline = _grabbableObject.GetComponentInChildren<Outline>();
            Lever lever = _grabbableObject.GetComponent<Lever>();
            if (_grabbableObjectOutline && lever && lever.returning == false)
            {
                _grabbableObjectOutline.enabled = true;
            }
            CupManager cupManager = _grabbableObject.GetComponent<CupManager>();
            if(cupManager) {
                _grabbableObjectOutline.enabled = true;
            }
        } else if(o.CompareTag("KeypadButton")) {
            if(_collidedKeypadButton) {
                if(_collidedKeypadButtonOutline) { 
                    _collidedKeypadButtonOutline.enabled = false;
                }
            }
            _collidedKeypadButton = other.gameObject;
            _collidedKeypadButtonOutline = _collidedKeypadButton.GetComponentInChildren<Outline>();
            if(_collidedKeypadButtonOutline) {
                _collidedKeypadButtonOutline.enabled = true;
            }
        }
        if (other.gameObject.CompareTag("Block Teleport"))
        {
            var collisionPoint = other.ClosestPoint(transform.position);
            var directionToObject = (collisionPoint - Camera.main.transform.position).normalized;
            // Get the cosine between the direction to the collision and forward
            // the vectors are normalized so dot product will suffice
            var angleCos = Vector3.Dot(Camera.main.transform.forward, directionToObject);
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
    private void HandleContinuousMovement()
    {
        var analogPos = GetAnalogPosition();

        if (Mathf.Abs(analogPos.y) > 0.5f)
        {
            var direction = Camera.main.transform.forward;
            direction.y = 0;
            if ((analogPos.y > 0 && _collisionState != CollisionState.FROM_FRONT) ||
                (analogPos.y < 0 && _collisionState != CollisionState.FROM_BACK))
            {
                CameraOffset.Instance.transform.position += analogPos.y * continuousMovementSpeed * direction;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        _collisionState = CollisionState.NO_COLLISION;
        GameObject o = other.gameObject;
        if (o.CompareTag("Interactable"))
        {
            if (o == _collidedObject)
            {
                if (_collidedObjectOutline)
                {
                    _collidedObjectOutline.enabled = false;
                }
                if (_collidedObjectList.Count > 0)
                {
                    _interactingWithObject = true;
                    _collidedObject = _collidedObjectList[0];
                    _collidedObjectList.RemoveAt(0);
                    _collidedObjectOutline = _collidedObject.GetComponent<Outline>();
                    if (_collidedObjectOutline)
                    {
                        _collidedObjectOutline.enabled = true;
                    }
                }
                else
                {
                    _collidedObject = null;
                    _interactingWithObject = false;
                }
            }
            else if (_collidedObjectList.Contains(o))
            {
                _collidedObjectList.Remove(o);
            }
        } else if (o.CompareTag("Grabbable") && GetButton(CommonUsages.gripButton) != ButtonState.Held) {
            _grabbableObject = null;
            if (_grabbableObjectOutline)
            {
                _grabbableObjectOutline.enabled = false;
            }
            _grabbableObjectOutline = null;

            Outline outline = o.GetComponent<Outline>();
            if(outline) {
                outline.enabled = false;
            }
        }  else if(o.CompareTag("KeypadButton")) {
            if(GameObject.ReferenceEquals(o, _collidedKeypadButton)) {
                if(_collidedKeypadButtonOutline) { 
                    _collidedKeypadButtonOutline.enabled = false;
                }
                _collidedKeypadButton = null;
                _collidedKeypadButtonOutline = null;
            }
        }
    }
}

