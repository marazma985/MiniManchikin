using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DiceRollAnimationPlayer))]
public sealed class DiceRollAnimationPlayerEditor : Editor
{
    private SerializedProperty clipsResourcePath;
    private SerializedProperty rollClips;
    private SerializedProperty boardAnimation;
    private SerializedProperty battleAnimation;
    private SerializedProperty escapeAnimation;
    private SerializedProperty fallbackDisplaySeconds;

    private void OnEnable()
    {
        clipsResourcePath = serializedObject.FindProperty("clipsResourcePath");
        rollClips = serializedObject.FindProperty("rollClips");
        boardAnimation = serializedObject.FindProperty("boardAnimation");
        battleAnimation = serializedObject.FindProperty("battleAnimation");
        escapeAnimation = serializedObject.FindProperty("escapeAnimation");
        fallbackDisplaySeconds = serializedObject.FindProperty("fallbackDisplaySeconds");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(clipsResourcePath);
        EditorGUILayout.PropertyField(rollClips, true);
        EditorGUILayout.PropertyField(fallbackDisplaySeconds);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Rebuild Animation Clips"))
                DiceRollAnimationAssetBuilder.Rebuild();
        }

        DrawAnimationBlock(boardAnimation, "Test Board Animation", player => player.TestBoardAnimation());
        DrawAnimationBlock(battleAnimation, "Test Battle Animation", player => player.TestBattleAnimation());
        DrawAnimationBlock(escapeAnimation, "Test Escape Animation", player => player.TestEscapeAnimation());

        serializedObject.ApplyModifiedProperties();

        if (!Application.isPlaying)
            EditorGUILayout.HelpBox("Preview buttons are available in Play Mode and do not change game state.", MessageType.Info);
    }

    private void DrawAnimationBlock(SerializedProperty property, string buttonLabel, System.Action<DiceRollAnimationPlayer> action)
    {
        EditorGUILayout.Space(6f);
        EditorGUILayout.PropertyField(property, true);

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button(buttonLabel))
            {
                serializedObject.ApplyModifiedProperties();
                InvokeOnTargets(action);
            }
        }
    }

    private void InvokeOnTargets(System.Action<DiceRollAnimationPlayer> action)
    {
        for (var i = 0; i < targets.Length; i++)
        {
            if (targets[i] is DiceRollAnimationPlayer player)
                action.Invoke(player);
        }
    }
}
