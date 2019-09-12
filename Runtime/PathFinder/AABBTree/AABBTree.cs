using ds;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;


/**
 * Values that can be returned from query and raycast callbacks to decide how to proceed.
 */
public enum HitBehaviour
{
	SKIP,               // continue but don't include in results
	INCLUDE,            // include and continue (default)
	INCLUDE_AND_STOP,   // include and break out of the search
	STOP                // break out of the search
}

/**
 * AABBTree implementation. A spatial partitioning data structure.
 * 
 * Note: by default compiling in DEBUG mode will enable a series of tests
 * to ensure the structure's validity (will affect performance), while
 * in RELEASE mode they won't be executed.
 * 
 * You can force the validation by passing -DTREE_CHECKS to the compiler,
 * or forcefully disable it with -DNO_TREE_CHECKS.
 * 
 * The `isValidationEnabled` property will be set consequently.
 * 
 * @author azrafe7
 */


public class AABBTree<T> : IEnumerable<T>
{

#if ((debug && !NO_TREE_CHECKS) || TREE_CHECKS)
	public bool isValidationEnabled = true;
#else
	public bool isValidationEnabled = false;
#endif

	///How much to fatten the aabbs.
	public float FattenDelta;

	/// Algorithm to use for choosing where to insert a new leaf. 
	public IInsertStrategy<T> InsertStrategy;

	/// Total number of leaves.
	private int NumLeaves = 0;

	/// <summary>
	/// Total size of elements
	/// </summary>
	public int Count
	{
		get { return NumLeaves; }
	}

	/// Height of the tree.
	public int Height
	{
		get { return _root != null ? _root.invHeight : -1; }
	}

	private Node<T> _root = null;

	/// <summary>
	/// Creates a new AABBTree.
	/// </summary>
	/// <param name="fattenDelta">How much to fatten the aabbs (to avoid updating the nodes too frequently when the underlying data moves/resizes).</param>
	/// <param name="insertStrategy">Strategy to use for choosing where to insert a new leaf. Defaults to `InsertStrategyPerimeter`.</param>
	/// <returns></returns>
	public AABBTree(IInsertStrategy<T> insertStrategy, float fattenDelta = 10)
	{

		this.FattenDelta = fattenDelta;

		this.InsertStrategy = insertStrategy != null ? insertStrategy : new InsertStrategyPerimeter<T>();
	}

	/// <summary>
	/// Inserts a leaf node with the specified `aabb` values and associated `data`.
	/// </summary>
	/// <returns>The index of the inserted node.</returns>
	public Node<T> Add(T data, AABB insertedAABB)
	{
		// create new node and fatten its aabb
		var leafNode = new Node<T>(insertedAABB, data, null);
		leafNode.aabb.inflate(FattenDelta, FattenDelta);
		leafNode.invHeight = 0;
		NumLeaves++;

		if (_root == null)
		{
			_root = leafNode;
			return leafNode;
		}

		// find best sibling to insert the leaf
		var leafAABB = leafNode.aabb;
		var combinedAABB = new AABB();
		Node<T> left;
		Node<T> right;
		var node = _root;
		while (!node.isLeaf())
		{
			bool breakWhile = false;
			switch (InsertStrategy.choose(leafAABB, node))
			{
				case InsertChoice.PARENT:
					breakWhile = true;
					break;
				case InsertChoice.DESCEND_LEFT:
					node = node.left;
					break;
				case InsertChoice.DESCEND_RIGHT:
					node = node.right;
					break;
			}

			if (breakWhile)
			{
				break;
			}
		}

		var sibling = node;

		// create a new parent
		var oldParent = sibling.parent;
		combinedAABB.asUnionOf(leafAABB, sibling.aabb);
		var newParent = new Node<T>(combinedAABB.x, combinedAABB.y, combinedAABB.width, combinedAABB.height, default(T),
			oldParent);
		newParent.invHeight = sibling.invHeight + 1;

		// the sibling was not the root
		if (oldParent != null)
		{

			if (oldParent.left == sibling)
			{
				oldParent.left = newParent;
			}
			else
			{
				oldParent.right = newParent;
			}
		}
		else
		{

			// the sibling was the root
			_root = newParent;
		}

		newParent.left = sibling;
		newParent.right = leafNode;
		sibling.parent = newParent;
		leafNode.parent = newParent;

		// walk back up the tree fixing heights and AABBs
		node = leafNode.parent;
		while (node != null)
		{
			node = Balance(node);

			left = node.left;
			right = node.right;

			Assert(left != null);
			Assert(right != null);

			node.invHeight = 1 + (Math.Max(left.invHeight, right.invHeight));
			node.aabb.asUnionOf(left.aabb, right.aabb);

			node = node.parent;
		}

		Validate();
		return leafNode;
	}

