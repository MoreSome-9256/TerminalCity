using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum ItemTraits
{
    None = 0,
    Name = 1 << 0, // 0001
    Time = 1 << 1, // 0010
    Place = 1 << 2, // 0100
    // ... 可以添加更多特性
}
