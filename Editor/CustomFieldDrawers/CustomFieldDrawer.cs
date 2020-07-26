using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace BeeTree.Editor {
	public abstract class CustomFieldDrawer
	{
		public abstract Type FieldType
        {
			get;
        }

		public Node targetNode;
		public FieldInfo targetField;

		protected UnityEngine.Object TargetObject
		{
			get { return (UnityEngine.Object)targetField.GetValue(targetNode); }
			set { targetField.SetValue(targetNode, value);}
		}
		
		
	    public virtual void Draw() { }
	}
}