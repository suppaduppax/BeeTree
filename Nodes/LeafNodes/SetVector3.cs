using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeeTree {
	public class SetVector3 : LeafNode
	{
        public override string Id
        {
            get { return "SetVector3"; }
        }

        public override bool TickEnabled {
            get { return false; }
        }

        public string varName;
        public Vector3 vector3;

        public override void Initialize()
        {
            base.Initialize();

            if (varName == null)
            {
                Terminate(NodeState.Failure, null);
            }

            _tree.SetVariable(varName, vector3);
            Terminate(NodeState.Success, null);
        }

    }
}