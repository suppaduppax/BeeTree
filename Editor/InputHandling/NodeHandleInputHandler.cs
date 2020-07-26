using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeeTree.Editor {
	public class NodeHandleInputHandler : CanvasInputHandler
	{
        public NodeHandleInputHandler(NodeCanvas nodeCanvas) : base(nodeCanvas) { }

        private NodeHandle _draggingHandle;

        //public override void OnPointerDown(PointerEvent evt)
        //{
        //    //NodeHandle handle = (NodeHandle)evt.downObject;
        //    //if (handle.type == NodeHandle.HandleType.Out)
        //    //{
                
        //    //}
        //    //else
        //    //{
                
        //    //}
        //}

        //public override void OnPointerUp(PointerEvent evt)
        //{
        //    //_nodeCanvas.DestroyPreviewConnection();
        //    //_draggingHandle = null;
        //}

        public override void OnPointerClick(PointerEvent evt)
        {
            NodeHandle handle = (NodeHandle)evt.downObject;
            if (handle != null && handle.type == NodeHandle.HandleType.In)
            {
                _nodeCanvas.RemoveChild(handle.NodePanel);
            }
        }

        public override void OnPointerDrag(PointerEvent evt)
        {
            NodeHandle handle = (NodeHandle)evt.downObject;
            Vector2 mousePos = evt.mousePos;

            if (_draggingHandle == null)
            {
                if (handle.type == NodeHandle.HandleType.Out)
                {
                    _nodeCanvas.CreatePreviewConnection(handle, evt.mousePos);
                    _draggingHandle = handle;
                }
                else
                {
                    NodePanel parent = handle.NodePanel.Parent;
                    if (parent != null)
                    {
                        _draggingHandle = parent.outHandle;
                        _nodeCanvas.RemoveChild(handle.NodePanel);
                        _nodeCanvas.CreatePreviewConnection(parent.outHandle, evt.mousePos);
                    }
                }
            }

            if (_draggingHandle != null && _nodeCanvas.previewConnection != null)
            {
                _nodeCanvas.previewConnection.SetEndPosition(CanvasUtility.ScreenToCanvasPosition(mousePos, _nodeCanvas.canvasState));
                _nodeCanvas.previewConnection.UpdatePoints(new List<NodeConnection>());
            }
        }

        public override void OnPointerDrop(PointerEvent evt)
        {
            _nodeCanvas.DestroyPreviewConnection();

            NodeHandle droppingHandle = GetNodeHandleAtPosition(evt.droppingObjects, evt.canvasPos);

            if (droppingHandle != null && droppingHandle.type == NodeHandle.HandleType.In)
            {
                _nodeCanvas.ConnectHandles(_draggingHandle, droppingHandle);
            }


            _draggingHandle = null;
        }

        private NodeHandle GetNodeHandleAtPosition (IInputReceiver[] receivers, Vector2 canvasPos)
        {
            for (int i = receivers.Length - 1; i >= 0; i--)
            {
                if (receivers[i].GetType() == typeof(NodeHandle))
                {
                    return (NodeHandle)receivers[i];
                }
            }

            return null;
        }
    }
}