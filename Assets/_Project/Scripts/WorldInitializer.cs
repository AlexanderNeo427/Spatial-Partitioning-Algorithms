using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Neo {

    [DisallowMultipleComponent]
    public sealed class WorldInitializer : MonoBehaviour {
        [Header("--- Settings ---")]
        [SerializeField] uint _sphereCount = 100;

        [Header("--- Dependencies ---")]
        [SerializeField] GameObject _spherePrefab;

        Boundary m_worldBoundary;

        void Awake() {
            Assert.IsNotNull(_spherePrefab);
            GetComponent<MeshRenderer>().enabled = false;
        }

        void Start() {
            m_worldBoundary = _InitializeBoundingBoxWalls(0.2f);
            _InitializeSpheres((int)_sphereCount, Random.Range(0.2f, 0.8f), m_worldBoundary);
        }

        void OnDrawGizmos() {
            _DrawBoundary(m_worldBoundary);
        }

        private Boundary _InitializeBoundingBoxWalls(float depth) {
            GameObject front = GameObject.CreatePrimitive(PrimitiveType.Cube);
            front.name = "Front Wall";
            front.transform.position += transform.forward * ((transform.localScale.z * 0.5f) + (depth * 0.5f));
            front.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, depth);
            front.GetComponent<MeshRenderer>().enabled = false;

            GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "Back Wall";
            back.transform.position -= transform.forward * ((transform.localScale.z * 0.5f) + (depth * 0.5f));
            back.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, depth);
            back.GetComponent<MeshRenderer>().enabled = false;

            GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
            right.name = "Right Wall";
            right.transform.position += transform.right * ((transform.localScale.x * 0.5f) + (depth * 0.5f));
            right.transform.localScale = new Vector3(depth, transform.localScale.y, transform.localScale.z);
            right.GetComponent<MeshRenderer>().enabled = false;

            GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
            left.name = "Left Wall";
            left.transform.position -= transform.right * ((transform.localScale.x * 0.5f) + (depth * 0.5f));
            left.transform.localScale = new Vector3(depth, transform.localScale.y, transform.localScale.z);
            left.GetComponent<MeshRenderer>().enabled = false;

            GameObject up = GameObject.CreatePrimitive(PrimitiveType.Cube);
            up.name = "Up Wall";
            up.transform.position += transform.up * ((transform.localScale.y * 0.5f) + (depth * 0.5f));
            up.transform.localScale = new Vector3(transform.localScale.x, depth, transform.localScale.z);
            up.GetComponent<MeshRenderer>().enabled = false;

            GameObject down = GameObject.CreatePrimitive(PrimitiveType.Cube);
            down.name = "Down Wall";
            down.transform.position -= transform.up * ((transform.localScale.y * 0.5f) + (depth * 0.5f));
            down.transform.localScale = new Vector3(transform.localScale.x, depth, transform.localScale.z);
            down.GetComponent<MeshRenderer>().enabled = false;

            return new Boundary {
                min = transform.position - transform.localScale * 0.5f,
                max = transform.position + transform.localScale * 0.5f
            };
        }

        List<GameObject> _InitializeSpheres(int sphereCount, float radius, Boundary worldBoundary) {
            float PADDING = radius + 3f;
            
            List<GameObject> allSpheres = new();
            for (int i = 0; i < sphereCount; i++) {
                GameObject sphere = Instantiate(_spherePrefab);
                sphere.transform.localScale = new Vector3(radius, radius, radius);
                sphere.transform.localPosition = new Vector3(
                    Random.Range(worldBoundary.min.x + PADDING, worldBoundary.max.x - PADDING),
                    Random.Range(worldBoundary.min.x + PADDING, worldBoundary.max.x - PADDING),
                    Random.Range(worldBoundary.min.x + PADDING, worldBoundary.max.x - PADDING)
                );

                sphere.transform.rotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0, 360f), 0f);
                Rigidbody rb = sphere.GetComponent<Rigidbody>();
                rb.AddForce(sphere.transform.forward * Random.Range(50f, 500f));
            }
            return allSpheres;
        }

        void _DrawBoundary(Boundary boundary) {
            Vector3 vMinToMax = boundary.max - boundary.min;
            float distanceMinToMax = Vector3.Magnitude(vMinToMax);
            Gizmos.DrawWireCube(boundary.min + vMinToMax.normalized * distanceMinToMax * 0.5f, transform.localScale);
        }
    }
}
