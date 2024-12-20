using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Class handling the 8-puzzle game
/// </summary>
public class EightPuzzle : MiniGame
{
    [SerializeField] private List<GameObject> tileObjects = new List<GameObject>();
    [SerializeField] private GameObject emptyTile;
    [SerializeField] private GameObject numberSpawn;
    // at which number position is the empty tile 
    // passed as the number - 1 to form index
    [SerializeField] private int startEmptyIdx = 0;

    /// <summary>
    /// Just for debug purposes now to test the interaction. Will be removed.
    /// </summary>
    [SerializeField] private GameObject controllerObject;

    private List<Tile> _tiles = new List<Tile>();
    private int _emptyIdx;
    private Vector3 _emptyPosition;

    /// <summary>
    /// Code number
    /// </summary>
    private Vector3 _numberSpawnPos;
    private Quaternion _numberSpawnRotation;

    //for easy access into the list
    private Dictionary<int, int> _idxByNumber = new Dictionary<int, int>();
    //to remember idx in the current ordering
    private Dictionary<int, int> _orderIdxByNumber = new Dictionary<int, int>();
    //to check whether the board is in apropriate state
    private int[] _curOrder = new int[9];
    /// <summary>
    /// Debug
    /// </summary>
    private Dictionary<int, bool> _lastState = new();
    private Dictionary<int, bool> _scheduled = new();

    void Start()
    {
        // the empty tile represents number 9 in this version of 8-puzzle
        _curOrder[startEmptyIdx] = 9;
        _numberSpawnPos = numberSpawn.transform.position;
        _numberSpawnRotation = numberSpawn.transform.rotation;
        _orderIdxByNumber.Add(9, startEmptyIdx);
        int tile_idx = 0;
        for(int i = 0; i < tileObjects.Count; i++)
        {
            if(i == startEmptyIdx)
            {
                tile_idx++;
            }
            var t = tileObjects[i];
            var tile = t.GetComponent<Tile>();
            tile.SetPuzzle(this);
            _tiles.Add(tile);
            _idxByNumber.Add(tile.GetNumber(), i);
            _orderIdxByNumber.Add(tile.GetNumber(), tile_idx);
            _curOrder[tile_idx] = tile.GetNumber();
            tile_idx++;
        }
        _emptyIdx = startEmptyIdx;
        PrintCurOrder();
    }

    public Vector3 GetNumberSpawnPos()
    {
        return _numberSpawnPos;
    }

    /// <summary>
    /// Helper debug method to instantly complete the puzzle (only works in the initial configuration).
    /// </summary>
    public void CompleteThisThing()
    {
        //_emptyPosition = emptyTile.transform.position;
        //int one_idx = _idxByNumber[1];
        //int one_order_idx = _orderIdxByNumber[1]; 
        //int three_idx = _idxByNumber[3];
        //int three_order_idx = _orderIdxByNumber[3];

        //emptyTile.transform.position = tileObjects[three_idx].transform.position;
        //tileObjects[three_idx].transform.position = _emptyPosition;
        ////Perform the switch in cur order to check whether the board is in appropriate state
        //_curOrder[three_order_idx] = 9;
        //_orderIdxByNumber[9] = three_order_idx;
        //_curOrder[_emptyIdx] = 3;
        //_orderIdxByNumber[3] = _emptyIdx;
        //_emptyIdx = three_order_idx;
        //_emptyPosition = emptyTile.transform.position;

        //emptyTile.transform.position = tileObjects[one_idx].transform.position;
        //tileObjects[one_idx].transform.position = _emptyPosition;
        ////Perform the switch in cur order to check whether the board is in appropriate state
        //_curOrder[one_order_idx] = 9;
        //_orderIdxByNumber[9] = one_order_idx;
        //_curOrder[_emptyIdx] = 1;
        //_orderIdxByNumber[1] = _emptyIdx;
        //_emptyIdx = one_order_idx;
        //if (IsCorrect())
        //{
        //    // game sucesfully solved, give the player the code part and return the puzzle back down
        //    MiniGameManager.Instance.EndGame(MiniGameManager.MiniGameType.EightPuzzle, _numberSpawnPos);
        //}
        int[] winning_moves = { 6, 1, 8, 5, 2, 3, 4, 2, 1, 6, 3, 1, 2, 4, 1, 2, 5, 8 };
        foreach (int move in winning_moves) {
            TileActivated(move);
        }
    }

    private bool IsCorrect()
    {
        int correct_val = 1;
        foreach (int val in _curOrder) {
            if (val != correct_val) {
                return false;
            }
            correct_val++;
        }
        return true;
    }

    private void PrintCurOrder()
    {
       string print_str = "";
       foreach (int val in _curOrder)
       {
           print_str += val.ToString() + " ";
       }
       print(print_str);
    }

    public void TileActivated(int tileNumber)
    {
        _emptyPosition = emptyTile.transform.position;
        int idx = _idxByNumber[tileNumber];
        int order_idx = _orderIdxByNumber[tileNumber];
        int empty_row = _emptyIdx / 3;
        int empty_col = _emptyIdx % 3;
        int tile_row = order_idx / 3;
        int tile_col = order_idx % 3;
        int distance = Mathf.Abs(empty_row - tile_row) + Mathf.Abs(empty_col - tile_col);
        //The tiles that have a distance of one row/column 
        // from the curently empty tile can be moved
        if(distance == 1)
        {
            emptyTile.transform.position = tileObjects[idx].transform.position;
            tileObjects[idx].transform.position = _emptyPosition;
            //Perform the switch in cur order to check whether the board is in appropriate state
            _curOrder[order_idx] = 9;
            _orderIdxByNumber[9] = order_idx;
            _curOrder[_emptyIdx] = tileNumber;
            _orderIdxByNumber[tileNumber] = _emptyIdx;
            _emptyIdx = order_idx;
        }

        if (IsCorrect()) {
            // game sucesfully solved, give the player the code part and return the puzzle back down
            MiniGameManager.Instance.EndGame(MiniGameManager.MiniGameType.EightPuzzle, _numberSpawnPos, _numberSpawnRotation);
        }
        _tiles[idx].TurnOffOutline();
    }

    public override void StartGame()
    {
        base.StartGame();
    }

    public override void EndGame()
    {
        base.EndGame();
    }

    /// <summary>
    /// Debug function to artificially trigger the tile interaction
    /// </summary>
    /// <param name="context"></param>
    public void HandleTileActivatedPressed(InputAction.CallbackContext context)
    {
        int tile_num = int.Parse(context.control.name);
        if(!_lastState.ContainsKey(tile_num))
        {
            _lastState.Add(tile_num, false);
        }
        if (!_scheduled.ContainsKey(tile_num))
        {
            _scheduled.Add(tile_num, false);
        }
        bool pressed = false;
        if (context.performed)
        {
            pressed = true;
        }
        else if (context.started)
        {
            pressed = true;
        }
        else if (context.canceled)
        {
            pressed = false;
        }
        var tile = _tiles[_idxByNumber[tile_num]];
        Collider controller_collider = controllerObject.GetComponent<Collider>();
        if (pressed && !_lastState[tile_num])
        {
            if (!_scheduled[tile_num])
            {
                tile.Schedule(controller_collider);
            }
            else
            {
                tile.Unschedule(controller_collider);
            }
            _scheduled[tile_num] = !_scheduled[tile_num];
        }
        _lastState[tile_num] = pressed;

    }
}
