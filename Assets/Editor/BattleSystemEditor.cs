using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BattleSystem))]
public sealed class BattleSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Battle Test Tools", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("Test Random Battle"))
                InvokeOnTargets(battleSystem => battleSystem.TestStartRandomBattle());

            if (GUILayout.Button("Close Battle Without Consequences"))
                InvokeOnTargets(battleSystem => battleSystem.CloseBattleWithoutConsequences());
        }

        if (!Application.isPlaying)
            EditorGUILayout.HelpBox("Battle test buttons are available in Play Mode.", MessageType.Info);
    }

    private void InvokeOnTargets(System.Action<BattleSystem> action)
    {
        for (var i = 0; i < targets.Length; i++)
        {
            if (targets[i] is BattleSystem battleSystem)
                action.Invoke(battleSystem);
        }
    }
}
