using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeeTree {
	[Node("ImportTree")]
	public class ImportTree : LeafNode
	{
		public override string Id => "ImportTree";

		[NodeField(label = "Behaviour Tree")]
		public BehaviourTree behaviourTree;

		private BehaviourTree _runtimeBehaviourTree;
		
		public override void Initialize()
		{
			base.Initialize();

			if (behaviourTree == null)
			{
				Terminate(NodeState.Failure, "Behaviour tree is null");
				return;
			}

			if (_runtimeBehaviourTree == null)
			{
				_runtimeBehaviourTree = behaviourTree.Clone();
			}
			
			_runtimeBehaviourTree.Initialize(this, _tree.Blackboard);
		}

		public override Node Clone(BehaviourTree tree)
		{
			ImportTree node = (ImportTree)base.Clone(tree);
			node.behaviourTree = behaviourTree;
			return node;
		}
	}
}