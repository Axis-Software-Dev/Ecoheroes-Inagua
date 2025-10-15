using UnityEngine;

public class ShowCharacter : MonoBehaviour
{
    public MeshRenderer aguita;
    public MeshRenderer lluvia;

    private persistanceData persistanceData;
    private void Awake()
    {
        persistanceData = Resources.Load<persistanceData>("persistanceData");
        lluvia.enabled = false;
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
                break;
            default:
                Debug.Log("No character selected");
                break;
        }
        
        
    }


}
