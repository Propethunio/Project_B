using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingSystem : MonoBehaviour {

    public static BuildingSystem instance { get; private set; }

    [SerializeField] private List<BuildableObjectSO> buildableObjectSOList;
    [SerializeField] private LayerMask mouseColliderMask;

    private RoadFixerManager roadFixer;
    private BuildingGhostManager buildingGhost;
    private Grid3D<GridCell> grid;
    private BuildableObjectSO buildableObjectSO;
    private BuildableObjectSO.Dir dir;
    private bool placingRoad;
    private bool placingFarm;
    private Vector3 startBuildPos;
    private Vector2 lastBuildPos;

    void Awake() {
        if(instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        grid = GridManager.instance.GetGrid();
        buildingGhost = BuildingGhostManager.Instance;
        roadFixer = RoadFixerManager.Instance;
    }

    private void Update() {
        if(Input.GetMouseButtonDown(0)) {
            if(EventSystem.current.currentSelectedGameObject != null) {
                return;
            }
            TryBuild();
        }
        if(Input.GetKeyDown(KeyCode.R)) {
            dir = BuildableObjectSO.GetNextDir(dir);
        }
        if(Input.GetKeyDown(KeyCode.X)) {
            DestroyObject();
        }
        if(placingRoad) {
            Vector3 mousePosition = GetMouseWorldPosition3D();
            grid.GetXZ(mousePosition, out int x, out int z);
            Vector2Int currGridPos = new Vector2Int(x, z);
            if(lastBuildPos != currGridPos) {
                lastBuildPos = currGridPos;
                List<Vector3> path = PathfindingSystem.instance.FindPath(startBuildPos, mousePosition);
                //path.ForEach(path => print(path));
                buildingGhost.RefreshRoad(path);
            }
        }
        if(placingFarm) {
            Vector3 mousePosition = GetMouseWorldPosition3D();
            grid.GetXZ(mousePosition, out int x, out int z);
            Vector2Int currGridPos = new Vector2Int(x, z);
            if(lastBuildPos != currGridPos) {
                lastBuildPos = currGridPos;
                List<GridCell> farmNodes = GetFarmNodes(startBuildPos, mousePosition);
                buildingGhost.RefreshFarm(farmNodes);
            }
        }
    }

    private void TryBuild() {
        if(buildableObjectSO == null) {
            return;
        }
        Vector3 mousePosition = GetMouseWorldPosition3D();
        grid.GetXZ(mousePosition, out int x, out int z);
        GridCell node = grid.GetGridCell(x, z);
        if(node == null) {
            return;
        }
        if(buildableObjectSO.isFarm) {
            if(node.CanBuild() || node.isFarm) {
                BuildFarm(mousePosition, x, z);
                return;
            }
        }
        if(buildableObjectSO.isRoad) {
            if(node.CanBuild() || node.isRoad) {
                BuildRoad(mousePosition, x, z);
                return;
            }
        }
        bool canBuild = true;
        List<Vector2Int> gridPositionList = buildableObjectSO.GetGridPositionList(new Vector2Int(x, z), dir);
        foreach(Vector2Int gridPosition in gridPositionList) {
            if(!grid.GetGridCell(gridPosition.x, gridPosition.y).CanBuild()) {
                canBuild = false;
                break;
            }
        }
        if(canBuild) {
            BuildBuilding(x, z, gridPositionList);
        }
    }

    private void BuildFarm(Vector3 mousePosition, int x, int z) {
        if(!placingFarm) {
            placingFarm = true;
            startBuildPos = mousePosition;
            lastBuildPos = new Vector2Int(x, z);
            return;
        }
        List<GridCell> gridNodes = GetFarmNodes(startBuildPos, mousePosition);
        bool canBuild = true;
        foreach(GridCell node in gridNodes) {
            if(!node.CanBuild() && !node.isFarm) {
                canBuild = false;
                break;
            }
        }
        if(canBuild) {
            foreach(GridCell node in gridNodes) {
                if(!node.isFarm) {
                    node.isFarm = true;
                    Vector2Int rotationOffset = buildableObjectSO.GetRotationOffset(dir);
                    Vector3 objectWorldPosition = grid.GetWorldPosition(node.x, node.z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
                    BuildedObject buildedObject = BuildedObject.CreateBuilding(objectWorldPosition, new Vector2Int(node.x, node.z), dir, buildableObjectSO);
                    node.SetBuildedObject(buildedObject);
                }
            }
            placingFarm = false;
            buildingGhost.RefreshVisual();
        }
    }

    private List<GridCell> GetFarmNodes(Vector3 startPos, Vector3 endPos) {
        List<GridCell> nodes = new List<GridCell>();
        grid.GetXZ(startPos, out int startX, out int startZ);
        grid.GetXZ(endPos, out int endX, out int endZ);
        for(int x = Mathf.Min(startX, endX); x <= Mathf.Max(startX, endX); x++) {
            for(int z = Mathf.Min(startZ, endZ); z <= Mathf.Max(startZ, endZ); z++) {
                GridCell node = grid.GetGridCell(x, z);
                if(node == null) {
                    return null;
                }
                nodes.Add(grid.GetGridCell(x, z));
            }
        }
        return nodes;
    }

    private void BuildRoad(Vector3 mousePosition, int x, int z) {
        if(!placingRoad) {
            placingRoad = true;
            startBuildPos = mousePosition;
            lastBuildPos = new Vector2Int(x, z);
        } else {
            placingRoad = false;
            buildingGhost.CleanOldVisual();
            List<Vector3> posList = PathfindingSystem.instance.FindPath(startBuildPos, mousePosition);
            List<GridCell> gridNodes = new List<GridCell>();
            foreach(Vector3 pos in posList) {
                grid.GetXZ(pos, out x, out z);
                GridCell node = grid.GetGridCell(x, z);
                node.isRoad = true;
                gridNodes.Add(node);
            }
            foreach(GridCell node in gridNodes) {
                roadFixer.RefreshNeighborns(node.x, node.z, gridNodes);
                roadFixer.RefreshRoadNode(node);
            }
            buildingGhost.RefreshVisual();
        }
    }

    private void BuildBuilding(int x, int z, List<Vector2Int> gridPositionList) {
        Vector2Int rotationOffset = buildableObjectSO.GetRotationOffset(dir);
        Vector3 objectWorldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
        BuildedObject buildedObject = BuildedObject.CreateBuilding(objectWorldPosition, new Vector2Int(x, z), dir, buildableObjectSO);
        GridCell node;
        foreach(Vector2Int gridPosition in gridPositionList) {
            node = grid.GetGridCell(gridPosition.x, gridPosition.y);
            node.SetBuildedObject(buildedObject);
            node.isWalkable = false;
        }
    }

    private void DestroyObject() {
        Vector3 mousePosition = GetMouseWorldPosition3D();
        GridCell node = grid.GetGridCell(mousePosition);
        BuildedObject buildedObject = node.GetBuildedObject();
        if(buildedObject != null) {
            buildedObject.DestroySelf();
            List<Vector2Int> gridPositionList = buildedObject.GetGridPositionList();
            foreach(Vector2Int gridPosition in gridPositionList) {
                node = grid.GetGridCell(gridPosition.x, gridPosition.y);
                if(node.isRoad) {
                    node.isRoad = false;
                    roadFixer.RefreshNeighborns(node.x, node.z, new List<GridCell>());
                }
                node.ClearBuildedObject();
            }
        }
    }

    private Vector3 GetMouseWorldPosition3D() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit raycastHit, 999f, mouseColliderMask)) {
            return raycastHit.point;
        } else return default;
    }

    public Vector3 GetMouseOverGridPosition() {
        Vector3 mousePosition = GetMouseWorldPosition3D();
        grid.GetXZ(mousePosition, out int x, out int z);
        Vector3 objectWorldPosition = grid.GetWorldPosition(x, z);
        return objectWorldPosition;
    }

    public int GetBuildableRotation() {
        return buildableObjectSO.GetRotationAngle(dir);
    }

    public BuildableObjectSO GetBuildableObjectSO() {
        return buildableObjectSO;
    }

    public Vector3 GetOffset() {
        Vector2Int offset = buildableObjectSO.GetRotationOffset(dir);
        Vector3 rotationOffset = new Vector3(offset.x, 0, offset.y) * grid.GetCellSize();
        return rotationOffset;
    }

    public void BuildTree(BuildableObjectSO buildableObjectSO, int x, int z, float cellSize) {
        Vector3 objectWorldPosition = new Vector3(x * cellSize + cellSize / 2, 0f, z * cellSize + cellSize / 2);
        BuildedObject buildedObject = BuildedObject.CreateBuilding(objectWorldPosition, new Vector2Int(x, z), dir, buildableObjectSO);
        buildedObject.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
        buildedObject.transform.localScale = Vector3.one * Random.Range(0.8f, 1.2f);
        GridCell node = grid.GetGridCell(x, z);
        node.SetBuildedObject(buildedObject);
        node.isWalkable = false;
    }

    public void SetBuildableObject(BuildableObjectSO buildableObjectSO) {
        this.buildableObjectSO = buildableObjectSO;
        dir = BuildableObjectSO.Dir.Down;
        buildingGhost.RefreshVisual();
    }
}