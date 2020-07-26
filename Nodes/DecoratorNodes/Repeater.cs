using System.Collections;
using System.Collections.Generic;
using BeeTree;
using UnityEngine;

namespace PocketGuild {
	[Node("Repeater")]
	public class Repeater : DecoratorNode
	{

		public override string Id => "Repeater";

		public override bool TickEnabled
		{
			get { return _tickEnabled; }
		}

		[NodeField(label = "Until Successful")]
		public bool untilSuccessful = false;
		
		private bool _repeatOnNextTick = false;
		private bool _tickEnabled = false;

		public override void Initialize()
		{
			base.Initialize();
			Repeat();
		}

		private void Repeat()
		{
			_childrenQueue = new List<Node>(Children);
			_tickEnabled = false;
			ChangeState(NodeState.Running);

			GetNextChild().Initialize();
		}

		public override void Tick()
		{
			if (_repeatOnNextTick)
			{
				_repeatOnNextTick = false;
				Repeat();
			}
		}

		public override void ReturnState(NodeState state)
		{
			Terminate(state, null);
		}

		public override void Terminate(NodeState state, string message)
		{
			if (untilSuccessful)
			{
				Print( "SUCCESS REPEAT!", PRINT_COLOUR_MAGENTA);
				if (state == NodeState.Success)
				{
					StateMessage = "Child Successful, returning success";
					_tree.RemoveRunningNode(this);
					ChangeState(NodeState.Success);

					if (Parent != null)
					{
						Parent.ReturnState(NodeState.Success);
					}
					
					return;
				}
			}

			// wait for next tick to repeat
			_tickEnabled = true;
			_repeatOnNextTick = true;
		}

		public override Node Clone(BehaviourTree tree)
		{
			Repeater clone = (Repeater)base.Clone(tree);
			clone.untilSuccessful = untilSuccessful;
			return clone;
		}
	}
}