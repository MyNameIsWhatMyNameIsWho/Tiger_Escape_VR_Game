using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CupManager : MonoBehaviour, Interactable
{
    [SerializeField] private CupsMiniGameManager cupsMiniGameManager;
    public void Dropped()
    {
        
    }

    public void Grabbed(GameObject controller, GameObject grabbedObject)
    {
        print("GRABBED");
        cupsMiniGameManager.choseCup(gameObject);
    }

    // Start is called before the first frame update

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
