using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class StartCameraController : MonoBehaviour
{
    [Header("カメラ設定")]
    [SerializeField] private CinemachineCamera startCamera;
    [SerializeField] private int activePriority = 100;

    [Header("ブレンド設定（任意）")]
    [SerializeField] private CinemachineBlendDefinition.Styles transitionStyle = CinemachineBlendDefinition.Styles.EaseInOut;
    [SerializeField, Min(0f)] private float transitionDuration = 1.2f;

    [Header("デバッグ/仮入力（ステート完成後はオフ推奨）")]
    [SerializeField] private bool useTemporaryInputRelease = true;

    public bool IsActive { get; private set; } = true;
    public event Action OnCameraReleased;

    private void Awake()
    {
        if (startCamera == null)
        {
            Debug.LogError($"{name}: StartCamera が設定されていません。");
            enabled = false;
            return;
        }
        startCamera.Priority = activePriority;
        IsActive = true;
    }

    private void Update()
    {
        if (useTemporaryInputRelease && IsActive && HasAnyInput())
        {
            ReleaseStartCamera();
        }
    }

    private bool HasAnyInput()
    {
        // キーボードの任意のキー押下
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            return true;

        return false;
    }

    public void ReleaseStartCamera()
    {
        if (!IsActive) return;

        IsActive = false;

        if (Camera.main != null && Camera.main.TryGetComponent(out CinemachineBrain brain))
        {
            brain.DefaultBlend = new CinemachineBlendDefinition(transitionStyle, transitionDuration);
        }    
        startCamera.gameObject.SetActive(false);

        OnCameraReleased?.Invoke();
    }
}
