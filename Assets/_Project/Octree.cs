using System.Collections.Generic;
using UnityEngine;

namespace Neo {   
    public struct Boundary {
        public Vector3 min;
        public Vector3 max;
    }

    public struct BoundaryDepthPair {
        public Boundary Boundary;
        public int Depth; // The depth of the corresponding boundary
    }

    public sealed class Octree {
        public static int MAX_DEPTH = 12;
        const int         MAX_OBJECTS_COUNT = 1;
        const float       MIN_SIZE = 1f;

        public Octree[]  ChildNodes   { get; private set; }
        public Boundary  NodeBoundary { get; private set; }
        public bool      IsLeafNode   { get; private set; }
        public int       CurrentDepth { get; private set; }
        List<GameObject> m_allGameObjects;

        public Octree(Boundary boundary, int currentDepth) {
            ChildNodes       = null;
            NodeBoundary     = boundary;
            IsLeafNode       = true;
            m_allGameObjects = new List<GameObject>();
            CurrentDepth     = currentDepth;
        }

        public void SetAllGameObjects(List<GameObject> gameObjects) => m_allGameObjects = gameObjects;

        public void AddGameObject(GameObject go) => m_allGameObjects.Add(go);

        public void BuildOctreeIfNecessary() {
            // Check if need to build octree in the first place
            bool needsToSubdivide = m_allGameObjects.Count > MAX_OBJECTS_COUNT;
            if (!needsToSubdivide) return;

            bool canSubdivide = (NodeBoundary.max.x - NodeBoundary.min.x) > MIN_SIZE &&
                                (NodeBoundary.max.y - NodeBoundary.min.y) > MIN_SIZE &&
                                (NodeBoundary.max.z - NodeBoundary.min.z) > MIN_SIZE &&
                                CurrentDepth < MAX_DEPTH;
            if (!canSubdivide) return;

            // If 'needsToSubdivide' and 'canSubdivide', then actually build the child octree nodes
            IsLeafNode = false;
            ChildNodes = new Octree[8];
            Vector3 childSize = (NodeBoundary.max - NodeBoundary.min) * 0.5f;
            int childNodeIdx = 0;
            for (int x = 0; x < 2; x++) {
                float startX = NodeBoundary.min.x + (x * childSize.x);
                for (int y = 0; y < 2; y++) {
                    float startY = NodeBoundary.min.y + (y * childSize.y);
                    for (int z = 0; z < 2; z++) {
                        float startZ = NodeBoundary.min.z + (z * childSize.z);
                        ChildNodes[childNodeIdx++] = new Octree(new Boundary {
                            min = new Vector3(startX, startY, startZ),
                            max = new Vector3(startX + childSize.x, startY + childSize.y, startZ + childSize.z)
                        }, CurrentDepth + 1);
                    }
                }
            }

            // Once the trees have been subdivided, add all the
            // gameObjects into whatever child node they intersect with
            m_allGameObjects.ForEach(go => {
                for (int i = 0; i < 8; i++) {
                    Octree childNode = ChildNodes[i];

                    Boundary goAABB = _GetAABB(go);
                    Boundary nodeAABB = childNode.NodeBoundary;
                    if (_IsIntersecting(goAABB, nodeAABB)) {
                        childNode.AddGameObject(go);
                    }
                }
            });

            // Now all the gameObjects are within the child octree nodes
            // Trigger the building of the Octree for each of said child nodes
            for (int i = 0; i < 8; i++) {
                ChildNodes[i].BuildOctreeIfNecessary();
            }
        }

        public static void GetLeafNodeBoundaries(Octree node, ref List<BoundaryDepthPair> outLeafNodeBoundaries) {
            if (node == null) return; 
            for (int i = 0; i < 8; i++) {
                Octree childNode = node.ChildNodes[i];
                if (childNode == null) return;
                if (childNode.IsLeafNode) {
                    outLeafNodeBoundaries.Add(new BoundaryDepthPair {
                        Boundary = node.NodeBoundary, 
                        Depth = node.CurrentDepth
                    });
                }
                else {
                    GetLeafNodeBoundaries(childNode, ref outLeafNodeBoundaries);
                }
            }
        }

        Boundary _GetAABB(GameObject go) {
            Vector3 pos = go.transform.position;
            Vector3 halfScale = go.transform.lossyScale * 0.5f;
            return new Boundary() {
                min = pos - halfScale,
                max = pos + halfScale
            };
        }

        bool _IsIntersecting(Boundary a, Boundary b) {
            if (a.max.x < b.min.x || a.min.x > b.max.x) return false;
            if (a.max.y < b.min.y || a.min.y > b.max.y) return false;
            if (a.max.z < b.min.z || a.min.z > b.max.z) return false;
            return true;
        }
    }
}
