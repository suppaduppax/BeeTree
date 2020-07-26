using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

A composite node is a node that can have one or more children. They will process one or more of these children in either a first to last 
sequence or random order depending on the particular composite node in question, and at some stage will consider their processing complete 
and pass either success or failure to their parent, often determined by the success or failure of the child nodes. During the time they are 
processing children, they will continue to return Running to the parent.

The most commonly used composite node is the Sequence, which simply runs each child in sequence, returning failure at the point any of the 
children fail, and returning success if every child returned a successful status.

*/

namespace BeeTree {
	public class CompositeNode : Node
	{
		protected Node _currentNode;
    }
}