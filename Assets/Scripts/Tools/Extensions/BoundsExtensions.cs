using System.Diagnostics.Contracts;
using UnityEngine;

namespace Tools.Extensions
{
	public static class BoundsExtensions
	{
		[Pure]
		public static Bounds ProjectOnPlane(this Bounds bounds, Vector3 origin, Vector3 normal, Vector3 up, Vector3 right) =>
			bounds.ProjectOnPlane(origin, normal, up, right, true);

		[Pure]
		public static Bounds ProjectOnPlane(this Bounds bounds, Vector3 origin, Vector3 normal, Vector3 up, Vector3 right, bool worldSpace)
		{
			(Vector2 min, Vector2 max) = ProjectAndRebaseOnPlane(bounds, origin, normal, up, right);

			Bounds projected = new();
			if (worldSpace)
			{
				Vector3 worldSpaceMin = origin + min.x * right + min.y * up;
				Vector3 worldSpaceMax = origin + max.x * right + max.y * up;
				projected.SetMinMax(worldSpaceMin, worldSpaceMax);
			}
			else projected.SetMinMax(min, max);

			return projected;
		}

		private static (Vector2 min, Vector2 max) ProjectAndRebaseOnPlane(Bounds bounds, Vector3 origin, Vector3 normal, Vector3 up, Vector3 right)
		{
			Vector3 min = bounds.min;
			Vector3 max = bounds.max;

			// ReSharper disable InconsistentNaming
			Vector3 MMM = new(max.x, max.y, max.z);
			Vector3 mMM = new(min.x, max.y, max.z);
			Vector3 MmM = new(max.x, min.y, max.z);
			Vector3 MMm = new(max.x, max.y, min.z);
			Vector3 mmM = new(min.x, min.y, max.z);
			Vector3 Mmm = new(max.x, min.y, min.z);
			Vector3 mMm = new(min.x, max.y, min.z);
			Vector3 mmm = new(min.x, min.y, min.z);

			Vector3 closest_MMM = ClosestPointOnPlane(origin, normal, MMM);
			Vector3 closest_mMM = ClosestPointOnPlane(origin, normal, mMM);
			Vector3 closest_MmM = ClosestPointOnPlane(origin, normal, MmM);
			Vector3 closest_MMm = ClosestPointOnPlane(origin, normal, MMm);
			Vector3 closest_mmM = ClosestPointOnPlane(origin, normal, mmM);
			Vector3 closest_Mmm = ClosestPointOnPlane(origin, normal, Mmm);
			Vector3 closest_mMm = ClosestPointOnPlane(origin, normal, mMm);
			Vector3 closest_mmm = ClosestPointOnPlane(origin, normal, mmm);

			Vector2 rebased_MMM = closest_MMM.Rebase(bounds.center, up, right);
			Vector2 rebased_mMM = closest_mMM.Rebase(bounds.center, up, right);
			Vector2 rebased_MmM = closest_MmM.Rebase(bounds.center, up, right);
			Vector2 rebased_MMm = closest_MMm.Rebase(bounds.center, up, right);
			Vector2 rebased_mmM = closest_mmM.Rebase(bounds.center, up, right);
			Vector2 rebased_Mmm = closest_Mmm.Rebase(bounds.center, up, right);
			Vector2 rebased_mMm = closest_mMm.Rebase(bounds.center, up, right);
			Vector2 rebased_mmm = closest_mmm.Rebase(bounds.center, up, right);
			// ReSharper restore InconsistentNaming

			Vector2 rebasedMin = rebased_MMM;
			rebasedMin = Vector2.Min(rebasedMin, rebased_mMM);
			rebasedMin = Vector2.Min(rebasedMin, rebased_MmM);
			rebasedMin = Vector2.Min(rebasedMin, rebased_MMm);
			rebasedMin = Vector2.Min(rebasedMin, rebased_mmM);
			rebasedMin = Vector2.Min(rebasedMin, rebased_Mmm);
			rebasedMin = Vector2.Min(rebasedMin, rebased_mMm);
			rebasedMin = Vector2.Min(rebasedMin, rebased_mmm);

			Vector2 rebasedMax = rebased_MMM;
			rebasedMax = Vector2.Max(rebasedMax, rebased_mMM);
			rebasedMax = Vector2.Max(rebasedMax, rebased_MmM);
			rebasedMax = Vector2.Max(rebasedMax, rebased_MMm);
			rebasedMax = Vector2.Max(rebasedMax, rebased_mmM);
			rebasedMax = Vector2.Max(rebasedMax, rebased_Mmm);
			rebasedMax = Vector2.Max(rebasedMax, rebased_mMm);
			rebasedMax = Vector2.Max(rebasedMax, rebased_mmm);
			return (rebasedMin, rebasedMax);
		}

		public static Vector3 ClosestPointOnPlane(Vector3 origin, Vector3 normal, Vector3 point) =>
			point + DistanceFromPlane(origin, normal, point) * normal;

		public static float DistanceFromPlane(Vector3 origin, Vector3 normal, Vector3 point) =>
			Vector3.Dot(origin - point, normal);
	}
}