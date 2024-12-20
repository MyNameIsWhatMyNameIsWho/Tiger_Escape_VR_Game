using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Interactable
{
    void Grabbed(GameObject controller, GameObject grabbedObject);
    void Dropped();
}
