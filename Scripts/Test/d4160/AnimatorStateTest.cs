using UnityEngine;

namespace NaughtyAttributes.Test
{
	public class AnimatorStateTest : MonoBehaviour
	{
		public Animator animator0;

		[AnimatorState("animator0")]
		public int hash0;

		[AnimatorState("animator0")]
		public string name0;

		public AnimatorStateNest1 nest1;

		[Button("Log 'hash0' and 'name0'")]
		private void TestLog()
		{
			Debug.Log($"hash0 = {hash0}");
			Debug.Log($"name0 = {name0}");
			Debug.Log($"Animator.StringToHash(name0) = {Animator.StringToHash(name0)}");
		}
	}

	[System.Serializable]
	public class AnimatorStateNest1
	{
		public Animator animator1;
		private Animator Animator1 => animator1;

		[AnimatorState("Animator1")]
		public int hash1;

		[AnimatorState("Animator1")]
		public string name1;

		public AnimatorStateNest2 nest2;
	}

	[System.Serializable]
	public class AnimatorStateNest2
	{
		public Animator animator2;
		private Animator GetAnimator2() => animator2;

		[AnimatorState("GetAnimator2")]
		public int hash1;

		[AnimatorState("GetAnimator2")]
		public string name1;
	}
}