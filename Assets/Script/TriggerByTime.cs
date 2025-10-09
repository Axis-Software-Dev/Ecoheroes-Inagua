using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TriggerByTime : MonoBehaviour
{
    [Range(0f, 10f)] public float delayInSeconds = 0;
    public UnityEvent eventToExecute;
    void Start()
    {
        if (eventToExecute != null) StartCoroutine(DelayAndExecute());
    }

    IEnumerator DelayAndExecute()
    {
        yield return new WaitForSeconds(delayInSeconds);
        if (eventToExecute != null) eventToExecute.Invoke();
    }
}
