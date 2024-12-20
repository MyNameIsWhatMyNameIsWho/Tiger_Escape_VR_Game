using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunRotation : MonoBehaviour
{
    [SerializeField] private TurnWheel wheel;
    void Awake()
    {
        wheel.rotAction += (rot) =>
        {
            transform.rotation = Quaternion.Euler(rot * Vector3.right);
        };
    }
}
