using UnityEngine;

public class ShowCharacter : MonoBehaviour
{
    public SkinnedMeshRenderer aguita;
    public SkinnedMeshRenderer lluvia;
    private Animator lluviaAnimator;
    private persistanceData persistanceData;
    private void Awake()
    {
        persistanceData = Resources.Load<persistanceData>("persistanceData");
        lluvia.enabled = false;
        lluviaAnimator = lluvia.GetComponentInParent<Animator>();
        lluviaAnimator.enabled = false;
        aguita.enabled = false;
    }
    public void showCharacters()
    {
        switch (persistanceData.getSelectedCharacter().ToLower())
        {
            case "aguita":
                
                aguita.enabled = true;
                
                break;
            case "lluvia":
                lluvia.enabled = true;
                lluviaAnimator.enabled = true;
                break;
            default:
                Debug.Log("No character selected");
                break;
        }
        
        
    }


}
