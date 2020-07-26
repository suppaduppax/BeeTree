using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using System.IO;

using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

using System.Linq;
/*
*		Place anything that needs to be serialized here related to the node canvas editor.
* 
*/

using BeeTree.Serialization;
using System;

namespace BeeTree.Editor
{
	public class CanvasState : ScriptableObject
	{
		public BehaviourTree tree;

		public Rect canvasRect;

		public List<NodePanel> nodePanels = new List<NodePanel>();

		public float zoom = 1;
		public Vector2 zoomPos { get { return canvasRect.size / 2; } }
		public Vector2 panOffset = Vector2.zero;

		[SerializeField] private int _rootNodePanelGuid = int.MinValue;
		[SerializeField] private int _canvasTransformGuidCount = int.MinValue;

		private List<IInputReceiver> _inputReceivers = new List<IInputReceiver>();
		private List<CanvasTransform> _canvasTransforms = new List<CanvasTransform>();

		private Dictionary<Node, NodePanel> _nodeToNodePanelDict = new Dictionary<Node, NodePanel>();
		private Dictionary<int, NodePanel> _guidToNodePanelDict = new Dictionary<int, NodePanel>();

		private Dictionary<int, CanvasTransform> _guidToCanvasTransform = new Dictionary<int, CanvasTransform>();
		public List<string> debugList = new List<string>();
		public IInputReceiver[] InputReceivers
        {
			get { return _inputReceivers.ToArray(); }
        }

		public NodePanel RootNodePanel
		{
			get
			{
				if (_rootNodePanelGuid == int.MinValue)
                {
					return null;
                }
				return GetNodePanel(_rootNodePanelGuid);
			}
			set
			{
				_rootNodePanelGuid = value != null ? value.guid : int.MinValue;
			}
		}

        private void OnEnable()
        {
			if (tree != null)
			{
				tree.CreateGuidToNodeTable();
				CreateGuidToNodePanelTable();
				CreateNodeToNodePanelTable();

				RebuildCanvasTransforms();
				RebuildInputReceivers();

				if (!Application.isPlaying)
				{
					RebuildConnections();
				}
			}
        }

        public void RebuildConnections()
        {
	        for (int i = 0; i < nodePanels.Count; i++)
	        {
		        NodePanel panel = nodePanels[i];
		        NodeHandle outHandle = panel.outHandle;

		        List<NodeConnection> curConnections = panel.outHandle.Connections;
		        List<NodeHandle> inHandles = new List<NodeHandle>();

		        for (int j = 0; j < panel.childrenGuids.Count; j++)
		        {
			        var child = GetNodePanel(panel.childrenGuids[j]);
			        inHandles.Add(child.inHandle);
		        }
		        
		        foreach (var inHandle in inHandles)
		        {
			        outHandle.Disconnect(inHandle);
		        }
	        }

	        foreach (var nodePanel in nodePanels)
	        {
		        foreach (var child in nodePanel.Children)
		        {
			        nodePanel.outHandle.Connect(child.inHandle);
		        }
		        
		        nodePanel.outHandle.UpdateConnections();
	        }
	        
        }

		public void RebuildInputReceivers()
        {
			_inputReceivers.Clear();


            for (int i = 0; i < nodePanels.Count; i++)
            {
				_inputReceivers.Add(nodePanels[i]);
				_inputReceivers.Add(nodePanels[i].inHandle);
				_inputReceivers.Add(nodePanels[i].outHandle);
			}
		}

		public void RebuildCanvasTransforms ()
        {
			_guidToCanvasTransform.Clear();
			_canvasTransforms.Clear();

            for (int i = 0; i < nodePanels.Count; i++)
            {
				AddCanvasTransform(nodePanels[i].transform, false);
				AddCanvasTransform(nodePanels[i].inHandle.transform, false);
				if (nodePanels[i].hasOuthandle)
				{
					AddCanvasTransform(nodePanels[i].outHandle.transform, false);

					List<NodeConnection> connections = nodePanels[i].outHandle.Connections;

					for (int c = 0; c < connections.Count; c++)
	                {
						AddCanvasTransform(connections[c]._start, false);
						AddCanvasTransform(connections[c]._end, false);
					}
				}
			}
		}

		public void AddCanvasTransform(CanvasTransform canvasTransform, bool createGuid = true)
        {

			if (createGuid)
			{
				canvasTransform.guid = GetNewCanvasTransformGuid();
			}

			if (canvasTransform.guid == 0)
			{
				Debug.Log("guid 0!");
			}
			
			if (canvasTransform.guid != int.MinValue)
			{

				if (!_guidToCanvasTransform.ContainsKey(canvasTransform.guid))
				{
					_guidToCanvasTransform.Add(canvasTransform.guid, canvasTransform);
				}

				if (!_canvasTransforms.Contains(canvasTransform))
				{
					_canvasTransforms.Add(canvasTransform);
				}
			}

			debugList.Clear();
			foreach (var i in _guidToCanvasTransform)
			{
				debugList.Add(i.Key + ": " + i.Value.id);
			}
        }

