using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneManager : MonoBehaviour
{
    persistanceData persistanceData;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    void Start()
    {
        persistanceData = Resources.Load<persistanceData>("persistanceData");   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void loadingScene(SceneField sceneLoaded) 
    {
        persistanceData.changeSceneToLoad(sceneLoaded);
        SceneManager.LoadSceneAsync("LoadingScene");
    
    }




}
