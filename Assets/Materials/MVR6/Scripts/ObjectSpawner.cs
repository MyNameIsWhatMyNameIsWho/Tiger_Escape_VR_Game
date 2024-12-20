using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] private Material[] materials;

    public void SpawnObject()
    {
        var spawnedObject = Instantiate(objectToSpawn, transform.position, Quaternion.identity);
        if(materials.Length != 0)
        {
            var rand = Random.Range(0, materials.Length);
            spawnedObject.GetComponent<MeshRenderer>().material = materials[rand];
        }
    }
}
