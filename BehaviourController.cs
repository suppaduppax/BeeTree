using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the behaviour tree.
/// </summary>
namespace BeeTree {
	public class BehaviourController : MonoBehaviour
	{
		public BehaviourTree behaviourTree;
		
		private BehaviourTree _runtimeBehaviourTree;
		private Blackboard _blackboard;

		public BehaviourTree RuntimeBehaviourTree => _runtimeBehaviourTree;

		private void Start ()
        {
	        if (behaviourTree == null)
	        {
		        Debug.LogWarning("No Behaviour Tree set in Behaviour Controller", gameObject);
		        return;
	        }

	        _runtimeBehaviourTree = behaviourTree.Clone();
	        _blackboard = new Blackboard(this);
	        _runtimeBehaviourTree.Initialize(_blackboard);
	        
        }

		private void Update ()
	    {
		    if (_runtimeBehaviourTree != null)
		    {
			    _runtimeBehaviourTree.Tick();
		    }
	    }

		public void ReturnState(Node.NodeState state)
		{
			Debug.Log("Tree completed with state: " + state);
		}

	}
}