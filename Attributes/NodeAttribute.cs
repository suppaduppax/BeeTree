using UnityEngine;
using System.Collections;
using System;

namespace BeeTree {
	public class NodeAttribute : Attribute {
		public string displayName { get; protected set; }

        public bool renameable = true;
		public bool createable = true;

		public NodeAttribute (string displayName) {
			this.displayName = displayName;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class NodeField : Attribute {
		public string label;
	}

	public abstract class VariableAttribute : NodeField
    {
    }

	[AttributeUsage(AttributeTargets.Field)]
	public class VariableGetter : VariableAttribute
	{
		public string overrideField;
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class VariableSetter : VariableAttribute
	{
    }

}