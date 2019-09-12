using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ds
{
    public class InsertStrategyArea<T> : IInsertStrategy<T>
    {
        AABB combinedAABB = new AABB();

        public InsertStrategyArea()
        {
        }

        public InsertChoice choose<T>(AABB leafAABB, Node<T> parent, Object extraData = null)
        {
            var left = parent.left;
            var right = parent.right;
            var area = parent.aabb.getArea();

            combinedAABB.asUnionOf(parent.aabb, leafAABB);
            var combinedArea = combinedAABB.getArea();

// cost of creating a new parent for this node and the new leaf
            var costParent = 2 * combinedArea;

// minimum cost of pushing the leaf further down the tree
            var costDescend = 2 * (combinedArea - area);

// cost of descending into left node
            combinedAABB.asUnionOf(leafAABB, left.aabb);
            var costLeft = combinedAABB.getArea() + costDescend;
            if (!left.isLeaf())
            {
                costLeft -= left.aabb.getArea();
            }

            // cost of descending into right node
            combinedAABB.asUnionOf(leafAABB, right.aabb);
            var costRight = combinedAABB.getArea() + costDescend;
            if (!right.isLeaf())
            {
                costRight -= right.aabb.getArea();
            }

            // break/descend according to the minimum cost
            if (costParent < costLeft && costParent < costRight)
            {
                return InsertChoice.PARENT;
            }

            // descend
            return costLeft < costRight ? InsertChoice.DESCEND_LEFT : InsertChoice.DESCEND_RIGHT;
        }
    }
}