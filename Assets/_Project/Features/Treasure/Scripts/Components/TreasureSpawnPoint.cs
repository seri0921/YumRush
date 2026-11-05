using UnityEngine;

namespace Project.Treasure
{
    [DisallowMultipleComponent]
    public sealed class TreasureSpawnPoint : MonoBehaviour
    {
        [SerializeField, Range(0, 3)] private int _biomeIndex;
        [SerializeField, Min(0)] private float _keyWeight = 1;
        [SerializeField] private bool _canSpawnChest;
        public int BiomeIndex => _biomeIndex;
        public float KeyWeight => _keyWeight;
        public bool CanSpawnChest => _canSpawnChest;

        private void OnDrawGizmos()
        {
            Gizmos.color = _canSpawnChest ? Color.yellow : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.6f);
        }
    }
}
