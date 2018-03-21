using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(AISpawnerBox), true)]
public class AISpawnerBoxInspector : Editor {
    AISpawnerBox aibox;

    void OnEnable()
    {
        aibox = (AISpawnerBox)target;
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
    void OnSceneGUI()
    {
        if (Application.isPlaying) return;

        AIFlag parent = aibox.transform.parent.GetComponent<AIFlag>();
        if (parent == null) return;

        Handles.color = parent.Colour;
        Handles.DrawWireArc(parent.transform.position, parent.transform.forward, -parent.transform.right, 360, parent.Radius);

        Handles.BeginGUI();

        Vector2 pos = HandleUtility.WorldToGUIPoint(parent.transform.position);
        GUI.DrawTexture(new Rect(pos.x - 10f, pos.y - 10f, 20f, 20f), parent.Icon, ScaleMode.ScaleToFit, true, 0, parent.Colour, Vector4.zero, 0);

        pos = HandleUtility.WorldToGUIPoint(aibox.transform.position);
        GUI.DrawTexture(new Rect(pos.x - 10f, pos.y - 10f, 20f, 20f), aibox.Icon, ScaleMode.ScaleToFit, true, 0, parent.Colour, Vector4.zero, 0);

        Handles.EndGUI();
    }
}
#endif
