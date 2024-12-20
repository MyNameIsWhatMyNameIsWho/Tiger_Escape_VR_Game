using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnWheel : MonoBehaviour, IGrabbable
{
    private GameObject _controller;
    private bool _grabbed = false;
    private Vector3 _previousControllerPosition;
    private Quaternion _initialWheelRotation;
    public delegate void Action(float a);
    public Action rotAction;

    void Start()
    {
        _initialWheelRotation = transform.rotation;
    }

    void Update()
    {
        if(_grabbed)
        {
            var currentControllerPosition = _controller.transform.position;
            var rotation = Quaternion.FromToRotation(currentControllerPosition - transform.position, _previousControllerPosition - transform.position);
            var rotationEuler = rotation.eulerAngles.y * Vector3.up;
            transform.rotation *= Quaternion.Euler(rotationEuler);
            _previousControllerPosition = currentControllerPosition;
            rotAction?.Invoke(transform.rotation.eulerAngles.y);
        }
    }

    public void Grabbed(GameObject controller)
    {
        _grabbed = true;
        _controller = controller;
        _previousControllerPosition = _controller.transform.position;
    }

    public void Dropped()
    {
        _grabbed = false;
    }
}
