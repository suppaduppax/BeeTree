using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;

/*
 *	TODO: Separate Inspector from VariableInspector, and NodeCreator 
 * 
 */


namespace BeeTree.Editor
{
    public class NodeInspector
    {

        Rect panelRect;
        Rect contentRect;

        Rect menuRect;

        Node selectedNode;
        private NodePanel selectedPanel;
        
        Node oldSelectedNode;
        bool redrawInspector = false;

        //		int menuHeight;

        int selectedMenu = 0;
        bool renameable = true;

        private NodeCanvas _nodeCanvas;

        bool isCreatingNode = false;
        Vector2 nodeScroll;
        int createNodeIndex = -1;
        string search = "";

        ContextData contextData;

        //List<string> NodeFieldNames;

        //Dictionary<string, FieldInfo> nodeDefaultVariables;
        //Dictionary<string, FieldInfo> nodeGetterVariables;
        //Dictionary<string, FieldInfo> nodeSetterVariables;
        
        private List<FieldInfo> _fields;
        private Dictionary<FieldInfo, NodeField> _fieldToAttributeDict;

        Vector2 variableScroll;

        bool showComposites = true;
        bool showDecorators = true;
        bool showLeaves = true;

     
        public NodeInspector(Rect rect, NodeCanvas nodeCanvas)
        {
            _fields = new List<FieldInfo>();
            _fieldToAttributeDict = new Dictionary<FieldInfo, NodeField>();

            SetRect(rect);

            _nodeCanvas = nodeCanvas;

            _nodeCanvas.RegisterNodeSelectedCallback(OnNodeSelected);
        }

        public void SetRect(Rect rect)
        {
            panelRect = rect;
            menuRect = new Rect(panelRect.x, panelRect.y, panelRect.width, 18);

            int padding = 0;
            RectOffset paddingOffset = new RectOffset(padding, padding, ((int)menuRect.height) + padding, padding);
            contentRect = paddingOffset.Remove(panelRect);
        }


        void OnNodeSelected(Node n, NodePanel p)
        {
            redrawInspector = true;

            selectedNode = n;
            selectedPanel = p;

            if (selectedNode == null)
                return;

            renameable = true;
            System.Type nodeType = selectedNode.GetType();
            object[] nodeAttrs = nodeType.GetCustomAttributes(typeof(NodeAttribute), true);

            _fields.Clear();
            _fieldToAttributeDict.Clear();

            NodeAttribute attr = (NodeAttribute)nodeAttrs[0];
            renameable = attr.renameable;

            //NodeFieldNames = new List<string>();
            //nodeDefaultVariables = new Dictionary<string, FieldInfo>();
            //nodeGetterVariables = new Dictionary<string, FieldInfo>();
            //nodeSetterVariables = new Dictionary<string, FieldInfo>();
            _fields = new List<FieldInfo>();
            _fieldToAttributeDict = new Dictionary<FieldInfo, NodeField>();

            foreach (var field in nodeType.GetFields())
            {
                NodeField[] varField = (NodeField[])field.GetCustomAttributes(typeof(NodeField), true);

                bool addField = false;
                string label = null;
                string fieldName = null;

                NodeField[] nodeField = (NodeField[])field.GetCustomAttributes(typeof(NodeField), true);

                if (nodeField != null && nodeField.Length > 0)
                {
                    _fields.Add(field);
                    _fieldToAttributeDict.Add(field, nodeField[0]);
                }

            }
        }

        public VariableAttribute[] GetVariableAttributes(Node node)
        {
            List<VariableAttribute> result = new List<VariableAttribute>();

            System.Type nodeType = node.GetType();
            object[] nodeAttrs = nodeType.GetCustomAttributes(typeof(NodeAttribute), true);


            foreach (var field in nodeType.GetFields())
            {
                VariableAttribute[] varAttr = (VariableAttribute[])field.GetCustomAttributes(typeof(VariableAttribute), true);

                if (varAttr != null && varAttr.Length > 0)
                {
                    result.Add(varAttr[0]);
                }
            }

            return result.ToArray();
        }

        public FieldInfo[] GetVariableAttributeFields(Node node)
        {
            List<FieldInfo> result = new List<FieldInfo>();

            System.Type nodeType = node.GetType();
            object[] nodeAttrs = nodeType.GetCustomAttributes(typeof(NodeAttribute), true);


            foreach (var field in nodeType.GetFields())
            {
                VariableAttribute[] varAttr = (VariableAttribute[])field.GetCustomAttributes(typeof(VariableAttribute), true);

                if (varAttr != null && varAttr.Length > 0)
                {
                    result.Add(field);
                }
            }

            return result.ToArray();
        }

