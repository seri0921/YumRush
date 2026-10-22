using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Unity.Cinemachine;
public class CameraBiormSwitcher : MonoBehaviour
{
    [Header("�o�C�I�[���Ή��J����")]
    [SerializeField] private CinemachineCamera roomACamera;
    [SerializeField] private CinemachineCamera roomBCamera;

    [Header("���o�ݒ�A��B")]
    [SerializeField] private Vector2 pushDirectionToB = Vector2.up;
    [SerializeField] private float pushDistance = 1.5f;

    [Header("�J�������p���ݒ�")]
    [Tooltip("���[����")]
    [SerializeField] private CinemachineBlendDefinition.Styles blendStyle = CinemachineBlendDefinition.Styles.EaseInOut;
    [Tooltip("���o�b")]
    [SerializeField] private float transitionDuration = 0.8f;

    private bool isTransitioning = false;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTransitioning)
            return;

        PlayerPresenter player = other.GetComponentInParent<PlayerPresenter>();
        // Non-player colliders can also enter this trigger.
        if (player == null)
            return;

        TransitionAsync(player, destroyCancellationToken).Forget();
    }

    private async UniTask TransitionAsync(PlayerPresenter player, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        using var transitionCts = CancellationTokenSource.CreateLinkedTokenSource(ct, player.destroyCancellationToken);
        ct = transitionCts.Token;
        ct.ThrowIfCancellationRequested();
        IDisposable movement = player.BeginScriptedMovement();
        isTransitioning = true;

        try
        {
            Transform playerTransform = player.transform;
            // 2. CinemachineBrain �̃u�����h�ݒ�
            if (Camera.main != null)
            {
                CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
                if (brain != null)
                {
                    brain.DefaultBlend = new CinemachineBlendDefinition(blendStyle, transitionDuration);
                }
            }

            // 3. �J�����D��x�؂�ւ�
            bool isFromAToB = roomACamera.Priority >= roomBCamera.Priority;
            CinemachineCamera targetCamera = isFromAToB ? roomBCamera : roomACamera;
            CinemachineCamera currentCamera = isFromAToB ? roomACamera : roomBCamera;

            Vector3 moveDirection = (Vector3)(isFromAToB ? pushDirectionToB.normalized : -pushDirectionToB.normalized);

            targetCamera.Priority = 20;
            currentCamera.Priority = 0;

            // 4. �v���C���[�̈ړ�����
            Vector3 startPos = playerTransform.position;
            Vector3 targetPos = startPos + (moveDirection * pushDistance);

            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / transitionDuration;

                if (!player.isActiveAndEnabled)
                    throw new OperationCanceledException("Player was disabled during scripted movement.", ct);

                player.SetScriptedPosition(Vector3.Lerp(startPos, targetPos, t));
                await UniTask.NextFrame(PlayerLoopTiming.Update, ct);
            }

            if (!player.isActiveAndEnabled)
                throw new OperationCanceledException("Player was disabled during scripted movement.", ct);

            player.SetScriptedPosition(targetPos);

        }
        finally
        {
            movement.Dispose();
            isTransitioning = false;
        }
    }
}
