using UnityEngine;
using System.Collections;
using System;

namespace BeeTree
{
	public class NodeUtility
	{
		public static Vector3 StringToVector3(string s)
		{
			if (s == null || s == "")
				return Vector3.zero;

			//		Debug.Log ("Parsing Vector3: " + s);
			// Remove the parentheses
			if (s.StartsWith("(") && s.EndsWith(")"))
			{
				s = s.Substring(1, s.Length - 2);
			}

			// split the items
			string[] sArray = s.Split(',');

			// store as a Vector3
			Vector3 result = new Vector3(
				float.Parse(sArray[0]),
				float.Parse(sArray[1]),
				float.Parse(sArray[2]));

			return result;
		}

		public static Vector2 StringToVector2(string s)
		{
			if (s == null || s == "")
				return Vector2.zero;

			// Remove the parentheses
			if (s.StartsWith("(") && s.EndsWith(")"))
			{
				s = s.Substring(1, s.Length - 2);
			}

			// split the items
			string[] sArray = s.Split(',');

			// store as a Vector3
			Vector2 result = new Vector2(
				float.Parse(sArray[0]),
				float.Parse(sArray[1]));

			return result;
		}

		/// <summary>
		/// Checks if string is empty... if it is set to null
		/// </summary>
		/// <returns>The variable.</returns>
		public static string ParseVariableKey(string varKey)
		{
			if (varKey == "")
				varKey = null;

			return varKey;
		}

		public static bool IntersectsRect(Vector2 a, Vector2 b, Rect r)
		{
			var minX = Math.Min(a.x, b.x);
			var maxX = Math.Max(a.x, b.x);
			var minY = Math.Min(a.y, b.y);
			var maxY = Math.Max(a.y, b.y);

			if (r.xMin > maxX || r.xMax < minX)
			{
				return false;
			}

			if (r.yMin > maxY || r.yMax < minY)
			{
				return false;
			}

			if (r.xMin < minX && maxX < r.xMax)
			{
				return true;
			}

			if (r.yMin < minY && maxY < r.yMax)
			{
				return true;
			}

			Func<float, float> yForX = x => a.y - (x - a.x) * ((a.y - b.y) / (b.x - a.x));

			var yAtRectLeft = yForX(r.xMin);
			var yAtRectRight = yForX(r.xMax);

			if (r.yMax < yAtRectLeft && r.yMax < yAtRectRight)
			{
				return false;
			}

			if (r.yMin > yAtRectLeft && r.yMin > yAtRectRight)
			{
				return false;
			}

			return true;
		}
	}
}