        public void Update()
        {
            Draw();
            ProcessInput();
        }

        void ProcessInput()
        {
            if (Event.current == null)
                return;

            if (Event.current.type == EventType.MouseDown)
            {
                if (!panelRect.Contains(Event.current.mousePosition))
                {
                    isCreatingNode = false;
                }
            }
        }

        void Draw()
        {
            //			Debug.Log (isCreatingNode);
            if (isCreatingNode)
            {
                DrawCreateNode();
                return;
            }

            //			if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp && !panelRect.Contains (Event.current.mousePosition))
            //				return;

            DrawMenu();

            //			if (Event.current.type != EventType.MouseDown)
            //			{
            GUILayout.BeginArea(contentRect);

            switch (selectedMenu)
            {
                case 1:
                    DrawVariables();
                    break;

                case 0:
                default:
                    DrawInspector();
                    break;
            }

            GUILayout.EndArea();
        }

        void DrawMenu()
        {
            GUILayout.BeginArea(menuRect);
            //			EditorGUILayout.BeginHorizontal (EditorStyles.toolbar);


            //			GUILayout.Button ("Inspector", EditorStyles.toolbarButton);
            //			GUILayout.Button ("Parameters", EditorStyles.toolbarButton, GUILayout.);

            int newSelectedMenu = GUILayout.Toolbar(selectedMenu, new string[] { "Inspector", "Variables" }, EditorStyles.toolbarButton);
            if (newSelectedMenu != selectedMenu)
            {
                selectedMenu = newSelectedMenu;
                redrawInspector = true;
            }

            //			EditorGUILayout.EndHorizontal ();
            GUILayout.EndArea();
        }

        void DrawInspector()
        {
            if (selectedNode == null)
                return;

            //			Debug.Log (selectedNode);

            //			if (Event.current.type != EventType.Repaint || Event.current.type != EventType.Layout || Event.current.type != )
            //				return;

            EditorGUILayout.LabelField(selectedNode.Id);
            DrawId();

            if (_fields != null)
            {

                for (int i = 0; i < _fields.Count; i++)
                {

                    EditorGUILayout.Separator();
                    EditorGUILayout.BeginVertical(EditorStyles.textArea);

                    FieldInfo field = _fields[i];
                    string fieldName = field.Name;
                    NodeField nodeField = _fieldToAttributeDict[field];

                    if (nodeField.GetType() == typeof(NodeField))
                    {
                        CustomFieldDrawer customFieldDrawer = CustomFieldDrawerManager.GetCustomFieldDrawer(field.FieldType);
                        if (customFieldDrawer == null)
                        {
                            // draw normally if you can...?
                            if (field.FieldType == typeof(bool))
                            {
                                field.SetValue(selectedNode,EditorGUILayout.ToggleLeft(nodeField.label, (bool)field.GetValue(selectedNode)));
                            }
                        }
                        else
                        {
                            customFieldDrawer.targetField = field;
                            customFieldDrawer.targetNode = selectedNode;
                            customFieldDrawer.Draw();
                        }
                    }
                    else if (nodeField.GetType() == typeof(VariableGetter))
                    {
                        VariableGetter getterAttr = (VariableGetter)nodeField;
                        if (getterAttr.overrideField == null)
                        {
                            // no override just draw normal
                            DrawVariableDropdown(field, getterAttr);
                        }
                        else
                        {
                            string fieldValue = (string)field.GetValue(selectedNode);
                            if (fieldValue == null)
                            {
                                // draw field normally
                                
                            }
                        }

                    }
                    else if (nodeField.GetType() == typeof(VariableSetter))
                    {
                        VariableSetter setterAttr = (VariableSetter)nodeField;
                        DrawVariableDropdown(field, setterAttr);
                    }

                    EditorGUILayout.EndVertical();
                }
            }

            if (Application.isPlaying)
            {
                EditorGUILayout.TextArea(selectedNode.StateMessage, GUILayout.Height(60));
            }
            
            if (redrawInspector)
            {
                GUI.FocusControl(null);
                redrawInspector = false;
            }
        }

