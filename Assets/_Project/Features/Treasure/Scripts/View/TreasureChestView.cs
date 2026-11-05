using UnityEngine;

namespace Project.Treasure
{
    public sealed class TreasureChestView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;

        public void Show(bool unlocked, bool opened)
        {
            _renderer.enabled = !opened;
            _renderer.color = unlocked ? Color.white : new Color(0.55f, 0.55f, 0.55f);
        }
    }
}
