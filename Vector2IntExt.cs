using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2IntExt
{
    /// <summary>
    /// A list of Vector2Ints making a circle around (0, 0)
    /// </summary>
    public static List<Vector2Int> Around = new List<Vector2Int> {
         new Vector2Int(-1, -1),
         new Vector2Int(-1, 0),
         new Vector2Int(-1, 1),
         new Vector2Int(0, -1),
         new Vector2Int(0, 1),
         new Vector2Int(1, -1),
         new Vector2Int(1, 0),
         new Vector2Int(1, 1),
    };

    /// <summary>
    /// A list of Vector2Ints making a circle around and including (0, 0)
    /// </summary>
    public static List<Vector2Int> AroundAndCentre = new List<Vector2Int> {
         new Vector2Int(-1, -1),
         new Vector2Int(-1, 0),
         new Vector2Int(-1, 1),
         new Vector2Int(0, -1),
         new Vector2Int(0, 1),
         new Vector2Int(1, -1),
         new Vector2Int(1, 0),
         new Vector2Int(1, 1),
         new Vector2Int(0, 0)
    };
}
