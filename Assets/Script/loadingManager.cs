using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class loadingManager : MonoBehaviour

{
    persistanceData persistanceScene;
    AsyncOperation async;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        persistanceScene = Resources.Load<persistanceData>("persistanceData");
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(loadPersistanceScene());
    }
    IEnumerator loadPersistanceScene()
    {
        if (async == null) {
            async = SceneManager.LoadSceneAsync(persistanceScene.sceneToLoad);
            async.allowSceneActivation = false;

        }
        while (!async.isDone)
        {
            if (async.progress>=0.9f) {
                
                async.allowSceneActivation=true;
		
            }
	yield return null;
        }
        
    }
}
