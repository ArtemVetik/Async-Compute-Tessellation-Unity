using System;
using UnityEngine;

namespace AV.AsyncComputeTessellation
{
    public class FreeCamera : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 50.0f;
        [SerializeField] private float _moveLerp = 10.0f;
        [SerializeField] private float _rotateSpeed = 1.0f;
        [SerializeField] private float _rotateLerp = 10.0f;

        private Vector3 _targetPosition;
        private Quaternion _targetRotation;

        private void Start()
        {
            _targetPosition = transform.position;
            _targetRotation = transform.rotation;
        }

        private void Update()
        {
            float moveMult = Input.GetKey(KeyCode.LeftShift) ? 2.0f : 1.0f;

            var vertical = Input.GetAxis("Vertical") * _moveSpeed * moveMult * Time.deltaTime;
            var horizontal = Input.GetAxis("Horizontal") * _moveSpeed * moveMult * Time.deltaTime;
            var upDown = Input.GetKey(KeyCode.Q) ? -1.0f : Input.GetKey(KeyCode.E) ? 1.0f : 0.0f;
            upDown *= _moveSpeed * moveMult * Time.deltaTime;

            _targetPosition = transform.position + transform.forward * vertical + transform.right * horizontal +
                              Vector3.up * upDown;
            if (Input.GetMouseButton(1))
            {
                var eulerAngles = transform.eulerAngles;
                eulerAngles.x -= Input.mousePositionDelta.y * _rotateSpeed;
                eulerAngles.y += Input.mousePositionDelta.x * _rotateSpeed;
                eulerAngles.z = 0;

                _targetRotation = Quaternion.Euler(eulerAngles);
            }

            transform.position = Vector3.Lerp(transform.position, _targetPosition, _moveLerp * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, _rotateLerp * Time.deltaTime);
        }
    }
}