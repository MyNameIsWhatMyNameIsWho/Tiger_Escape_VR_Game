using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class MockInputHandler : MonoBehaviour
{

    public enum ButtonState { Released, Down, Held, Up }



    /// <summary>
    /// INPUT
    /// </summary>
    private ButtonState _teleportationState;
    private ButtonState _upState;
    private ButtonState _downState;
    private ButtonState _forwardState;
    private ButtonState _backwardState;
    private ButtonState _resetState;
    private ButtonState _triggerState;
    private ButtonState _eightPuzzleState;
    private ButtonState _targetsState;
    private ButtonState _targetHitState;

    /// <summary>
    /// GETTERS AND SETTERS
    /// </summary>
    public ButtonState TeleportationState
    {
        get { return _teleportationState; }
        set { _teleportationState = value; }
    }
    public ButtonState UpState
    {
        get { return _upState; }
    }
    public ButtonState DownState
    {
        get { return _downState; }
    }
    public ButtonState ForwardState
    {
        get { return _forwardState; }
    }

    public ButtonState BackwardState
    {
        get { return _backwardState; }
    }
    public ButtonState ResetState
    {
        get { return _resetState; }
    }
    public ButtonState TriggerState
    {
        get { return _triggerState; }
    }

    private Dictionary<string, bool> _lastState = new Dictionary<string, bool>();


    private void Start()
    {
        
    }

    private ButtonState GetButton(string name, bool pressed)
    {
        var state = ButtonState.Released;
        if (!_lastState.ContainsKey(name))
        {
            _lastState.Add(name, false);
        }
        if (pressed)
        {
            if (!_lastState[name])
            {
                state = ButtonState.Down;
            }
            else
                state = ButtonState.Held;
            //EventManager.Instance.InvokeByInput(name);
        }
        else
        {
            if (_lastState[name])
                state = ButtonState.Up;
            else
                state = ButtonState.Released;
        }
        _lastState[name] = pressed;
        return state;
    }
    private bool GetPressedFromContext(InputAction.CallbackContext context)
    {
        bool pressed = false;
        if (context.performed)
        {
            pressed = true;
        }
        else if (context.started)
        {
            pressed = true;
        }
        else if (context.canceled)
        {
            pressed = false;
        }
        return pressed;
    }

    public void HandleTeleportPressed(InputAction.CallbackContext context)
    {
        bool pressed = GetPressedFromContext(context);
        string name = "teleport";
        _teleportationState = GetButton(name, pressed);

    }

    public void HandleUpPressed(InputAction.CallbackContext context)
    {

        bool pressed = GetPressedFromContext(context);
        string name = "up";
        _upState = GetButton(name, pressed);
    }

    public void HandleDownPressed(InputAction.CallbackContext context)
    {

        bool pressed = GetPressedFromContext(context);
        string name = "down";
        _downState = GetButton(name, pressed);
    }
    public void HandleResetPressed(InputAction.CallbackContext context)
    {
        bool pressed = GetPressedFromContext(context);
        string name = "reset";
        _resetState = GetButton(name, pressed);
    }

    public void HandleForwardPressed(InputAction.CallbackContext context)
    {
        bool pressed = GetPressedFromContext(context);
        string name = "forward";
        _forwardState = GetButton(name, pressed);
    }
    public void HandleBackwardPressed(InputAction.CallbackContext context)
    {
        bool pressed = GetPressedFromContext(context);
        string name = "backward";
        _backwardState = GetButton(name, pressed);
    }

    public void HandleTriggerPressed(InputAction.CallbackContext context) {
        bool pressed = GetPressedFromContext(context);
        string name = "trigger";
        _triggerState = GetButton(name, pressed);
    }
    public void HandleEightPuzzlePressed(InputAction.CallbackContext context)
    {
        bool pressed = GetPressedFromContext(context);
        string name = "puzzle";
        _eightPuzzleState = GetButton(name, pressed);
        if (_eightPuzzleState == ButtonState.Down) {
            MiniGameManager.Instance.StartGame(MiniGameManager.MiniGameType.EightPuzzle);
            MiniGameManager.Instance.CompleteEightPuzzle();
        }
    }
    public void HandleTargetsPressed(InputAction.CallbackContext context)
    {
        bool pressed = GetPressedFromContext(context);
        string name = "targets";
        _targetsState = GetButton(name, pressed);
        if (_targetsState == ButtonState.Down)
        {
            MiniGameManager.Instance.StartGame(MiniGameManager.MiniGameType.Targets);
        }
    }
    public void HandleTargetHitPressed(InputAction.CallbackContext context)
    {
        bool pressed = GetPressedFromContext(context);
        string name = "target_hit";
        _targetHitState = GetButton(name, pressed);
        if (_targetHitState == ButtonState.Down)
        {
            //MiniGameManager.Instance.HitTarget();
        }
    }

}
