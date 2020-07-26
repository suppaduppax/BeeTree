using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeeTree;
using UnityEngine.TextCore;

namespace BeeTree {
	[Node("SetNull")]
	public class SetNull : LeafNode
	{
		public override string Id => "SetNull";

		[VariableSetter(label = "Output Variable")]
		public string variable;
		
		public override void Initialize()
		{
			base.Initialize();

			if (string.IsNullOrEmpty(variable))
			{
				Terminate(NodeState.Failure, "Variable was not set");
				return;
			}
			
			_tree.SetVariable(variable, null);
			
			Terminate(NodeState.Success, "Set variable to null");
		}
		
		public override Node Clone(BehaviourTree tree)
		{
			SetNull node = (SetNull)base.Clone(tree);
			node.variable = variable;
			return node;
		}
	}
}