	/** 
	 * Updates the aabb of leaf node with the specified `leafId` (must be a leaf node).
	 * 
	 * @param	dx	Movement prediction along the x axis.
	 * @param	dy	Movement prediction along the y axis.
	 * 
	 * @return false if the fat aabb didn't need to be expanded.
	 */

	public bool UpdateLeaf(Node<T> leafNode, float x, float y, float width = 0, float height = 0, float dx = 0,
		float dy = 0)
	{
		Assert(leafNode.isLeaf());

		var newAABB = new AABB(x, y, width, height);

		if (leafNode.aabb.contains(newAABB))
		{
			return false;
		}

		var data = leafNode.data;
		RemoveLeaf(leafNode);

		// add movement prediction
		dx *= 2;
		dy *= 2;
		if (dx < 0)
		{
			x += dx;
			width -= dx;
		}
		else
		{
			width += dx;
		}

		if (dy < 0)
		{
			y += dy;
			height -= dy;
		}
		else
		{
			height += dy;
		}

		var newNode = Add(data, new AABB(x, y, width, height));

		Assert(newNode == leafNode);

		return true;
	}

	/** 
	 * Removes the leaf node with the specified `leafId` from the tree (must be a leaf node).
	 */
	public void RemoveLeaf(Node<T> leafNode)
	{
		Assert(leafNode.isLeaf());

		if (leafNode == _root)
		{
			_root = null;
			return;
		}

		var parent = leafNode.parent;
		var grandParent = parent.parent;
		var sibling = parent.left == leafNode ? parent.right : parent.left;

		if (grandParent != null)
		{
			// connect sibling to grandParent
			if (grandParent.left == parent)
			{
				grandParent.left = sibling;
			}
			else
			{
				grandParent.right = sibling;
			}

			sibling.parent = grandParent;

			// adjust ancestor bounds
			var node = grandParent;
			while (node != null)
			{
				node = Balance(node);

				var left = node.left;
				var right = node.right;

				node.aabb.asUnionOf(left.aabb, right.aabb);
				node.invHeight = 1 + (Math.Max(left.invHeight, right.invHeight));

				node = node.parent;
			}
		}
		else
		{
			_root = sibling;
			_root.parent = null;
		}

		////////////////assert(numLeaves == [for (k in leaves.keys()) k].length);

		Validate();
	}

	/** 
	 * Removes all nodes from the tree. 
	 * 
	 * @param	resetPool	If true the internal pool will be reset to its initial capacity.
	 */
	public void Clear(bool resetPool = false)
	{
		_root = null;
		NumLeaves = 0;
	}

	public void Balance()
	{
		Rebuild();
	}

