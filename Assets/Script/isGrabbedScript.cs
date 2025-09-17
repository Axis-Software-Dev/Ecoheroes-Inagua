using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.SceneManagement;

public class isGrabbedScript : MonoBehaviour
{
    private XRGrabInteractable grabInteraction;
    sceneManager Manager;
    public SceneField nextScene;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        grabInteraction=GetComponent<XRGrabInteractable>();
        Manager = GameObject.Find("sceneManager").GetComponent<sceneManager>();
    }
    private void OnEnable()
    {
        grabInteraction.selectEntered.AddListener(onGrab);
        grabInteraction.selectExited.AddListener(onLetGo);
    }
    void onGrab(SelectEnterEventArgs args) {
        Manager.loadingScene(nextScene);
    }
    void onLetGo(SelectExitEventArgs args)
    {

    }

}
