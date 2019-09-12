using System;
using System.Collections.Generic;
using UnityEngine;

public static class Collider2dExtension
{
	public static bool Intersect(this Collider2D collider, Rect rect)
	{
		if (collider.GetType() != typeof(PolygonCollider2D)) return false;
		PolygonCollider2D polygonCollider = (PolygonCollider2D)collider;

		var pointsB = new[]
		{
			new Vector2(rect.xMin, rect.yMin),
			new Vector2(rect.xMin, rect.yMax),
			new Vector2(rect.xMax, rect.yMax),
			new Vector2(rect.xMax, rect.yMin),
		};
		return Intersect(TransformPoints(polygonCollider.points, collider.transform.position), pointsB);
	}

	public static bool Intersect(this Collider2D collider, AABB aabb)
	{
		if (collider.GetType() != typeof(PolygonCollider2D)) return false;
		PolygonCollider2D polygonCollider = (PolygonCollider2D)collider;

		var pointsB = new[]
		{
			new Vector2(aabb.minX, aabb.minY),
			new Vector2(aabb.minX, aabb.maxY),
			new Vector2(aabb.maxX, aabb.maxY),
			new Vector2(aabb.maxX, aabb.minY),
		};
		return Intersect(TransformPoints(polygonCollider.points, collider.transform.position), pointsB);
	}
	private static Vector2[] TransformPoints(Vector2[] points, Vector2 offset)
	{
		for (int i = 0; i < points.Length; i++)
		{
			points[i] += offset;
		}
		return points;
	}

	////////////////private static bool Intersect(Vector2[] polygon1, Vector2[] polygon2)
	////////////////{

	////////////////    var pointsA = polygon1;
	////////////////    var edgesA = BuildEdges(pointsA);
	////////////////    int edgeCountA = edgesA.Length;
	////////////////    var pointsB = polygon2;
	////////////////    var edgesB = BuildEdges(pointsB);
	////////////////    int edgeCountB = edgesB.Length;

	////////////////    Vector2 edge;

	////////////////    // Loop through all the edges of both polygons
	////////////////    for (int edgeIndex = 0; edgeIndex < edgeCountA + edgeCountB; edgeIndex++)
	////////////////    {
	////////////////        if (edgeIndex < edgeCountA)
	////////////////        {
	////////////////            edge = edgesA[edgeIndex];
	////////////////        }
	////////////////        else
	////////////////        {
	////////////////            edge = edgesB[edgeIndex - edgeCountA];
	////////////////        }

	////////////////        // ===== 1. Find if the polygons are currently intersecting =====

	////////////////        // Find the axis perpendicular to the current edge
	////////////////        Vector2 axis = new Vector2(-edge.y, edge.x);
	////////////////        axis.Normalize();


	////////////////        // Find the projection of the polygon on the current axis
	////////////////        float minA = 0; float minB = 0; float maxA = 0; float maxB = 0;
	////////////////        ProjectPolygon(axis, pointsA, ref minA, ref maxA);
	////////////////        ProjectPolygon(axis, pointsB, ref minB, ref maxB);

	////////////////        // Check if the polygon projections are currentlty intersecting
	////////////////        //if (IntervalDistance(minA, maxA, minB, maxB) <= 0.000001) return true;
	////////////////        if (Intersects(minA, maxA, minB, maxB, true))
	////////////////            return true;

	////////////////    }
	////////////////    return false;
	////////////////}

	////////////////private static Vector2[] BuildEdges(Vector2[] points/*,Vector2 offset*/)
	////////////////{
	////////////////    Vector2 p1;
	////////////////    Vector2 p2;
	////////////////    List<Vector2> edges = new List<Vector2>(points.Length - 1);
	////////////////    for (int i = 0; i < points.Length; i++)
	////////////////    {
	////////////////        p1 = points[i];
	////////////////        if (i + 1 >= points.Length)
	////////////////        {
	////////////////            p2 = points[0];
	////////////////        }
	////////////////        else
	////////////////        {
	////////////////            p2 = points[i + 1];
	////////////////        }
	////////////////        //edges.Add(offset+p2 - p1);
	////////////////        edges.Add(p2 - p1);

	////////////////    }

	////////////////    return edges.ToArray();
	////////////////}

	////////////////// Calculate the distance between [minA, maxA] and [minB, maxB]
	////////////////// The distance will be negative if the intervals overlap
	////////////////private static float IntervalDistance(float minA, float maxA, float minB, float maxB)
	////////////////{
	////////////////    if (minA < minB)
	////////////////    {
	////////////////        return minB - maxA;
	////////////////    }
	////////////////    else
	////////////////    {
	////////////////        return minA - maxB;
	////////////////    }
	////////////////}

