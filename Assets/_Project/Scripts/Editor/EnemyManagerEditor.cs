#if UNITY_EDITOR
using MarioTest.Enemies;
using UnityEditor;
using UnityEngine;

namespace MarioTest.Editor
{
    [CustomEditor(typeof(EnemyManager))]
    public sealed class EnemyManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var manager = (EnemyManager)target;

            EditorGUILayout.Space();
            if (GUILayout.Button("Refresh Enemies"))
            {
                manager.RefreshEnemyList();
                EditorUtility.SetDirty(manager);
            }
        }
    }
}
#endif
