using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeeTree {
    [Node("Inverter")]
    public class Inverter : DecoratorNode
    {
        public override string Id => "Inverter";

        public override bool TickEnabled => base.TickEnabled;

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
                    Terminate(NodeState.Failure, "Inverting success to failure");
                    break;

                case NodeState.Failure:
                    Terminate(NodeState.Success, "Inverting failure to success");
                    break;

                case NodeState.AbortComplete:
                    Terminate(NodeState.AbortComplete, "Abort complete");
                    break;
            }
        }
        
    }
}