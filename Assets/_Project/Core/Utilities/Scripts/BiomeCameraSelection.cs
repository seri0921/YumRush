using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

// Keeps the selected camera inside the player's biome and gives it exclusive priority.
internal sealed class BiomeCameraSelection
{
    private const int SelectedPriority = 20;
    private readonly Dictionary<CinemachineCamera, PolygonCollider2D> _cameraBounds =
        new Dictionary<CinemachineCamera, PolygonCollider2D>();
    private readonly List<Vector2> _pathPoints = new List<Vector2>();

    public BiomeCameraSelection(IEnumerable<CinemachineCamera> cameras)
    {
        foreach (CinemachineCamera camera in cameras)
        {
            if (camera == null || !camera.TryGetComponent(out CinemachineConfiner2D confiner) ||
                !(confiner.BoundingShape2D is PolygonCollider2D bounds))
                throw new InvalidOperationException("Each biome camera requires a Confiner2D with polygon biome bounds.");

            _cameraBounds.Add(camera, bounds);
        }
    }

    public void Select(CinemachineCamera target, Vector2 playerPosition, float margin, CinemachineBlendDefinition blend)
    {
        if (!target.isActiveAndEnabled)
            return;

        bool alreadySelected = true;
        foreach (CinemachineCamera camera in _cameraBounds.Keys)
        {
            int priority = camera == target ? SelectedPriority : 0;
            if (camera.Priority != priority)
                alreadySelected = false;
        }
        // At a junction, a border's side alone cannot distinguish all neighbouring biomes.
        if (alreadySelected || !ContainsWithMargin(_cameraBounds[target], playerPosition, margin))
            return;

        if (Camera.main != null && Camera.main.TryGetComponent(out CinemachineBrain brain))
            brain.DefaultBlend = blend;

        foreach (CinemachineCamera camera in _cameraBounds.Keys)
            camera.Priority = camera == target ? SelectedPriority : 0;
    }

    private bool ContainsWithMargin(PolygonCollider2D bounds, Vector2 position, float margin)
    {
        if (!bounds.OverlapPoint(position))
            return false;

        // Apply the same deadband to every edge, so another border cannot bypass it.
        for (int pathIndex = 0; pathIndex < bounds.pathCount; pathIndex++)
        {
            bounds.GetPath(pathIndex, _pathPoints);
            for (int index = 0; index < _pathPoints.Count; index++)
            {
                Vector2 start = bounds.transform.TransformPoint(_pathPoints[index] + bounds.offset);
                Vector2 end = bounds.transform.TransformPoint(_pathPoints[(index + 1) % _pathPoints.Count] + bounds.offset);
                Vector2 edge = end - start;
                float t = edge.sqrMagnitude > 0f
                    ? Mathf.Clamp01(Vector2.Dot(position - start, edge) / edge.sqrMagnitude)
                    : 0f;
                if ((position - (start + edge * t)).sqrMagnitude < margin * margin)
                    return false;
            }
        }
        return true;
    }
}
