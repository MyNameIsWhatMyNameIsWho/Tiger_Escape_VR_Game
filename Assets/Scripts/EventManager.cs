using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;
using UnityEngine.XR;

/// <summary>
/// A static singleton that is used 
/// to store methods 
/// </summary>
public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    public delegate void SingleIntDelegate(int value);

    public int INVALID_INT_VALUE = int.MinValue;

    //string now for mocking purposes afterwards will be
    // InputFeatureUsage<bool>

    /// <summary>
    /// Need to store input/method for each method about to be executed
    /// </summary>
    private Dictionary<InputFeatureUsage<bool>, List<Tuple<int, SingleIntDelegate>>> oneIntEventsMap = new();

    void Awake()
    {
        if (Instance != null)
            Destroy(Instance);
        else
            Instance = this;
        print(Instance);

        DontDestroyOnLoad(this);
    }

    private void DummySingleInt(int value)
    {
        
    }

    /// <summary>
    /// Execute all methods queued for execution by the given input signal
    /// </summary>
    /// <param name="input"></param>
    public void InvokeByInput(InputFeatureUsage<bool> input)
    {
        print("invoking by " + input);
        if (oneIntEventsMap.TryGetValue(input, out List<Tuple<int, SingleIntDelegate>> result))
        {
            //print("Invoking actions triggered by " + input);
            //print(result[0]);
            for (int i = 0; i < result.Count; i++)
            {
                var inputAndMethod = result[i];
                //print(inputAndMethod);
                if (inputAndMethod.Item1 == INVALID_INT_VALUE)
                {
                    continue;
                }
                //print("Invoking");
                inputAndMethod.Item2.Invoke(inputAndMethod.Item1);
                result[i] = Tuple.Create<int, SingleIntDelegate>(INVALID_INT_VALUE, DummySingleInt);
            }
        }
    }
    /// <summary>
    /// Add a delegate function with a single int parameter and no return value for execution
    /// </summary>
    /// <param name="keyTrigger"> Input key which will set off the function </param>
    /// <param name="funcInput"> The integer used as function input. Do not use int.MinValue as 
    /// a function input as that is used to recognize empty positions in the list.</param>
    /// <param name="func"> The function </param>
    /// <returns> Index at which the function was stored. Can be used to remove the function</returns>
    public int AddSingleIntFunc(InputFeatureUsage<bool> keyTrigger, int funcInput, SingleIntDelegate func)
    {
        //print("Adding " + funcInput + " " + func);
        int add_idx = 0;
        if(!oneIntEventsMap.ContainsKey(keyTrigger))
        {
            oneIntEventsMap.Add(keyTrigger, new());
        }
        var list = oneIntEventsMap[keyTrigger];
        bool found = false;
        for(int i = 0; i < list.Count; i++)
        {
            //if the argument is int.MinValue the place is free and we
            // can put the function there
            if(list[i].Item1 == INVALID_INT_VALUE)
            {
                add_idx = i;
                list[i] = Tuple.Create(funcInput, func);
                found = true;
                break;
            }
        }
        //if no empty place was found put the function at the end of the list
        if (!found) {
            add_idx = list.Count;
            list.Add(Tuple.Create(funcInput, func));
            //print("Inserting new " + funcInput + " " + func + " at " + add_idx + " trigger " + keyTrigger);
        }
        //print(list);
        return add_idx;
    }
    /// <summary>
    /// Clears a previously prepared delegate with a single int
    /// parameter to not execute it.
    /// </summary>
    /// <param name="index"> The int index returned by a previous AddSingleIntFunc call.</param>
    /// <param name="keyTrigger"> Input key which sets off the function. </param>
    public void RemoveSingleIntFunc(int index, InputFeatureUsage<bool> keyTrigger)
    {
        //print("Removing at index " + index);
        if (oneIntEventsMap.TryGetValue(keyTrigger, out List<Tuple<int, SingleIntDelegate>> result))
        {
            if (index < 0 || index >= oneIntEventsMap.Count) {
                return;
            }
            result[index] = Tuple.Create<int, SingleIntDelegate>(INVALID_INT_VALUE, DummySingleInt);

        }
    }

}
