using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Neo {

    [DisallowMultipleComponent]
    public sealed class WorldInitializer : MonoBehaviour {
        [Header("--- Settings ---")]
        [SerializeField] uint _sphereCount = 100;

        [Header("--- Dependencies ---")]
        [SerializeField] GameObject _spherePrefab;

        Boundary m_worldBoundary;
        List<GameObject> m_allSpheres;
        Octree m_rootNode;

        List<Color> m_debugColors = new();

        void Awake() {
            Assert.IsNotNull(_spherePrefab);
            GetComponent<MeshRenderer>().enabled = false;
        }

        void Start() {
            for (int i = 0; i < 30; i++) {
                m_debugColors.Add(new Color(
                    Random.Range(0f, 1f),
                    Random.Range(0f, 1f),
                    Random.Range(0f, 1f)
                )); 
            }

            m_worldBoundary = _InitializeBoundingBoxWalls(0.2f);
            m_allSpheres = _InitializeSpheres((int)_sphereCount, Random.Range(0.2f, 0.8f), m_worldBoundary);
        }

        void Update() {
            //m_rootNode = new Octree(m_worldBoundary, m_allSpheres, 0);
        }

        void OnDrawGizmos() {
            if (!Application.isPlaying) { return; }

            //List<Boundary> leafNodeBoundaries = new();
            //Octree.GetLeafNodeBoundaries(m_rootNode, ref leafNodeBoundaries);
            //
            //int colorIdx = 0;
            //leafNodeBoundaries.ForEach(boundary => {
            //    _DrawBoundary(boundary, m_debugColors[++colorIdx]);
            //});
            //_DrawBoundary(m_worldBoundary, Color.white);
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
                sphere.transform.localScale = new Vector3(radius, radius, radius);
                sphere.transform.position = new Vector3(
                    Random.Range(worldBoundary.min.x + PADDING, worldBoundary.max.x - PADDING),
                    Random.Range(worldBoundary.min.y + PADDING, worldBoundary.max.y - PADDING),
                    Random.Range(worldBoundary.min.z + PADDING, worldBoundary.max.z - PADDING)
                );

                sphere.transform.rotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0, 360f), 0f);
                Rigidbody rb = sphere.GetComponent<Rigidbody>();
                rb.AddForce(sphere.transform.forward * Random.Range(20f, 100f));

                allSpheres.Add(sphere);
            }
            return allSpheres;
        }

        void _DrawBoundary(Boundary boundary, Color color) {
            Vector3 vMinToMax = boundary.max - boundary.min;
            float distanceMinToMax = Vector3.Magnitude(vMinToMax);

            Gizmos.color = color;
            Gizmos.DrawWireCube(boundary.min + vMinToMax.normalized * distanceMinToMax * 0.5f, transform.localScale);
        }
    }
}
