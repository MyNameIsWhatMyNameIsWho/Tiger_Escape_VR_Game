using UnityEngine;

public class Ship : MonoBehaviour
{
    [SerializeField] private Joystick _joystick;

    void Start()
    {
        _joystick.xAction += (x) => {
            /* rotate around ship's y axis */
            transform.rotation *= Quaternion.Euler(x * Vector3.down);
        };

        _joystick.yAction += (y) => {
            /* move back and forth */
            transform.GetComponent<Rigidbody>().AddForce(
                transform.rotation * (y * 0.2f * Vector3.left), ForceMode.VelocityChange
            );
        };
    }
}
