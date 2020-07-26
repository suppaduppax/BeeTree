using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BeeTree.Editor {
	public class CustomNodePanelAttribute : Attribute
	{
		public Type nodeType;

		public CustomNodePanelAttribute (Type nodeType)
        {
			this.nodeType = nodeType;
        }
	}
}