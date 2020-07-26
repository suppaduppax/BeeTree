using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;

namespace BeeTree.Editor {
	public class CustomFieldDrawerManager
	{
		private static Dictionary<Type, string> _nodeTypeToQualifedName;
		private static Dictionary<Type, CustomFieldDrawer> _customNodePanelPrototypes;
		
		public static void FetchCustomFieldDrawers()
		{
			_nodeTypeToQualifedName = new Dictionary<Type, string>();
			_customNodePanelPrototypes = new Dictionary<Type, CustomFieldDrawer>();
	
			List<Assembly> scriptAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where((Assembly assembly) => assembly.FullName.Contains("Assembly")).ToList();
			if (!scriptAssemblies.Contains(Assembly.GetExecutingAssembly()))
				scriptAssemblies.Add(Assembly.GetExecutingAssembly());

			foreach (Assembly assembly in scriptAssemblies)
			{
				foreach (Type type in assembly.GetTypes().Where(T => T.IsClass && !T.IsAbstract && T.IsSubclassOf(typeof(CustomFieldDrawer))))
				{
					CustomFieldDrawer proto = (CustomFieldDrawer)Activator.CreateInstance(type);

					_customNodePanelPrototypes[proto.FieldType] = proto;
					_nodeTypeToQualifedName[proto.FieldType] = type.AssemblyQualifiedName;
				}
			}
		}
		
		public static CustomFieldDrawer GetCustomFieldDrawer(Type type)
		{
			if (_customNodePanelPrototypes.ContainsKey(type))
			{
				return _customNodePanelPrototypes[type];
			}
			else
			{
				return null;
			}
		}
	}
}