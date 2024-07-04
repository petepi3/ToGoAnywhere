using System;
using Petepi.TGA.Grid;
using UnityEngine;

namespace Petepi.TGA.Gameplay
{
    public class CameraController : MonoBehaviour
    {
        public Camera playerCamera;
        public float acceleration = 6;
        public float speed = 20;
        public float zoomSpeedMultiplier = 2;
        public float zoomSpeed = 2;
        public float minCameraDistance = 5;
        public float maxCameraDistance = 40;
        private float _cameraDistance = 40;
        private Vector2 _velocity;
        private Vector2 _input;
        private GridSystem _grid;

        private void Awake()
        {
            _grid = FindAnyObjectByType<GridSystem>();
        }

        private void Update() 
        {
            // decided to use the old input system, this is practically all input in this project
            // so there would be very little benefit from using input system
            // todo: consider moving to input system or allow some different kind of way to rebind inputs.
            _input = Vector2.zero;

            if (Input.GetKey(KeyCode.W))
            {
                _input += Vector2.up;
            }
            if (Input.GetKey(KeyCode.A))
            {
                _input += Vector2.left;
            }
            if (Input.GetKey(KeyCode.S))
            {
                _input += Vector2.down;
            }
            if (Input.GetKey(KeyCode.D))
            {
                _input += Vector2.right;
            }

            _cameraDistance += -Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            _cameraDistance = Mathf.Clamp(_cameraDistance, minCameraDistance, maxCameraDistance);
            playerCamera.transform.localPosition = Vector3.up * _cameraDistance;
            
            _input.Normalize();
            
            _velocity = Vector2.Lerp(_velocity, _input * (speed * (zoomSpeedMultiplier*_cameraDistance)), acceleration*Time.deltaTime);
            var next = transform.position + (new Vector3(_velocity.x, 0, _velocity.y) * Time.deltaTime);
            transform.position =
                new Vector3(
                    Mathf.Clamp(next.x, 0, _grid.numberOfChunks.x * GridSystem.ChunkSize),
                    next.y,
                    Mathf.Clamp(next.z, 0, _grid.numberOfChunks.y * GridSystem.ChunkSize * GridSystem.TileWidth));
        }
    }
}