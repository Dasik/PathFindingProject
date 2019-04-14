using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ds
{
    public class InsertStrategyPerimeter<T> : IInsertStrategy<T>
    {
        AABB combinedAABB = new AABB();

        public InsertStrategyPerimeter()
        {
        }

        public InsertChoice choose<T>(AABB leafAABB, Node<T> parent, Object extraData = null)
        {
            var left = parent.left;
            var right = parent.right;
            var perimeter = parent.aabb.getPerimeter();

            combinedAABB.asUnionOf(parent.aabb, leafAABB);
            var combinedPerimeter = combinedAABB.getPerimeter();

// cost of creating a new parent for this node and the new leaf
            var costParent = 2 * combinedPerimeter;

// minimum cost of pushing the leaf further down the tree
            var costDescend = 2 * (combinedPerimeter - perimeter);

// cost of descending into left node
            combinedAABB.asUnionOf(leafAABB, left.aabb);
            var costLeft = combinedAABB.getPerimeter() + costDescend;
            if (!left.isLeaf())
            {
                costLeft -= left.aabb.getPerimeter();
            }

            // cost of descending into right node
            combinedAABB.asUnionOf(leafAABB, right.aabb);
            var costRight = combinedAABB.getPerimeter() + costDescend;
            if (!right.isLeaf())
            {
                costRight -= right.aabb.getPerimeter();
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