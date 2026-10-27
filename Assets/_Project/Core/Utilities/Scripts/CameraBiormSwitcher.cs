using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Unity.Cinemachine;

// Observes the player's side of a biome boundary without changing player movement.
public class CameraBiormSwitcher : MonoBehaviour
{
    [Header("バイオームごとのカメラ")]
    [SerializeField] private CinemachineCamera roomACamera;
    [SerializeField] private CinemachineCamera roomBCamera;

    [Header("境界の判定設定")]
    [FormerlySerializedAs("pushDirectionToB")]
    [Tooltip("境界からバイオームBへ向かうワールド方向")]
    [SerializeField] private Vector2 directionToB = Vector2.up;
    [Tooltip("境界付近でカメラが頻繁に切り替わるのを防ぐ距離")]
    [SerializeField, Min(0.01f)] private float switchMargin = 0.1f;

    [Header("カメラの切り替え設定")]
    [SerializeField] private CinemachineBlendDefinition.Styles blendStyle = CinemachineBlendDefinition.Styles.EaseInOut;
    [SerializeField, Min(0f)] private float transitionDuration = 0.8f;

    private Collider2D _boundary;
    private PlayerPresenter _player;
    private BiomeCameraSelection _cameraSelection;

    private void Awake()
    {
        _boundary = GetComponent<Collider2D>();
        if (_boundary == null || !_boundary.isTrigger || roomACamera == null || roomBCamera == null ||
            directionToB.sqrMagnitude == 0f || switchMargin <= 0f)
            throw new InvalidOperationException($"{name}: A boundary trigger, two cameras, direction and positive switch margin are required.");

        // All borders under the same parent share one set of biome cameras.
        Transform borderGroup = transform.parent != null ? transform.parent : transform;
        var cameras = new HashSet<CinemachineCamera>();
        foreach (CameraBiormSwitcher border in borderGroup.GetComponentsInChildren<CameraBiormSwitcher>(true))
        {
            cameras.Add(border.roomACamera);
            cameras.Add(border.roomBCamera);
        }
        _cameraSelection = new BiomeCameraSelection(cameras);
    }

    private void LateUpdate()
    {
        // Respawning can replace the player without entering a boundary trigger.
        if (_player == null)
            _player = FindFirstObjectByType<PlayerPresenter>();
        if (_player == null || !_player.isActiveAndEnabled)
            return;

        // Interpolated transforms lag behind the physics position at fast crossings.
        Vector2 position = _player.GetComponent<Rigidbody2D>().position;
        float signedDistance = Vector2.Dot(position - (Vector2)_boundary.bounds.center, directionToB.normalized);
        if (Mathf.Abs(signedDistance) < switchMargin)
            return;

        CinemachineCamera targetCamera = signedDistance > 0f ? roomBCamera : roomACamera;
        _cameraSelection.Select(targetCamera, position, switchMargin,
            new CinemachineBlendDefinition(blendStyle, transitionDuration));
    }
}
