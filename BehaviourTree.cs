using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeeTree.Serialization;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace BeeTree {
	[CreateAssetMenu(fileName = "New Behaviour Tree", menuName = "BeeTree/Behaviour Tree")]
	public class BehaviourTree : ScriptableObject
	{
		public const bool DEBUG_MODE = false; 
		
		public List<Node> nodes = new List<Node>();

		[SerializeField] private int _guidCounter = int.MinValue;
		[SerializeField] private List<string> _variableNames = new List<string>();            // a list to keep track of the variables that have been created. the blackboard is where the actual values go
		[SerializeField] private int _rootNodeGuid;

		private Dictionary<int, Node> _guidToNodeDict = new Dictionary<int, Node>();

		private Blackboard _blackboard;

		private List<Node> _runningNodes = new List<Node>();
		private Node _parentNode;

		private bool _initialized = false;

		public Blackboard Blackboard => _blackboard;

		protected virtual void OnEnable()
		{
			#if UNITY_EDITOR
			var assetPath = AssetDatabase.GetAssetPath(this);
			var filename = Path.GetFileNameWithoutExtension(assetPath);
			name = filename;
			#endif
		}

		public Node RootNode
		{
			get => GetNode(_rootNodeGuid);
			set
			{
				if (value == null)
				{
					throw new System.Exception("BehaviourTree.SetRootNode: Cannot set root node to null.");
				}

				_rootNodeGuid = value.guid;
			}
		}

		public Dictionary<int, Node> GuidToNodeDict => _guidToNodeDict;

		public string[] VariableNames => _variableNames.ToArray();

		public void Initialize(Node parentNode, Blackboard blackboard)
		{
			_parentNode = parentNode;
			Initialize(blackboard);
		}
		
		public void Initialize(Blackboard blackboard)
		{
			_initialized = true;
			_blackboard = blackboard;

			for (int i = 0; i < _variableNames.Count; i++)
			{
				_blackboard.SetVariable(_variableNames[i], null);
			}

			RootNode.Initialize();
		}

		public void PrintNodes()
		{
			Debug.Log("Total nodes: " + nodes.Count);
		}

		public void Tick()
		{
			//Debug.Log("BehaviourTree tick");
			for (int i = 0; i < _runningNodes.Count; i++)
			{
				if (_runningNodes[i].State == Node.NodeState.Running)
				{
					if (_runningNodes[i].TickEnabled)
					{
						_runningNodes[i].Tick();
					}
				}

				else if (_runningNodes[i].State == Node.NodeState.Aborting)
				{
					if (_runningNodes[i].AbortTickEnabled)
					{
						_runningNodes[i].AbortTick();
					}
				}
			}
		}

		public void CreateGuidToNodeTable()
		{
			_guidToNodeDict.Clear();
			for (int i = 0; i < nodes.Count; i++)
			{
				_guidToNodeDict.Add(nodes[i].guid, nodes[i]);
			}
		}

		/// <summary>
		/// gets a value between int.MinValue + 1 and int.MaxValue.
		/// int.MinValue is reserved for a null parent
		/// </summary>
		/// <returns></returns>
		public int CreateGuid()
		{
			_guidCounter += 1;
			return _guidCounter;
		}

		public Node GetNode(int guid)
		{
			if (!_guidToNodeDict.ContainsKey(guid))
			{
				Debug.Log("GetNode: Current items in dict:");
				foreach (var item in _guidToNodeDict)
				{
					Debug.Log(item.Key);
				}

				throw new System.Exception("Node guid not found in node dict: " + guid);
			}

			return _guidToNodeDict[guid];
		}

		public void AddNode(Node node)
		{
			if (node == null)
			{
				throw new System.Exception("BehaviourTree.AddNode: Cannot add null node.");
			}

			if (nodes.Contains(node))
			{
				throw new System.Exception("BehaviourTree.AddNode: Tree already contains node: " + node.Id);
			}

			if (_guidToNodeDict.ContainsKey(node.guid))
			{
				throw new System.Exception("BehaviourTree.AddNode: Tree already contains node with guid:" + node.guid);
			}

			if (node.Tree != null && node.Tree != this)
			{
				Debug.LogWarning("Node " + node.Id + " already belongs to a tree. Changing tree.");
			}



			node.Tree = this;
			if (node.guid == int.MinValue)
			{
				node.guid = CreateGuid();
			}

			nodes.Add(node);
			_guidToNodeDict.Add(node.guid, node);
		}

		public void RemoveNode(Node node)
		{
			if (node == null)
			{
				throw new System.Exception("BehaviourTree.RemoveNode: Cannot add null node.");
			}

			for (int i = 0; i < node.Children.Count; i++)
			{
				node.Children[i].Parent = null;
			}

			nodes.Remove(node);
			_guidToNodeDict.Remove(node.guid);

		}

		public void DeleteNode(Node node)
		{

			if (node.Children != null && node.Children.Count > 0)
			{
				for (int i = 0; i < node.Children.Count; i++)
				{
					node.Children[i].Parent = null;
				}
			}

			if (node.Parent != null)
			{
				node.Parent.RemoveChild(node);
			}

			RemoveNode(node);
			node = null;
		}

		public void DeleteNodeHierarchy(Node node)
		{

			if (node.Children != null && node.Children.Count > 0)
			{
				for (int i = 0; i < node.Children.Count; i++)
				{
					DeleteNodeHierarchy(node.Children[i]);
				}
			}


			//Debug.Log("Deleting node: " + node.name);
			DeleteNode(node);
		}

		public void ReturnState(Node.NodeState state)
		{
			switch (state)
			{
				case Node.NodeState.Success:
				case Node.NodeState.Failure:
				case Node.NodeState.AbortComplete:
					Terminate(state);
					break;
			}
		}

		public void Terminate(Node.NodeState state)
		{
			if (_parentNode == null)
			{
				Debug.Log("BehaviourTree completed with state: " + state);
				return;
			}
			
			if (state != Node.NodeState.Success && state != Node.NodeState.Failure && state != Node.NodeState.AbortComplete)
			{
				throw new System.Exception("Cannot terminate node with state: " + state + ". Must be Success, Failure, or AbortComplete");
			}

			_parentNode.ReturnState(state);
		}

		public void AddRunningNode(Node node)
		{
			if (_runningNodes.Contains(node))
			{
				throw new System.Exception("BehaviourTree.AddRunningNode: Node is already running: (" + node.Id + ") " + node.name);
			}

			_runningNodes.Add(node);
		}

		public void RemoveRunningNode(Node node)
		{
			_runningNodes.Remove(node);
		}

		public string GetUniqueVariableName(string name)
		{
			if (_variableNames == null || !_variableNames.Contains(name))
			{
				return name;
			}

			string checkName = name;

			int maxTries = 10000;
			int tries = 0;

			do
			{
				// add to suffix number if it exists
				Match numberMatch = Regex.Match(checkName, "\\d+$");
				int num = 1;
				if (numberMatch.Success)
				{
					num = int.Parse(numberMatch.Value);
					num++;
					checkName = Regex.Replace(checkName, "\\d+$", num.ToString());
				}
				else
				{
					checkName = checkName + "1";
				}


				tries++;

			} while (_variableNames.Contains(checkName) || tries > maxTries);

			return checkName;
		}

		public void CreateVariable(string name)
		{
			_variableNames.Add(GetUniqueVariableName(name));
		}

		public void SetVariable(string name, object value)
		{
			if (!_initialized)
			{
				throw new System.Exception("BehaviourTree.SetVariable: Cannot set a variable, BehaviourTree isn't initialized. Are you trying to set it outside of playmode?");
			}

			if (name == null)
			{
				throw new System.Exception("BehaviourTree.GetVariable: variable name cannot be null.");
			}

			_blackboard.SetVariable(name, value);
		}

		public object GetVariable(string name)
		{
			if (!_initialized)
			{
				throw new System.Exception("BehaviourTree.GetVariable: Cannot get a variable, BehaviourTree isn't initialized. Are you trying to get it outside of playmode?");
			}

			if (name == null)
			{
				throw new System.Exception("BehaviourTree.GetVariable: variable name cannot be null.");
			}

			return _blackboard.GetVariable(name);
		}

		public bool HasVariable(string name)
        {
			if (!_variableNames.Contains(name))
            {
				return false;
            }

			return _blackboard.HasVariable(name);
        }

		public void RenameVariable(string oldName, string newName)
		{
			if (_variableNames.Contains(newName))
			{
				newName = GetUniqueVariableName(newName);
				//throw new System.Exception("Cannot rename: " + oldName + " to " + newName + ". Another variable with that name exists already.");
			}

			int index = _variableNames.IndexOf(oldName);
			_variableNames.Remove(oldName);
			_variableNames.Insert(index, newName);

			//_blackboard.SetVariable(newName, _blackboard.GetVariable(oldName));
			//_blackboard.DeleteVariable(oldName);
		}

		public void DeleteVariable(string name)
		{
			if (!_variableNames.Contains(name))
			{
				throw new System.Exception("Cannot delete: " + name + " it doesn't exists.");
			}

			_variableNames.Remove(name);
		}

		public void CreateNodeGuidTable ()
        {
			_guidToNodeDict.Clear();
            for (int i = 0; i < nodes.Count; i++)
            {
				_guidToNodeDict.Add(nodes[i].guid, nodes[i]); ;
            }
		}

		public BehaviourTree Clone ()
        {
			BehaviourTree tree = ScriptableObject.CreateInstance<BehaviourTree>();
			tree._rootNodeGuid = _rootNodeGuid;

			tree._variableNames = new List<string>(_variableNames);

			tree.nodes = new List<Node>();
            for (int i = 0; i < nodes.Count; i++)
            {
				tree.nodes.Add(nodes[i].Clone(tree));
            }

			tree.CreateNodeGuidTable();
			return tree;
        }
	}
}