	/// <summary>
	/// Rebuilds the tree using a bottom-up strategy (should result in a better tree, but is very expensive).
	/// </summary>
	public void Rebuild()
	{
		if (_root == null) return;

		// free non-leaf nodes
		var leafs = new List<Node<T>>();
		using (var nodesEnum = GetNodesEnumerator())
		{
			while (nodesEnum.MoveNext())
			{
				var node = nodesEnum.Current;
				if (node == null)
					continue;
				if (node.isLeaf())
				{
					node.parent = null;
					leafs.Add(node);
				}
			}
		}
		_root = null;

		var aabb = new AABB();
		var count = leafs.Count;
		while (count > 1)
		{
			if (count % 10 == 0)
				Debug.Log("count: " + count);
			var minCost = float.PositiveInfinity;
			var iMin = -1;
			var jMin = -1;

			// find pair with least perimeter enlargement
			for (var i = 0; i < count; i++)
			{
				var iAABB = leafs[i].aabb;

				for (var j = i + 1; j < count; j++)
				{
					var jAABB = leafs[j].aabb;

					aabb.asUnionOf(iAABB, jAABB);
					var cost = aabb.getPerimeter();
					if (cost < minCost)
					{
						iMin = i;
						jMin = j;
						minCost = cost;
					}
				}
			}

			var left = leafs[iMin];
			var right = leafs[jMin];
			aabb.asUnionOf(left.aabb, right.aabb);
			var parent = new Node<T>(aabb, default(T), null);
			parent.left = left;
			parent.right = right;
			parent.invHeight = 1 + Math.Max(left.invHeight, right.invHeight);
			//nodes[parent.id] = parent;

			left.parent = parent;
			right.parent = parent;

			leafs[iMin] = parent;
			leafs[jMin] = leafs[count - 1];

			count--;
		}

		_root = leafs[0];

		Validate();
	}


	public List<T> FindValuesAt(AABB targetAABB, bool strictMode = false, AdditionalCheckDelegate callback = null)
	{
		var result = new List<T>();
		TryFindValuesAt(targetAABB, out result, strictMode, callback);
		return result;
	}

	public bool TryFindValuesAt(AABB targetAABB, out List<T> results, bool strictMode = false,
		AdditionalCheckDelegate callback = null)
	{
		var result = Query(targetAABB, strictMode);
		results = result;
		if (result.Count == 0)
		{
			return false;
		}

		return true;

	}

	/**
	 * Queries the tree for objects in the specified AABB.
	 * 
	 * @param	into			Hit objects will be appended to this (based on additionalCheck return value).
	 * @param	strictMode		If set to true only objects fully contained in the AABB will be processed. Otherwise they will be checked for intersection (default).
	 * @param	additionalCheck		A function called for every object hit (function additionalCheck(data:T, id:Int):HitBehaviour).
	 * 
	 * @return A list of all the objects found (or `into` if it was specified).
	 */

	public delegate HitBehaviour AdditionalCheckDelegate(Node<T> node);

	public List<T> Query(AABB queryAABB, bool strictMode = false, List<T> into = null,
		AdditionalCheckDelegate additionalCheck = null)
	{
		var res = into != null ? into : new List<T>();
		if (_root == null)
		{
			return res;
		}

		var stack = new Stack<Node<T>>();
		stack.Push(_root);
		//var cnt = 0;
		while (stack.Count > 0)
		{
			var node = stack.Pop();
			//cnt++;
			if (!queryAABB.overlaps(node.aabb))
			{
				continue;
			}

			if (node.isLeaf() && (!strictMode || (queryAABB.contains(node.aabb))))
			{
				if (additionalCheck == null)
				{
					res.Add(node.data);
				}
				else
				{
					var hitBehaviour = additionalCheck(node);
					if (hitBehaviour == HitBehaviour.INCLUDE || hitBehaviour == HitBehaviour.INCLUDE_AND_STOP)
					{
						res.Add(node.data);
					}

					if (hitBehaviour == HitBehaviour.STOP || hitBehaviour == HitBehaviour.INCLUDE_AND_STOP)
					{
						break;
					}
				}
			}
			else
			{
				if (node.left != null)
				{
					stack.Push(node.left);
				}

				if (node.right != null)
				{
					stack.Push(node.right);
				}
			}
		}

		//trace("examined: " + cnt);
		return res;
	}

