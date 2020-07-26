using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeeTree {
	public class Blackboard
	{
		//List<>
		private BehaviourController _controller;

		private Dictionary<string, object> _variables;

		public BehaviourController BehaviourController
		{
			get { return _controller; }
		}

		public Blackboard(BehaviourController controller)
        {
			_controller = controller;

			_variables = new Dictionary<string, object>();
        }

		public void SetVariable(string name, object value)
		{
			if (name == null)
			{
				throw new System.Exception("BehaviourTree.GetVariable: variable name cannot be null.");
			}

			if (_variables.ContainsKey(name))
			{
				_variables[name] = value;
			}
			else
			{
				_variables.Add(name, value);
			}
		}

		public object GetVariable(string name)
		{
			if (name == null)
			{
				throw new System.Exception("Blackboard.GetVariable: variable name cannot be null.");
			}

			if (_variables.ContainsKey(name))
			{
				return _variables[name];
			}
			else
			{
				throw new System.Exception("Blackboard.GetVariable: Cannot find variable: " + name);
			}
		}

		public bool HasVariable(string name)
		{
			if (name == null)
			{
				throw new System.Exception("Blackboard.HasVariable: variable name cannot be null.");
			}

			return _variables.ContainsKey(name);
		}

		public void DeleteVariable(string name)
        {
			if (name == null)
			{
				throw new System.Exception("Blackboard.GetVariable: variable name cannot be null.");
			}

			if (!_variables.ContainsKey(name))
			{
				throw new System.Exception("Blackboard.DeleteVariable: cannot delete variable, it doesn't exist: " + name);
			}

			_variables.Remove(name);
		}
	}
}