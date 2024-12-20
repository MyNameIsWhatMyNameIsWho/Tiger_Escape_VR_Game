using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] private GameObject textObject;
    [SerializeField] private float animTime = 3.0f;

    private float _curTime;
    private float _timeToGo;
    private bool _isRunning;
    /// <summary>
    /// Whether the animation was finished
    /// </summary>
    private bool _isSetup;

    private TextMeshPro _textMesh;
    private Animator _timerAnimator;
    private MeshRenderer _renderer;

    private void Start()
    {
        _textMesh = textObject.GetComponent<TextMeshPro>();
        _textMesh.enabled = false;
        _timerAnimator = gameObject.GetComponent<Animator>();
        _renderer = gameObject.GetComponent<MeshRenderer>();
    }

    /// COUROUTINES. Handling to first wait for animation finish before updating/disabling text.

    private IEnumerator WaitForAnimAndSetup(float time)
    {
        // the animation takes 2 seconds
        yield return new WaitForSeconds(animTime + 1.0f);
        _textMesh.enabled = true;
        _isSetup = true; 
        _timeToGo = time;
        _curTime = Time.time;
    }

    private IEnumerator WaitForAnimAndDisable()
    {
        // the animation takes 2 seconds
        yield return new WaitForSeconds(animTime + 1.0f);
        _isRunning = false;
        //_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    /// <summary>
    /// Set the time limit for the timer when game is starting
    /// </summary>
    /// <param name="time"></param>
    public void SetTimeLimit(float time)
    {
        if(!_isRunning)
        { 
            _isRunning = true;
            //_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            StartCoroutine(WaitForAnimAndSetup(time));
            _timerAnimator.SetBool("GameRunning", true);
        }

    }
    /// <summary>
    /// End timer and play the exit animation
    /// </summary>
    public void EndTimer()
    {
        if (_isSetup) {
            _timeToGo = 0;
            _isSetup = false;
            _textMesh.enabled = false;
            TimeToText(_timeToGo);
            StartCoroutine(WaitForAnimAndDisable());
            _timerAnimator.SetBool("GameRunning", false);
        }
    }
    /// <summary>
    /// Convert float time into a string representation of type MM:SS
    /// </summary>
    private void TimeToText(float time)
    {
        int rounded_time = (int)time;
        int minutes = (int)Mathf.Round(rounded_time / 60);
        int seconds = (int)Mathf.Round(rounded_time % 60);
        string time_str = "";
        //to create MM representation for the string
        time_str += (minutes / 10).ToString() + (minutes % 10).ToString();
        time_str += ":";
        //SS representation
        time_str += (seconds / 10).ToString() + (seconds % 10).ToString();
        _textMesh.text = time_str;

    }

    void Update()
    {
        if (_isSetup)
        {
            float deltaTime = Time.time - _curTime;
            _timeToGo = Mathf.Max(_timeToGo - deltaTime, 0.0f);
            TimeToText(_timeToGo);
            _curTime = Time.time;
            if (_timeToGo <= 0.0f)
            {
                EndTimer();
                // notify the minigame manager that the game was not completed in time
                MiniGameManager.Instance.EndGames();
            }
        }
    }
}
