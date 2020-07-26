using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeeTree.Editor {
	public class NodePanelInputHandler : CanvasInputHandler
	{
        public List<NodePanel> topLevelPanels;

        public Vector2 deltaPos;

        public NodePanelInputHandler(NodeCanvas nodeCanvas) : base(nodeCanvas)
        {
        }

        public override void OnPointerDown(PointerEvent evt)
        {
            NodePanel downPanel = (NodePanel)evt.downObject;

            if (!_nodeCanvas.selectedPanels.Contains(downPanel))
            {
                _nodeCanvas.SelectPanel(downPanel, evt.shift);
            }
            else if (evt.shift)
            {
                _nodeCanvas.selectedPanels.Remove(downPanel);
            }

        }

        public override void OnPointerDrag(PointerEvent evt)
        {
            if (evt.shift)
            {
                return;
            }

            if (topLevelPanels == null)
            {
                topLevelPanels = GetTopLevelPanels(_nodeCanvas.selectedPanels);
            }

            float factor = BehaviourEditor.SNAPPING_FACTOR;

            deltaPos += evt.deltaPos;

            float deltaX = Mathf.Abs(deltaPos.x) > factor ? deltaPos.x : 0;
            float deltaY = Mathf.Abs(deltaPos.y) > factor ? deltaPos.y : 0;

            for (int i = 0; i < topLevelPanels.Count; i++)
            {
                NodePanel draggingPanel = topLevelPanels[i];
                draggingPanel.transform.position += new Vector2(deltaX, deltaY);
                draggingPanel.transform.position = CanvasUtility.SnapPosition(draggingPanel.transform.position, factor);

                if (draggingPanel.Parent != null)
                {
                    draggingPanel.Parent.outHandle.UpdateConnections();
                }

                draggingPanel.UpdateAllConnections();
            }

            deltaPos = new Vector2(deltaX == 0 ? deltaPos.x : 0, deltaY == 0 ? deltaPos.y : 0);
        }

        public override void OnPointerDrop(PointerEvent evt)
        {
            List<NodePanel> updatedPanels = new List<NodePanel>();
            for (int i = 0; i < _nodeCanvas.selectedPanels.Count; i++)
            {
                NodePanel parent = _nodeCanvas.selectedPanels[i].Parent; 
                if (parent != null && !updatedPanels.Contains(parent))
                {
                    updatedPanels.Add(parent);
                    if (parent.SortChildren())
                    {
                        _nodeCanvas.canvasState.SaveState();
                    }
                }
            }
            
            topLevelPanels = null;
            deltaPos = Vector2.zero;
        }

        private List<NodePanel> GetTopLevelPanels(List<NodePanel> panels)
        {
            List<NodePanel> remaining = new List<NodePanel>(panels);
            List<NodePanel> processed = new List<NodePanel>();

            List<NodePanel> result = new List<NodePanel>(panels);

            while (remaining.Count > 0)
            {
                NodePanel current = remaining[0];
                remaining.Remove(current);

                processed.Add(current);

                List<NodePanel> descendants = current.Children;

                while (descendants.Count > 0)
                {
                    NodePanel child = descendants[0];
                    descendants.Remove(child);

                    if (result.Contains(child))
                    {
                        result.Remove(child);
                    }

                    if (processed.Contains(child))
                    {
                        continue;
                    }

                    descendants.AddRange(child.Children);
                }

            }

            foreach (var item in result)
            {
                Debug.Log(item.guid);
            }

            return result;


            //List<NodePanel> result = new List<NodePanel>(panels);
            //List<NodePanel> unique = new List<NodePanel>();

            //for (int i = 0; i < panels.Count; i++)
            //{
            //    List<NodePanel> descendants = GetDescendants(panels[i]);
            //    Debug.Log(descendants.Count);
            //    for (int j = 0; j < descendants.Count; j++)
            //    {
            //        if (!unique.Contains(descendants[j]))
            //        {
            //            unique.Add(descendants[j]);
            //        }
            //    }

            //    if (unique.Contains(panels[i]))
            //    {
            //        result.Remove(panels[i]);
            //    }
            //}

            //Debug.Log("UNIQ");
            //for (int i = 0; i < unique.Count; i++)
            //{
            //    Debug.Log(unique[i].label);
            //}

            //Debug.Log("RESULT");
            //for (int i = 0; i < result.Count; i++)
            //{
            //    Debug.Log(result[i].label);
            //}

            //return result;
        }

        private List<NodePanel> GetDescendants (NodePanel panel)
        {
            List<NodePanel> result = new List<NodePanel>();

            for (int i = 0; i < panel.Children.Count; i++)
            {
                result.Add(panel.Children[i]);

                List<NodePanel> descendants = GetDescendants(panel.Children[i]);
                for (int j = 0; j < descendants.Count; j++)
                {
                    if (!result.Contains(descendants[j]))
                    {
                        result.Add(descendants[j]);
                    }
                }
            }

            return result;
        }

    }
}