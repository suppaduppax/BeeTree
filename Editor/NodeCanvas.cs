using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

using System.Text.RegularExpressions;

/*
 *	FIXME: 	Right now it's really confusing to know how to PROPERLY add a new node... and the order
 *			which to do the creation.. 
 *
 *			ie. do I CreateNode then CreateNodePanel... then do I update? Do I need to AddChild to NodePanels AND Nodes?
 *				do I then need to AddPanel to the panel group?! There's just so many things to do and no real way to know
 *				what to actually do... I think there needs to be a base "CreateNode" as well as a SetNodeParent function
 *				in the NodeCanvas which takes care of EVERYTHING and then I don't have to worry about how it works behind the scenes
 *				Some major refactoring might need to be done because I can't understand how it works anymore LOL
 *
 * 
 * 	TODO: 	Zooming
 * 
 * 	TODO:	A proper toolbar
 * 
 *	TODO:	Separate input logic from NodeCanvas
 * 
 * 	FIXME:	While a field in the NodeInspector is in focus, and you try to move the textfield cursor, you can still navigate through nodePanels, messing up
 * 			the current selected NodePanel and therefor ruining your field editing
 *
 *	TODO:	Create a temp file for saving the state in case the user accidently updates a script and the hot-load of unity destroys the old window state!
 *
 *	FIXME:	Panning should NOT remove currently selected node. 
 * 	
 *	TODO:	Separate inspector drawing logic for fields into seperate field drawers for easy extensibility
 * 
 * 	TODO:	BehaviourManager to load behaviours from a folder? Then being able to assign a behaviour to a behaviour controller
 * 	
 * 	TODO:	Set proper names... BehaviourHandler, BehaviourController, BehaviourManager?
 * 			BehaviourHandler -> the thing to attach a BehaviourTree to
 * 			BehaviourManager -> singleton which handles the loading of behaviours from a file?
 *
 *	TODO:	Maybe zip the tree/state together?
 *
 *	TODO:	StateManager/SessionManager - keep a serialized temp file
 * 
 * 	TODO:	Change the state of the editor when in play mode... can't edit anything but you can see things...
 * 			If you click on a GameObject with a BehaviourHandler, then the editor will display the current
 * 			status of the behaviour tree... which nodes are running which have failed etc
 * 
 * 	IDEAS:
 * 		create comments. comments will be like nodes that you can drag around but you can "pin" them to nodes so they follow certain nodes
 * 			maybe the comments could also have a "range" so you can see that the comments are directed at a range of nodes or something
 * 			but what happens to the range when you pull a node out? or the end node? or the starting node? OH maybe what happens
 * 			is that the node will point to the opposite.. ie.. if you pull out the end node, you are left with a comment pointing to the
 * 			start node only and if you pull out the start node, it only points to the end node instead
 * 
 * 
 */

namespace BeeTree.Editor
{

    /// <summary>
    /// object that gets passed when the context menu comes up (when the player right clicks something)
    /// so this passes any relevant information to a function
    /// </summary>
    public class ContextData
    {
        public Vector2 mousePosition;
        public Vector2 canvasPosition;

        public NodePanel nodePanel;
    }

    /// <summary>
    /// Handles the majority of the logic in the canvas window
    /// </summary>
    public class NodeCanvas
    {
        public CanvasState canvasState { get; protected set; }          // stores the panOffset and other state related things

        #region Canvas Input

        //public Vector2 mousePos { get; protected set; }                 // the current mouse position
        private bool isPanning = false;                                 // is the user currently panning the canvas
        private float dragThreshold = 5f;                               // the distance threshold from a mousedown to change the state from mousedown to dragging

        private bool isMouseDown = false;                               // has the mouse been pressed down
        private Vector2 mouseDownPos;                                   // the position where the mouse was first moused down

        #endregion

        public List<NodePanel> selectedPanels { get; protected set; }          // the currently selected panel
        public NodeConnection previewConnection { get; protected set; }
        private CanvasTransform previewEndPoint;
        
        public Rect boxSelection { get; protected set; }

        private NodeCanvasRenderer _canvasRenderer;                       // the class that handles all the drawing of the canvas (nodePanels, groups, ghost, etc)

        private Rect _canvasRect;
        private NodeHandle _selectedHandle;

        private PointerEvent _curPointerEvent;
        private NodePanelInputHandler _nodePanelInputHandler;
        private NodeHandleInputHandler _nodeHandleInputHandler;

        
        // a map to hold which node corresponds to which panel. needed if you have a node but dont know which panel it belongs to.
        private Dictionary<Node, NodePanel> _nodeToNodePanelTable;

        // callbacks to invoke when a node is selected
        private Action<Node, NodePanel> nodeSelectedCallback;

        public NodeCanvas(Rect canvasRect)
        {
            selectedPanels = new List<NodePanel>();
            _nodeToNodePanelTable = new Dictionary<Node, NodePanel>();

            _canvasRenderer = new NodeCanvasRenderer(this);
            _canvasRect = canvasRect;


            _nodePanelInputHandler = new NodePanelInputHandler(this);
            _nodeHandleInputHandler = new NodeHandleInputHandler(this);
        }

        public void Update()
        {
            if (Application.isPlaying)
            {
                UpdateNodePanelsPlaymode();
            }
            
            if (canvasState != null)
            {
                ProcessInput();
                _canvasRenderer.Update();
            }
        }

