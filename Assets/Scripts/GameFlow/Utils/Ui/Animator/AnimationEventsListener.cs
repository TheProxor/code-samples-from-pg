using UnityEngine;
using System;
using System.Collections.Generic;


namespace Drawmasters.Helpers
{
	public class AnimationEventsListener : MonoBehaviour
	{
		#region Fields

		private readonly Dictionary<string, Action> callbacks = new Dictionary<string, Action>();

		#endregion



		#region Methods

		public void AddListener(string id, Action callback)
		{
			RemoveListener(id);
			callbacks.Add(id, callback);
		}


		public void RemoveListener(string id) =>
			callbacks.Remove(id);
		

		public void Clear() =>
			callbacks.Clear();


		public void AnimationEvent(string id)
		{
			if (callbacks.TryGetValue(id, out Action savedCallback))
			{
				savedCallback?.Invoke();
			}

			RemoveListener(id);
		}

		#endregion
	}
}
