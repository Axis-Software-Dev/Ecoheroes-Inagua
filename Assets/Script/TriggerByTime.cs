using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TriggerByTime : MonoBehaviour
{
    [Range(0f, 10f)] 
    public float delayInSeconds;
    public UnityEvent eventToExecute;

    private const float MIN_DELAY = 0.1f;

    private void Start()
    {
        TriggerEvent(eventToExecute, delayInSeconds);
    }

    public void TriggerEvent(UnityEvent eventToRun, float delay)
    {
        StartCoroutine(DelayAndExecute(eventToRun, delay));
    }

    private IEnumerator DelayAndExecute(UnityEvent eventToInvoke, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (eventToInvoke != null && delay > MIN_DELAY)
        {
            eventToInvoke.Invoke();
        }
    }
}
