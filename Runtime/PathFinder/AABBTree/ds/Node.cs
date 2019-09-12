using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ds
{
    public class Node<T>
    {
        public Node<T> left = null;
        public Node<T> right = null;
        public Node<T> parent = null;

        // fat AABB
        public AABB aabb;

        // 0 for leafs
        public int invHeight = -1;

        public T data;
        
        public Node(AABB aabb, T data, Node<T> parent = null)
        {
            this.aabb = aabb;
            this.data = data;
            this.parent = parent;
        }

        public Node(float x, float y, float width = 0, float height = 0, T data = default(T), Node<T> parent = null,int id = -1)
        {
            this.aabb = new AABB(x, y, width, height);
            this.data = data;
            this.parent = parent;
        }

/** If it's a leaf both left and right nodes should be null. */
        public bool isLeaf()
        {
            return left == null;
        }
    }

}