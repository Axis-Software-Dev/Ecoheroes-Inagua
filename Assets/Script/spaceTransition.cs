using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.SceneManagement;
using System.Collections;
public class spaceTransition : MonoBehaviour
{
    private XRGrabInteractable grabInteraction;
    sceneManager Manager;
    public SceneField nextScene;
    AsyncOperation async;
    GameObject cilinderLight;
    public float maxCylinderHigh=1.25f, minCylinderHigh=0f;
    bool upOrDown=false, playCilinderAnimation=false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        grabInteraction = GetComponent<XRGrabInteractable>();
        cilinderLight = GameObject.Find("cilinderLight");
    }
    private void Update()
    {
        if (playCilinderAnimation)
        {
            playAnimation();
            Invoke("startSceneLoading", 3f);
        }
        
    }
    private void OnEnable()
    {
        grabInteraction.selectEntered.AddListener(onGrab);
        grabInteraction.selectExited.AddListener(onLetGo);
    }
    void onGrab(SelectEnterEventArgs args)
    {
        playCilinderAnimation = true;
    }
    void onLetGo(SelectExitEventArgs args)
    {

    }
    IEnumerator loadPersistanceScene()
    {
        if (async == null)
        {
            async = SceneManager.LoadSceneAsync(nextScene);
            async.allowSceneActivation = false;

        }
        while (!async.isDone)
        {
            if (async.progress >= 0.9f)
            {

                async.allowSceneActivation = true;

            }
            yield return null;
        }

    }
	
	
    void startSceneLoading(){
        StartCoroutine(loadPersistanceScene());

    }
    void playAnimation()
    {

        if (cilinderLight.transform.position.y>=maxCylinderHigh)
        {
            upOrDown = true;
        }else if (cilinderLight.transform.position.y <= minCylinderHigh)
        {
            upOrDown = false;
        }

        if (!upOrDown)
        {
            cilinderLight.transform.position = new Vector3(cilinderLight.transform.position.x, cilinderLight.transform.position.y + 0.05f, cilinderLight.transform.position.z);
        }
        else
        {
            cilinderLight.transform.position = new Vector3(cilinderLight.transform.position.x, cilinderLight.transform.position.y - 0.05f, cilinderLight.transform.position.z);

        }
    }

}
