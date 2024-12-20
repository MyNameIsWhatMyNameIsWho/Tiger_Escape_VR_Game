using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class handling the target game logic
/// </summary>
public class TargetGame : MiniGame
{
    [SerializeField] private List<GameObject> targetObjects = new List<GameObject>();
    [SerializeField] private GameObject codeNumberSpawnObject;

    private List<Target> _targets = new List<Target>();


    private int _targetCount;
    private int _curHitTargets;

    private Vector3 _codeSpawnPosition;
    private Quaternion _codeSpawnRotation;


    // Start is called before the first frame update
    void Start()
    {
        foreach (var target in targetObjects)
        {
            //Give all target scripts reference to this game
            var targetScript = target.GetComponent<Target>();
            targetScript.SetTargetGame(this);
            _targets.Add(targetScript);
        }
        _targetCount = _targets.Count;
        _curHitTargets = 0;
        _codeSpawnPosition = codeNumberSpawnObject.transform.position;
        _codeSpawnRotation = codeNumberSpawnObject.transform.rotation;
    }

    public void TargetHit()
    {
        _curHitTargets++;
        if(_curHitTargets == _targetCount)
        {
            MiniGameManager.Instance.EndGame(MiniGameManager.MiniGameType.Targets, _codeSpawnPosition, _codeSpawnRotation);
        }
    }


    public override void StartGame()
    {
        foreach (var target in _targets) {
            target.TryStarted();
        }
        _curHitTargets = 0;
    }

    public override void EndGame()
    {
        foreach (var target in _targets) {
            target.TryEnded();
        }
        _curHitTargets = 0;
    }
}
