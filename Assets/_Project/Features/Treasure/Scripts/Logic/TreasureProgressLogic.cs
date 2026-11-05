using System;
using System.Collections.Generic;

namespace Project.Treasure
{
    public sealed class TreasureProgressLogic
    {
        private readonly HashSet<int> _collected = new HashSet<int>();
        public int RequiredKeys { get; }
        public int CollectedKeys => _collected.Count;
        public bool CanOpen => CollectedKeys == RequiredKeys && !IsOpened;
        public bool IsOpened { get; private set; }

        public TreasureProgressLogic(int requiredKeys)
        {
            if (requiredKeys < 1) throw new ArgumentOutOfRangeException(nameof(requiredKeys));
            RequiredKeys = requiredKeys;
        }

        public bool TryCollect(int keyId)
        {
            if (keyId < 0 || keyId >= RequiredKeys) throw new ArgumentOutOfRangeException(nameof(keyId));
            return _collected.Add(keyId);
        }

        public bool TryOpen()
        {
            if (!CanOpen) return false;
            IsOpened = true;
            return true;
        }
    }
}
