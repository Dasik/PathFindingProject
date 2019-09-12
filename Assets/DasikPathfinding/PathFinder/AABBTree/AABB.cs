using System;
using UnityEngine;

public class AABB
{
	public float minX;
	public float maxX;
	public float minY;
	public float maxY;

	public float x
	{
		get { return minX; }
		set
		{
			maxX += value - minX;
			minX = value;
		}
	}

	public float y
	{
		get { return minY; }
		set
		{
			maxY += value - minY;
			minY = value;
		}
	}

	public float width
	{
		get { return maxX - minX; }
		set { maxX = minX + value; }
	}

	public float height
	{
		get { return maxY - minY; }
		set { maxY = minY + value; }
	}


	/** 
     * Creates an AABB from the specified parameters.
     * 
     * Note: `width` and `height` must be non-negative.
     */
	public AABB(float x = 0, float y = 0, float width = 0, float height = 0)
	{
		minX = x;
		minY = y;
		maxX = x + width;
		maxY = y + height;
	}

	public AABB(float x, float y)
	{
		minX = x;
		minY = y;
		maxX = x;
		maxY = y;
	}

	public AABB(Vector2 leftBottomPoint, Vector2 rightTopPoint)
	{
		minX = leftBottomPoint.x;
		minY = leftBottomPoint.y;
		maxX = rightTopPoint.x;
		maxY = rightTopPoint.y;
	}


	public void setTo(float x, float y, float width = 0, float height = 0)
	{
		minX = x;
		minY = y;
		maxX = x + width;
		maxY = y + height;
	}

	public AABB inflate(float deltaX, float deltaY)
	{
		minX -= deltaX;
		minY -= deltaY;
		maxX += deltaX;
		maxY += deltaY;
		return this;

	}

	public float getPerimeter()
	{
		return 2 * ((maxX - minX) + (maxY - minY));
	}

	public float getArea()
	{
		return (maxX - minX) * (maxY - minY);
	}

	public float getCenterX()
	{
		return minX + .5f * (maxX - minX);
	}

	public float getCenterY()
	{
		return minY + .5f * (maxY - minY);
	}

	/** Resizes this instance so that it tightly encloses `aabb`. */
	public AABB union(AABB aabb)
	{
		minX = Math.Min(minX, aabb.minX);
		minY = Math.Min(minY, aabb.minY);
		maxX = Math.Max(maxX, aabb.maxX);
		maxY = Math.Max(maxY, aabb.maxY);
		return this;

	}

	/** Resizes this instance to the union of `aabb1` and `aabb2`. */
	public AABB asUnionOf(AABB aabb1, AABB aabb2)
	{
		minX = Math.Min(aabb1.minX, aabb2.minX);
		minY = Math.Min(aabb1.minY, aabb2.minY);
		maxX = Math.Max(aabb1.maxX, aabb2.maxX);
		maxY = Math.Max(aabb1.maxY, aabb2.maxY);
		return this;

	}

	/** Returns true if this instance intersects `aabb`. */
	public bool overlaps(AABB aabb)
	{
		return !(minX > aabb.maxX || maxX < aabb.minX || minY > aabb.maxY || maxY < aabb.minY);
	}

	/** Returns true if this instance fully contains `aabb`. */
	public bool contains(AABB aabb)
	{
		return (aabb.minX >= minX && aabb.maxX <= maxX && aabb.minY >= minY && aabb.maxY <= maxY);
	}

	/** Returns a new instance that is the intersection with `aabb`, or null if there's no interesection. */
	public AABB getIntersection(AABB aabb)
	{
		var intersection = this.clone();
		intersection.minX = Math.Max(minX, aabb.minX);
		intersection.maxX = Math.Min(maxX, aabb.maxX);
		intersection.minY = Math.Max(minY, aabb.minY);
		intersection.maxY = Math.Min(maxY, aabb.maxY);
		return (intersection.minX > intersection.maxX || intersection.minY > intersection.maxY) ? null : intersection;
	}

	public AABB clone()
	{
		return new AABB(minX, minY, maxX - minX, maxY - minY);
	}

	/** Copies values from the specified `aabb`. */
	public AABB fromAABB(AABB aabb)
	{
		minX = aabb.minX;
		minY = aabb.minY;
		maxX = aabb.maxX;
		maxY = aabb.maxY;
		return this;

	}

	public string toString()
	{
		return "[x:${minX} y:${minY} w:${width} h:${height}]";
	}
}
