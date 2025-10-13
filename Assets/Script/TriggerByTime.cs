using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TriggerByTime : MonoBehaviour
{
    [Range(0f, 10f)] public float delayInSeconds;
    public UnityEvent eventToExecute;
    void Start()
    {
        TriggerEvent(eventToExecute, delayInSeconds);
    }

    public void TriggerEvent(UnityEvent eventToRun, float d)
    {
        StartCoroutine(DelayAndExecute(eventToRun, d));
    }
    IEnumerator DelayAndExecute(UnityEvent e, float d)
    {
        yield return new WaitForSeconds(d);
        if (e != null && d > .1f) eventToExecute.Invoke();
    }
}
