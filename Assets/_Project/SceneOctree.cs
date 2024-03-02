using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Neo {
    [DisallowMultipleComponent]
    public sealed class SceneOctree : MonoBehaviour {
        [Header("--- Settings ---")]
        [SerializeField] uint _sphereCount = 80;

        [Header("--- Dependencies ---")]
        [SerializeField] GameObject _spherePrefab;

        Boundary         m_worldBoundary;
        List<GameObject> m_allSpheres;
        Octree           m_rootNode;
        List<Color>      m_debugColors = new();

        void Awake() {
            Assert.IsNotNull(_spherePrefab);
            GetComponent<MeshRenderer>().enabled = false;
        }

        void Start() {
            for (int i = 0; i < Octree.MAX_DEPTH; i++) {
                //if      (i % 3 == 0) m_debugColors.Add(new Color(1f, 0.50196f, 0.4294f));
                //else if (i % 2 == 0) m_debugColors.Add(Color.cyan);
                //else if (i % 1 == 0) m_debugColors.Add(new Color(0.45f, 1f, 0.48f));

                if      (i % 5 == 0) m_debugColors.Add(new Color(0.627451f, 0.125490f, 0.941176f));
                else if (i % 4 == 0) m_debugColors.Add(Color.green);
                else if (i % 3 == 0) m_debugColors.Add(Color.yellow);
                else if (i % 2 == 0) m_debugColors.Add(Color.blue);
                else if (i % 1 == 0) m_debugColors.Add(Color.red);
            }
            Assert.IsTrue(m_debugColors.Count >= Octree.MAX_DEPTH);
            m_worldBoundary = _InitializeBoundingBoxWalls(0.8f);
            m_allSpheres = _InitializeSpheres((int)_sphereCount, 0.4f, m_worldBoundary);
        }

        void Update() {
            m_rootNode = new Octree(m_worldBoundary, 0);
            m_rootNode.SetAllGameObjects(m_allSpheres);
            m_rootNode.BuildOctreeIfNecessary();
        }

        void OnDrawGizmos() {
            if (!Application.isPlaying) { return; }

            List<BoundaryDepthPair> leafNodeBoundaries = new();
            Octree.GetLeafNodeBoundaries(m_rootNode, ref leafNodeBoundaries);

            // Color color = new(0.48f, 0.93f, 0.5f); // Light green
            leafNodeBoundaries.ForEach(boundaryDepthPair => {
                _DrawBoundary(boundaryDepthPair.Boundary, m_debugColors[boundaryDepthPair.Depth]);
            });
            _DrawBoundary(m_worldBoundary, new Color(0.937f, 0.275f, 1f));
        }

        private Boundary _InitializeBoundingBoxWalls(float depth) {
            GameObject front = GameObject.CreatePrimitive(PrimitiveType.Cube);
            front.name = "Front Wall";
            front.transform.localPosition = transform.position + transform.forward * ((transform.localScale.z * 0.5f) + (depth * 0.5f));
            front.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, depth);
            front.GetComponent<MeshRenderer>().enabled = false;

            GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "Back Wall";
            back.transform.localPosition = transform.position - transform.forward * ((transform.localScale.z * 0.5f) + (depth * 0.5f));
            back.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, depth);
            back.GetComponent<MeshRenderer>().enabled = false;

            GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
            right.name = "Right Wall";
            right.transform.localPosition += transform.position + transform.right * ((transform.localScale.x * 0.5f) + (depth * 0.5f));
            right.transform.localScale = new Vector3(depth, transform.localScale.y, transform.localScale.z);
            right.GetComponent<MeshRenderer>().enabled = false;

            GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
            left.name = "Left Wall";
            left.transform.localPosition = transform.position - transform.right * ((transform.localScale.x * 0.5f) + (depth * 0.5f));
            left.transform.localScale = new Vector3(depth, transform.localScale.y, transform.localScale.z);
            left.GetComponent<MeshRenderer>().enabled = false;

            GameObject up = GameObject.CreatePrimitive(PrimitiveType.Cube);
            up.name = "Up Wall";
            up.transform.localPosition = transform.position + transform.up * ((transform.localScale.y * 0.5f) + (depth * 0.5f));
            up.transform.localScale = new Vector3(transform.localScale.x, depth, transform.localScale.z);
            up.GetComponent<MeshRenderer>().enabled = false;

            GameObject down = GameObject.CreatePrimitive(PrimitiveType.Cube);
            down.name = "Down Wall";
            down.transform.localPosition = transform.position - transform.up * ((transform.localScale.y * 0.5f) + (depth * 0.5f));
            down.transform.localScale = new Vector3(transform.localScale.x, depth, transform.localScale.z);
            down.GetComponent<MeshRenderer>().enabled = false;

            return new Boundary {
                min = transform.position - transform.lossyScale * 0.5f,
                max = transform.position + transform.lossyScale * 0.5f
            };
        }

        List<GameObject> _InitializeSpheres(int sphereCount, float radius, Boundary worldBoundary) {
            float PADDING = radius + 3f;
            
            List<GameObject> allSpheres = new();
            for (int i = 0; i < sphereCount; i++) {
                GameObject sphere = Instantiate(_spherePrefab);
                sphere.name += " " + i;
                sphere.transform.localScale = new Vector3(radius, radius, radius);
                sphere.transform.position = new Vector3(
                    Random.Range(worldBoundary.min.x + PADDING, worldBoundary.max.x - PADDING),
                    Random.Range(worldBoundary.min.y + PADDING, worldBoundary.max.y - PADDING),
                    Random.Range(worldBoundary.min.z + PADDING, worldBoundary.max.z - PADDING)
                );

                sphere.transform.rotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0, 360f), 0f);
                Rigidbody rb = sphere.GetComponent<Rigidbody>();
                rb.AddForce(sphere.transform.forward * Random.Range(180f, 600f));

                allSpheres.Add(sphere);
            }
            return allSpheres;
        }

        void _DrawBoundary(Boundary boundary, Color color) {
            Vector3 vMinToMax = boundary.max - boundary.min;
            float distanceMinToMax = Vector3.Magnitude(vMinToMax);

            Gizmos.color = color;
            Gizmos.DrawWireCube(
                boundary.min + (vMinToMax.normalized * distanceMinToMax * 0.5f), 
                boundary.max - boundary.min
            );
        }
    }
}
