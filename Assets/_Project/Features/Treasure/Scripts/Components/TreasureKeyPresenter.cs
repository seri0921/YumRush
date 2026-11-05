using UnityEngine;

namespace Project.Treasure
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class TreasureKeyPresenter : MonoBehaviour, IEnemy
    {
        private TreasureSessionPresenter _session;
        private int _keyId;
        public bool IsDefeated { get; private set; }

        public void Initialize(TreasureSessionPresenter session, int keyId)
        {
            _session = session;
            _keyId = keyId;
        }

        public void ReceiveAttack()
        {
            if (IsDefeated || !_session.CollectKey(_keyId)) return;
            IsDefeated = true;
            if (TryGetComponent<ItemCollectEffectView>(out var effectView))
                effectView.Play(transform.position);
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
