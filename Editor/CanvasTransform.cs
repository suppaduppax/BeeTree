using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace BeeTree.Editor {
	[Serializable]
	public class CanvasTransform
	{
		public string id;
		public CanvasState canvasState;

		public int guid = int.MinValue;
		public int parentGuid = int.MinValue;

		public List<int> childrenGuids;

		[SerializeField] private Rect _rect;
		[SerializeField] private Rect _localRect;
		
		[SerializeField] private Vector2 _position;
		[SerializeField] private Vector2 _localPosition;

		[SerializeField] private Vector2 _anchor;
		
		public static Vector2 ANCHOR_UPPER_LEFT = new Vector2(0.0f, 0.0f);
		public static Vector2 ANCHOR_UPPER_CENTER = new Vector2(0.5f, 0.0f);
		public static Vector2 ANCHOR_UPPER_RIGHT = new Vector2(1.0f, 0.0f);
		public static Vector2 ANCHOR_MIDDLE_LEFT = new Vector2(0.0f, 0.5f);
		public static Vector2 ANCHOR_MIDDLE_CENTER = new Vector2(0.5f, 0.5f);
		public static Vector2 ANCHOR_MIDDLE_RIGHT = new Vector2(1.0f, 0.5f);
		public static Vector2 ANCHOR_BOTTOM_LEFT = new Vector2(0.0f, 1.0f);
		public static Vector2 ANCHOR_BOTTOM_CENTER = new Vector2(0.5f, 1.0f);
		public static Vector2 ANCHOR_BOTTOM_RIGHT = new Vector2(0.0f, 1.0f);
		
		public CanvasTransform Parent { 
			get
			{
				if (parentGuid == int.MinValue)
                {
					return null;
                }

				return canvasState.GetCanvasTransform(parentGuid);
			}

			set
			{				
				SetParent(value);
			} 
		}



		public List<CanvasTransform> Children {
			get {
				List<CanvasTransform> children = new List<CanvasTransform>();
                for (int i = 0; i < childrenGuids.Count; i++)
                {
					children.Add(canvasState.GetCanvasTransform(childrenGuids[i]));
                }

				return children;
			}
		}

		public Rect rect
		{
			get => _rect;
			set { SetAllFromRect(value); }
		} 

		public Rect localRect {
			get { return _localRect; }
			set { SetAllFromLocalRect (value); }
		}

		public Vector2 position {
			get { return _position; }
			set {
				// _rect.position = value;
				// SetAllFromRect (_rect);
				SetWorldPosition(value);
				SetLocalFromWorld();
				UpdateChildren();
			}
		}

		public Vector2 localPosition {
			get { return _localPosition; }
			set {
				// _localRect.position = value;
				// SetAllFromLocalRect (_localRect);
				SetLocalPosition(value);
				SetWorldFromLocal();
				UpdateChildren();
			}
		}
		
		public float height
		{
			get => _rect.height;
			set
			{
				_rect.height = value;
				SetRectFromPosition();
			} 
		}
		
		public float width
		{
			get => _rect.width;
			set
			{
				_rect.width = value;
				SetRectFromPosition();
			}
		}

		public CanvasTransform (string id, Rect rect, Vector2 anchor, CanvasState canvasState)
		{
			this.id = id;
			this.canvasState = canvasState;
			_anchor = anchor;
			
			childrenGuids = new List<int>();
			Debug.Log(anchor);
			SetAllFromRect (rect);
			canvasState.AddCanvasTransform(this);
		}

		public CanvasTransform (string id, Vector2 position, Vector2 size, CanvasState canvasState) :
			this(id, new Rect(position, size), canvasState)
		{
		}

		public CanvasTransform(string id, Rect rect, CanvasState canvasState) :
			this(id, rect, new Vector2(0.5f, 0.5f), canvasState)
		{
		}

		private void AddChild (CanvasTransform child) {
			childrenGuids.Add (child.guid);
		}

		private void RemoveChild (CanvasTransform child) {
			childrenGuids.Remove (child.guid);
		}

		private void SetParent (CanvasTransform parent) {
			Parent?.RemoveChild (this);
			parentGuid = parent?.guid ?? int.MinValue;
			SetLocalFromWorld ();
			parent?.AddChild (this);

			UpdateChildren ();
		}

		public Vector2 AnchorOffset => _rect.size * _anchor;

		private void SetAllFromRect (Rect newRect)
		{
			_rect = newRect;
			SetWorldPosition(newRect.position + AnchorOffset);
			SetLocalFromWorld ();
			UpdateChildren ();
		}

		private void SetAllFromRect(Rect newRect, Vector2 anchor)
		{
			_anchor = anchor;
			SetAllFromRect(newRect);
		}

		private void SetAllFromLocalRect (Rect newLocalRect) {
			SetLocalPosition(newLocalRect.position + AnchorOffset);
			SetWorldFromLocal ();
			UpdateChildren ();
		}

		// private void SetRect (Rect newRect) {
		// 	_rect = newRect;
		// 	_position = newRect.position + AnchorOffset;
		//
		// 	// if (Parent != null) {
		// 	// 	// _localRect = new Rect(rect)
		// 	// 	// {
		// 	// 	// 	position = _position - Parent._position
		// 	// 	// };
		// 	// 	
		// 	// 	_localPosition = _position - Parent._position;
		// 	// 	_localRect = new Rect(_position - AnchorOffset, newRect.size);
		// 	// }
		// 	
		// 	SetLocalFromWorld();
		// }

		private void SetPosition (Vector2 newPosition) {
			_position = newPosition;
			_rect.position = newPosition - AnchorOffset;
		}

		private void SetWorldPosition (Vector2 newPosition) {
			Debug.Log(_rect);
			_position = newPosition;
			Debug.Log(_position);
			_rect.position = newPosition - AnchorOffset;
			Debug.Log(_rect);
		}

		private void SetRectFromPosition()
		{
			_rect.position = _position - AnchorOffset;
		}	
		
		private void SetLocalPosition (Vector2 newPosition) {
			_localPosition = newPosition;
			_localRect.position = newPosition - AnchorOffset;
		}

		private void SetLocalFromWorld () {
			if (Parent == null) {
				// local = world if there's no parent
				_localPosition = _position;
				_localRect = new Rect(_rect);
			} else {
				_localPosition = _position - Parent._position;
				_localRect.position = _localPosition;
			}
		}

		private void SetWorldFromLocal () {
			if (parentGuid == int.MinValue) {
				// no parent... local == world
				_position = _localPosition;
				SetRectFromPosition();
			} else {
				_position = Parent._position + _localPosition;
				SetRectFromPosition();
			}
		}
		
		

		public void UpdateChildren () {
			for (int i = 0; i < childrenGuids.Count; i++)
			{
				if (!canvasState.CanvasTransformExists(childrenGuids[i]))
				{
					Debug.Log(id + " cannot find child: " + childrenGuids[i]);
				}
				CanvasTransform child = canvasState.GetCanvasTransform(childrenGuids[i]);
				
                child.SetWorldFromLocal ();
                child.UpdateChildren ();
			}
		}

		public CanvasTransform Clone(CanvasState state)
		{
			CanvasTransform clone = new CanvasTransform(id + " (Clone)", rect, state);
			clone.guid = guid;
			clone.childrenGuids = childrenGuids;
			clone.parentGuid = guid;
			
			return clone;
		}

	}
}