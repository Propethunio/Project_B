using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour {

    public static GridManager instance { get; private set; }

    [SerializeField] Transform terrainParent;

    [SerializeField] BuildableObjectSO[] treePrefabs;
    [SerializeField] bool showDebug;
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] float cellSize;
    [SerializeField] float mapGenerationNoiseScale;
    [SerializeField] float terrainColorNoiseScale;
    [SerializeField] float treeNoiseScale;
    [SerializeField] float treeDensity;
    [SerializeField] float waterLevel;
    [SerializeField] Color terrainColor1, terrainColor2, terrainColor3;
    [SerializeField] Material terrainMaterial, edgeMaterial;

    Grid3D<GridCell> grid;

    void Awake() {
        if(instance == null) {
            instance = this;
            grid = new Grid3D<GridCell>(width, height, cellSize, showDebug, (int x, int z) => new GridCell(x, z));
        } else {
            Destroy(gameObject);
        }
    }

    void Start() {
        CreateWater();
        ScaleWaterMesh();
        DrawTerrainMesh();
        DrawEdgeMesh();
        CreateTrees();
    }

    void CreateWater() {
        float[,] noiseMap = CreateNoiseMap(mapGenerationNoiseScale);
        float[,] falloffMap = CreateFalloffMap();

        for(int x = 0; x < width; x++) {
            for(int z = 0; z < height; z++) {
                GridCell cell = grid.GetGridCell(x, z);
                cell.isWater = noiseMap[x, z] - falloffMap[x, z] < waterLevel;
                if (!cell.isWater) {
                    cell.isWalkable = true;
                }
            }
        }
    }

    float[,] CreateNoiseMap(float scale) {
        float[,] noiseMap = new float[width, height];
        float xOffset = Random.Range(-10000f, 10000f);
        float zOffset = Random.Range(-10000f, 10000f);
        for(int x = 0; x < width; x++) {
            for(int z = 0; z < height; z++) {
                float noiseValue = Mathf.PerlinNoise(x * scale + xOffset, z * scale + zOffset);
                noiseMap[x, z] = noiseValue;
            }
        }
        return noiseMap;
    }

    float[,] CreateFalloffMap() {
        float[,] falloffMap = new float[width, height];
        for(int x = 0; x < width; x++) {
            for(int z = 0; z < height; z++) {
                float xv = x / (float)width * 2 - 1;
                float zv = z / (float)height * 2 - 1;
                float v = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(zv));
                float a = 3f;
                float b = 4.4f;
                falloffMap[x, z] = Mathf.Pow(v, a) / (Mathf.Pow(v, a) + Mathf.Pow(b - b * v, a));
            }
        }
        return falloffMap;
    }

    void ScaleWaterMesh() {
        Water.instance.ChangeWaterScale(width, height);
    }

    void DrawTerrainMesh() {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        for(int x = 0; x < width; x++) {
            for(int z = 0; z < height; z++) {
                if(!grid.GetGridCell(x, z).isWater) {
                    Vector3 a = new Vector3(x * cellSize, 0, z * cellSize + cellSize);
                    Vector3 b = new Vector3(x * cellSize + cellSize, 0, z * cellSize + cellSize);
                    Vector3 c = new Vector3(x * cellSize, 0, z * cellSize);
                    Vector3 d = new Vector3(x * cellSize + cellSize, 0, z * cellSize);

                    Vector2 uvA = new Vector2(x / (float)width, z / (float)height);
                    Vector2 uvB = new Vector2((x + 1) / (float)width, z / (float)height);
                    Vector2 uvC = new Vector2(x / (float)width, (z + 1) / (float)height);
                    Vector2 uvD = new Vector2((x + 1) / (float)width, (z + 1) / (float)height);

                    Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                    Vector2[] uv = new Vector2[] { uvA, uvB, uvC, uvB, uvD, uvC };

                    for(int i = 0; i < 6; i++) {
                        vertices.Add(v[i]);
                        triangles.Add(triangles.Count);
                        uvs.Add(uv[i]);
                    }
                }
            }
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        GameObject terrainObj = new GameObject("Terrain");
        terrainObj.transform.SetParent(terrainParent);
        MeshFilter meshFilter = terrainObj.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        MeshRenderer meshRenderer = terrainObj.AddComponent<MeshRenderer>();
        DrawTexture(meshRenderer);
    }

    void DrawTexture(MeshRenderer meshRenderer) {
        float[,] noiseMap = CreateNoiseMap(terrainColorNoiseScale);
        Texture2D texture = new Texture2D(width, height);
        Color[] colorMap = new Color[width * height];
        for(int x = 0; x < width; x++) {
            for(int z = 0; z < height; z++) {
                if(grid.GetGridCell(x, z).isWater) {
                    colorMap[z * width + x] = Color.blue;
                } else {
                    if(noiseMap[x, z] < .35f) {
                        colorMap[z * width + x] = terrainColor1;
                    } else if(noiseMap[x, z] < .7f) {
                        colorMap[z * width + x] = terrainColor2;
                    } else {
                        colorMap[z * width + x] = terrainColor3;
                    }
                }
            }
        }
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colorMap);
        texture.Apply();
        meshRenderer.material = terrainMaterial;
        meshRenderer.material.mainTexture = texture;
    }

    void DrawEdgeMesh() {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        for(int x = 1; x < width - 1; x++) {
            for(int z = 1; z < height - 1; z++) {
                if(!grid.GetGridCell(x, z).isWater) {
                    if(grid.GetGridCell(x - 1, z).isWater) {
                        Vector3 a = new Vector3(x * cellSize, 0, z * cellSize + cellSize);
                        Vector3 b = new Vector3(x * cellSize, 0, z * cellSize);
                        Vector3 c = new Vector3(x * cellSize, -6, z * cellSize + cellSize);
                        Vector3 d = new Vector3(x * cellSize, -6, z * cellSize);
                        Vector3[] v = new Vector3[] { a, b, c, b, d, c };

                        for(int i = 0; i < 6; i++) {
                            vertices.Add(v[i]);
                            triangles.Add(triangles.Count);
                        }
                    }
                    if(grid.GetGridCell(x + 1, z).isWater) {
                        Vector3 a = new Vector3(x * cellSize + cellSize, 0, z * cellSize);
                        Vector3 b = new Vector3(x * cellSize + cellSize, 0, z * cellSize + cellSize);
                        Vector3 c = new Vector3(x * cellSize + cellSize, -6, z * cellSize);
                        Vector3 d = new Vector3(x * cellSize + cellSize, -6, z * cellSize + cellSize);
                        Vector3[] v = new Vector3[] { a, b, c, b, d, c };

                        for(int i = 0; i < 6; i++) {
                            vertices.Add(v[i]);
                            triangles.Add(triangles.Count);
                        }
                    }
                    if(grid.GetGridCell(x, z - 1).isWater) {
                        Vector3 a = new Vector3(x * cellSize, 0, z * cellSize);
                        Vector3 b = new Vector3(x * cellSize + cellSize, 0, z * cellSize);
                        Vector3 c = new Vector3(x * cellSize, -6, z * cellSize);
                        Vector3 d = new Vector3(x * cellSize + cellSize, -6, z * cellSize);
                        Vector3[] v = new Vector3[] { a, b, c, b, d, c };

                        for(int i = 0; i < 6; i++) {
                            vertices.Add(v[i]);
                            triangles.Add(triangles.Count);
                        }
                    }
                    if(grid.GetGridCell(x, z + 1).isWater) {
                        Vector3 a = new Vector3(x * cellSize + cellSize, 0, z * cellSize + cellSize);
                        Vector3 b = new Vector3(x * cellSize, 0, z * cellSize + cellSize);
                        Vector3 c = new Vector3(x * cellSize + cellSize, -6, z * cellSize + cellSize);
                        Vector3 d = new Vector3(x * cellSize, -6, z * cellSize + cellSize);
                        Vector3[] v = new Vector3[] { a, b, c, b, d, c };

                        for(int i = 0; i < 6; i++) {
                            vertices.Add(v[i]);
                            triangles.Add(triangles.Count);
                        }
                    }
                }
            }
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        GameObject edgeObj = new GameObject("Edges");
        edgeObj.transform.SetParent(terrainParent);
        MeshFilter meshFilter = edgeObj.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        MeshRenderer meshRenderer = edgeObj.AddComponent<MeshRenderer>();
        meshRenderer.material = edgeMaterial;
    }

    void CreateTrees() {
        float[,] noiseMap = CreateNoiseMap(treeNoiseScale);
        GameObject treeObj = new GameObject("Trees");
        treeObj.transform.SetParent(terrainParent);
        BuildingSystem buildingSystem = BuildingSystem.instance;
        for(int x = 0; x < width; x++) {
            for(int z = 0; z < height; z++) {
                if(!grid.GetGridCell(x, z).isWater) {
                    float v = Random.Range(0f, treeDensity);
                    if(noiseMap[x, z] < v) {
                        BuildableObjectSO buildableObjectSO = treePrefabs[Random.Range(0, treePrefabs.Length)];
                        buildingSystem.BuildTree(buildableObjectSO, x, z, cellSize);
                    }
                }
            }
        }
    }

    public Grid3D<GridCell> GetGrid() {
        return grid;
    }
}