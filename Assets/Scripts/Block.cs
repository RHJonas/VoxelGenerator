using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Block
{
    //x y z is the internal position in the three-dimensional array, position is the actual world space position
    public Vector3 position;
    public Color color;
    public int x;
    public int y;
    public int z;

    public Block(Vector3 position, Color color, int x, int y, int z)
    {
        this.position = position;
        this.color = color;
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static bool operator == (Block first, Block second)
    {
        if (first.x == second.x && first.y == second.y && first.z == second.z &&
            first.color == second.color && first.position == second.position)
            return true;

        return false;
    }

    public static bool operator != (Block first, Block second)
    {
        if (first.x == second.x && first.y == second.y && first.z == second.z &&
            first.color == second.color && first.position == second.position)
            return false;

        return true;
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (GetType() != obj.GetType())
            return false;

        Block block = (Block)obj;

        if (x == block.x && y == block.y && z == block.z &&
            position == block.position && color == block.color)
            return true;

        return false;
    }
    public override int GetHashCode()
    {
        //function taken from: https://www.loganfranken.com/blog/692/overriding-equals-in-c-part-2/

        unchecked
        {
            // Choose large primes to avoid hashing collisions
            const int HashingBase = (int)2166136261;
            const int HashingMultiplier = 16777619;

            int hash = HashingBase;
            hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, x) ? x.GetHashCode() : 0);
            hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, y) ? y.GetHashCode() : 0);
            hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, z) ? z.GetHashCode() : 0);
            hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, position) ? position.GetHashCode() : 0);
            hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, color) ? color.GetHashCode() : 0);
            return hash;
        }
    }
}
