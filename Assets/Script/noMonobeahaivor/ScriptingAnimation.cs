using System;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ActionsInIntervals
{
    [Tooltip("Starting from 0, the order in which scrpts play")]
    public int Order;
    public UnityEvent Actions;
}
[CreateAssetMenu(fileName = "scriptingAnimation", menuName = "Scriptable Objects/scriptingAnimation")]
public class scriptingAnimation : ScriptableObject
{
    [Tooltip("A sorted list of timestamps (seconds). Intervals are read as ranges [interval[i], interval[i+1]). The last element is treated as the final stop threshold.")]
    public float[] Intervals;
    [Tooltip("The actions made on each time interval")]
    public ActionsInIntervals[] listOfActions;
    [Tooltip("Target positions (world-space by default). If PositionsAreLocal is true, these are local to PositionsReference.")]
    public Vector3[] toFollowPosition;
}

