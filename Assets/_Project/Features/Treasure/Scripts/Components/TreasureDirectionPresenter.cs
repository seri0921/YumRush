using System;
using UnityEngine;

namespace Project.Treasure
{
    [DisallowMultipleComponent]
    public sealed class TreasureDirectionPresenter : MonoBehaviour
    {
        [SerializeField] private TreasureSessionPresenter _session;
        [SerializeField] private Transform _player;
        [SerializeField] private Camera _camera;
        [SerializeField] private TreasureDirectionView _view;
        private double[] _squaredDistances = Array.Empty<double>();

        private void Awake()
        {
            if (_session == null || _player == null || _camera == null || _view == null)
                throw new InvalidOperationException("Treasure direction UI requires the session, player, camera and view.");
        }

        private void OnDisable() => _view.Hide();

        private void LateUpdate()
        {
            if (!_session.isActiveAndEnabled || _session.IsOpened || _player == null || !_camera.isActiveAndEnabled)
            {
                _view.Hide();
                return;
            }

            bool showChest = _session.CanOpen;
            Transform target;
            if (showChest)
            {
                target = _session.Chest.transform;
            }
            else
            {
                var keys = _session.Keys;
                if (_squaredDistances.Length != keys.Count) _squaredDistances = new double[keys.Count];
                for (int index = 0; index < keys.Count; index++)
                {
                    var key = keys[index];
                    _squaredDistances[index] = key != null && !key.IsDefeated && key.isActiveAndEnabled
                        ? ((Vector2)(key.transform.position - _player.position)).sqrMagnitude
                        : double.PositiveInfinity;
                }
                int nearest = TreasureDirectionLogic.FindNearest(_squaredDistances);
                if (nearest < 0)
                {
                    _view.Hide();
                    return;
                }
                target = keys[nearest].transform;
            }

            Vector3 viewport = _camera.WorldToViewportPoint(target.position);
            Rect pixels = _camera.pixelRect;
            if (TreasureDirectionLogic.TryGetMarker(viewport.x, viewport.y, viewport.z,
                pixels.width, pixels.height, _view.InsetPixels, out double x, out double y, out double angle))
                _view.Render(new Vector2(pixels.x + (float)x, pixels.y + (float)y), (float)angle, showChest);
            else
                _view.Hide();
        }
    }
}
