using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeeTree
{
	[Node("IsNull")]
	public class IsNull : LeafNode
	{
		public override string Id => "IsNull";

		[VariableGetter(label = "Variable")]
		public string variable;

        public override void Initialize()
        {
            base.Initialize();

            if (string.IsNullOrEmpty(variable))
            {
                Terminate(NodeState.Failure, "Variable was not set");
                return;
            }

            if (!_tree.HasVariable(variable))
            {
                Terminate(NodeState.Failure, "Variable could not be found");
                return;
            }


            if (_tree.GetVariable(variable) == null)
            {
                Terminate(NodeState.Success, "Variable was null");
            }
            else
            { 
                Terminate(NodeState.Failure, "Variable was not null");
            }
        }

        public override Node Clone(BehaviourTree tree)
        {
            IsNull node = (IsNull)base.Clone(tree);
			node.variable = variable;

			return node;
        }

    }
}