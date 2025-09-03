using UnityEngine;
using UnityEngine.SceneManagement;

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
    Character character;
    public SceneField sceneToLoad;
    public void changeCharacter(Character newSelection) {

        character = newSelection;
    }
    public void changeSceneToLoad(SceneField scene)
    {
        sceneToLoad = scene;
    }

}
