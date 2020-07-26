using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace BeeTree.Editor
{
    public class SequenceNodePanel : CustomNodePanel
    {
        public override Type NodeType
        {
            get { return typeof(Sequence); }
        }

        public override NodePanel Create(Node node, CanvasState canvasState)
        {
            return NodePanelFactory.CreateDefaultNodePanel(node, canvasState);
        }
    }
}