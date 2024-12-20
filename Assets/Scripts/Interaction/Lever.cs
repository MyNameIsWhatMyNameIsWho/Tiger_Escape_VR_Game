using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour, Interactable
{
    [SerializeField] private float bound = 0.5f;
    [SerializeField] private float maxLeverRotation = 30.0f;
    [SerializeField] private GameObject leverKnob;
    [SerializeField] private float returnDuration = 1.0f;
    [SerializeField] private Outline outline;
    [SerializeField] private GameObject meatObject;
    [SerializeField] private GameObject meatSpawnObject;
    private GameObject _controller;
    private bool _grabbed = false;
    public bool returning = false;
    private Vector3 _initialControllerPosition;
    private Quaternion _initialStickRotation;
    private float _currentRotation = 0f;
    // Start is called before the first frame update
    void Start()
    {
        //leverKnob.transform.localRotation = Quaternion.Euler(0, 0, -maxLeverRotation);
        leverKnob.transform.localRotation = Quaternion.Euler(-maxLeverRotation, 0, 0);
        //leverKnob.transform.eulerAngles = new Vector3(-maxLeverRotation, 0, 0);
        _currentRotation = -leverKnob.transform.rotation.eulerAngles.x;
        Debug.Log(_currentRotation);
        _initialStickRotation = leverKnob.transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (_grabbed) {
            Vector3 positionOffset = _initialControllerPosition - _controller.transform.position;
            float posy = Mathf.Clamp(positionOffset.y, -bound, bound) / bound;
            float targetRotation = posy * maxLeverRotation * 0.5f;
            _currentRotation = Mathf.Clamp(_currentRotation + targetRotation, -maxLeverRotation, maxLeverRotation);

            leverKnob.transform.localRotation = Quaternion.Euler(_currentRotation, 0, 0);
            if(_currentRotation == maxLeverRotation) {
                SpawnObject(meatObject, meatSpawnObject.transform.position);
                Dropped();
                StartCoroutine(RotateBackToOriginalPosition());
            }
            
        }
    }
    private void SpawnObject(GameObject gameObject, Vector3 position) {
        GameObject o = Instantiate(gameObject);
        o.transform.position = position;
    }
    public void Grabbed(GameObject controller, GameObject grabbedObject) {
        if(!returning) {
            _grabbed = true;
            _controller = controller;
            _initialControllerPosition = _controller.transform.position;
        }
    }
    public void Dropped() {
        _grabbed = false;
        //StartCoroutine(RotateBackToOriginalPosition());
        _initialStickRotation = leverKnob.transform.localRotation;
        outline.enabled = false;
    }
    private IEnumerator RotateBackToOriginalPosition()
    {
        returning = true;
        Quaternion initialRotation = leverKnob.transform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(-maxLeverRotation, 0, 0);
        float elapsedTime = 0f;
        while (elapsedTime < returnDuration)
        {
            leverKnob.transform.localRotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime / returnDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        leverKnob.transform.localRotation = targetRotation;
        _currentRotation = -leverKnob.transform.rotation.eulerAngles.z;
        _initialStickRotation = leverKnob.transform.localRotation;
        returning = false;
    }
}
