using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// A singleton managing the minigames
/// </summary>
public class MiniGameManager : MonoBehaviour
{
    public enum MiniGameType {EightPuzzle, Targets, Cups};
    public static MiniGameManager Instance;
    [SerializeField] private GameObject eightPuzzleObject;
    [SerializeField] private GameObject targetsObject;
    [SerializeField] private GameObject timerObject;
    [SerializeField] private CupsMiniGameManager cupsMiniGameManager;
    [SerializeField] private float eightPuzzleTime = 120.0f;
    [SerializeField] private float targetsTime = 120.0f;


    private Timer _timer;
    private SerialKeyManager _serialKeyManager;

    /// <summary>
    /// ANIMATORS
    /// </summary>
    private List<Animator> _miniGameAnimators = new();

    /// <summary>
    /// MINIGAME INFO
    /// </summary>
    private List<float> _miniGameTimeLimits = new();

    /// <summary>
    /// MINIGAME STATE VARIABLES
    /// </summary>=
    private List<bool> _miniGamesRunning = new();
    private List<MiniGame> _miniGames = new();
    private bool completeCourotineRunning = false;
    private bool minigameIsRunning = false;
    private List<bool> _miniGamesCompleted = new();

    void Awake()
    {
        if (Instance != null)
            Destroy(Instance);
        else
            Instance = this;

        DontDestroyOnLoad(this);
    }
    
    void Start()
    {
        _timer = timerObject.GetComponent<Timer>();
        _serialKeyManager = gameObject.GetComponent<SerialKeyManager>();
        ///Add game variables in the same order as they are specified in the enum for easy indexing
         ///ANIMATORS
         _miniGameAnimators.Add(eightPuzzleObject.GetComponent<Animator>());
         _miniGameAnimators.Add(targetsObject.GetComponent<Animator>());
        ///TIME LIMITS
        _miniGameTimeLimits.Add(eightPuzzleTime);
        _miniGameTimeLimits.Add(targetsTime);
        //MINIGAME SCRIPTS
        _miniGames.Add(eightPuzzleObject.GetComponent<EightPuzzle>());
        _miniGames.Add(targetsObject.GetComponent<TargetGame>());
        /// GAME RUNNING STATES (all false at the beggining)
        _miniGamesRunning.Add(false);
        _miniGamesRunning.Add(false);

        _miniGamesCompleted.Add(false);
        _miniGamesCompleted.Add(false);
        _miniGamesCompleted.Add(false);
    }

    /// <summary>
    /// Starts the game specified by the given enum type, plays its starting animaton and starts timer.
    /// </summary>
    public void StartGame(MiniGameType gameType)
    {
        if(minigameIsRunning || _miniGamesCompleted[(int)gameType]) {
            return;
        }
        if(gameType == MiniGameType.Cups) {
            cupsMiniGameManager.StartGame();
        } else {
            minigameIsRunning = true;
            _miniGamesRunning[(int)gameType] = true;
            _miniGameAnimators[(int)gameType].SetBool("GameRunning", true);
            _miniGames[(int)gameType].StartGame();
        //_miniGameAnimators[(int)gameType].SetBool("TryEnded", false);
            _timer.SetTimeLimit(_miniGameTimeLimits[(int)gameType]);
        }
    }
    /// <summary>
    /// Ends the game speicfied by the given enum type, plays its ending animation and ends timer
    /// </summary>
    public void EndGame(MiniGameType gameType, Vector3? codeNumberSpawnPos = null, Quaternion? codeNumberRotation = null)
    {
        minigameIsRunning = false;
        print("end: " + gameType); 
        _miniGamesRunning[(int)gameType] = false;
        _miniGameAnimators[(int)gameType].SetBool("GameRunning", false);
        _miniGames[(int)gameType].EndGame();
        //_miniGameAnimators[(int)gameType].SetBool("TryEnded", true);
        _timer.EndTimer();
        //if the game was solved succesfully spawn the number
        if (codeNumberSpawnPos.HasValue && codeNumberRotation.HasValue) {
            _serialKeyManager.showPartOfFinalKey(codeNumberSpawnPos.Value, codeNumberRotation.Value);
            _miniGamesCompleted[(int)gameType] = true;
        }
    }

    /// <summary>
    /// Ends all currently running games
    /// </summary>
    public void EndGames(Vector3? codeNumberSpawnPos = null, Quaternion? codeNumberRotation = null)
    {
        foreach (var gameType in Enum.GetValues(typeof(MiniGameType)).Cast<MiniGameType>())
        {
            if(_miniGamesRunning[(int)gameType] && gameType != MiniGameType.Cups)
            {
                EndGame(gameType, codeNumberSpawnPos, codeNumberRotation);
            }    
        }
    }

    /// <summary>
    /// Helper method to instantly complete 8-puzzle in its initial configuration
    /// </summary>
    public void CompleteEightPuzzle()
    {
        if (_miniGamesRunning[(int)MiniGameType.EightPuzzle] && !completeCourotineRunning)
        {
            completeCourotineRunning=true;
            StartCoroutine(WaitAndCompletePuzzle());
        }
    }

    //public void HitTarget()
    //{
    //    _miniGames[(int)MiniGameType.Targets].TargetHit();
    //}

    private IEnumerator WaitAndCompletePuzzle()
    {
        // the 8-puzzle opening animation takes 3 seconds
        yield return new WaitForSeconds(5.0f);
        completeCourotineRunning = false;
        eightPuzzleObject.GetComponent<EightPuzzle>().CompleteThisThing();
    }

}