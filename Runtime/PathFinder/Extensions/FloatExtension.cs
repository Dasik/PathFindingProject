using System;

namespace Dasik.PathFinder
{
	public static class FloatExtension
	{
		public static int CompareTo(this float a, float b, float eps)
		{
			if (a.Equals(b, epsilon: eps))
				return 0;
			return a.CompareTo(b);
		}

		public static bool Equals(this float a, float b, float epsilon)
		{
			const float floatNormal = (1 << 23) * float.Epsilon;
			float absA = Math.Abs(a);
			float absB = Math.Abs(b);
			float diff = Math.Abs(a - b);

			if (a == b)
			{
				// Shortcut, handles infinities
				return true;
			}

			if (a == 0.0f || b == 0.0f || diff < floatNormal)
			{
				// a or b is zero, or both are extremely close to it.
				// relative error is less meaningful here
				return diff < (epsilon * floatNormal);
			}

			// use relative error
			return diff / Math.Min((absA + absB), float.MaxValue) < epsilon;
		}
	}
}