	public List<Node<T>> FindNodes(AABB queryAABB, bool strictMode = false, List<Node<T>> into = null,
		AdditionalCheckDelegate additionalCheck = null)
	{
		var res = into != null ? into : new List<Node<T>>();
		if (_root == null)
		{
			return res;
		}

		var stack = new Stack<Node<T>>();
		stack.Push(_root);
		//var cnt = 0;
		while (stack.Count > 0)
		{
			var node = stack.Pop();
			//cnt++;
			if (!queryAABB.overlaps(node.aabb))
			{
				continue;
			}

			if (node.isLeaf() && (!strictMode || (queryAABB.contains(node.aabb))))
			{
				if (additionalCheck == null)
				{
					res.Add(node);
				}
				else
				{
					var hitBehaviour = additionalCheck(node);
					if (hitBehaviour == HitBehaviour.INCLUDE || hitBehaviour == HitBehaviour.INCLUDE_AND_STOP)
					{
						res.Add(node);
					}

					if (hitBehaviour == HitBehaviour.STOP || hitBehaviour == HitBehaviour.INCLUDE_AND_STOP)
					{
						break;
					}
				}
			}
			else
			{
				if (node.left != null)
				{
					stack.Push(node.left);
				}

				if (node.right != null)
				{
					stack.Push(node.right);
				}
			}
		}

		//trace("examined: " + cnt);
		return res;
	}

	/**
	 * Queries the tree for objects overlapping the specified point.
	 * 
	 * @param	into			Hit objects will be appended to this (based on additionalCheck return value).
	 * @param	additionalCheck		A function called for every object hit (function additionalCheck(data:*, id:int):HitBehaviour).
	 * 
	 * @return A list of all the objects found (or `into` if it was specified).
	 */
	public List<T> QueryPoint(float x, float y, List<T> into, AdditionalCheckDelegate additionalCheck)
	{
		return Query(new AABB(x, y), false, into, additionalCheck);
	}

	/**
	 * Queries the tree for objects crossing the specified ray.
	 * 
	 * Notes: 
	 * 	- the intersecting objects will be returned in no particular order (closest ones to the start point may appear later in the list!).
	 *  - the additionalCheck will also be called if an object fully contains the ray's start and end point.
	 * 
	 * TODO: see how this can be optimized and return results in order
	 * 
	 * @param	into		Hit objects will be appended to this (based on additionalCheck return value).
	 * @param	additionalCheck	A function called for every object hit (function additionalCheck(data:T, id:Int):HitBehaviour).
	 * 
	 * @return A list of all the objects found (or `into` if it was specified).
	 */
	public List<T> RayCast(float fromX, float fromY, float toX, float toY, List<T> into,
		AdditionalCheckDelegate additionalCheck)
	{
		var res = into != null ? into : new List<T>();
		if (_root == null)
		{
			return res;
		}

		var queryAABBResults = new List<Node<T>>();
		float tmp;
		var rayAABB = new AABB(fromX, fromY, toX - fromX, toY - fromY);
		if (rayAABB.minX > rayAABB.maxX)
		{
			tmp = rayAABB.maxX;
			rayAABB.maxX = rayAABB.minX;
			rayAABB.minX = tmp;
		}

		if (rayAABB.minY > rayAABB.maxY)
		{
			tmp = rayAABB.maxY;
			rayAABB.maxY = rayAABB.minY;
			rayAABB.minY = tmp;
		}

		Query(rayAABB, false, null, (node) =>
		{
			var aabb = node.aabb;
			var fromPointAABB = new AABB(fromX, fromY);

			var hit = false;
			for (int i = 0; i < 4; i++)
			{
				// test for intersection with node's aabb edges
				switch (i)
				{
					case 0: // top edge
						hit = SegmentIntersect(fromX, fromY, toX, toY, aabb.minX, aabb.minY, aabb.maxX, aabb.minY);
						break;
					case 1: // left edge
						hit = SegmentIntersect(fromX, fromY, toX, toY, aabb.minX, aabb.minY, aabb.minX, aabb.maxY);
						break;
					case 2: // bottom edge
						hit = SegmentIntersect(fromX, fromY, toX, toY, aabb.minX, aabb.maxY, aabb.maxX, aabb.maxY);
						break;
					case 3: // right edge
						hit = SegmentIntersect(fromX, fromY, toX, toY, aabb.maxX, aabb.minY, aabb.maxX, aabb.maxY);
						break;
					default:
						break;
				}

				if (hit)
				{
					break;
				}
			}

			// add intersected node id to array
			if (hit || (aabb.contains(fromPointAABB)))
			{
				queryAABBResults.Add(node);
			}

			return HitBehaviour.SKIP; // don't bother adding to results
		});

		foreach (var node in queryAABBResults)
		{
			if (additionalCheck != null)
			{
				var hitBehaviour = additionalCheck(node);
				if (hitBehaviour == HitBehaviour.INCLUDE || hitBehaviour == HitBehaviour.INCLUDE_AND_STOP)
				{
					res.Add(node.data);
				}

				if (hitBehaviour == HitBehaviour.STOP || hitBehaviour == HitBehaviour.INCLUDE_AND_STOP)
				{
					break;
				}
			}
			else
			{
				res.Add(node.data);
			}
		}

		return res;
	}