        private void UpdateNodePanelsPlaymode()
        {
            if (canvasState == null)
            {
                return;
            }
            
            for (int i = 0; i < canvasState.nodePanels.Count; i++)
            {
                NodePanel nodePanel = canvasState.nodePanels[i]; 
                Node node = nodePanel.Node;

                switch (node.State)
                {
                    case Node.NodeState.Running:
                        nodePanel.SetColours(Color.white, BehaviourEditorStyles.playMode_nodeRunningColour, 1);
                        break;
                    
                    case Node.NodeState.Success:
                        nodePanel.SetColours(Color.white, BehaviourEditorStyles.playMode_nodeSuccessColour, 1);
                        break;
                    
                    case Node.NodeState.Failure:
                        nodePanel.SetColours(Color.white, BehaviourEditorStyles.playMode_nodeFailureColour, 1);
                        break;
                    
                    case Node.NodeState.Idle:
                    default:
                        nodePanel.SetColours(Color.white, BehaviourEditorStyles.playMode_nodeIdleColour, 1);
                        break;    
                }
            }
        }

        public void Load(BehaviourTree treeAsset)
        {
            if (treeAsset == null)
            {
                Debug.LogError("Treeasset is null for some reason");
                return;
            }

            string treePath = AssetDatabase.GetAssetPath(treeAsset);
            if (treePath == null || treePath == "")
            {
                Debug.LogWarning("Treepath was empty for soe reason");
                return;
            }

            // clear the canvas state and try to find it in the tree asset
            canvasState = GetCanvasStateSubAsset(treeAsset);

            // couldn't find it in the tree asset, create a new canvasState asset and parent it under the treeasset
            if (canvasState == null)
            {
                canvasState = ScriptableObject.CreateInstance<CanvasState>();
                canvasState.name = "Canvas State";
                AssetDatabase.AddObjectToAsset(canvasState, treePath);
                canvasState.canvasRect = _canvasRect;
                canvasState.tree = treeAsset;
            }

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(canvasState));
        }

        public void LoadBehaviourController(BehaviourController controller)
        {
            Debug.Log("Loading behaviour controller");
            if (controller == null)
            {
                return;
            }

            BehaviourTree treeAsset = controller.behaviourTree;
            BehaviourTree runtimeTree = controller.RuntimeBehaviourTree;
            CanvasState canvasStateSubAsset = GetCanvasStateSubAsset(treeAsset);

            if (runtimeTree == null || canvasStateSubAsset == null)
            {
                Debug.LogError("can't find sub asset?");
                return;
            }

            canvasState = canvasStateSubAsset.CloneWithSubstituteTree(runtimeTree);
            
            if (canvasState == null)
            {
                Debug.LogError("CAnvasstate still null");
                return;
            }
            
            Debug.Log(canvasState.RootNodePanel.Node.State);

            Debug.Log(canvasState);
        }
        
