using UnityEngine;

namespace Project.Treasure
{
    [RequireComponent(typeof(Collider2D), typeof(TreasureChestView))]
    public sealed class TreasureChestPresenter : MonoBehaviour, IEnemy
    {
        private TreasureSessionPresenter _session;
        private TreasureChestView _view;
        // The attack interface also uses this property to exclude locked/opened targets.
        public bool IsDefeated => _session == null || !_session.CanOpen;

        public void Initialize(TreasureSessionPresenter session)
        {
            _session = session;
            _view = GetComponent<TreasureChestView>();
            Refresh();
        }

        public void Refresh() => _view.Show(_session.CanOpen, _session.IsOpened);
        public void ReceiveAttack() => _session.OpenChest();
    }
}
