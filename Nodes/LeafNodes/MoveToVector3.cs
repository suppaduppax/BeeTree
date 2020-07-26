using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeeTree {
	public class MoveToVector3 : LeafNode
	{
        public override string Id
        {
            get { return "MoveToVector3"; }
        }

        public Transform trans;
        public Vector3 destination;
        public float maxDistanceDelta = 0.1f;
        public float reachedDestinationThreshold = 0.01f;

        public override void Tick()
        {   
            trans.position = Vector3.MoveTowards(trans.position, destination, maxDistanceDelta);
            if (Vector3.Distance(trans.position, destination) < reachedDestinationThreshold)
            {
                trans.position = destination;
                Terminate(NodeState.Success, null);
            }
        }
    }
}