using System;
using UnityEngine;

namespace NaughtyAttributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class AnimatorParamAnimAssetAttribute : DrawerAttribute
    {
        public string AnimatorAssetName { get; private set; }
        public AnimatorControllerParameterType? AnimatorParamType { get; private set; }

        public AnimatorParamAnimAssetAttribute(string animatorAssetName)
        {
            AnimatorAssetName = animatorAssetName;
            AnimatorParamType = null;
        }

        public AnimatorParamAnimAssetAttribute(string animatorAssetName, AnimatorControllerParameterType animatorParamType)
        {
            AnimatorAssetName = animatorAssetName;
            AnimatorParamType = animatorParamType;
        }
    }
}