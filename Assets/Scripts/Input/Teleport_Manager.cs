using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport_Manager : MonoBehaviour
{
    [SerializeField] private GameObject teleportRingPrefab;
    [SerializeField] private Material invalidTeleportMaterial;

    private Material _originalMaterial;
    private GameObject _teleportRing;
    private MeshRenderer _renderer;
    private bool _usingInvalid = false;


    public void Instantiate_Teleport(bool canTeleport, Vector3 position, Quaternion rotation)
    {
        _teleportRing = Instantiate(teleportRingPrefab, position, rotation);
        _renderer = _teleportRing.GetComponent<MeshRenderer>();
        _originalMaterial = _renderer.material;
        if (!canTeleport)
        {
            _renderer.material = invalidTeleportMaterial;
            _usingInvalid = true;
        }
    }

    public void MoveTeleport(bool canTeleport, Vector3 new_position)
    {
        if(_teleportRing == null)
        {
            Instantiate_Teleport(canTeleport, new_position, Quaternion.identity);
        }
        else
        {
            _teleportRing.transform.position = new_position;
            if (canTeleport && _usingInvalid)
            {
                _renderer.material = _originalMaterial;
                _usingInvalid = false;
            }
            else if (!canTeleport && !_usingInvalid)
            {
                _renderer.material = invalidTeleportMaterial;
                _usingInvalid = true;
            }
        }
    }
    public void DestroyTeleport()
    {
        if(_teleportRing != null)
        {
            Destroy(_teleportRing);
            _teleportRing = null;
            _usingInvalid = false;
            _renderer = null;
            _originalMaterial = null;
        }
    }
}
