using UnityEngine;

namespace CozyYard
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float _panSpeed = 8f;
        [SerializeField] private float _zoomSpeed = 3f;
        [SerializeField] private float _minOrtho = 3f;
        [SerializeField] private float _maxOrtho = 14f;

        private Camera _cam;
        private float _minX, _maxX, _minY, _maxY;

        public void Initialize(int gridWidth, int gridHeight)
        {
            _cam = GetComponent<Camera>();
            if (_cam == null) _cam = Camera.main;

            _minX = -2f;
            _maxX = gridWidth * GridUtils.TileSize + 2f;
            _minY = -gridHeight * GridUtils.TileSize - 2f;
            _maxY = 2f;

            float cx = gridWidth * GridUtils.TileSize * 0.5f;
            float cy = -gridHeight * GridUtils.TileSize * 0.5f;
            transform.position = new Vector3(cx, cy, transform.position.z);

            if (_cam != null) _cam.orthographicSize = 8f;
        }

        private void Update()
        {
            if (_cam == null) return;

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            if (h != 0 || v != 0)
            {
                var delta = new Vector3(h, v, 0) * (_panSpeed * Time.deltaTime);
                transform.position += delta;
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                _cam.orthographicSize = Mathf.Clamp(
                    _cam.orthographicSize - scroll * _zoomSpeed,
                    _minOrtho, _maxOrtho);
            }

            ClampPosition();
        }

        private void ClampPosition()
        {
            var pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, _minX, _maxX);
            pos.y = Mathf.Clamp(pos.y, _minY, _maxY);
            transform.position = pos;
        }
    }
}
