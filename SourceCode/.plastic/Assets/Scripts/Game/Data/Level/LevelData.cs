using System.Collections.Generic;
using UnityEngine;

public class LevelData : ScriptableObject
{
    [SerializeField]
    private List<(uint,uint)> _matchPairs = new();
    
    public uint GetMatch(uint itemID)
    {
        foreach (var pair in _matchPairs)
        {
            if (pair.Item1 == itemID)
                return pair.Item2;
        }
        return 0;
    }
}