	/**
     * Performs a left or right rotation if `nodeId` is unbalanced.
     * 
     * @return The new parent index.
     */
	private Node<T> Balance(Node<T> node)
	{
		var A = node;
		Assert(A != null);

		if (A.isLeaf() || A.invHeight < 2)
		{
			return A;
		}

		var B = A.left;
		var C = A.right;

		var balanceValue = C.invHeight - B.invHeight;

		// rotate C up
		if (balanceValue > 1)
		{
			return RotateLeft(A, B, C);
		}

		// rotate B up
		if (balanceValue < -1)
		{
			return RotateRight(A, B, C);
		}

		return A;
	}

	/** Returns max height distance between two children (of the same parent) in the tree. */
	public int GetMaxBalance()
	{
		var maxBalance = 0;
		using (var nodesEnum = GetNodesEnumerator())
		{
			while (nodesEnum.MoveNext())
			{
				var node = nodesEnum.Current;
				if (node == null || node.invHeight <= 1)
					continue;

				Assert(!node.isLeaf());

				var left = node.left;
				var right = node.right;
				var balance = Math.Abs(right.invHeight - left.invHeight);
				maxBalance = (Math.Max(maxBalance, balance));
			}
		}

		return maxBalance;
	}

	/*
	 *           A			parent
	 *         /   \
	 *        B     C		left and right nodes
	 *             / \
	 *            F   G
	 */
	private Node<T> RotateLeft(Node<T> parentNode, Node<T> leftNode, Node<T> rightNode)
	{
		var F = rightNode.left;
		var G = rightNode.right;

		// swap A and C
		rightNode.left = parentNode;
		rightNode.parent = parentNode.parent;
		parentNode.parent = rightNode;

		// A's old parent should point to C
		if (rightNode.parent != null)
		{
			if (rightNode.parent.left == parentNode)
			{
				rightNode.parent.left = rightNode;
			}
			else
			{
				Assert(rightNode.parent.right == parentNode);
				rightNode.parent.right = rightNode;
			}
		}
		else
		{
			_root = rightNode;
		}

		// rotate
		if (F.invHeight > G.invHeight)
		{
			rightNode.right = F;
			parentNode.right = G;
			G.parent = parentNode;
			parentNode.aabb.asUnionOf(leftNode.aabb, G.aabb);
			rightNode.aabb.asUnionOf(parentNode.aabb, F.aabb);

			parentNode.invHeight = 1 + (Math.Max(leftNode.invHeight, G.invHeight));
			rightNode.invHeight = 1 + (Math.Max(parentNode.invHeight, F.invHeight));
		}
		else
		{
			rightNode.right = G;
			parentNode.right = F;
			F.parent = parentNode;
			parentNode.aabb.asUnionOf(leftNode.aabb, F.aabb);
			rightNode.aabb.asUnionOf(parentNode.aabb, G.aabb);

			parentNode.invHeight = 1 + (Math.Max(leftNode.invHeight, F.invHeight));
			rightNode.invHeight = 1 + (Math.Max(parentNode.invHeight, G.invHeight));
		}

		return rightNode;
	}

