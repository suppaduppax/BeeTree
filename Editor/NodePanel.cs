using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace BeeTree.Editor {
	[Serializable]
	public class NodePanel : IInputReceiver
	{
		public CanvasState canvasState;
		public int guid = int.MinValue;

		public Color bgColour = Color.white;
		public Color contentColour = Color.white;
		public float alpha = 1;

		public Texture icon;

		public List<int> childrenGuids;
		public int parentGuid = int.MinValue;

		public int nodeGuid;

		public CanvasTransform transform;

		public NodeHandle inHandle;
		public NodeHandle outHandle;

		public bool hasOuthandle = true;

		public bool isVisible = true;


		public string label { get { return Node.name; } }


		public Node Node {
			get
			{
				return canvasState.tree.GetNode(nodeGuid);
			}
		}

		public NodePanel Parent {
			get
            {
				if (parentGuid == int.MinValue)
                {
					return null;
                }

				return canvasState.GetNodePanel(parentGuid);
            }

			set
            {
				parentGuid = value == null ? int.MinValue : value.guid;
            }
		}

		public BehaviourTree Tree
        {
			get { return Node.Tree; }
        }

		public List<NodePanel> Children
        {
			get
            {
				List<NodePanel> children = new List<NodePanel>();
                for (int i = 0; i < childrenGuids.Count; i++)
                {
					children.Add(canvasState.GetNodePanel(childrenGuids[i]));
                }

				return children;
            }
        }

        int IInputReceiver.Priority
        {
			get { return BehaviourEditor.NODE_HANDLE_INPUT_PRIORITY; }
        }

        CanvasTransform IInputReceiver.Transform
        {
            get { return transform; }
        }

        public NodePanel (Node node, Rect rect, Rect inHandle, Rect outHandle, CanvasState canvasState)
		{
			this.nodeGuid = node.guid;
			this.guid = node.guid;
			this.canvasState = canvasState;

			this.transform = new CanvasTransform(node.Id + " NodePanel", rect, canvasState);

			this.inHandle = new NodeHandle(this, NodeHandle.HandleType.In, inHandle, canvasState);
			this.inHandle.transform.Parent = this.transform;

			this.outHandle = new NodeHandle(this, NodeHandle.HandleType.Out, outHandle, canvasState);
			this.outHandle.transform.Parent = this.transform;
			this.hasOuthandle = true;

			childrenGuids = new List<int>();
			this.canvasState.AddInputReceiver(this);
		}
        
		public NodePanel(Node node, Rect rect, Rect inHandle, CanvasState canvasState)
		{
			this.nodeGuid = node.guid;
			this.guid = node.guid;
			this.canvasState = canvasState;

			this.transform = new CanvasTransform(node.Id + " NodePanel", rect, canvasState);

			this.inHandle = new NodeHandle(this, NodeHandle.HandleType.In, inHandle, canvasState);
			this.inHandle.transform.Parent = this.transform;
			this.hasOuthandle = false;

			childrenGuids = new List<int>();
			this.canvasState.AddInputReceiver(this);
		}

		public void AddChild(NodePanel child) {
			if (child == null) {
				Debug.LogError("Cannot add null child!");
				return;
			}

			if (child.Parent != null && child.Parent != this) {
				child.Parent.RemoveChild(child);
			}

			child.Parent = this;
			childrenGuids.Add(child.guid);

			outHandle.Connect(child.inHandle);
		}


		public void SetParent(NodePanel parent) {
			if (parent != null) {
				parent.AddChild(this);
			}

			this.Parent = parent;

			this.transform.Parent = parent != null ? parent.transform : null;
		}

		public void RemoveChild(NodePanel child) {

			if (childrenGuids == null) {
				Debug.LogError("Cannot remove child, children is null!");
				return;
			}

			if (!childrenGuids.Contains(child.guid)) {
				Debug.LogError("Cannot remove child, the child nodePanel is not currently a child!");
				return;
			}

            outHandle.Disconnect(child.inHandle);
            childrenGuids.Remove(child.guid);

			child.Parent = null;
			child.transform.Parent = null;

			Node.RemoveChild(child.Node);
		}

		public void MoveChild(NodePanel child, int newIndex) {
			if (!childrenGuids.Contains(child.guid)) {
				if (child.Parent != null) {
					child.Parent.RemoveChild(child);
				}

				AddChild(child);
			}


			int oldIndex = childrenGuids.IndexOf(child.guid);
			if (newIndex > oldIndex)
				newIndex--;

			if (newIndex > childrenGuids.Count) {
				Debug.LogWarning("Cannot move child to index " + newIndex + ". It is out of range. Total children: " + childrenGuids.Count);
				return;
			}

			childrenGuids.RemoveAt(oldIndex);
			if (newIndex < childrenGuids.Count) {
				childrenGuids.Insert(newIndex, child.guid);
			} else {
				// add child to the end of the list
				childrenGuids.Add(child.guid);
			}
			
			Debug.Log("Moving child from " + oldIndex + " to " + newIndex);
		}

		/// <summary>
		/// sort children based on the x position
		/// returns true if order has changed
		/// </summary>
		public bool SortChildren()
		{
			List<int> oldGuids = new List<int>(childrenGuids);
			childrenGuids.Sort((x, y) => 
				canvasState.GetNodePanel(x).transform.position.x.CompareTo(canvasState.GetNodePanel(y).transform.position.x)
				);

			if (oldGuids != childrenGuids)
			{
				List<int> nodeChildren = new List<int>();
				for (int i = 0; i < childrenGuids.Count; i++)
				{
					nodeChildren.Add(canvasState.GetNodePanel(childrenGuids[i]).Node.guid);
				}

				Node.childrenGuids = nodeChildren;
			}
			
			return oldGuids != childrenGuids;
		}

		public void UpdateAllConnections()
		{
			outHandle.UpdateConnections();

			for (int i = 0; i < Children.Count; i++)
			{
				Children[i].UpdateAllConnections();
			}
		}

		public int IndexOfParent()
		{
			return Parent.childrenGuids.IndexOf(this.guid);
		}

		public void SetColours(Color content, Color bg, float alpha) {
			this.contentColour = content;
			this.bgColour = bg;
			this.alpha = alpha;
		}

		public void SetIcon(Texture icon) {
			this.icon = icon;
		}

		public NodePanel Clone(BehaviourTree tree, CanvasState state)

		{
			NodePanel clone = hasOuthandle ?
				new NodePanel(tree.GetNode(nodeGuid), transform.rect, inHandle.transform.rect, outHandle.transform.rect, state) :
				new NodePanel(tree.GetNode(nodeGuid), transform.rect, inHandle.transform.rect, state);

			clone.guid = guid;
			clone.childrenGuids = childrenGuids;
			clone.parentGuid = parentGuid;
			clone.transform = transform.Clone(state);
			clone.icon = icon;
			
			clone.inHandle = inHandle.Clone(state);
			clone.hasOuthandle = hasOuthandle;
			if (hasOuthandle)
			{
				clone.outHandle = outHandle.Clone(state);
			}

			return clone;
		}
    }

	
}