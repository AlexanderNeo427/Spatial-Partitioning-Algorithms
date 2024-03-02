using System;
using UnityEngine;
using UnityEngine.Assertions;

/**
 * So how you are going to want to use thi is:
 * - Have an empty gameObject (the _pivot reference), which is what the camera will look at
 * - Let that pivot have a child gameObject (this will be the follow target)
 * - The below code will rotate the pivot, and thus the 'follow target' (the child of the pivot) will rotate around pivot
 * - You can then lerp the camera towards that 'follow target'
 * - Place this script on the camera
 */
namespace Neo {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class CameraController : MonoBehaviour {
        [Header("---- Settings ----")]
        [SerializeField][Range(0.1f, 2f)] float _idleRotationSpeed = 0.8f;
        [SerializeField][Range(10f, 50f)] float _rotationSpeedMultiplier = 18f;
        [SerializeField][Range(10f, 50f)] float _scrollSpeed = 20f;

        [Header("--- Dependenceies ---")]
        [SerializeField] GameObject _pivot;
        [SerializeField] GameObject _followTarget;

        Vector3 m_pivotRotation;
        Camera  m_camera;

        void Awake() {
            Assert.IsNotNull(_pivot);
            Assert.IsNotNull(_followTarget);

            m_camera = GetComponent<Camera>();
            m_camera.transform.rotation = Quaternion.identity;
        }

        void Update() {
            Vector3 dirTargetToPivot = (_pivot.transform.position - _followTarget.transform.position).normalized;
            _followTarget.transform.position += dirTargetToPivot * Input.GetAxis("Mouse ScrollWheel") * _scrollSpeed;
            const float MIN_DISTANCE = 3f;
            float distanceFromPivot = Vector3.Distance(_followTarget.transform.position, _pivot.transform.position);
            if (distanceFromPivot < MIN_DISTANCE) {
                _followTarget.transform.position -= dirTargetToPivot * (MIN_DISTANCE - distanceFromPivot);
            }
            m_camera.transform.position = Vector3.Lerp(
                m_camera.transform.position,
                _followTarget.transform.position, 0.1f
            );

            if (Input.GetMouseButton(0)) {
                m_pivotRotation.y += Input.GetAxis("Mouse X") * _rotationSpeedMultiplier;
                m_pivotRotation.x -= Input.GetAxis("Mouse Y") * _rotationSpeedMultiplier;
            }
            else {
                m_pivotRotation.y += _idleRotationSpeed * _rotationSpeedMultiplier * Time.deltaTime;
            }
            m_pivotRotation.x = Mathf.Clamp(m_pivotRotation.x, -85f, 85f);
            _pivot.transform.rotation = Quaternion.Lerp(
                _pivot.transform.rotation,
                Quaternion.Euler(m_pivotRotation.x, m_pivotRotation.y, 0f), 0.1f
            );
        }

        void LateUpdate() => m_camera.transform.LookAt(_pivot.transform, new Vector3(0, 1, 0));
    }
}
