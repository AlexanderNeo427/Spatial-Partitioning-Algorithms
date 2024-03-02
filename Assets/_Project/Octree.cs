using System.Collections.Generic;
using UnityEngine;

namespace Neo {   
    public struct Boundary {
        public Vector3 min;
        public Vector3 max;
    }

    public sealed class Octree {
        const int MAX_OBJECTS_COUNT = 15;
        const int MAX_DEPTH = 5;
        const int MIN_SIZE = 10;

        public Octree[] ChildNodes { get; private set; }
        public Boundary NodeBoundary { get; private set; }
        public bool IsLeafNode { get; private set; }
        public int CurrentDepth { get; private set; }
 
        public Octree(Boundary boundary, List<GameObject> gameObjects, int currentDepth) {
            ChildNodes = null;
            NodeBoundary = boundary;
            IsLeafNode = true;
            CurrentDepth = currentDepth;

            bool needsToSubdivide = gameObjects.Count > MAX_OBJECTS_COUNT;
            if (!needsToSubdivide) return; 

            bool canSubdivide = (boundary.max.x - boundary.min.x) > MIN_SIZE &&
                                (boundary.max.y - boundary.min.y) > MIN_SIZE &&
                                (boundary.max.z - boundary.min.z) > MIN_SIZE &&
                                currentDepth < MAX_DEPTH;
            if (!canSubdivide) return; 

            // If 'needsToSubdivide' and 'canSubdivide'
            IsLeafNode = false;
            ChildNodes = new Octree[8];
            Vector3 childSize = new(
                (boundary.max.x - boundary.min.x) * 0.5f,
                (boundary.max.y - boundary.min.y) * 0.5f,
                (boundary.max.z - boundary.min.z) * 0.5f
            );
            int idx = 0;
            for (int x = 0; x < 2; x++) {
                float startX = boundary.min.x + x * childSize.x;
                for (int y = 0; y < 2; y++) {
                    float startY = boundary.min.y + y * childSize.y;
                    for (int z = 0; z < 2; z++) {
                        float startZ = boundary.min.z + z * childSize.z;
                        ChildNodes[idx++] = new Octree(new Boundary {
                            min = new Vector3(startX, startY, startZ),
                            max = new Vector3(startX + childSize.x, startY + childSize.y, startZ + childSize.z)
                        }, gameObjects, currentDepth + 1);
                    }
                }
            }
        }

        public static void GetLeafNodeBoundaries(Octree node, ref List<Boundary> outLeafNodeBoundaries, ref int itrCount) {
            if (node == null) return; 
            for (int i = 0; i < 8; i++) {
                Octree childNode = node.ChildNodes[i];
                if (childNode == null) return;
                if (childNode.IsLeafNode) {
                    outLeafNodeBoundaries.Add(childNode.NodeBoundary);
                }
                else {
                    GetLeafNodeBoundaries(childNode, ref outLeafNodeBoundaries, ref itrCount);
                }
                itrCount++;
            }
        }
    }
}
