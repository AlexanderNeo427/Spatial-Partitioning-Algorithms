using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Neo {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class CameraController : MonoBehaviour {
        [Header("---- Settings ----")]
        [SerializeField][Range(0.1f, 40f)] float _rotationSpeed = 8f;
        [SerializeField][Range(0.1f, 15f)] float _scrollSpeed = 8f;

        GameObject m_anchor;
        Vector2 m_anchorRotation;
        Camera m_camera;

        void Awake() {
            m_camera = GetComponent<Camera>();
            m_camera.transform.rotation = Quaternion.identity;

            Assert.IsNotNull(gameObject.transform.parent);
            m_anchor = gameObject.transform.parent.gameObject;
        }

        void Update() {
            Vector3 dirCamToAnchor = (m_anchor.transform.position - m_camera.transform.position).normalized;
            Vector3 newCamPosition = m_camera.transform.position + dirCamToAnchor * Input.GetAxis("Mouse ScrollWheel") * _scrollSpeed;
            const float MIN_DISTANCE = 3f;
            float distanceFromAnchor = Vector3.Distance(newCamPosition, m_anchor.transform.position);
            if (distanceFromAnchor < MIN_DISTANCE) {
                float offsetAmount = MIN_DISTANCE - distanceFromAnchor;
                newCamPosition -= dirCamToAnchor * offsetAmount;
            }
            m_camera.transform.position = newCamPosition;

            if (Input.GetMouseButton(0)) {
                m_anchorRotation.y += Input.GetAxis("Mouse X") * _rotationSpeed;
                m_anchorRotation.x -= Input.GetAxis("Mouse Y") * _rotationSpeed; 
                m_anchorRotation.x = Mathf.Clamp(m_anchorRotation.x, -85f, 85f);
                m_anchor.transform.rotation = Quaternion.Euler(m_anchorRotation.x, m_anchorRotation.y, 0f);
            }
        }

        void LateUpdate() => m_camera.transform.LookAt(m_anchor.transform, new Vector3(0, 1, 0));
    }
}
