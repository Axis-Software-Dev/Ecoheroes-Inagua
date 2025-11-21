using UnityEngine;

public class ShowCharacter : MonoBehaviour
{
    public SkinnedMeshRenderer aguita;
    public SkinnedMeshRenderer lluvia;

    private Animator lluviaAnimator;
    private Animator aguitaAnimator;
    private persistanceData persistanceData;

    private void Awake()
    {
        persistanceData = Resources.Load<persistanceData>("persistanceData");

        if (lluvia != null)
        {
            lluvia.enabled = false;
            lluviaAnimator = lluvia.GetComponentInParent<Animator>();
            if (lluviaAnimator != null)
            {
                lluviaAnimator.enabled = false;
            }
        }

        if (aguita != null)
        {
            aguita.enabled = false;
            aguitaAnimator = aguita.GetComponentInParent<Animator>();
            if (aguitaAnimator != null)
            {
                aguitaAnimator.enabled = false;
            }
        }
    }

    public void showCharacters()
    {
        if (persistanceData == null)
        {
            Debug.LogWarning("No persistence data found.");
            return;
        }

        string selectedCharacter = persistanceData.getSelectedCharacter()?.ToLower();

        switch (selectedCharacter)
        {
            case "aguita":
                if (aguita != null)
                {
                    aguita.enabled = true;
                }
                if (aguitaAnimator != null)
                {
                    aguitaAnimator.enabled = true;
                }
                break;
            case "lluvia":
                if (lluvia != null)
                {
                    lluvia.enabled = true;
                }
                if (lluviaAnimator != null)
                {
                    lluviaAnimator.enabled = true;
                }
                break;
            default:
                Debug.Log("No character selected");
                break;
        }
    }
}
