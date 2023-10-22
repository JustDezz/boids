using System;
using UnityEngine;

namespace Tools.GizmosExtensions
{
	public readonly struct GizmosColorScope : IDisposable
	{
		private readonly Color _previousColor;

		public GizmosColorScope(Color color)
		{
			_previousColor = Gizmos.color;
			Gizmos.color = color;
		}

		public void Dispose() => Gizmos.color = _previousColor;
	}
}