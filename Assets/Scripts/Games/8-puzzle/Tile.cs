using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;
/// <summary>
/// Class handling single tile of the 8-puzzle game
/// </summary>
public class Tile : MonoBehaviour, Interactable
{
    [SerializeField] private int number;
    private EightPuzzle _puzzle;
    private bool _initialized = false;
    /// <summary>
    /// Used to remember whether the tile was scheduled for interaction
    /// </summary>
    private int _eventIndex = -1;

    private Outline _objectOutline;
    private bool _usingOutline = false;

    private void Start()
    {
        _objectOutline = gameObject.GetComponent<Outline>();
    }
    public void SetPuzzle(EightPuzzle puzzle)
    {
        _puzzle = puzzle;
        _initialized = true;
    }
    public int GetNumber()
    {
        return number;
    }

    public void TurnOffOutline()
    {
        if (_usingOutline)
        {
            _objectOutline.enabled = false;
            _usingOutline = false;
        }
    }
    public void TurnOnOutline() {
        if (!_usingOutline)
        {
            _objectOutline.enabled = true;
            _usingOutline = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(_initialized && other.gameObject.CompareTag("GameController"))
        {
            //schedule the signal to be send to the puzzle
            //if (_eventIndex == -1) {
            //_eventIndex = EventManager.Instance.AddSingleIntFunc(CommonUsages.gripButton, number, _puzzle.TileActivated);
            //}
            TurnOnOutline();
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_eventIndex > -1)
        {
            EventManager.Instance.RemoveSingleIntFunc(_eventIndex, CommonUsages.gripButton);
            _eventIndex = -1;
        }
        TurnOffOutline();
    }

    /// Debug methods to simulate OnTriggerEnter and OnTriggerExit

    public void Schedule(Collider other)
    {
        if (_initialized && other.gameObject.CompareTag("GameController"))
        {
            //schedule the signal to be send to the puzzle
            if (_eventIndex == -1)
            {
                //print("Adding event from tile " + number);
                _eventIndex = EventManager.Instance.AddSingleIntFunc(CommonUsages.gripButton, number, _puzzle.TileActivated);
            }
            TurnOnOutline();
        }
    }

    public void Unschedule(Collider other)
    {
        if (_eventIndex > -1)
        {
            EventManager.Instance.RemoveSingleIntFunc(_eventIndex, CommonUsages.gripButton);
            _eventIndex = -1;
        }
        TurnOffOutline();
    }

    public void Grabbed(GameObject controller, GameObject grabbedObject)
    {
        _puzzle.TileActivated(number);
    }

    public void Dropped()
    {
        
    }

}
