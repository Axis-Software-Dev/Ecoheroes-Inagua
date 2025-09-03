using UnityEngine;
using UnityEngine.SceneManagement;

public class characterSelector : MonoBehaviour
{
    persistanceData characterData;
    bool modelRotation=false;
    persistanceData.Character selectedCharacter = persistanceData.Character.none;
    public SceneField newScene;
    GameObject[] showCharacterModels = new GameObject[2];
    Transform[] modelBasePosition = new Transform[2];
    float timeForRotation = 0f;
    sceneManager sceneManagerEdit;
    public Scene sceneHolder;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        characterData = Resources.Load<persistanceData>("persistanceData");
        showCharacterModels[0] = GameObject.Find("LluviaSelection 1");
        showCharacterModels[1] = GameObject.Find("AguitaSelection 1");
        sceneManagerEdit = GameObject.Find("sceneManager").GetComponent<sceneManager>();
        modelBasePosition[0]=showCharacterModels[0].transform;
        modelBasePosition[1] = showCharacterModels[1].transform;

    }
    void Start()
    {
        showCharacterModels[0].SetActive(false);
        showCharacterModels[1].SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (modelRotation)
        {
            if (timeForRotation <= 2)
            {
                if (selectedCharacter == persistanceData.Character.lluvia)
                {
                    rotateModel(showCharacterModels[0]);
                }
                else if (selectedCharacter == persistanceData.Character.aguita)
                {
                    rotateModel(showCharacterModels[1]);
                }
            }
            else
            {
                modelRotation = false;
               
            }
        }
    }
    void selectCharacter()
    {
        if (selectedCharacter == persistanceData.Character.lluvia)
        {
            showCharacterModels[0].SetActive(true);
           
            showCharacterModels[1].SetActive(false);
        }
        else if (selectedCharacter == persistanceData.Character.aguita)
        {
            showCharacterModels[0].SetActive(false);
            showCharacterModels[1].SetActive(true);
        }
        if (selectedCharacter == persistanceData.Character.lluvia)

        showCharacterModels[0].transform.rotation = Quaternion.Euler(0f, -180f, 0f);
        showCharacterModels[1].transform.rotation = Quaternion.Euler(0f,-180f,0f);
        
        activateRotation();
        
    }
    public void lluviaSelected()
    {
        selectedCharacter= persistanceData.Character.lluvia;
        selectCharacter();
    }
    public void aguitaSelected()
    {
        selectedCharacter = persistanceData.Character.aguita;
        selectCharacter();
    }
    public void gameStart()
    {
        characterData.changeCharacter(selectedCharacter);
        if (selectedCharacter != persistanceData.Character.none)
        {
            
            sceneManagerEdit.loadingScene(newScene);        }
    }
    void rotateModel(GameObject objectToRotate)
    {

        objectToRotate.transform.rotation = Quaternion.Slerp(objectToRotate.transform.rotation, Quaternion.Euler(0f, objectToRotate.transform.rotation.y + 360f, 0f), 2f * Time.deltaTime);
        timeForRotation += Time.deltaTime;
    }
    void activateRotation()
    {
        timeForRotation = 0f;
        modelRotation = true;
    }
 }