	////////////////public static bool Intersects(float min1, float max1, float min2, float max2, bool strict, bool correctMinMax = true)
	////////////////{
	////////////////    if (correctMinMax)
	////////////////    {
	////////////////        float tmp1 = min1, tmp2 = max1;
	////////////////        min1 = Math.Min(tmp1, tmp2);
	////////////////        max1 = Math.Max(tmp1, tmp2);

	////////////////        tmp1 = min2;
	////////////////        tmp2 = max2;
	////////////////        min2 = Math.Min(tmp1, tmp2);
	////////////////        max2 = Math.Max(tmp1, tmp2);

	////////////////    }

	////////////////    if (strict)
	////////////////        return (min1 <= min2 && max1 > min2) || (min2 <= min1 && max2 > min1);
	////////////////    else
	////////////////        return (min1 <= min2 && max1 >= min2) || (min2 <= min1 && max2 >= min1);
	////////////////}

	////////////////// Calculate the projection of a polygon on an axis and returns it as a [min, max] interval
	////////////////private static void ProjectPolygon(Vector2 axis, Vector2[] polygon, ref float min, ref float max)
	////////////////{
	////////////////    // To project a point on an axis use the dot product
	////////////////    //float d = Vector2.Dot(axis, polygon[0]);

	////////////////    float d = DotProduct(axis, polygon[0]);
	////////////////    min = d;
	////////////////    max = d;
	////////////////    for (int i = 0; i < polygon.Length; i++)
	////////////////    {
	////////////////        d = DotProduct(polygon[i], axis);
	////////////////        if (d < min)
	////////////////        {
	////////////////            min = d;
	////////////////        }
	////////////////        else
	////////////////        {
	////////////////            if (d > max)
	////////////////            {
	////////////////                max = d;
	////////////////            }
	////////////////        }
	////////////////    }
	////////////////}

	////////////////public static float DotProduct(Vector2 vectorA, Vector2 vectorB)
	////////////////{
	////////////////    return vectorA.x * vectorB.x + vectorA.y * vectorB.y;
	////////////////}

	private static bool Intersect(Vector2[] polygon1, Vector2[] polygon2)
	{
		var into = new ShapeCollision();
		var flip = false;

		var tmp1 = checkPolygons(polygon1, polygon2, flip);
		var tmp2 = checkPolygons(polygon2, polygon1, !flip);
		if (tmp1 == null)
		{
			return false;
		}

		if (tmp2 == null)
		{
			return false;
		}

		ShapeCollision result = null;
		ShapeCollision other = null;

		if (System.Math.Abs(tmp1.overlap) < System.Math.Abs(tmp2.overlap))
		{
			result = tmp1;
			other = tmp2;
		}
		else
		{
			result = tmp2;
			other = tmp1;
		}

		result.otherOverlap = other.overlap;
		result.otherSeparationX = other.separationX;
		result.otherSeparationY = other.separationY;
		result.otherUnitVectorX = other.unitVectorX;
		result.otherUnitVectorY = other.unitVectorY;

		into.copy_from(result);
		result = other = null;
		//return Math.Abs(into.otherOverlap) > 0.00001;
		return true;
	}

	private static ShapeCollision checkPolygons(Vector2[] polygon1, Vector2[] polygon2, bool flip = false)
	{
		var result = new ShapeCollision();

		// TODO: This is unused, check original source
		var offset = 0.0f;
		var test1 = 0.0f;
		var test2 = 0.0f;
		var testNum = 0.0f;
		var min1 = 0.0f;
		var max1 = 0.0f;
		var min2 = 0.0f;
		var max2 = 0.0f;
		var closest = float.MaxValue;

		var axisX = 0.0f;
		var axisY = 0.0f;
		var verts1 = polygon1;
		var verts2 = polygon2;


		var founded = false;

		// loop to begin projection
		for (var i = 0; i < verts1.Length; i++)
		{

			axisX = findNormalAxisX(verts1, i);
			axisY = findNormalAxisY(verts1, i);
			var aLen = vec_length(axisX, axisY);
			axisX = vec_normalize(aLen, axisX);
			axisY = vec_normalize(aLen, axisY);

			// project polygon1
			min1 = vec_dot(axisX, axisY, verts1[0].x, verts1[0].y);
			max1 = min1;

			for (var j = 1; i < verts1.Length; i++)
			{
				testNum = vec_dot(axisX, axisY, verts1[j].x, verts1[j].y);
				if (testNum < min1) min1 = testNum;
				if (testNum > max1) max1 = testNum;
			}

			// project polygon2
			min2 = vec_dot(axisX, axisY, verts2[0].x, verts2[0].y);
			max2 = min2;

			for (var j = 1; i < verts2.Length; i++)
			{
				testNum = vec_dot(axisX, axisY, verts2[j].x, verts2[j].y);
				if (testNum < min2) min2 = testNum;
				if (testNum > max2) max2 = testNum;
			}

			test1 = min1 - max2;
			test2 = min2 - max1;

			if (test1 > 0 || test2 > 0) return null;

			//if (test1 > 0 || test2 > 0) continue;
			//founded = true;
			var distMin = -(max2 - min1);
			if (flip) distMin *= -1;

			if (System.Math.Abs(distMin) < closest)
			{
				result.unitVectorX = axisX;
				result.unitVectorY = axisY;
				result.overlap = distMin;
				closest = System.Math.Abs(distMin);
			}

		}

		//if (!founded)
		//    return null;

		result.shape1 = flip ? polygon2 : polygon1;
		result.shape2 = flip ? polygon1 : polygon2;
		result.separationX = -result.unitVectorX * result.overlap;
		result.separationY = -result.unitVectorY * result.overlap;

		if (flip)
		{
			result.unitVectorX = -result.unitVectorX;
			result.unitVectorY = -result.unitVectorY;
		}

		return result;

	}



