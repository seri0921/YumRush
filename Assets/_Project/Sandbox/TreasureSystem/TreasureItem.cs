using UnityEngine;
using System.Collections.Generic;

public class TreasureItem : MonoBehaviour
{
    private readonly List<GameObject> _registeredKeys = new List<GameObject>();
    public void RegisterKey(GameObject key)
    {
        if (key != null && !_registeredKeys.Contains(key))
        {
            _registeredKeys.Add(key);
        }
    }

    public void OnKeyDestroyed(GameObject key)
    {
        _registeredKeys.Remove(key);

        if (_registeredKeys.Count == 0)
        {
            BreakBarrier();
        }
    }

    private void BreakBarrier()
    {
        Destroy(gameObject);
    }
}
