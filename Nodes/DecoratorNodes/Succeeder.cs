using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeeTree {
	[Node("Succeeder")]
	public class Succeeder : DecoratorNode
	{
        public override string Id => "Succeeder";



        public override void Initialize()
        {
            base.Initialize();

            GetNextChild().Initialize();
        }

        public override void ReturnState(NodeState state)
        {
            switch (state)
            {
                case NodeState.Success:
                    Terminate(NodeState.Success, "Success!");
                    break;

                case NodeState.Failure:
                    Terminate(NodeState.Success, "Failure returned but sending success!");
                    break;

                case NodeState.AbortComplete:
                    Terminate(NodeState.AbortComplete, "Abort complete");
                    break;
            }
        }

        
    }
}