	/*
	 *           A			parent
	 *         /   \
	 *        B     C		left and right nodes
	 *       / \
	 *      D   E
	 */
	private Node<T> RotateRight(Node<T> parentNode, Node<T> leftNode, Node<T> rightNode)
	{
		var D = leftNode.left;
		var E = leftNode.right;

		// swap A and B
		leftNode.left = parentNode;
		leftNode.parent = parentNode.parent;
		parentNode.parent = leftNode;

		// A's old parent should point to B
		if (leftNode.parent != null)
		{
			if (leftNode.parent.left == parentNode)
			{
				leftNode.parent.left = leftNode;
			}
			else
			{
				Assert(leftNode.parent.right == parentNode);
				leftNode.parent.right = leftNode;
			}
		}
		else
		{
			_root = leftNode;
		}

		// rotate
		if (D.invHeight > E.invHeight)
		{
			leftNode.right = D;
			parentNode.left = E;
			E.parent = parentNode;
			parentNode.aabb.asUnionOf(rightNode.aabb, E.aabb);
			leftNode.aabb.asUnionOf(parentNode.aabb, D.aabb);

			parentNode.invHeight = 1 + (Math.Max(rightNode.invHeight, E.invHeight));
			leftNode.invHeight = 1 + (Math.Max(parentNode.invHeight, D.invHeight));
		}
		else
		{
			leftNode.right = E;
			parentNode.left = D;
			D.parent = parentNode;
			parentNode.aabb.asUnionOf(rightNode.aabb, D.aabb);
			leftNode.aabb.asUnionOf(parentNode.aabb, E.aabb);

			parentNode.invHeight = 1 + (Math.Max(rightNode.invHeight, D.invHeight));
			leftNode.invHeight = 1 + (Math.Max(parentNode.invHeight, E.invHeight));
		}

		return leftNode;
	}

	/** Tests validity of node with the specified `id` (and its children). */
	private void validateNode(Node<T> nodeForValidate)
	{
		var aabb = new AABB();
		var root = nodeForValidate;
		var stack = new Stack<Node<T>>();
		stack.Push(root);
		while (stack.Count > 0)
		{
			var node = stack.Pop();
			Assert(node != null);

			var left = node.left;
			var right = node.right;

			if (node.isLeaf())
			{
				Assert(left == null);
				Assert(right == null);
				node.invHeight = 0;
				continue;
			}

			Assert(node.invHeight == 1 + Math.Max(left.invHeight, right.invHeight));
			aabb.asUnionOf(left.aabb, right.aabb);
			Assert(Math.Abs(node.aabb.minX - aabb.minX) < 0.000001);
			Assert(Math.Abs(node.aabb.minY - aabb.minY) < 0.000001);
			Assert(Math.Abs(node.aabb.maxX - aabb.maxX) < 0.000001);
			Assert(Math.Abs(node.aabb.maxY - aabb.maxY) < 0.000001);
		}
	}

	public IEnumerable<T> RemoveAll(AABB queryAABB)
	{
		if (_root == null)
			return new List<T>();
		var leafNodes = FindNodes(queryAABB);
		foreach (var leafNode in leafNodes)
		{
			var parent = leafNode.parent;
			var grandParent = parent.parent;
			var sibling = parent.left == leafNode ? parent.right : parent.left;

			if (grandParent != null)
			{
				// connect sibling to grandParent
				if (grandParent.left == parent)
				{
					grandParent.left = sibling;
				}
				else
				{
					grandParent.right = sibling;
				}
				sibling.parent = grandParent;

				// adjust ancestor bounds
				var node = grandParent;
				while (node != null)
				{
					node = Balance(node);

					var left = node.left;
					var right = node.right;

					node.aabb.asUnionOf(left.aabb, right.aabb);
					node.invHeight = 1 + Math.Max(left.invHeight, right.invHeight);

					node = node.parent;
				}
			}
			else
			{
				_root = sibling;
				_root.parent = null;
			}

			// destroy parent
			//Assert(parent.id != -1);		
			Assert(parent != null);
			//disposeNode(parent.id);
			//disposeNode(leafId);

			//Assert(NumLeaves == [for (k in leaves.keys()) k].length);

			Validate();
		}

		return leafNodes.Select(node => node.data);
	}

