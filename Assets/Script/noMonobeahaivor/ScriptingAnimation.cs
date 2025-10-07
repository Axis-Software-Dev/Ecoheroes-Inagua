using System;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ActionsInIntervals
{
    [Tooltip("Starting from 0, the order in which scripts play")]
    public int Order;
    public UnityEvent Actions;
}
[CreateAssetMenu(fileName = "scriptingAnimation", menuName = "Scriptable Objects/scriptingAnimation")]
public class scriptingAnimation : ScriptableObject
{
    public float[] Intervals;

    public ActionsInIntervals[] listOfActions;
}