        private void DrawVariableDropdown(FieldInfo field, VariableAttribute varAttr)
        {
            List<string> variables = GetVariables(true);

            EditorGUILayout.LabelField(varAttr.label);
            object curGetterVariableName = field.GetValue(selectedNode);

            int index = 0;
            if (curGetterVariableName != null)
            {
                string varNameToString = curGetterVariableName.ToString();
                index = variables.IndexOf(varNameToString);
                if (index < 0)
                    index = 0;
            }

            int newIndex = EditorGUILayout.Popup(index, variables.ToArray());
            if (newIndex > 0)
            {
                field.SetValue(selectedNode, variables[newIndex]);
            }
            else
            {
                field.SetValue(selectedNode, null);
            }
        }

        void DrawCreateNode()
        {
            float closeWidth = 25;
            float searchPaddingX = 10;
            float searchPaddingTop = 1;
            float searchPaddingBottom = 2;
            Rect closeRect = new Rect(menuRect.width - closeWidth, 0, closeWidth, menuRect.height);
            Rect searchRect = new Rect(searchPaddingX, searchPaddingTop, menuRect.width - closeRect.width - (searchPaddingX * 2), menuRect.height - (searchPaddingTop + searchPaddingBottom));

            GUILayout.BeginArea(menuRect, EditorStyles.toolbar);

            GUILayout.BeginArea(closeRect);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("X", EditorStyles.toolbarButton))
            {
                isCreatingNode = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            GUILayout.BeginArea(searchRect);
            GUILayout.BeginHorizontal();
            GUISkin builtinSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            search = GUILayout.TextField(search, builtinSkin.FindStyle("ToolbarSeachTextField"));
            if (GUILayout.Button("", builtinSkin.FindStyle("ToolbarSeachCancelButton")))
            {
                // Remove focus if cleared
                search = "";
                GUI.FocusControl(null);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            GUILayout.EndArea();

            GUILayout.BeginArea(contentRect);
            nodeScroll = GUILayout.BeginScrollView(nodeScroll);

            List<Node> compositeNodes = NodeFactory.GetPrototypes(typeof(CompositeNode), false);
            List<Node> decoratorNodes = NodeFactory.GetPrototypes(typeof(DecoratorNode), false);
            List<Node> leafNodes = NodeFactory.GetPrototypes(typeof(LeafNode), false);
            //Debug.Log(compositeNodes.Count);

            //			compositites.Sort ();
            //			decorators.Sort ();
            //			leaves.Sort ();


            List<string> leafNames = NodeFactory.GetDisplayNames(leafNodes);
            List<string> compositeNames = NodeFactory.GetDisplayNames(compositeNodes);
            List<string> decoratorNames = NodeFactory.GetDisplayNames(decoratorNodes);

            if (search != null && search != "")
            {
                leafNames = FilterNames(leafNames, search);
                compositeNames = FilterNames(compositeNames, search);
                decoratorNames = FilterNames(decoratorNames, search);
            }

            if (compositeNames.Count > 0)
                showComposites = EditorGUILayout.Foldout(showComposites, "Composites");

            if (showComposites)
            {
                for (int i = 0; i < compositeNames.Count; i++)
                {
                    if (GUILayout.Button(NodeFactory.GetDisplayName(compositeNames[i]), EditorStyles.toolbarButton))
                    {
                        //						NodeTypes.Create (compositeNames [i]);
                        CreateNode(compositeNames[i]);
                        isCreatingNode = false;
                    }
                }
            }

            if (decoratorNames.Count > 0)
                showDecorators = EditorGUILayout.Foldout(showDecorators, "Decorators", EditorStyles.foldout);

            if (showDecorators)
            {
                for (int i = 0; i < decoratorNames.Count; i++)
                {
                    if (GUILayout.Button(NodeFactory.GetDisplayName(decoratorNames[i]), EditorStyles.toolbarButton))
                    {
                        CreateNode(decoratorNames[i]);
                        isCreatingNode = false;
                    }
                }
            }

            if (leafNames.Count > 0)
                showLeaves = EditorGUILayout.Foldout(showLeaves, "Leaves", EditorStyles.foldout);

            if (showLeaves)
            {
                for (int i = 0; i < leafNames.Count; i++)
                {
                    if (GUILayout.Button(NodeFactory.GetDisplayName(leafNames[i]), EditorStyles.toolbarButton))
                    {
                        CreateNode(leafNames[i]);
                        isCreatingNode = false;
                    }
                }
            }

            //			createNodeIndex = GUILayout.SelectionGrid(createNodeIndex, NodeTypes.GetDisplayNames(leaves).ToArray(), 1, EditorStyles.toolbarButton);

            GUILayout.EndScrollView();
            //			if (GUILayout.Button ("Close", EditorStyles.toolbarButton)) {
            //				Debug.Log ("Close");
            //			}
            GUILayout.EndArea();
        }

        List<string> FilterNames(List<string> unfilteredList, string filter, bool caseSensitive = false)
        {
            if (!caseSensitive)
                filter = filter.ToLower();

            List<string> result = new List<string>();
            for (int i = 0; i < unfilteredList.Count; i++)
            {
                string toCheck = unfilteredList[i];

                if (!caseSensitive)
                    toCheck = toCheck.ToLower();

                if (toCheck.Contains(filter))
                    result.Add(unfilteredList[i]);
            }

            return result;
        }

        void DrawId()
        {
            if (!renameable)
                GUI.enabled = false;

            EditorGUILayout.BeginVertical(EditorStyles.textArea);
            EditorGUILayout.LabelField("Name");
            EditorGUI.BeginChangeCheck();
            string nodeId = EditorGUILayout.TextField(selectedNode.name);
            
            if (nodeId == "")
                nodeId = NodeFactory.GetDisplayName(selectedNode.Id);

            if (GUI.changed)
            {
                _nodeCanvas.OnNodeNameChanged(selectedPanel, nodeId);
            }
            
            EditorGUI.EndChangeCheck();
           
            EditorGUILayout.EndVertical();

            selectedNode.name = nodeId;

        }


        void DrawVariables()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                _nodeCanvas.canvasState.tree.CreateVariable("NewVariable");
            }

