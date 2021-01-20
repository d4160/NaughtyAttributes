using System;
using UnityEngine;

namespace NaughtyAttributes
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class AnimatorStateAttribute : DrawerAttribute
	{
		public string AnimatorName { get; private set; }
		public string Layer { get; private set; }
		public string Duration { get; private set; }

		public AnimatorStateAttribute(string animatorName)
		{
			AnimatorName = animatorName;
			Layer = null;
		}

		public AnimatorStateAttribute(string animatorName, string layer)
		{
			AnimatorName = animatorName;
			Layer = layer;
		}
		
		public AnimatorStateAttribute(string animatorName, string layer, string duration)
		{
			AnimatorName = animatorName;
			Layer = layer;
			Duration = duration;
		}
	}
}