        private CanvasState GetCanvasStateSubAsset (BehaviourTree treeAsset)
        {
            CanvasState result = null;
            
            string treePath = AssetDatabase.GetAssetPath(treeAsset);
            if (treePath == null || treePath == "")
            {
                Debug.LogWarning("Treepath was empty for soe reason");
                return null;
            }
            
            UnityEngine.Object[] allAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(treePath);
            if (allAssets == null)
            {
                Debug.Log("Something went wrong.. allassets is null");
                return null;
            }

            for (int i = 0; i < allAssets.Length; i++)
            {
                if (allAssets[i] == null)
                {
                    Debug.Log("asset is null");

                    continue;
                }

                if (allAssets[i].GetType() == typeof(CanvasState))
                {
                    result = allAssets[i] as CanvasState;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Resets the camera to 0, 0.
        /// </summary>
        public void ResetCamera()
        {
            canvasState.panOffset = Vector2.zero;
        }

        public void CenterCanvas()
        {
            if (canvasState.RootNodePanel == null)
            {
                ResetCamera();
                return;
            }

            // the rect to center around
            Rect focusRect;
            //if (selectedPanel == null) {
            //NodePanelGroup rootGroup = rootNodePanel.group;
            //focusRect = rootGroup.transform.rect;
            //} else {
            //focusRect = selectedPanels.transform.rect;
            //}

            //CenterCanvas(focusRect);
        }

        /// <summary>
        /// Centers the canvas depending on what is focused. If nothing is selected, it centers on the group containing the rootNodePanel.
        /// If a NodePanel is selected, it centers on that NodePanel
        /// If there's no rootNodePanel... centers to 0, 0
        /// </summary>
        public void CenterCanvas(Rect focusRect)
        {
            canvasState.panOffset = new Vector2(
                (canvasState.canvasRect.width - focusRect.size.x) / 2,
                (canvasState.canvasRect.height - focusRect.size.y) / 2)
                - focusRect.position - canvasState.zoomPos;
        }

        public void SetRootNodePanel(NodePanel nodePanel)
        {
            if (canvasState.RootNodePanel != null)
            {
                canvasState.RootNodePanel.SetColours(Color.white, BehaviourEditorStyles.nodeNormalColour, 1);
            }

            nodePanel.SetColours(Color.white, BehaviourEditorStyles.nodeRootColour, 1);
            canvasState.RootNodePanel = nodePanel;
            canvasState.tree.RootNode = nodePanel.Node;
        }

        public void CreatePreviewConnection(NodeHandle source, Vector2 mousePos)
        {
            previewConnection = new PreviewConnection(source, mousePos, canvasState);
        }

        public void DestroyPreviewConnection()
        {

            // TODO: DO This!
            canvasState.RemoveCanvasTransform(previewConnection._start);
            canvasState.RemoveCanvasTransform(previewConnection._end);
            previewConnection = null;
        }


        public void ConnectHandles(NodeHandle outHandle, NodeHandle inHandle, bool connectNode = true, bool forceConnection = false)
        {
            NodePanel parentPanel = outHandle.NodePanel;
            NodePanel childPanel = inHandle.NodePanel;
            NodePanel oldParentPanel = childPanel.Parent;
            
            if (forceConnection || parentPanel.Node.CanAddChild(childPanel.Node))
            {
                SetNodePanelParent(childPanel, parentPanel, connectNode);
            }
            else if (parentPanel.Node.MaxChildren == 1)
            {
                RemoveChild(parentPanel.Children[0]);
                SetNodePanelParent(childPanel, parentPanel, connectNode);
            }
            

            parentPanel.outHandle.UpdateConnections();

            if (canvasState.RootNodePanel == childPanel)
            {
                NodePanel topLevelParent = parentPanel;
                while (topLevelParent.Parent != null)
                {
                    topLevelParent = topLevelParent.Parent;
                }
                
                SetRootNodePanel(topLevelParent);
            }
            
            canvasState.SaveState();
        }

        private void ProcessInput()
        {
            Event e = Event.current;

            if (e.button == 0)
            {
                ProcessLeftMouseButtonInput(e);
            }
            else if (e.button == 2)
            {
                float panMultiplier = 0.5f;
                canvasState.panOffset += e.delta * canvasState.zoom * panMultiplier;
                _canvasRenderer.Repaint();
            }
            else if (e.type == EventType.ContextClick && IsInsideCanvas(e.mousePosition))
            {
                e.Use();

                ContextData contextData = new ContextData()
                {
                    canvasPosition = CanvasUtility.ScreenToCanvasPosition(e.mousePosition, canvasState),
                    mousePosition = e.mousePosition,
                    nodePanel = GetNodePanelAtPosition(CanvasUtility.ScreenToCanvasPosition(e.mousePosition, canvasState)),
                };

                GenericMenu menu = new GenericMenu();

                if (contextData.nodePanel != null)
                {
                    //SelectPanel(contextData.nodePanel);
                    //BehaviourEditorWindow.RepaintWindow();
                    //_canvasRenderer.Draw();

                    // right clicked on nodepanel
                    if (canvasState.RootNodePanel != contextData.nodePanel && contextData.nodePanel.Parent == null)
                    {
                        Debug.Log("EH");
                        menu.AddItem(
                            new GUIContent("Set as Root Node"),
                            false,
                            SetRootNodeContextCallback,
                            contextData
                            );
                    }
                    else
                    {
                        Debug.Log("2");
                        menu.AddDisabledItem(
                             new GUIContent("Set as Root Node")
                             );
                    }

                    //menu.AddItem(new GUIContent("Copy Node"), false, null, contextData);
                    menu.AddItem(new GUIContent("Delete Node"), false, DeleteNodeContextCallback, contextData);

                }
                else
                {
                    menu.AddItem(new GUIContent("Create Node"), false, CreateNodeContextCallback, contextData);
                }

                menu.ShowAsContext();
            }

            if (e.type == EventType.KeyUp)
            {
                UpdateNavigation(e.keyCode);
            }

        }

        private void ProcessLeftMouseButtonInput(Event e)
        {
            if (e.type == EventType.Repaint)
            {
                return;
            }

            int controlId = GUIUtility.GetControlID(FocusType.Passive);

            Vector2 mousePos = e.mousePosition;
            Vector2 mousePosToCanvasPos = CanvasUtility.ScreenToCanvasPosition(mousePos, canvasState);

            IInputReceiver[] inputReceivers = GetInputReceiversAtPosition(mousePosToCanvasPos);
            IInputReceiver topInputReceiver = inputReceivers.Length > 0 ? inputReceivers[inputReceivers.Length - 1] : null;

            if (e.type == EventType.MouseMove)
            {
                _curPointerEvent = new PointerEvent()
                {
                    type = PointerEvent.Type.PointerMove,
                    mousePos = mousePos,
                    canvasPos = mousePosToCanvasPos,
                    hoveredObjects = inputReceivers
                };
            }

            if (e.type == EventType.MouseDown && IsInsideCanvas(mousePos))
            {
                GUIUtility.hotControl = controlId;

                _curPointerEvent = new PointerEvent()
                {
                    type = PointerEvent.Type.PointerDown,
                    mousePos = mousePos,
                    canvasPos = mousePosToCanvasPos,
                    hoveredObjects = inputReceivers,
                    downObject = topInputReceiver,
                    startPos = mousePos,
                    shift = e.shift,
                };

                if (topInputReceiver != null)
                {
                    if (topInputReceiver.GetType() == typeof(NodePanel))
                    {
                        _nodePanelInputHandler.OnPointerDown(_curPointerEvent);
                    }
                    else if (topInputReceiver.GetType() == typeof(NodeHandle))
                    {
                        _nodeHandleInputHandler.OnPointerDown(_curPointerEvent);
                    }
                }

            }

            if (GUIUtility.hotControl == controlId && Event.current.rawType == EventType.MouseUp)
            {
                if (_curPointerEvent.type == PointerEvent.Type.PointerDown)
                {
                    // Regular mouse up
                    _curPointerEvent = new PointerEvent()
                    {
                        type = PointerEvent.Type.PointerUp,
                        mousePos = mousePos,
                        canvasPos = mousePosToCanvasPos,
                        deltaPos = e.delta,
                        startPos = _curPointerEvent.startPos,

                        hoveredObjects = inputReceivers,
                        downObject = _curPointerEvent.downObject,
                        upObjects = inputReceivers,
                        shift = _curPointerEvent.shift,
                    };

                    if (_curPointerEvent.downObject != null)
                    {
                        if (_curPointerEvent.downObject.GetType() == typeof(NodePanel))
                        {
                            _nodePanelInputHandler.OnPointerUp(_curPointerEvent);
                            _nodePanelInputHandler.OnPointerClick(_curPointerEvent);
                        }
                        else if (_curPointerEvent.downObject.GetType() == typeof(NodeHandle))
                        {
                            _nodeHandleInputHandler.OnPointerUp(_curPointerEvent);
                            _nodeHandleInputHandler.OnPointerClick(_curPointerEvent);
                        }
                    }
                    else
                    {
                        if (!_curPointerEvent.shift)
                        {
                            selectedPanels.Clear();
                        }
                    }
                }
                else if (_curPointerEvent.type == PointerEvent.Type.PointerDrag)
                {
                    // Mouse up after drag
                    _curPointerEvent = new PointerEvent()
                    {
                        type = PointerEvent.Type.PointerDrop,
                        mousePos = mousePos,
                        canvasPos = mousePosToCanvasPos,
                        deltaPos = e.delta,
                        startPos = _curPointerEvent.startPos,
                        hoveredObjects = inputReceivers,
                        downObject = _curPointerEvent.downObject,
                        draggingObject = _curPointerEvent.downObject,
                        droppingObjects = inputReceivers,
                        shift = _curPointerEvent.shift,
                    };

                    if (_curPointerEvent.draggingObject != null)
                    {
                        if (_curPointerEvent.draggingObject.GetType() == typeof(NodePanel))
                        {
                            _nodePanelInputHandler.OnPointerDrop(_curPointerEvent);
                        }
                        else if (_curPointerEvent.draggingObject.GetType() == typeof(NodeHandle))
                        {
                            _nodeHandleInputHandler.OnPointerDrop(_curPointerEvent);
                        }
                    }
                    else
                    {
                        // was dragging canvas
                        BoxSelect(boxSelection, _curPointerEvent.shift);
                        boxSelection = new Rect();
                    }
                }
            }

            if (e.type == EventType.MouseDrag && IsInsideCanvas(_curPointerEvent.startPos))
            {
                _curPointerEvent = new PointerEvent()
                {
                    type = PointerEvent.Type.PointerDrag,
                    mousePos = mousePos,
                    canvasPos = mousePosToCanvasPos,
                    deltaPos = e.delta,
                    startPos = _curPointerEvent.startPos,
                    hoveredObjects = inputReceivers,
                    downObject = _curPointerEvent.downObject,
                    draggingObject = _curPointerEvent.downObject,
                    shift = e.shift,
                };

                if (_curPointerEvent.downObject != null)
                {
                    if (_curPointerEvent.draggingObject.GetType() == typeof(NodePanel))
                    {
                        _nodePanelInputHandler.OnPointerDrag(_curPointerEvent);
                    }
                    else if (_curPointerEvent.draggingObject.GetType() == typeof(NodeHandle))
                    {
                        _nodeHandleInputHandler.OnPointerDrag(_curPointerEvent);
                    }
                }
                else
                {
                    // dragging the canvas, lets do a box selection
                    Vector2 startCanvasPos = CanvasUtility.ScreenToCanvasPosition(_curPointerEvent.startPos, canvasState);
                    Vector2 endCanvasPos = CanvasUtility.ScreenToCanvasPosition(_curPointerEvent.mousePos, canvasState);
                    float startX = startCanvasPos.x;
                    float endX = endCanvasPos.x;
                    float startY = startCanvasPos.y;
                    float endY = endCanvasPos.y;

                    if (startX > endX)
                    {
                        float oldX = startX;
                        startX = endX;
                        endX = oldX;
                    }

                    if (startY > endY)
                    {
                        float oldY = startY;
                        startY = endY;
                        endY = oldY;
                    }

                    boxSelection = new Rect(startX, startY, endX - startX, endY - startY);
                }
            }


            _canvasRenderer.Repaint();
        }

        private IInputReceiver[] GetInputReceiversAtPosition(Vector2 canvasPos)
        {
            List<IInputReceiver> result = new List<IInputReceiver>();
            IInputReceiver[] inputReceivers = canvasState.InputReceivers;
            for (int i = 0; i < inputReceivers.Length; i++)
            {
                if (inputReceivers[i] == null)
                {
                    continue;
                }

                if (inputReceivers[i].Transform.rect.Contains(canvasPos))
                {
                    result.Add(inputReceivers[i]);
                }
            }

            result.Sort((x, y) => y.Priority.CompareTo(x.Priority));

            return result.ToArray();
        }


        private void BoxSelect(Rect rect, bool addToSelection = false)
        {
            if (!addToSelection)
            {
                selectedPanels.Clear();
            }

            for (int i = 0; i < canvasState.nodePanels.Count; i++)
            {
                if (rect.Contains(canvasState.nodePanels[i].transform.rect.center))
                {
                    selectedPanels.Add(canvasState.nodePanels[i]);
                }
            }
        }

        /*
        private void ProcessInput()
        {
            Event e = Event.current;

            mousePos = e.mousePosition;
            Vector2 mousePosToCanvasPos = CanvasUtility.ScreenToCanvasPosition(mousePos, canvasState);

            if (isPanning)
            {
                float panMultiplier = 0.5f;
                canvasState.panOffset += e.delta * canvasState.zoom * panMultiplier;
                _canvasRenderer.Repaint();
            }

            if (e.button == 0)
            {
                if (e.type == EventType.MouseDown && IsMouseOnCanvas())
                {

                    NodePanel hoveredPanel = GetNodePanelAtPosition(mousePosToCanvasPos);
                    NodeHandle hoveredHandle = GetNodeHandleAtPosition(mousePosToCanvasPos);

                    SelectHandle(hoveredHandle);
                    SelectPanel(hoveredPanel);
                    //if (nodeSelectedCallback != null)
                    //{
                    //    if (selectedPanel != null)
                    //    {
                    //        nodeSelectedCallback(selectedPanel.Node);
                    //    }
                    //    else
                    //    {
                    //        nodeSelectedCallback(null);
                    //    }
                    //}

                    isPanning = selectedPanel == null && _selectedHandle == null;
                    isMouseDown = true;
                    mouseDownPos = mousePos;
                    //					requireRepaint = true;

                }
                else if (e.type == EventType.MouseUp)
                {
                    NodePanel hoveredPanel = GetNodePanelAtPosition(mousePosToCanvasPos);
                    NodeHandle hoveredHandle = GetNodeHandleAtPosition(mousePosToCanvasPos);

                    if (_draggingPanel != null)
                    {
                        if (_draggingPanel.Parent != null)
                        {
                            SetChildrenOrder(_draggingPanel.Parent);
                        }
                    }
                    else if (_draggingHandle != null)
                    {
                        if (hoveredHandle != null && hoveredHandle != _selectedHandle)
                        {
                            if (hoveredHandle.type == NodeHandle.HandleType.In)
                            {
                                ConnectHandles(_selectedHandle, hoveredHandle);
                            }
                        }
                        previewConnection = null;
                    }

                    previewConnection = null;
                    SelectHandle(null);
                    isPanning = false;
                    isMouseDown = false;
                    _draggingPanel = null;
                    _draggingHandle = null;
                }
            }

            if (e.type == EventType.ContextClick && IsMouseOnCanvas())
            {
                NodePanel hoveredPanel = GetNodePanelAtPosition(mousePosToCanvasPos);
                SelectPanel(hoveredPanel);
                if (nodeSelectedCallback != null)
                {
                    if (selectedPanel != null)
                    {
                        nodeSelectedCallback(selectedPanel.Node);
                    }
                    else
                    {
                        nodeSelectedCallback(null);
                    }
                }

                ContextData contextData = new ContextData()
                {
                    canvasPosition = CanvasUtility.ScreenToCanvasPosition(mousePos, canvasState),
                    mousePosition = mousePos,
                    nodePanel = hoveredPanel
                };


                GenericMenu menu = new GenericMenu();

                if (hoveredPanel != null)
                {
                    // clicked on canvas
                    if (canvasState.RootNodePanel != hoveredPanel && hoveredPanel.Parent == null)
                    {
                        menu.AddItem(new GUIContent("Set as Root Node"), false, SetRootNodeContextCallback, contextData);
                    }

                    if (hoveredPanel.Node.CanAddChild(null))
                    {
                        menu.AddItem(new GUIContent("Create Node As Child"), false, CreateNodeContextCallback, contextData);
                    }
                    else
                    {
                        menu.AddDisabledItem(new GUIContent("Create Node As Child"));
                    }


                    menu.AddItem(new GUIContent("Delete Node"), false, DeleteNodeContextCallback, contextData);

                }
                else
                {
                    menu.AddItem(new GUIContent("Create Node"), false, CreateNodeContextCallback, contextData);
                }


                menu.ShowAsContext();
                //				Event.current.Use ();
            }

            if (_draggingPanel != null)
            {
                DragPanel();
            }
            else if (_draggingHandle != null)
            {
                DragHandle();
            }

            // check if mouse moved away from panel to detect a drag
            if (isMouseDown && Vector2.Distance(mouseDownPos, mousePos) > dragThreshold)
            {
                if (selectedPanel != null && _draggingPanel == null)
                {
                    _draggingPanel = selectedPanel;
                    _selectionOffset = _draggingPanel.transform.position - CanvasUtility.ScreenToCanvasPosition(mousePos, canvasState);
                }
                else if (_selectedHandle != null && _draggingHandle == null)
                {
                    _draggingHandle = _selectedHandle;
                    CreatePreviewConnection(_draggingHandle);
                }
            }

            if (e.type == EventType.KeyUp)
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    if (_draggingPanel != null)
                    {
                        isMouseDown = false;
                        _draggingPanel = null;
                    }

                }
                else
                {
                    UpdateNavigation(e.keyCode);
                }
            }

            //if (e.type == EventType.ScrollWheel)
            //{
            //    canvasState.zoom = (float)Math.Round(Math.Min(2.0f, Math.Max(0.6f, canvasState.zoom + e.delta.y / 15)), 2);
            //    canvasRenderer.Repaint();
            //}
        }
        */

        /*
        /// <summary>
        /// Handles panel dragging
        /// </summary>
        void DragPanel()
        {
            _draggingPanel.transform.position = CanvasUtility.ScreenToCanvasPosition(mousePos, canvasState) + _selectionOffset;


            if (_draggingPanel.Parent != null)
            {
                _draggingPanel.Parent.outHandle.UpdateConnections();
            }

            _draggingPanel.UpdateAllConnections();

            return;
        }

        private void DragHandle()
        {
            previewConnection.SetEndPosition(CanvasUtility.ScreenToCanvasPosition(mousePos, canvasState));
            previewConnection.UpdatePoints(new List<NodeConnection>());

        }
        */

        private void SetChildrenOrder(NodePanel parent)
        {
            parent.Children.Sort((x, y) => x.transform.position.x.CompareTo(y.transform.position.x));
        }

        /*
        /// <summary>
        /// Handles edge panning while dragging
        /// </summary>
        void EdgeOfCanvasPan()
        {
            float panWhileDraggingThreshold = 35;
            float edgePanSpeed = 1f;

            //			float maxSpeed = 1.5f;
            //
            //			float maxRange = 30.0f;
            //
            //			float speedX = edgePanSpeed;
            //			float speedY = edgePanSpeed;


            float xMinEdge = canvasState.canvasRect.xMin + panWhileDraggingThreshold;
            float xMaxEdge = canvasState.canvasRect.xMax - panWhileDraggingThreshold;
            float yMinEdge = canvasState.canvasRect.yMin + panWhileDraggingThreshold;
            float yMaxEdge = canvasState.canvasRect.yMax - panWhileDraggingThreshold;

            if (mousePos.x < xMinEdge)
            {
                canvasState.panOffset.x += edgePanSpeed;
            }

            if (mousePos.x > xMaxEdge)
            {
                canvasState.panOffset.x -= edgePanSpeed;
            }

            if (mousePos.y < yMinEdge)
            {
                canvasState.panOffset.y += edgePanSpeed;
            }

            if (mousePos.y > yMaxEdge)
            {
                canvasState.panOffset.y -= edgePanSpeed;
            }

        }
        */

        /// <summary>
        /// Check for node traversal.. up/down moves between node children and siblings... left/right expands or collapses panels
        /// </summary>
        /// <param name="k">The keycode</param>
        void UpdateNavigation(KeyCode k)
        {
            switch (k)
            {
                case KeyCode.Delete:
                    DeleteNodePanelConfirmation(selectedPanels);
                    break;

                case KeyCode.UpArrow:
                    if (selectedPanels != null)
                        NavigateVertical(selectedPanels[0], -1);

                    break;

                case KeyCode.DownArrow:
                    if (selectedPanels != null)
                        NavigateVertical(selectedPanels[0], 1);

                    break;

                case KeyCode.LeftArrow:
                    //if (selectedPanel != null) {
                    //	if (selectedPanel.expanded) {
                    //		ExpandPanel (selectedPanel, false);
                    //	} else {
                    //		NavigateVertical (selectedPanel, -1, true);
                    //	}
                    //}

                    break;

                case KeyCode.RightArrow:
                    //if (selectedPanel != null) {
                    //	if (!selectedPanel.expanded) {
                    //		ExpandPanel (selectedPanel, true);
                    //	} else {
                    //		NavigateVertical (selectedPanel, 1, true);
                    //	}

                    //}

                    break;
            }
        }

        /// <summary>
        /// Navigates through nodePanels vertically
        /// </summary>
        void NavigateVertical(NodePanel nodePanel, int direction, bool selectPanelWithChildrenOnly = false)
        {
            NodePanel parent = nodePanel.Parent;

            // get only the visible children
            List<NodePanel> children = CanvasUtility.GetVisibleChildren(nodePanel);
            if (selectPanelWithChildrenOnly)
                children = CanvasUtility.FilterPanelsWithChildren(children);

            // navigating down.. check first if there's visible children... then go to that child
            if (direction > 0 && children != null && children.Count > 0)
            {
                SelectPanel(children[0]);
                return;
            }

            if (parent != null)
            {
                // move within siblings first... get the next index according to the direciton of the navigation
                // ie. if this nodePanel is the 3rd child and navigating down... the potential new index is 3 + 1 = 4
                int newIndex = parent.Children.IndexOf(selectedPanels[0]) + direction;

                // the "parentSiblings" are the aunts/uncles AND parent of the nodePanel (visible only)
                List<NodePanel> parentSiblings = parent.Parent != null ? CanvasUtility.GetVisibleChildren(parent.Parent) : null;
                if (selectPanelWithChildrenOnly)
                    parentSiblings = CanvasUtility.FilterPanelsWithChildren(parentSiblings);

                // siblings are this nodePanel's brothers/sisters (visible only)
                List<NodePanel> siblings = CanvasUtility.GetVisibleChildren(parent);
                if (selectPanelWithChildrenOnly)
                    siblings = CanvasUtility.FilterPanelsWithChildren(siblings);

                // the index is out of range, so select the parent
                if (newIndex < 0)
                {
                    SelectPanel(parent);
                    return;

                }
                else if (newIndex >= siblings.Count)
                {
                    // the nodePanel has siblings but it is out of range... select the next aunt/uncle in line
                    if (parentSiblings != null)
                    {
                        int parentIndex = parentSiblings.IndexOf(parent) + direction;
                        if (parentIndex < parentSiblings.Count)
                        {
                            SelectPanel(parentSiblings[parentIndex]);
                            return;
                        }
                    }
                }
                else
                {
                    // the newIndex is within the range of siblings... get the children of that sibling
                    List<NodePanel> siblingChildren = CanvasUtility.GetVisibleChildren(siblings[newIndex]);
                    if (selectPanelWithChildrenOnly)
                        siblingChildren = CanvasUtility.FilterPanelsWithChildren(siblingChildren);

                    if (direction < 0 && siblingChildren != null && siblingChildren.Count > 0)
                    {
                        // moving up... don't select the sibling because the sibling has children... instead select
                        // the last child of that sibling
                        SelectPanel(siblingChildren[siblingChildren.Count - 1]);
                        return;
                    }
                    else
                    {
                        // all other checks failed so this panel must have a valid sibling in the direction being navigated
                        // select that sibling now
                        SelectPanel(siblings[newIndex]);
                        return;
                    }
                }
            }
        }

        
        private bool IsInsideCanvas(Vector2 pos)
        {
            return canvasState.canvasRect.Contains(pos);
        }
        

        /// <summary>
        /// Determines whether this the nodePanel descends from another.
        /// </summary>
        bool IsDescendant(NodePanel checkDescendant, NodePanel potentialAncestor)
        {
            NodePanel parent;

            do
            {
                parent = checkDescendant.Parent;

                if (parent == potentialAncestor)
                    return true;

                checkDescendant = parent;
            } while (parent != null);

            return false;
        }

        private void AddToNodeTable(Node node, NodePanel nodePanel)
        {
            if (_nodeToNodePanelTable.ContainsKey(node))
            {
                Debug.LogError("Cannot add to nodeToNodePanelTabe, it already has the node as key: " + node.Id);
            }

            _nodeToNodePanelTable.Add(node, nodePanel);
            canvasState.AddNodePanel(nodePanel);
        }

        /// <summary>
        /// Deletes the node panel.
        /// </summary>
        void DeleteNodePanel(NodePanel nodePanel)
        {
            if (nodePanel == canvasState.RootNodePanel)
            {
                canvasState.RootNodePanel = null;
            }


            if (nodePanel.Parent != null)
            {
                RemoveChild(nodePanel);
            }

            for (int i = 0; i < nodePanel.Children.Count; i++)
            {
                nodePanel.Children[i].SetParent(null);
            }

            _nodeToNodePanelTable.Remove(nodePanel.Node);
            SelectPanel(null);

            Node node = nodePanel.Node;
            canvasState.RemoveNodePanel(nodePanel);

            canvasState.tree.RemoveNode(node);
            NodeFactory.Destroy(node);

            _canvasRenderer.Repaint();
        }

        /// <summary>
        /// Sets the parent of the NodePanel.
        /// </summary>
        /// <param name="childPanel">The child.</param>
        /// <param name="newParentPanel">The new parent.</param>
        public void SetNodePanelParent(NodePanel childPanel, NodePanel newParentPanel, bool setNodeParent = true)
        {
            NodePanel oldParentPanel = childPanel.Parent;

            childPanel.SetParent(newParentPanel);

            if (setNodeParent)
            {
                newParentPanel.Node.AddChild(childPanel.Node);
            }
            
            newParentPanel.SortChildren();
        }

        
        /// <summary>
        /// Creates the a NodePanel from a given node from scratch.
        /// </summary>
        /// <returns>The NodePanel that was created.</returns>
        /// <param name="node">The Node to create the NodePanel from</param>
        public NodePanel CreateNodePanel(Node node, bool addToNodeTable = true)
        {
            NodePanel nodePanel = NodePanelFactory.CreateNodePanel(node, canvasState);
            ResizeNodePanelFromName(nodePanel, nodePanel.label);
            return nodePanel;
        }


        private NodePanel CreateNodePanelAsChild(Node node, NodePanel parent, bool addToNodeTable = true)
        {
            NodePanel nodePanel = CreateNodePanel(node);
            SetNodePanelParent(nodePanel, parent);
            return nodePanel;
        }

        /// <summary>
        /// Creates a new node at a specific canvas position. Then creates the nodepanel for it.
        /// </summary>
        /// <returns>The node.</returns>
        /// <param name="nodeId">Node identifier.</param>
        /// <param name="position">Position.</param>
        public Node CreateNode(string nodeId, Vector2 position)
        {
            Node node = NodeFactory.Create(nodeId);
            node.guid = canvasState.tree.CreateGuid();
            node.Tree = canvasState.tree;
            NodeFactory.CreateNodeAsset(canvasState.tree, node);
            canvasState.tree.AddNode(node);

            NodePanel nodePanel = CreateNodePanel(node);

            if (canvasState.tree.nodes.Count == 1)
            {
                canvasState.tree.RootNode = node;
                SetRootNodePanel(nodePanel);

            }

            nodePanel.transform.position = CanvasUtility.SnapPosition(position, BehaviourEditor.SNAPPING_FACTOR);
            Debug.Log(nodePanel.transform.position);
            Debug.Log(nodePanel.inHandle.transform.position);
            AddToNodeTable(node, nodePanel);

            canvasState.SaveState();

            return node;
        }

        public Node CreateNodeAsChild(string nodeId, NodePanel parent)
        {
            Node node = NodeFactory.Create(nodeId);

            if (!parent.Node.CanAddChild(node))
            {
                Debug.LogError("Cannot create node " + node.name + " as child for " + parent.Node.name + " it is not a valid child.");
                return null;
            }

            NodePanel nodePanel = CreateNodePanelAsChild(node, parent);

            return node;
        }

        void PositionNodePanelsInARow(CanvasTransform startPos, NodePanel[] panels)
        {
            float offset = 0;
            for (int i = 0; i < panels.Length; i++)
            {
                panels[i].transform.position = new Vector2(startPos.rect.position.x + offset, startPos.position.y);
                offset = panels[i].transform.rect.width + BehaviourEditor.siblingPadding;
            }
        }

        void PositionNodePanelBesideSibling(NodePanel panel, NodePanel sibling)
        {
            panel.transform.position = new Vector2(sibling.transform.position.x + sibling.transform.rect.width + BehaviourEditor.siblingPadding, sibling.transform.position.y);
        }

        void SelectHandle(NodeHandle handle)
        {
            if (handle != null)
            {
                if (handle.type == NodeHandle.HandleType.Out)
                {
                    _selectedHandle = handle;
                }
                else
                {
                    RemoveChild(handle.NodePanel);
                }
            }
            else
            {
                _selectedHandle = null;
            }

            // make sure to remove the event when a new selection is made to prevent
            // weird layout changes when a new panel is selected
            if (Event.current != null && handle != null)
            {
                Event.current.Use();
            }
        }

        /// <summary>
        /// Selects the panel.
        /// </summary>
        public void SelectPanel(NodePanel panel, bool addToSelection = false)
        {
            if (!addToSelection)
            {
                selectedPanels.Clear();
            }

            selectedPanels.Add(panel);

            // make sure to remove the event when a new selection is made to prevent
            // weird layout changes when a new panel is selected
            if (Event.current != null && panel != null)
                Event.current.Use();

            nodeSelectedCallback?.Invoke(panel?.Node, panel);
        }


        /// <summary>
        /// Selects the panel by node.
        /// </summary>
        public void SelectPanelByNode(Node node)
        {
            SelectPanel(_nodeToNodePanelTable[node]);
        }

        // TODO: WTF IS THIS HSITE
        public void RemoveChild(NodePanel panel)
        {
            if (panel.Parent == null)
            {
                return;
            }

            panel.Parent.Node.RemoveChild(panel.Node);
            panel.Parent.RemoveChild(panel);

            panel.SetParent(null);
        }

        void InsertPanel(NodePanel child, int index)
        {
            //
            Rect firstSiblingRect = new Rect(child.Parent.Children[0].transform.rect);
            child.Parent.MoveChild(child, index);
            NodePanel[] children = child.Parent.Children.ToArray();

            float offset = 0;
            for (int i = 0; i < children.Length; i++)
            {
                Debug.Log(children[i].label + ": first sib");
                children[i].transform.position = new Vector2(firstSiblingRect.position.x + offset, firstSiblingRect.position.y);
                offset += children[i].transform.rect.width + BehaviourEditor.siblingPadding;
            }
        }

        public NodePanel GetNodePanel(Node node)
        {
            return _nodeToNodePanelTable[node];
        }
        
        /// <summary>
        /// Gets the node panel at the Canvas position. Use ScreenToCanvasPosition to convert from mouse position to canvas position!
        /// </summary>
        /// <returns>The node panel at canvas position.</returns>
        /// <param name="canvasPos">The canvas position.</param>
        public NodePanel GetNodePanelAtPosition(Vector2 canvasPos)
        {
            for (int i = canvasState.nodePanels.Count - 1; i >= 0; i--)
            {
                if (canvasState.nodePanels[i].isVisible && canvasState.nodePanels[i].transform.rect.Contains(canvasPos))
                {
                    return canvasState.nodePanels[i];
                }
            }

            return null;
        }

        public NodeHandle GetNodeHandleAtPosition(Vector2 canvasPos)
        {
            for (int i = 0; i < canvasState.nodePanels.Count; i++)
            {
                NodeHandle inHandle = canvasState.nodePanels[i].inHandle;
                NodeHandle outHandle = canvasState.nodePanels[i].outHandle;
                if (canvasState.nodePanels[i].isVisible)
                {
                    if (inHandle.transform.rect.Contains(canvasPos))
                    {
                        return inHandle;
                    }
                    else if (outHandle.transform.rect.Contains(canvasPos))
                    {
                        return outHandle;
                    }
                }
            }

            return null;
        }

        #region Context Callbacks

        void CreateNodeContextCallback(object obj)
        {
            ContextData contextData = (ContextData)obj;
            BehaviourEditorWindow.nodeInspector.ShowCreateNodePanel(true, contextData);
        }

        void DeleteNodeContextCallback(object obj)
        {
            ContextData contextData = (ContextData)obj;
            if (contextData.nodePanel != null)
            {
                List<NodePanel> s = new List<NodePanel>();
                s.Add(contextData.nodePanel);
                DeleteNodePanelConfirmation(s);
            }
        }

        void DeleteNodePanelConfirmation(List<NodePanel> nodePanels)
        {
            Debug.Log("delete?~?");
            if (EditorUtility.DisplayDialog("Delete Node", "Are you sure you want to delete " + NodeFactory.GetDisplayName(nodePanels[0].Node.Id) + " and all its descendants?", "Yes", "No"))
            {
                for (int i = 0; i < nodePanels.Count; i++)
                {
                    DeleteNodePanel(nodePanels[i]);
                }
            }
        }

        #endregion

        void SetRootNodeContextCallback(object obj)
        {
            ContextData contextData = (ContextData)obj;
            SetRootNodePanel(contextData.nodePanel);
        }

        public void RegisterNodeSelectedCallback(Action<Node, NodePanel> callback)
        {
            nodeSelectedCallback += callback;
        }

        public void UnregisterNodeSelectedCallback(Action<Node, NodePanel> callback)
        {
            nodeSelectedCallback -= callback;
        }

        public void Resize(Rect newRect)
        {

            canvasState.canvasRect = newRect;
        }


        public void OnNodeNameChanged(NodePanel selectedPanel, string newName)
        {
            ResizeNodePanelFromName(selectedPanel, newName);
        }

        private void ResizeNodePanelFromName(NodePanel selectedPanel, string name)
        {
            Vector2 size = BehaviourEditorStyles.defaultSkin.label.CalcSize(new GUIContent(name));
            if (size.x > BehaviourEditorStyles.NODE_DEFAULT_WIDTH - BehaviourEditorStyles.NODE_LABEL_PADDING)
            {
                selectedPanel.transform.width = size.x + BehaviourEditorStyles.NODE_LABEL_PADDING;
            }
            else
            {
                selectedPanel.transform.width = BehaviourEditorStyles.NODE_DEFAULT_WIDTH;
            }
        }
        
    }


}



