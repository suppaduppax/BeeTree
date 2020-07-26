using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BeeTree.Editor {
    [CustomEditor(typeof(BehaviourTree))]
	public class BehaviourTreeInspector : UnityEditor.Editor
	{
        private BehaviourTree _tree;

        private void OnEnable()
        {
            _tree = (BehaviourTree)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.BeginVertical();
            foreach (var i in _tree.GuidToNodeDict)
            {
                EditorGUILayout.LabelField(i.Key.ToString());
            }
            EditorGUILayout.EndVertical();

        }
    }
}