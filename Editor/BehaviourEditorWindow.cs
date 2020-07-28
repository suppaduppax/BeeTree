using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

namespace BeeTree.Editor
{
    public class BehaviourEditorWindow : EditorWindow
    {

        public static BehaviourEditorWindow editor;
        public static NodeCanvas nodeCanvas;
        public static NodeInspector nodeInspector;

        private NodeCanvas _nodeCanvas;
        private NodeInspector _nodeInspector;
        private NodeCreator _nodeCreator;

        private Rect _menuRect;
        private Vector2 _curWindowSize;

        [MenuItem("Window/Behaviour Editor")]
        public static void OpenWindow()
        {

            editor = GetWindow<BehaviourEditorWindow>();
            editor.minSize = new Vector2(600, 300);
            editor.titleContent = new GUIContent("Behaviour Editor");
            editor._curWindowSize = editor.position.size;
            editor._menuRect = editor.GetMenuRect();

            nodeCanvas = new NodeCanvas(editor.GetCanvasRect());
            nodeInspector = new NodeInspector(editor.GetInspectorRect(), nodeCanvas);

            Selection.selectionChanged += TryLoadSelectedAsset;
                                    
            NodeFactory.FetchNodes();
            NodePanelFactory.FetchCustomPanels();
            CustomFieldDrawerManager.FetchCustomFieldDrawers();

            // tries to load the selected asset if it is a behaviour tree when the window is open
            // and also when the assembly is reloaded
            TryLoadSelectedAsset();
        }

        public static void RepaintWindow ()
        {
            editor.Repaint();
        }

        public void OnGUI()
        {
            EnsureGUI();

            wantsMouseMove = true;

            if (position.size != _curWindowSize)
            {
                _curWindowSize = position.size;
                OnResize();
            }
            
            OnResize();

            GUI.depth = 0;
            DrawMenu();

            nodeCanvas.Update();
            nodeInspector.Update();

            GUI.depth = 10;
        }
        
        public Rect GetCanvasRect()
        {
            Rect r = GetMenuRect();
            return new Rect(0, r.height, position.width - GetInspectorRect().width, position.height - r.height);
        }

        private void DrawMenu()
        {
            Rect menuContent = new Rect(new Vector2(_menuRect.position.x + 5, _menuRect.position.y), new Vector2(322, _menuRect.height));

            GUILayout.BeginArea(_menuRect, EditorStyles.toolbar);
            GUILayout.BeginArea(menuContent, EditorStyles.toolbar);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(BehaviourEditor.debugView ? "Normal View" : "Debug View", EditorStyles.toolbarButton))
            {
                ToggleDebugView();
            }

            GUILayout.EndHorizontal();

            GUILayout.EndArea();
            GUILayout.EndArea();
        }

        //void OnNodeCreated (Node n) {
        //	Debug.Log ("OnNodeCreated!");
        //}

        private void ToggleDebugView()
        {
            BehaviourEditor.debugView = BehaviourEditor.debugView == false ? BehaviourEditor.debugView = true : false;
        }


        private static void TryLoadSelectedAsset()
        {
            UnityEngine.Object activeObject = Selection.activeObject;

            if (Selection.activeObject == null)
            {
                return;
            }

            if (Selection.activeObject.GetType() == typeof(BehaviourTree))
            {
                nodeCanvas.Load((BehaviourTree) Selection.activeObject);
                BehaviourEditorWindow.RepaintWindow();
            }
            else if (Selection.activeObject is GameObject)
            {
                var controller = ((GameObject) activeObject).GetComponent<BehaviourController>();
                if (controller == null)
                {
                    return;
                }
                
                if (Application.isPlaying)
                {
                    nodeCanvas.LoadBehaviourController(controller);
                }
                else
                {
                    var tree = controller.behaviourTree;
                    if (tree == null)
                    {
                        return;
                    }
                    
                    nodeCanvas.Load(tree);
                    BehaviourEditorWindow.RepaintWindow();
                }
            }
        }
        private void EnsureGUI()
        {
            if (editor == null)
                OpenWindow();

            if (nodeCanvas == null)
                nodeCanvas = new NodeCanvas(editor.GetCanvasRect());

            if (nodeInspector == null)
                nodeInspector = new NodeInspector(editor.GetInspectorRect(), nodeCanvas);

        }

        private void OnResize()
        {
            _menuRect = GetMenuRect();
            nodeCanvas.Resize(GetCanvasRect());
            nodeInspector.SetRect(GetInspectorRect());
        }

        private Rect GetMenuRect()
        {
            return new Rect(0, 0, position.width, 18);
        }



        private Rect GetInspectorRect()
        {
            int width = 225;
            return new Rect(position.width - width, _menuRect.height, width, position.height - _menuRect.height);
        }

    }
}
