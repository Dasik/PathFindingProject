using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ds
{

    public enum InsertChoice
    {
        PARENT, // choose parent as sibling node
        DESCEND_LEFT, // descend left branch of the tree
        DESCEND_RIGHT // descent right branch of the tree
    }

/**
 * Interface for strategies to apply when inserting a new leaf.
 * 
 * @author azrafe7
 */
    public interface IInsertStrategy<T>
    {
        /** Choose which behaviour to apply in insert context. */
        InsertChoice choose<T>(AABB leafAABB, Node<T> parent, Object extraData=null);
    }
}