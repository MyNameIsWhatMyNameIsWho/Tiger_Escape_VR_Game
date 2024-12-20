using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Button : MonoBehaviour
{
    [SerializeField] private UnityEvent pressAction;
    [SerializeField] private GameObject buttonTop;
    [SerializeField] private Vector3 pressScale = new Vector3(0.9f, 0.5f, 0.9f);
    private Vector3 _initialScale;
    private Coroutine _scaleCoroutine;

    void Awake()
    {
        _initialScale = buttonTop.transform.localScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Controller>())
        {
            StartCoroutine(LerpToScale(pressScale));
            pressAction?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<Controller>())
            StartCoroutine(LerpToScale(_initialScale));
    }

    private IEnumerator LerpToScale(Vector3 targetScale)
    {
        Vector3 initialScale = buttonTop.transform.localScale;

        for (float i = 0; i <= 1; i += 0.1f)
        {
            buttonTop.transform.localScale = Vector3.Lerp(initialScale, targetScale, i);
            yield return new WaitForEndOfFrame();
        }
    }
}
