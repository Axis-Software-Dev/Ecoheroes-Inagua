using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelector : MonoBehaviour
{
    [Header("Character Prefabs")]
    public GameObject lluviaPrefab;
    public GameObject aguitaPrefab;

    [Header("Spawn Position")]
    public Transform spawnPoint;

    private persistanceData characterData;
    private persistanceData.Character selectedCharacter = persistanceData.Character.none;
    private GameObject currentCharacterInstance;

    private bool modelRotation = false;
    private float timeForRotation = 0f;

    private void Awake()
    {
        characterData = Resources.Load<persistanceData>("persistanceData");
    }

    private void Update()
    {
        if (modelRotation && currentCharacterInstance != null)
        {
            if (timeForRotation <= 2f)
            {
                RotateModel(currentCharacterInstance);
            }
            else
            {
                modelRotation = false;
            }
        }
    }

    private void SelectCharacter()
    {
        // Destroy the currently displayed character
        if (currentCharacterInstance != null)
        {
            Destroy(currentCharacterInstance);
        }

        // Choose which prefab to instantiate
        GameObject prefabToInstantiate = null;
        if (selectedCharacter == persistanceData.Character.lluvia)
        {
            prefabToInstantiate = lluviaPrefab;
        }
        else if (selectedCharacter == persistanceData.Character.aguita)
        {
            prefabToInstantiate = aguitaPrefab;
        }

        // Instantiate the chosen character
        if (prefabToInstantiate != null && spawnPoint != null)
        {
            currentCharacterInstance = Instantiate(prefabToInstantiate, spawnPoint.position + new Vector3(0f, 0.78f, 0f), spawnPoint.rotation);

            // Trigger the "selected" animation
            Animator anim = currentCharacterInstance.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("selected");
            }

            ActivateRotation();
        }
    }

    public void LluviaSelected()
    {
        selectedCharacter = persistanceData.Character.lluvia;
        SelectCharacter();
    }

    public void AguitaSelected()
    {
        selectedCharacter = persistanceData.Character.aguita;
        SelectCharacter();
    }

    public void GameStart()
    {
        characterData.changeCharacter(selectedCharacter);
        if (selectedCharacter != persistanceData.Character.none)
        {
            GameObject.Find("SceneManager").GetComponent<LoadingScreen>().LoadScene(1);
        }
    }

    private void RotateModel(GameObject objectToRotate)
    {
        objectToRotate.transform.Rotate(Vector3.up, 100f * Time.deltaTime);
        timeForRotation += Time.deltaTime;
    }

    private void ActivateRotation()
    {
        timeForRotation = 0f;
        modelRotation = true;
    }
}
