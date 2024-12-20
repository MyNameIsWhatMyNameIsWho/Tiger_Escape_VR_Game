using UnityEngine;
using UnityEngine.Events;

public class Joystick : MonoBehaviour, IGrabbable
{
    [SerializeField] private float bound = 0.5f;
    [SerializeField] private float maxStickRotation = 15.0f;
    [SerializeField] private GameObject stickBase;
    public delegate void Action(float a);
    public Action xAction, yAction;
    
    private GameObject _controller;
    private bool _grabbed = false;
    private Vector3 _initialControllerPosition;
    private Quaternion _initialStickRotation;

    void Start()
    {
        _initialStickRotation = stickBase.transform.rotation;
    }

    void Update()
    {
        if (_grabbed)
        {
            Vector3 positionOffset = _initialControllerPosition - _controller.transform.position;

            /* clamp and normalize offsets */
            float posx = Map(positionOffset.x, -bound, bound) / bound;
            float posy = Map(positionOffset.z, -bound, bound) / bound;

            stickBase.transform.rotation = _initialStickRotation * Quaternion.Euler(posy * maxStickRotation, 0, -posx * maxStickRotation);


            /* find out which axis has a larger offset */
            if (Mathf.Abs(posx) > Mathf.Abs(posy))
            {
                if (Mathf.Abs(posx) > 0.2f)
                {
                    xAction.Invoke(posx);
                }
            }
            else
            {
                if (Mathf.Abs(posy) > 0.2f)
                {
                    yAction.Invoke(posy);
                }
            }
        }
    }

    public void Grabbed(GameObject controller)
    {
        _grabbed = true;
        _controller = controller;
        _initialControllerPosition = _controller.transform.position;
    }

    public void Dropped()
    {
        _grabbed = false;
        stickBase.transform.rotation = _initialStickRotation;
    }

    private static float Map(float value, float min, float max)
    {
        if (value < min)
            return min;
        else if (value > max)
            return max;
        else
            return value;
    }
}
