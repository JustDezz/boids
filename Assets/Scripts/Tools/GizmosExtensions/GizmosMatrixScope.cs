using System;
using UnityEngine;

namespace Tools.GizmosExtensions
{
	public readonly struct GizmosMatrixScope : IDisposable
	{
		private readonly Matrix4x4 _previousMatrix;

		public GizmosMatrixScope(Matrix4x4 matrix)
		{
			_previousMatrix = Gizmos.matrix;
			Gizmos.matrix = matrix;
		}

		public void Dispose() => Gizmos.matrix = _previousMatrix;
	}
}