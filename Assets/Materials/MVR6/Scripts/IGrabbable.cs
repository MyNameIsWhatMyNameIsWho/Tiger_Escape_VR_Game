using UnityEngine;

public interface IGrabbable
{
    void Grabbed(GameObject _controller);
    void Dropped();
}
