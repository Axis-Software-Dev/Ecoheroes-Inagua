using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CreditsSetup))]
public class CreditsSetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CreditsSetup setup = (CreditsSetup)target;

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Create Credits UI", GUILayout.Height(40)))
        {
            setup.CreateCreditsUI();
        }

        EditorGUILayout.HelpBox(
            "Click the button above to automatically create the credits UI below the Iso A object.",
            MessageType.Info
        );
    }
}
