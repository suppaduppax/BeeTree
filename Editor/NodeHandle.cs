using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeeTree.Editor {
	[Serializable]
	public class NodeHandle : IInputReceiver
	{
		public CanvasState canvasState;
		public CanvasTransform transform;

		public HandleType type;
		public int nodePanelGuid = int.MinValue;

		[SerializeField] private List<NodeConnection> _connections = new List<NodeConnection>();

		public enum HandleType
		{
			In,
			Out
		}

		public NodePanel NodePanel
		{
			get {
				if (nodePanelGuid == int.MinValue)
                {
					throw new System.Exception("Node panel of nodehandle is null for some reason");
                }
				return canvasState.GetNodePanel(nodePanelGuid);
			}
		}

		public List<NodeConnection> Connections
        {
			get {
				if (_connections == null)
                {
					_connections = new List<NodeConnection>();
                }
				return _connections;
			}
        }

        int IInputReceiver.Priority
        {
			get { return BehaviourEditor.NODE_HANDLE_INPUT_PRIORITY; }
        }

        CanvasTransform IInputReceiver.Transform {
			get { return transform; }
		}

        public NodeHandle (NodePanel nodePanel, HandleType type, Rect rect, CanvasState canvasState) : 
	        this(nodePanel.guid, nodePanel.label, type, rect, canvasState)
        {
		}
        
        public NodeHandle (int nodePanelGuid, string id, HandleType type, Rect rect, CanvasState canvasState)
        {
	        this.canvasState = canvasState;
	        transform = new CanvasTransform(id + " NodeHandle " + type.ToString(), rect, canvasState);
	        this.nodePanelGuid = nodePanelGuid;

	        this.type = type;
	        _connections = new List<NodeConnection>();

	        this.canvasState.AddInputReceiver(this);
        }
        
		public void Connect(NodeHandle inHandle)
        {

			if (IsConnectedTo(inHandle))
            {
				Debug.Log("Already connected???");
				return;
            }

			NodeConnection connection = new NodeConnection(NodePanel, inHandle.NodePanel, canvasState);
			_connections.Add(connection);
        }

		public void Disconnect (NodeHandle inHandle)
        {
			NodeConnection connection = GetConnection(inHandle);

			if (connection != null)
            {
				canvasState.RemoveCanvasTransform(connection._start);
				canvasState.RemoveCanvasTransform(connection._end);
				_connections.Remove(connection);

            }
		}

		private NodeConnection GetConnection (NodeHandle inHandle)
        {
            for (int i = 0; i < _connections.Count; i++)
            {
				if (_connections[i].EndParentEquals(inHandle.transform))
                {
					return _connections[i];
                }
            }

			return null;
        }

		public bool IsConnectedTo (NodeHandle handle)
        {
			return GetConnection(handle) != null;
        }


		public void UpdateConnections ()
        {
            for (int i = 0; i < _connections.Count; i++)
            {
				_connections[i].UpdatePoints(_connections);
            }
        }

		public NodeHandle Clone(CanvasState state)
		{
			NodeHandle clone = new NodeHandle(nodePanelGuid, NodePanel.label, type, transform.rect, state)
			{
				_connections = new List<NodeConnection>()
			};
			
			for (int i = 0; i < _connections.Count; i++)
			{
				clone._connections.Add(_connections[i].Clone(state));
			}
			return clone;
		}
    }


}