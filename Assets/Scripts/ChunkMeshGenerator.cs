using System.Collections;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(VoxelChunk))]
public class ChunkMeshGenerator : MonoBehaviour
{
    private MeshFilter m_MeshFilter;
    private int dictionaryTrianglesCounter = 0;
    private int dictionaryVerticesCounter = 0;

    public Color VoxelColor = Color.green;
    public Dictionary<int, Block> m_BlockTriangleDictionary;
    public Dictionary<int, Block> m_BlockVerticesDictionary;

    private void Awake()
    {
        m_MeshFilter = GetComponent<MeshFilter>();
        VoxelChunk voxelChunk = GetComponent<VoxelChunk>();
        voxelChunk.BlockUpdate.AddListener(() => BlockUpdate(voxelChunk));

        m_BlockTriangleDictionary = new Dictionary<int, Block>();
        m_BlockVerticesDictionary = new Dictionary<int, Block>();

        Material material = GetComponent<MeshRenderer>().material;
        material.color = VoxelColor;
        Generate();
    }

    public void BlockUpdate(VoxelChunk chunk)
    {
        Mesh generatedMesh = GenerateMesh(chunk);
        if (!m_MeshFilter)
            m_MeshFilter = GetComponent<MeshFilter>();

        m_MeshFilter.mesh = generatedMesh;

        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = m_MeshFilter.mesh;
    }

    private void Generate()
    {
        VoxelChunk chunk = GetComponent<VoxelChunk>();
        Block[,,] blocks = new Block[chunk.Dimension, chunk.Dimension, chunk.Dimension];

        for (int z = 0; z < chunk.Dimension; z++)
            for (int y = 0; y < chunk.Dimension; y++)
                for (int x = 0; x < chunk.Dimension; x++)
                {
                    Vector3 position = new Vector3(x * chunk.UnitLength, y * chunk.UnitLength, z * chunk.UnitLength);
                    Block currentBlock = new Block(position, VoxelColor, x, y, z);
                    blocks[x, y, z] = currentBlock;
                }

        chunk.FillBlocks(blocks);
    }

    private Mesh GenerateMesh(VoxelChunk chunk)
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;    //support for bigger meshes

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int z = 0; z < chunk.Dimension; z++)
            for (int y = 0; y < chunk.Dimension; y++)
                for (int x = 0; x < chunk.Dimension; x++)
                {
                    Block block = chunk.GetBlock(x, y, z);
                    float halfLength = chunk.UnitLength / 2;
                    Vector3[] blockSides = new Vector3[]
                    {
                            new Vector3(block.position.x, block.position.y, block.position.z),  //bottom side
                            new Vector3(block.position.x, block.position.y + chunk.UnitLength, block.position.z), //top side
                            new Vector3(block.position.x + halfLength, block.position.y + halfLength, block.position.z), //left side
                            new Vector3(block.position.x - halfLength, block.position.y + halfLength, block.position.z), //right side
                            new Vector3(block.position.x, block.position.y + halfLength, block.position.z + halfLength), //front side
                            new Vector3(block.position.x, block.position.y + halfLength, block.position.z - halfLength) //back side
                    };

                    GenerateSides(chunk, block, blockSides, vertices, triangles);
                }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        return mesh;
    }

    private void GenerateSides(VoxelChunk chunk, Block block, Vector3[] blockSides, List<Vector3> vertices, List<int> triangles)
    {
        List<Vector3> currentVertices = new List<Vector3>();

        foreach (Vector3 sideCenterPoint in blockSides)
        {
            List<int> currentTriangles = new List<int>();

            int currentIndex = vertices.Count;
            int[] requiredVertexIndices = new int[4];
            float halfUnit = chunk.UnitLength / 2;
            Vector3 sideUp;
            Vector3 sideLeft;

            if (sideCenterPoint.y != block.position.y + halfUnit)
            {
                //top or bottom
                sideUp = new Vector3(0, 0, halfUnit);
                sideLeft = new Vector3(halfUnit, 0, 0);
            }
            else if (sideCenterPoint.x != block.position.x)
            {
                //left or right
                sideUp = new Vector3(0, halfUnit, 0);
                sideLeft = new Vector3(0, 0, -halfUnit);
            }
            else
            {
                //front or back
                sideUp = new Vector3(0, halfUnit, 0);
                sideLeft = new Vector3(-halfUnit, 0, 0);
            }

            Vector3[] requiredVertices = new Vector3[]
            {
                sideCenterPoint + sideUp + sideLeft, //top left
                sideCenterPoint + sideUp - sideLeft, //top right
                sideCenterPoint - sideUp + sideLeft, //bottom left
                sideCenterPoint - sideUp - sideLeft,  //bottom right
            };

            for (int i = 0; i < requiredVertexIndices.Length; i++)
            {
                int foundIndexOfVertex = currentVertices.FindIndex(obj => obj.x == requiredVertices[i].x &&
                    obj.y == requiredVertices[i].y && obj.z == requiredVertices[i].z);

                if (foundIndexOfVertex < 0)    //required vertex isn't already in the list
                {
                    currentVertices.Add(requiredVertices[i]);
                    requiredVertexIndices[i] = currentIndex + (currentVertices.Count - 1);    //Index depends on how many we've already set/found, also index starts at zero
                    dictionaryVerticesCounter++;
                }
                else   //the vertex that we're searching for is already in the list
                    requiredVertexIndices[i] = currentIndex + foundIndexOfVertex;
            }

            currentTriangles.Add(requiredVertexIndices[0]);
            currentTriangles.Add(requiredVertexIndices[1]);
            currentTriangles.Add(requiredVertexIndices[2]);
            currentTriangles.Add(requiredVertexIndices[1]);
            currentTriangles.Add(requiredVertexIndices[3]);
            currentTriangles.Add(requiredVertexIndices[2]);

            Vector3 centerPoint = new Vector3(block.position.x, block.position.y + halfUnit, block.position.z);
            Vector3 sideDirection = centerPoint - sideCenterPoint;
            if (sideDirection.x > 0 || sideDirection.y < 0 || sideDirection.z < 0)  //reverse triangle if they face the wrong direction
                currentTriangles.Reverse();

            triangles.AddRange(currentTriangles);
            dictionaryTrianglesCounter += 6;          

            for (int i = dictionaryTrianglesCounter - 6; i < dictionaryTrianglesCounter; i++)
                m_BlockTriangleDictionary.Add(i, block);
        }

        for (int i = dictionaryVerticesCounter - currentVertices.Count; i < dictionaryVerticesCounter; i++)
            m_BlockVerticesDictionary.Add(i, block);

        vertices.AddRange(currentVertices);
    }
}
