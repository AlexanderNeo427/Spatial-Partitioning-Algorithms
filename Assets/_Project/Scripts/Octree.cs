using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Neo
{   public struct Boundary {
        public Vector3 min;
        public Vector3 max;
    }

    public sealed class TreeNode {
        readonly Boundary m_boundary;

        public List<GameObject> gameObjects;
        public Boundary Boundary => m_boundary;

        public TreeNode(Boundary boundary) { m_boundary = boundary; }    
    }

    public sealed class Octree {
        enum NODE_INDEX {
            LEFT_BOTTOM_BACK,
            RIGHT_BOTTOM_BACK,
            LEFT_UP_BACK,
            RIGHT_UP_BACK,
            LEFT_BOTTOM_FRONT,
            RIGHT_BOTTOM_FRONT,
            LEFT_UP_FRONT,
            RIGHT_UP_FRONT,
        }
        const int NUM_OBJECTS_THRESHOLD = 10;

        public Octree[] ChildNodes { get; private set; }
        public List<GameObject> AllGameObjects { get; private set; }
        public Boundary NodeBoundary { get; private set; }
        public bool IsLeafNode { get; private set; }

        public Octree(Boundary boundary, List<GameObject> gameObjects) {
            ChildNodes = new Octree[8];
            for (int i = 0; i < 8; i++) {
                ChildNodes[i] = null;
            }
            AllGameObjects = gameObjects;
            NodeBoundary = boundary;
            IsLeafNode = AllGameObjects.Count > NUM_OBJECTS_THRESHOLD;

            if (AllGameObjects.Count > NUM_OBJECTS_THRESHOLD) {
                IsLeafNode = false;
            }
        }
    }
}
