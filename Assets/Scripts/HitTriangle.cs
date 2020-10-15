using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using System;

[RequireComponent(typeof(VoxelChunk))]
public class HitTriangle : MonoBehaviour
{
    private Camera currentCamera;
    private ChunkMeshGenerator m_ChunkMeshGenerator;
    private VoxelChunk m_VoxelChunk;

    private void Awake()
    {
        currentCamera = Camera.main;
        m_ChunkMeshGenerator = GetComponent<ChunkMeshGenerator>();
        m_VoxelChunk = GetComponent<VoxelChunk>();
    }

    private void Update()
    {
        //Source for triangle highlight: https://docs.unity3d.com/ScriptReference/RaycastHit-triangleIndex.html

        RaycastHit hit;
        if (!Physics.Raycast(currentCamera.ScreenPointToRay(Input.mousePosition), out hit))
            return;

        MeshCollider meshCollider = hit.collider as MeshCollider;
        if (meshCollider == null || meshCollider.sharedMesh == null)
            return;

        Mesh mesh = meshCollider.sharedMesh;
        List<Vector3> vertices = mesh.vertices.ToList();
        List<int> triangles = mesh.triangles.ToList();

        DrawDebugLines(vertices, triangles, hit);

        if (Input.GetMouseButtonDown(0))    //left-clicking to remove a voxel
        {
            Block block;
            m_ChunkMeshGenerator.m_BlockTriangleDictionary.TryGetValue(hit.triangleIndex * 3, out block);


            meshCollider.sharedMesh = GetAlteredMesh(mesh, block, vertices, triangles);
        }
    }

    private void DrawDebugLines(List<Vector3> vertices, List<int> triangles, RaycastHit hit)
    {
        Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
        Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
        Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];

        int id = triangles[hit.triangleIndex * 3] % 12;
        int startingPoint = triangles[hit.triangleIndex * 3] - id + 1;

        Transform hitTransform = hit.collider.transform;
        p0 = hitTransform.TransformPoint(p0);
        p1 = hitTransform.TransformPoint(p1);
        p2 = hitTransform.TransformPoint(p2);
        Debug.DrawLine(p0, p1);
        Debug.DrawLine(p1, p2);
        Debug.DrawLine(p2, p0);
    }

    private Mesh GetAlteredMesh(Mesh mesh, Block block, List<Vector3> vertices, List<int> triangles)
    {
        //36 = number of triangle indices per voxel
        int voxelVertexCount = 8;

        List<int> blockTriangleIndices = new List<int>();
        List<int> blockVertices = new List<int>();

        foreach (KeyValuePair<int, Block> item in m_ChunkMeshGenerator.m_BlockTriangleDictionary)
        {
            var blockToCompare = item.Value;

            if (blockToCompare == block)
                blockTriangleIndices.Add(item.Key);
        }

        foreach (KeyValuePair<int, Block> item in m_ChunkMeshGenerator.m_BlockVerticesDictionary)
        {
            var blockToCompare = item.Value;

            if (blockToCompare == block)
                blockVertices.Add(item.Key);
        }

        triangles.RemoveRange(blockTriangleIndices[0], blockTriangleIndices.Count);
        vertices.RemoveRange(blockVertices[0], blockVertices.Count);

        if (triangles.Count > 0)
        {
            if (triangles[triangles.Count - 1] >= vertices.Count)   //re-assign vertices if we didn't remove from the back
            {
                for (int i = blockTriangleIndices[0]; i < triangles.Count; i++)
                    triangles[i] = triangles[i] - voxelVertexCount;
            }
        }

        mesh.triangles = triangles.ToArray();
        mesh.vertices = vertices.ToArray();
        return mesh;
    }
}
