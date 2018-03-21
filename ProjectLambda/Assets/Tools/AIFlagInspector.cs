using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(AIFlag), true)]
class AIFlagInspector : Editor {
    private readonly GUIStyle style = new GUIStyle();
    AIFlag aiflag;

    void OnEnable()
    {
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;

        aiflag = (AIFlag)target;
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
    void OnSceneGUI() {
        if (Application.isPlaying) return;

        Handles.color = aiflag.Colour;
        Handles.DrawWireArc(aiflag.transform.position, aiflag.transform.forward, -aiflag.transform.right, 360, aiflag.Radius);

        Handles.BeginGUI();

        Vector2 pos = HandleUtility.WorldToGUIPoint(aiflag.transform.position);
        GUI.DrawTexture(new Rect(pos.x - 10f, pos.y - 10f, 20f, 20f), aiflag.Icon, ScaleMode.ScaleToFit, true, 0, aiflag.Colour, Vector4.zero, 0);
        AISpawnerBox[] boxes = aiflag.transform.GetComponentsInChildren<AISpawnerBox>();

        foreach (AISpawnerBox box in boxes) {
            pos = HandleUtility.WorldToGUIPoint(box.transform.position);
            GUI.DrawTexture(new Rect(pos.x - 10f, pos.y - 10f, 20f, 20f), box.Icon, ScaleMode.ScaleToFit, true, 0, aiflag.Colour, Vector4.zero, 0);
        }

        Handles.EndGUI();
    }
}
#endif