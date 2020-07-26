using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BeeTree
{
    [System.Serializable]
    public class Node : ScriptableObject
    {
        public int guid = int.MinValue;
        [SerializeField] protected BehaviourTree _tree;

        /// <summary>
        /// having a value of int.MinValue represents a null parent
        /// </summary>
        public int parentGuid = int.MinValue;
        public List<int> childrenGuids = new List<int>();

        /// <summary>
        /// Any negative number allows infinite children
        /// </summary>
        public virtual int MaxChildren => -1;
        
        protected List<Node> _childrenQueue;
        protected NodeState _state = NodeState.Idle;
        protected Node _currentChild = null; 

        public string StateMessage { get; set; }
        
        public enum NodeState
        {
            Idle,
            Running,
            Success,
            Failure,
            Aborting,
            AbortComplete
        }

        public virtual string Id
        {
            get { return "BaseNode"; }
        }

        public BehaviourTree Tree
        {
            get { return _tree; }
            set { _tree = value; }
        }

        public NodeState State
        {
            get { return _state; }
        }

        public Node Parent
        {
            get { return parentGuid != int.MinValue ? _tree.GetNode(parentGuid) : null; }
            set { parentGuid = value != null ? value.guid : int.MinValue; }
        }

        public List<Node> Children
        {
            get
            {
                List<Node> children = new List<Node>();

                for (int i = 0; i < this.childrenGuids.Count; i++)
                {
                    children.Add(_tree.GetNode(this.childrenGuids[i]));
                }

                return children;
            }

            set
            {
                List<int> _children = new List<int>();
                if (value != null)
                {
                    for (int i = 0; i < value.Count; i++)
                    {
                        _children.Add(value[i].guid);
                    }
                }
            }
        }

        public Node CurrentChild => _currentChild;

        public int CurrentChildGuid
        {
            get { return _currentChild != null ? _currentChild.guid : int.MinValue; }
        }

        public virtual bool CanHaveChildren
        {
            get { return true; }
        }


        /// <summary>
        /// Set to false to ignore ticks from being executed.
        /// Defaults to FALSE.
        /// </summary>
        public virtual bool TickEnabled
        {
            get { return false; }
        }

        /// <summary>
        /// Set to false to ignore ABORT ticks from being executed.
        /// Defaults to FALSE.
        /// </summary>
        public virtual bool AbortTickEnabled
        {
            get { return false; }
        }

        public virtual void Initialize()
        {
            StateMessage = "Initializing...";
            
            Print("Initializing");
            _tree.AddRunningNode(this);
            SetChildrenState(NodeState.Idle);
            
            _childrenQueue = new List<Node>(Children);

            ChangeState(NodeState.Running);
        }

        public virtual void SetChildrenState(NodeState state)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].ChangeState(state);
                Children[i].SetChildrenState(state);
            }
        }

        public virtual void Terminate(NodeState state, string message)
        {
            if (state != NodeState.Success && state != NodeState.Failure && state != NodeState.AbortComplete)
            {
                throw new System.Exception("Cannot terminate node with state: " + state + ". Must be Success, Failure, or AbortComplete");
            }

            if (message != null)
            {
                StateMessage = message;
            }

            _tree.RemoveRunningNode(this);
            ChangeState(state);

            if (Parent != null)
            {
                Parent.ReturnState(state);
            }
            else
            {
                _tree.ReturnState(state);
            }
        }

        public virtual void Tick() { }
        public virtual void AbortTick() { }

        public virtual void ChangeState(NodeState state)
        {
            Print("Changing state to: " + state);
            _state = state;
        }

        /// <summary>
        /// Used by a child to send up its current state
        /// </summary>
        public virtual void ReturnState(NodeState state)
        {
            switch (state)
            {
                case NodeState.Success:
                case NodeState.Failure:
                case NodeState.AbortComplete:
                    Terminate(state, null);
                    break;
            }
        }

        public virtual Node GetNextChild()
        {
            if (_childrenQueue.Count == 0)
            {
                return null;
            }

            _currentChild = _childrenQueue[0];
            _childrenQueue.Remove(_currentChild);

            return _currentChild;
        }

        public virtual bool CanAddChild(Node child)
        {
            if (child == this)
            {
                Debug.LogWarning("Cannot add child to itself");
                return false;
            }

            if (childrenGuids.Contains(child.guid))
            {
                Debug.LogWarning("Cannot add child, it is already a child");
                return false;
            }

            if (child.HasChild(this))
            {
                Debug.Log(Parent);
                Debug.LogWarning("Cannot add child, it is the parent");
                return false;
            }

            return true;
        }

        // Add a node as child to this node. Returns true if successful.
        public virtual bool AddChild(Node child)
        {
            if (child == null)
            {
                throw new System.Exception("Node.AddChild: cannot add null child.");
            }

            if (Children.Contains(child))
            {
                throw new System.Exception("Node.AddChild: already a child: " + Id);
            }

            if (child.Parent != null && child.Parent != this)
            {
                child.Parent.RemoveChild(child);
            }

            child.Parent = this;
            childrenGuids.Add(child.guid);

            return true;
        }

        public virtual bool RemoveChild(Node child)
        {
            if (child == null)
            {
                throw new System.Exception("Node.AddChild: cannot add null child: " + Id);
            }

            child.Parent = null;
            childrenGuids.Remove(child.guid);

            return true;
        }

        public virtual bool MoveChild(Node child, int newIndex)
        {
            if (!childrenGuids.Contains(child.guid))
            {
                throw new System.Exception("Node.MoveChild: Cannot move child of " + Id + " it's not currently its child! Call AddChild first and then MoveChild!");
            }

            int oldIndex = childrenGuids.IndexOf(child.guid);
            if (newIndex > oldIndex)
                newIndex--;

            if (newIndex > childrenGuids.Count)
            {
                Debug.LogWarning("Node.MoveChild: Cannot move child to index " + newIndex + ". It is out of range. Total children: " + childrenGuids.Count);
                return false;
            }

            childrenGuids.RemoveAt(oldIndex);
            if (newIndex < childrenGuids.Count)
            {
                childrenGuids.Insert(newIndex, child.guid);
            }
            else
            {
                // add child to the end of the list
                childrenGuids.Add(child.guid);
            }
            
            return true;
        }

        public bool HasParent ()
        {
            return parentGuid == int.MinValue;
        }

        public bool HasChild(Node node)
        {
            return childrenGuids.Contains(node.guid);
        }

        public virtual Node Clone(BehaviourTree tree)
        {
            Node node = (Node)ScriptableObject.CreateInstance(this.GetType());

            node.guid = guid;
            node.name = name;
            node.childrenGuids = new List<int>(childrenGuids);
            node.parentGuid = parentGuid;

            node._tree = tree;
            return node;
        }

        protected const string PRINT_COLOUR_BLACK = "black";
        protected const string PRINT_COLOUR_GREEN = "green";
        protected const string PRINT_COLOUR_WHITE = "white";
        protected const string PRINT_COLOUR_RED = "red";
        protected const string PRINT_COLOUR_BLUE = "blue";
        protected const string PRINT_COLOUR_GRAY = "grey";
        protected const string PRINT_COLOUR_MAGENTA = "magenta";
        protected const string PRINT_COLOUR_YELLOW = "yellow";
        
        protected void Print(object text)
        {
            if (BehaviourTree.DEBUG_MODE)
            {
                Print(text, PRINT_COLOUR_BLACK);
            }
        }
        
        protected void Print(object text, string colour)
        {
            if (BehaviourTree.DEBUG_MODE)
            {
                Debug.Log("<color='" + colour + "'>(" + Id + ") " + name + ": " + text + "</color>");
            }
        }
    }

}