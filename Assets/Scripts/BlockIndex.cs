using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BlockIndex
{
    //x y z is the internal position in the three-dimensional array, position is the actual world space position
    public BlockIndex(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public int x;
    public int y;
    public int z;
}