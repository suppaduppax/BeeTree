using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PocketGuild;

namespace BeeTree.Editor {
	[Serializable]
	public class NodeConnection
	{
		public Vector2 Start
		{
			get { return _start.position; }
		}

		public Vector2 End
		{
			get { return _end.position; }
		}

		public Vector2[] Points
		{
			get { return _points; }
		}

		public NodePanel StartPanel
		{
			get
			{
				if (_startPanelGuid == int.MinValue)
				{
					return null;
				}

				return canvasState.GetNodePanel(_startPanelGuid);
			}
		}

		public NodePanel EndPanel
		{
			get
			{
				if (_endPanelGuid == int.MinValue)
				{
					return null;
				}

				return canvasState.GetNodePanel(_endPanelGuid);
			}
		}

		public CanvasState canvasState;

		/// <summary>
		/// 0 - start position
		/// 3 - end position
		/// </summary>
		[SerializeField] protected Vector2[] _points;

		public CanvasTransform _start;
		public CanvasTransform _end;

		[SerializeField] int _startPanelGuid = int.MinValue;
		[SerializeField] int _endPanelGuid = int.MinValue;

		public NodeConnection() { }
		
		public NodeConnection(NodePanel startPanel, NodePanel endPanel, CanvasState canvasState)
		{
			this.canvasState = canvasState;
			
			// CanvasTransform startParent = canvasState.GetCanvasTransform(startParentGuid);
			// CanvasTransform endParent = canvasState.GetCanvasTransform(endParentGuid);

			// startParent.childrenGuids.Add(_start.guid);
			// endParent.childrenGuids.Add(_end.guid);

			_start = new CanvasTransform("Start Connection", startPanel.outHandle.transform.rect, canvasState);
			_end = new CanvasTransform("End Connection", endPanel.inHandle.transform.rect, canvasState);

			_start.Parent = startPanel.outHandle.transform;
			_end.Parent = endPanel.inHandle.transform;

			
			_startPanelGuid = startPanel.guid;
			_endPanelGuid = endPanel.guid;
			
			_points = new Vector2[4];
		}
		


		// public NodeConnection(NodeHandle source, Vector2 end, CanvasState canvasState)
		// {
		// 	this.canvasState = canvasState;
		//
		// 	_start = new CanvasTransform(source.NodePanel.label + " Start Connection", source.transform.rect.center, Vector2.zero, canvasState);
		// 	_start.Parent = source.transform;
		//
		// 	_end = new CanvasTransform(source.NodePanel.label + " End Connection", end, Vector2.zero, canvasState);
		//
		// 	_points = new Vector2[4];
		// }
		
		public void UpdatePoints (List<NodeConnection> allConnections)
        {
			float yMin = _end.position.y;

			// find other connections
			for (int i = 0; i < allConnections.Count; i++)
			{
                if (allConnections[i] == this)
                {
                    continue;
                }

                float compareY = allConnections[i].End.y;
				if (compareY < yMin)
				{
					yMin = compareY;
				}
			}

			yMin -= BehaviourEditor.nodeConnectionPadding;

			//			     [ ]
			//			      |
			//			      |
			//			      |
			//	(dx, yMin) o--o
			//		       |
			//		      [ ]

			_points[0] = _start.position;
			_points[1] = new Vector2(_start.position.x, yMin);
			_points[2] = new Vector2(_end.position.x, yMin);
			_points[3] = _end.position;
		}


		public void SetStartParent (NodeHandle startHandle)
	    {
			if (_start.Parent != startHandle.transform)
			{
				_start.Parent = startHandle.transform;
			}
        }

		public void SetEndParent (NodeHandle endHandle)
		{
			if (_end.Parent != endHandle.transform)
			{
				_end.Parent = endHandle.transform;
			}
		}

		public void SetStartPosition (Vector2 pos)
        {
			_start.position = pos;
			
        }

        public void SetEndPosition (Vector2 pos)
        {
			_end.position = pos;
        }

		public bool StartParentEquals(CanvasTransform parent)
		{
			return parent == _start.Parent;
		}

		public bool EndParentEquals(CanvasTransform parent)
		{
			return parent == _end.Parent;
		}

		public NodeConnection Clone(CanvasState state)
		{
			NodeConnection clone = new NodeConnection()
			{
				canvasState = state,
				_startPanelGuid = _startPanelGuid,
				_endPanelGuid = _endPanelGuid,
				_start = _start.Clone(state),
				_end = _end.Clone(state),
				_points = _points,
			};

			return clone;
		}
	}
}