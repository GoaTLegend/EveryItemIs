using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace EveryItemIs
{
	class ChallengePerformer : MonoBehaviour
	{
		public static float timeToWait = 0.1f;
		public static IEnumerator PerformStuff()
		{
			while (true)
			{
				MainModule.AllItemsAre();
				yield return new WaitForSeconds(timeToWait);
			}
		}
	}
}