	private static bool SegmentIntersect(float p0x, float p0y, float p1x, float p1y, float q0x, float q0y, float q1x, float q1y)
	{
		float intX, intY,
		 a1, a2,
		 b1, b2,
		 c1, c2;

		a1 = p1y - p0y;
		b1 = p0x - p1x;
		c1 = p1x * p0y - p0x * p1y;
		a2 = q1y - q0y;
		b2 = q0x - q1x;
		c2 = q1x * q0y - q0x * q1y;

		var denom = a1 * b2 - a2 * b1;
		if (denom == 0)
		{
			return false;
		}

		intX = (b1 * c2 - b2 * c1) / denom;
		intY = (a2 * c1 - a1 * c2) / denom;

		// check to see if distance between intersection and endpoints
		// is longer than actual segments.
		// return false otherwise.
		if (DistanceSquared(intX, intY, p1x, p1y) > DistanceSquared(p0x, p0y, p1x, p1y))
		{
			return false;
		}

		if (DistanceSquared(intX, intY, p0x, p0y) > DistanceSquared(p0x, p0y, p1x, p1y))
		{
			return false;
		}

		if (DistanceSquared(intX, intY, q1x, q1y) > DistanceSquared(q0x, q0y, q1x, q1y))
		{
			return false;
		}

		if (DistanceSquared(intX, intY, q0x, q0y) > DistanceSquared(q0x, q0y, q1x, q1y))
		{
			return false;
		}

		return true;
	}

	private static float DistanceSquared(float px, float py, float qx, float qy) { return Sqr(px - qx) + Sqr(py - qy); }

	private static float Sqr(float x) { return x * x; }


#if UNITY_EDITOR

	private void Validate()
	{
		if (_root != null)
		{
			validateNode(_root);
		}
	}

	private static void Assert(bool cond)
	{
		if (!cond)
		{
			throw new AssertionException("ASSERT FAILED!", "ASSERT FAILED!");
		}
	}

#else

    private void Validate()
    {
        return;
    }

    private static void Assert(bool cond)
    {
        return;
    }

#endif

	/** Iterator over leaves data. So you can do: `for (var data in tree) ...`. */
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<T> GetEnumerator()//todo: add versions
	{
		using (var nodesEnum = GetNodesEnumerator())
		{
			while (nodesEnum.MoveNext())
				if (nodesEnum.Current != null && nodesEnum.Current.isLeaf())
					yield return nodesEnum.Current.data;
		}
	}

	public IEnumerator<Node<T>> GetNodesEnumerator()//todo: add versions
	{
		var left = new Stack<Node<T>>();
		var right = new Stack<Node<T>>();

		var addLeft = new Action<Node<T>>(node =>
		{
			if (node.left != null)
			{
				left.Push(node.left);
			}
		});
		var addRight = new Action<Node<T>>(node =>
			{
				if (node.right != null)
				{
					right.Push(node.right);
				}
			}
		);

		if (_root != null)
		{
			yield return _root;

			addLeft(_root);
			addRight(_root);

			while (true)
			{
				if (left.Count > 0)
				{
					var item = left.Pop();

					addLeft(item);
					addRight(item);

					yield return item;
				}
				else if (right.Count > 0)
				{
					var item = right.Pop();

					addLeft(item);
					addRight(item);

					yield return item;
				}
				else
				{
					break;
				}
			}
		}
	}
}
