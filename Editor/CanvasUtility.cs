using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;
using System;

using System.Text.RegularExpressions;

namespace BeeTree.Editor
{
    public class CanvasUtility
    {
        public static Vector2 SnapPosition(Vector2 input, float factor = 1f)
        {
            if (factor <= 0f)
                throw new UnityException("factor argument must be above 0");

            float x = Mathf.Round(input.x / factor) * factor;
            float y = Mathf.Round(input.y / factor) * factor;

            return new Vector2(x, y);
        }

        public static Vector2 ScreenToCanvasPosition(Vector2 screenPosition, CanvasState canvasState)
        {
            Vector2 offset = canvasState.zoomPos + canvasState.panOffset / canvasState.zoom;

            return screenPosition - offset - canvasState.canvasRect.position;
        }


        public static Rect WorldToCanvasRect(Rect rect, CanvasState canvasState)
        {
            Rect r = new Rect(rect);

            Vector2 offset = canvasState.zoomPos + canvasState.panOffset / canvasState.zoom;
            r.position += offset;

            return r;
        }

        public static Vector2 WorldToCanvasPoint(Vector2 worldPoint, CanvasState canvasState)
        {
            Vector2 offset = canvasState.zoomPos + canvasState.panOffset / canvasState.zoom;
            return worldPoint + offset;
        }

        public static Texture GetIcon(Type type)
        {
            if (typeof(CompositeNode).IsAssignableFrom(type))
                return BehaviourEditor.LoadTexture("composite.png");

            if (typeof(DecoratorNode).IsAssignableFrom(type))
                return BehaviourEditor.LoadTexture("decorator.png");

            if (typeof(LeafNode).IsAssignableFrom(type))
                return BehaviourEditor.LoadTexture("leaf.png");

            return BehaviourEditor.LoadTexture("nodeIcon.png");
        }


        public static List<NodePanel> GetVisibleChildren(NodePanel nodePanel)
        {
            if (nodePanel.childrenGuids == null)
                return null;

            List<NodePanel> result = new List<NodePanel>();

            for (int i = 0; i < nodePanel.Children.Count; i++)
            {
                if (nodePanel.Children[i].isVisible)
                    result.Add(nodePanel.Children[i]);
            }

            if (nodePanel.childrenGuids.Count == 0)
                return null;

            return result;
        }

        public static List<NodePanel> FilterPanelsWithChildren(List<NodePanel> list)
        {
            if (list == null)
                return null;

            List<NodePanel> result = new List<NodePanel>();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].childrenGuids != null && list[i].childrenGuids.Count > 0)
                    result.Add(list[i]);
            }

            if (result.Count == 0)
                return null;

            return result;
        }

        public static void GetNodePanelsInHierarchy(NodePanel parent, ref List<NodePanel> list)
        {
            list.Add(parent);

            if (parent.childrenGuids != null)
            {
                for (int i = 0; i < parent.childrenGuids.Count; i++)
                {
                    GetNodePanelsInHierarchy(parent.Children[i], ref list);
                }
            }
        }


        public static string NodePanelHeirarchyToString(NodePanel nodePanel, ref int depthLevel)
        {
            string depthString = "";
            for (int i = 0; i < depthLevel; i++)
            {
                depthString += "-";
            }


            string result = depthString + nodePanel.Node.name + "\n";

            depthLevel++;
            if (nodePanel.childrenGuids != null)
            {
                for (int i = 0; i < nodePanel.childrenGuids.Count; i++)
                {
                    result += NodePanelHeirarchyToString(nodePanel.Children[i], ref depthLevel);
                }
            }

            depthLevel--;

            return result;
        }

        public static string NodeHierarchyToString(Node node, ref int depthLevel)
        {
            string depthString = "";
            for (int i = 0; i < depthLevel; i++)
            {
                depthString += "-";
            }


            string result = depthString + node.name + "\n";

            depthLevel++;
            if (node.Children != null)
            {
                for (int i = 0; i < node.Children.Count; i++)
                {
                    result += NodeHierarchyToString(node.Children[i], ref depthLevel);
                }
            }

            depthLevel--;

            return result;
        }


        const string EXTENSION_PATTERN = "[.][^./]*$";
        const string FILE_PATTERN = "[^/]*$";
        const string DIRECTORY_PATTERN = ".*[/]";

        public static string GetDirectory(string path)
        {
            return Regex.Replace(path, DIRECTORY_PATTERN, "");
        }

        public static string RemoveExtension(string path)
        {
            return Regex.Replace(path, EXTENSION_PATTERN, "");
        }

        public static string GetFile(string path)
        {
            return Regex.Match(path, FILE_PATTERN).Value;
        }
    }
}