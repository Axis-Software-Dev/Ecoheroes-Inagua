using System.Collections.Generic;
using UnityEngine;

public class CarPool : MonoBehaviour
{
    private Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<GameObject>();

        GameObject obj;
        if (pools[prefab].Count > 0)
        {
            obj = pools[prefab].Dequeue();
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab, position, rotation);
        }

        return obj;
    }

    public void Return(GameObject obj, float delay)
    {
        StartCoroutine(ReturnDelayed(obj, delay));
    }

    private System.Collections.IEnumerator ReturnDelayed(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);

        foreach (var kvp in pools)
        {
            if (obj.name.Contains(kvp.Key.name))
            {
                kvp.Value.Enqueue(obj);
                break;
            }
        }
    }
}