	private static Vector2[] BuildEdges(Vector2[] points/*,Vector2 offset*/)
	{
		Vector2 p1;
		Vector2 p2;
		List<Vector2> edges = new List<Vector2>(points.Length - 1);
		for (int i = 0; i < points.Length; i++)
		{
			p1 = points[i];
			if (i + 1 >= points.Length)
			{
				p2 = points[0];
			}
			else
			{
				p2 = points[i + 1];
			}
			//edges.Add(offset+p2 - p1);
			edges.Add(p2 - p1);

		}

		return edges.ToArray();
	}

	public static float findNormalAxisX(IList<Vector2> verts, int index)
	{
		var v2 = (index >= verts.Count - 1) ? verts[0] : verts[index + 1];
		return -(v2.y - verts[index].y);
	}

	public static float findNormalAxisY(IList<Vector2> verts, int index)
	{
		var v2 = (index >= verts.Count - 1) ? verts[0] : verts[index + 1];
		return (v2.x - verts[index].x);
	}

	public static float vec_length(float x, float y)
	{
		return (float)System.Math.Sqrt(vec_lengthsq(x, y));
	}

	public static float vec_lengthsq(float x, float y)
	{
		return x * x + y * y;
	}

	public static float vec_normalize(float length, float component)
	{
		if (Math.Abs(length) < 0.00000001) return 0;
		return component /= length;
	}

	public static float vec_dot(float x, float y, float otherx, float othery)
	{
		return x * otherx + y * othery;
	}

	public class ShapeCollision
	{
		/** The overlap amount */
		public float overlap = 0;
		/** X component of the separation vector, when subtracted from shape 1 will separate it from shape 2 */
		public float separationX = 0;
		/** Y component of the separation vector, when subtracted from shape 1 will separate it from shape */
		public float separationY = 0;
		/** X component of the unit vector, on the axis of the collision (i.e the normal of the face that was collided with) */
		public float unitVectorX = 0;
		/** Y component of the unit vector, on the axis of the collision (i.e the normal of the face that was collided with) */
		public float unitVectorY = 0;

		public float otherOverlap = 0;
		public float otherSeparationX = 0;
		public float otherSeparationY = 0;
		public float otherUnitVectorX = 0;
		public float otherUnitVectorY = 0;

		/** The shape that was tested */
		public Vector2[] shape1;
		/** The shape that shape1 was tested against */
		public Vector2[] shape2;

		public ShapeCollision()
		{
		}

		public void reset()
		{
			shape1 = shape2 = null;
			overlap = separationX = separationY = unitVectorX = unitVectorY = 0.0f;
			otherOverlap = otherSeparationX = otherSeparationY = otherUnitVectorX = otherUnitVectorY = 0.0f;
		}

		public ShapeCollision clone()
		{
			var clone = new ShapeCollision();
			clone.copy_from(this);

			return clone;
		}

		public void copy_from(ShapeCollision other)
		{
			overlap = other.overlap;
			separationX = other.separationX;
			separationY = other.separationY;
			unitVectorX = other.unitVectorX;
			unitVectorY = other.unitVectorY;
			otherOverlap = other.otherOverlap;
			otherSeparationX = other.otherSeparationX;
			otherSeparationY = other.otherSeparationY;
			otherUnitVectorX = other.otherUnitVectorX;
			otherUnitVectorY = other.otherUnitVectorY;
			shape1 = other.shape1;
			shape2 = other.shape2;
		}
	}
}