            EditorGUILayout.EndHorizontal();

            variableScroll = EditorGUILayout.BeginScrollView(variableScroll);
            string[] variables = _nodeCanvas.canvasState.tree.VariableNames;

            for (int i = 0; i < variables.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();

                string newName = EditorGUILayout.TextField(variables[i], EditorStyles.textField);

                if (newName != variables[i] && newName != "")
                {
                    RenameVariable(variables[i], newName);
                }

                if (GUI.changed)
                {
                    BehaviourEditorWindow.RepaintWindow();
                }

                if (GUILayout.Button("-", EditorStyles.miniButton))
                {
                    if (EditorUtility.DisplayDialog("Delete " + variables[i], "Are you sure you want to delete this variable?", "Yes", "No"))
                    {
                        _nodeCanvas.canvasState.tree.DeleteVariable(variables[i]);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            if (redrawInspector || Event.current.keyCode == KeyCode.Return)
            {
                redrawInspector = false;
                GUI.FocusControl(null);
                BehaviourEditorWindow.editor.Repaint();
            }


        }

        public void RenameVariable(string oldName, string newName)
        {
            _nodeCanvas.canvasState.tree.RenameVariable(oldName, newName);

            List<Node> nodes = _nodeCanvas.canvasState.tree.nodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                FieldInfo[] varFields = GetVariableAttributeFields(_nodeCanvas.canvasState.tree.nodes[i]);
                for (int j = 0; j < varFields.Length; j++)
                {
                    if ((string)varFields[j].GetValue(nodes[i]) == oldName)
                    {
                        varFields[j].SetValue(nodes[i], newName);
                    }
                }
            }


        }

        public void ShowCreateNodePanel(bool show, ContextData data)
        {
            isCreatingNode = show;
            createNodeIndex = -1;
            this.contextData = data;
        }

        void CreateNode(string nodeId)
        {
            Node selectedNode = contextData.nodePanel == null ? _nodeCanvas.CreateNode(nodeId, contextData.canvasPosition) : _nodeCanvas.CreateNodeAsChild(nodeId, contextData.nodePanel);
            _nodeCanvas.SelectPanelByNode(selectedNode);
            
            OnNodeSelected(selectedNode, _nodeCanvas.GetNodePanel(selectedNode));
        }


        List<string> GetVariables(bool addNone = false)
        {
            List<string> result = new List<string>();
            if (addNone)
                result.Add("None");

            if (_nodeCanvas.canvasState.tree.VariableNames != null)
            {
            	result.AddRange(_nodeCanvas.canvasState.tree.VariableNames);
            }

            return result;
        }

        List<string> GetVariables(Type type, bool addNone = false)
        {
            List<string> result = new List<string>();

            if (addNone)
                result.Add("None");

            //for (int i = 0; i < nodeCanvas.canvasState.tree.variableNames.Count; i++) {
            //             if (nodeCanvas.canvasState.tree.variableTable[nodeCanvas.canvasState.tree.variableNames[i]].GetType() == type)
            //             {
            //                 result.Add(nodeCanvas.canvasState.tree.variableNames[i]);
            //             }
            //         }

            return result;
        }


    }
}