		public void RemoveCanvasTransform(CanvasTransform canvasTransform, bool removeDescendants = true)
        {
			if (removeDescendants)
			{

				for (int i = 0; i < canvasTransform.Children.Count; i++)
				{
					canvasTransform.Children[i].Parent = null;
				}
			}

			canvasTransform.Parent = null;

			_guidToCanvasTransform.Remove(canvasTransform.guid);
			_canvasTransforms.Remove(canvasTransform);
        }

		public void AddInputReceiver(IInputReceiver inputReceiver)
        {
			_inputReceivers.Add(inputReceiver);
			_inputReceivers.Sort((x, y) => x.Priority.CompareTo(y.Priority));
		}

		public bool CanvasTransformExists(int guid)
		{
			return _guidToCanvasTransform.ContainsKey(guid);
		}
		
		public CanvasTransform GetCanvasTransform(int guid)
        {
			if (_guidToCanvasTransform.ContainsKey(guid) == false)
            {
				Debug.Log("Cannot find: " + guid);
                foreach (var i in _guidToCanvasTransform)
                {
					Debug.Log("cur: " + i.Key.ToString());
                }
            }

			return _guidToCanvasTransform[guid];
        }

		public int GetNewCanvasTransformGuid()
        {
			_canvasTransformGuidCount += 1;
			return _canvasTransformGuidCount;
        }

		private void CreateGuidToNodePanelTable()
		{

			_guidToNodePanelDict.Clear();

			for (int i = 0; i < nodePanels.Count; i++)
			{
				_guidToNodePanelDict.Add(nodePanels[i].guid, nodePanels[i]);
			}
		}

		private void CreateNodeToNodePanelTable()
		{
			_nodeToNodePanelDict.Clear();
			for (int i = 0; i < nodePanels.Count; i++)
			{
				_nodeToNodePanelDict.Add(nodePanels[i].Node, nodePanels[i]);
			}
		}
			
		public void SaveState ()
        {
			EditorUtility.SetDirty(this);
			EditorUtility.SetDirty(tree);
			AssetDatabase.SaveAssets();
		}

		public void AddNodePanel(NodePanel nodePanel)
        {
			nodePanels.Add(nodePanel);
			_nodeToNodePanelDict.Add(nodePanel.Node, nodePanel);
			_guidToNodePanelDict.Add(nodePanel.guid, nodePanel);
			SaveState();
        }

		public void RemoveNodePanel(NodePanel panel)
        {
			RemoveCanvasTransform(panel.inHandle.transform);

			if (panel.hasOuthandle)
			{
				RemoveCanvasTransform(panel.outHandle.transform);
			}

			RemoveCanvasTransform(panel.transform);

			_nodeToNodePanelDict.Remove(panel.Node);
			_guidToNodePanelDict.Remove(panel.guid);

			nodePanels.Remove(panel);
        }

		public NodePanel GetNodePanel(int guid)
        {
			if (!_guidToNodePanelDict.ContainsKey(guid))
            {
                foreach (var item in _guidToNodePanelDict)
                {
					Debug.Log(item.Key);
                }

				throw new Exception("Could not find node panel with guid: " + guid);
            }

			return _guidToNodePanelDict[guid];
        }

		public NodePanel GetNodePanel(Node node)
        {
			return _nodeToNodePanelDict[node];
        }

		public CanvasState CloneWithSubstituteTree(BehaviourTree treeAsset)
		{
			CanvasState canvasState = ScriptableObject.CreateInstance<CanvasState>();
			
			canvasState.tree = treeAsset;
			canvasState.RootNodePanel = this.RootNodePanel;

			canvasState.nodePanels = new List<NodePanel>();
			
			for (int i = 0; i < nodePanels.Count; i++)
			{
				canvasState.nodePanels.Add(nodePanels[i].Clone(treeAsset, canvasState));
			}
			
			
			canvasState.canvasRect = canvasRect;
			canvasState.zoom = zoom;
			canvasState.panOffset = panOffset;
			canvasState.OnEnable();

			for (int i = 0; i < nodePanels.Count; i++)
			{
				NodePanel clonedPanel = canvasState.GetNodePanel(nodePanels[i].guid);
				NodePanel oldPanel = nodePanels[i];
				
				clonedPanel.inHandle.transform.rect = oldPanel.inHandle.transform.rect;
				if (clonedPanel.hasOuthandle)
				{
					clonedPanel.outHandle.transform.rect = oldPanel.outHandle.transform.rect;
				}
			}

			for (int i = 0; i < nodePanels.Count; i++)
			{
				NodePanel nodePanel = nodePanels[i];
				
				for (int j = 0; j < nodePanel.Children.Count; j++)
				{
					NodePanel child = nodePanel.Children[j];
					nodePanel.outHandle.Connect(child.inHandle);
				}

			}
			
			return canvasState;
		}
		
    }


}