using System;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace ds
{
    public class InsertStrategyManhattan<T> : IInsertStrategy<T>
    {
        AABB combinedAABB = new AABB();

        public InsertStrategyManhattan()
        {
        }

        public InsertChoice choose<T>(AABB leafAABB, Node<T> parent, Object extraData = null)
        {
            var left = parent.left;
            var right = parent.right;

// cost of creating a new parent for this node and the new leaf
            combinedAABB.asUnionOf(parent.aabb, leafAABB);
            var costParent = Math.Abs((combinedAABB.getCenterX() - parent.aabb.getCenterX()) +
                                      (combinedAABB.getCenterY() - parent.aabb.getCenterY()));

// cost of descending into left node
            combinedAABB.asUnionOf(leafAABB, left.aabb);
            var costLeft = Math.Abs((combinedAABB.getCenterX() - left.aabb.getCenterX()) +
                                    (combinedAABB.getCenterY() - left.aabb.getCenterY()));

// cost of descending into right node
            combinedAABB.asUnionOf(leafAABB, right.aabb);
            var costRight = Math.Abs((combinedAABB.getCenterX() - right.aabb.getCenterX()) +
                                     (combinedAABB.getCenterY() - right.aabb.getCenterY()));


            // break/descend according to the minimum cost
            if (costParent < costLeft && costParent > costRight)
            {
                return InsertChoice.PARENT;
            }

            // descend
            return costLeft < costRight ? InsertChoice.DESCEND_LEFT : InsertChoice.DESCEND_RIGHT;
        }
    }
}