using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PressurePlateManager : MonoBehaviour
{
    private BoxCollider boxCollider;
    private MiniGameManager miniGameManager;
    [SerializeField] private MiniGameManager.MiniGameType miniGameType;
    // Start is called before the first frame update
    void Start()
    {
        boxCollider = gameObject.GetComponent<BoxCollider>();
        if(!boxCollider) {
            Debug.LogError("Pressure plate did not find a box collider");
        }
        miniGameManager = MiniGameManager.Instance;
        if(!miniGameManager) {
            Debug.LogError("Pressure plate did not find a minigame manager");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag == "GameController") {
            miniGameManager.StartGame(miniGameType);
            if(miniGameType == MiniGameManager.MiniGameType.EightPuzzle) {
                miniGameManager.CompleteEightPuzzle();
            }
        }
    }
}
