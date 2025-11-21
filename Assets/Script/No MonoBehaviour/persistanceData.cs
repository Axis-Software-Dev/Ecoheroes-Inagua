using UnityEngine;

[CreateAssetMenu(fileName = "persistanceData", menuName = "Scriptable Objects/persistanceData")]
public class persistanceData : ScriptableObject
{
    public enum Character
    {
        lluvia,
        aguita,
        none
    }

    [SerializeField]
    private Character character;
    
    public SceneField sceneToLoad;

    public void changeCharacter(Character newSelection)
    {
        character = newSelection;
    }

    public void changeSceneToLoad(SceneField scene)
    {
        sceneToLoad = scene;
    }

    public string getSelectedCharacter()
    {
        return character.ToString();
    }
}
