using UnityEngine;
using System;

public class Grid3D<GridCell> {

    int width, height;
    float cellSize;
    GridCell[,] cellArray;

    public Grid3D(int width, int height, float cellSize, bool showDebug, Func<int, int, GridCell> createGridCell) {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;

        cellArray = new GridCell[width, height];

        for(int x = 0; x < width; x++) {
            for(int z = 0; z < height; z++) {
                cellArray[x, z] = createGridCell(x, z);
            }
        }
        if(showDebug) {
            TextMesh[,] debugTextArray = new TextMesh[width, height];
            for(int x = 0; x < width; x++) {
                for(int z = 0; z < height; z++) {
                    debugTextArray[x, z] = CreateWorldText(cellArray[x, z]?.ToString(), null, GetWorldPosition(x, z) + new Vector3(cellSize, cellSize) * .5f, 20, Color.white, TextAnchor.MiddleCenter);
                    Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x, z + 1), Color.white, float.MaxValue);
                    Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x + 1, z), Color.white, float.MaxValue);
                }
            }
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, float.MaxValue);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, float.MaxValue);
        }
    }

    public Vector3 GetWorldPosition(int x, int z) {
        return new Vector3(x, 0, z) * cellSize;
    }

    public void SetGridObject(Vector3 worldPosition, GridCell value) {
        GetXZ(worldPosition, out int x, out int z);
        SetGridObject(x, z, value);
    }

    public void SetGridObject(int x, int z, GridCell value) {
        if(x >= 0 && z >= 0 && x < width && z < height) {
            cellArray[x, z] = value;
        }
    }

    public void GetXZ(Vector3 worldPosition, out int x, out int z) {
        x = Mathf.FloorToInt((worldPosition).x / cellSize);
        z = Mathf.FloorToInt((worldPosition).z / cellSize);
    }

    public GridCell GetGridCell(Vector3 worldPosition) {
        GetXZ(worldPosition, out int x, out int z);
        return GetGridCell(x, z);
    }

    public GridCell GetGridCell(int x, int z) {
        if(x >= 0 && z >= 0 && x < width && z < height) {
            return cellArray[x, z];
        } else return default(GridCell);
    }

    public int GetWidth() {
        return width;
    }

    public int GetHeight() {
        return height;
    }

    public float GetCellSize() {
        return cellSize;
    }

    // Create Text in the World
    static TextMesh CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default(Vector3), int fontSize = 40, Color? color = null, TextAnchor textAnchor = TextAnchor.UpperLeft, TextAlignment textAlignment = TextAlignment.Left, int sortingOrder = 5000) {
        if(color == null) color = Color.white;
        return CreateWorldText(parent, text, localPosition, fontSize, (Color)color, textAnchor, textAlignment, sortingOrder);
    }

    // Create Text in the World
    static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder) {
        GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        textMesh.alignment = textAlignment;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        return textMesh;
    }
}