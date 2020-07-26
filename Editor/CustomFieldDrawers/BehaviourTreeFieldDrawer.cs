using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using BeeTree;

namespace BeeTree.Editor {
	public class BehaviourTreeFieldDrawer : CustomFieldDrawer
	{
		public override Type FieldType => typeof(BehaviourTree);
		
		public override void Draw()
		{
			EditorGUILayout.LabelField("Behaviour Tree");
			TargetObject = EditorGUILayout.ObjectField(TargetObject, typeof(BehaviourTree), false);
		}

	}
}