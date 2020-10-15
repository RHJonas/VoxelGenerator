using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using System;

public class VoxelChunk : MonoBehaviour
{
    public int Dimension = 5;          //how long in units the chunk is(unit length of 5 means 5 * 5 * 5 voxels)
    public float UnitLength = 1.0f;
    [HideInInspector]
    public UnityEvent BlockUpdate;

    private Block[,,] m_ChunkData;

    private void Awake()
    {
        m_ChunkData = new Block[Dimension, Dimension, Dimension];
    }

    public Block GetBlock(int x, int y, int z)
    {
        return m_ChunkData[x, y, z];
    }

    public void FillBlocks(Block[,,] blockData)
    {
        m_ChunkData = blockData;
        BlockUpdate.Invoke();
    }
}
