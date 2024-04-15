using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuildingGhostManager : MonoBehaviour {

    public static BuildingGhostManager Instance { get; private set; }

    private GameObject ghostObject;
    private List<BuildedObject> ghostObjectList;
    private List<GridCell> roadNodesList;
    private Grid3D<GridCell> grid;
    private BuildingSystem buildingSystem;
    private RoadFixerManager roadFixer;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        grid = GridManager.instance.GetGrid();
        buildingSystem = BuildingSystem.instance;
        roadFixer = RoadFixerManager.Instance;
        ghostObjectList = new List<BuildedObject>();
        roadNodesList = new List<GridCell>();
        RefreshVisual();
    }

    private void LateUpdate() {
        if(ghostObject != null) {
            Vector3 ghostPosition = buildingSystem.GetMouseOverGridPosition() + buildingSystem.GetOffset();
            ghostPosition.y += 1f;
            ghostObject.transform.position = Vector3.Lerp(ghostObject.transform.position, ghostPosition, Time.deltaTime * 15f);
            ghostObject.transform.rotation = Quaternion.Lerp(ghostObject.transform.rotation, Quaternion.Euler(0, buildingSystem.GetBuildableRotation(), 0), Time.deltaTime * 15f);
        }
    }

    public void RefreshVisual() {
        CleanOldVisual();
        BuildableObjectSO buildableObjectSO = buildingSystem.GetBuildableObjectSO();
        if(buildableObjectSO != null) {
            ghostObject = Instantiate(buildableObjectSO.visual, buildingSystem.GetMouseOverGridPosition() + buildingSystem.GetOffset(), Quaternion.Euler(0, buildingSystem.GetBuildableRotation(), 0));
        }
    }

    public void RefreshFarm(List<GridCell> farmList) {
        CleanOldVisual();
        BuildableObjectSO buildableObjectSO = buildingSystem.GetBuildableObjectSO();
        int rotation = buildingSystem.GetBuildableRotation();
        bool canBuild = true;
        if(farmList == null) {
            return;
        }
        foreach(GridCell node in farmList) {
            Vector3 objectWorldPosition = grid.GetWorldPosition(node.x, node.z);
            BuildedObject buildedObject = Instantiate(buildableObjectSO.visual, objectWorldPosition, Quaternion.Euler(0, rotation, 0)).GetComponent<BuildedObject>();
            ghostObjectList.Add(buildedObject);
            if(!canBuild) {
                continue;
            }
            if(!node.CanBuild() && !node.isFarm) {
                canBuild = false;
            }
        }
        if(!canBuild) {
            foreach(BuildedObject ghostObject in ghostObjectList) {
                ghostObject.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
            }
        }
        
    }

    public void RefreshRoad(List<Vector3> RoadList) {
        CleanOldVisual();
        foreach(Vector3 road in RoadList) {
            grid.GetXZ(road, out int x, out int z);
            GridCell node = grid.GetGridCell(x, z);
            node.isTempRoad = true;
            roadNodesList.Add(node);
        }
        foreach(GridCell roadNode in roadNodesList) {
            Vector3 objectWorldPosition = roadFixer.SetRoadVisual(roadNode.x, roadNode.z) + new Vector3(0, 0.01f, 0);
            BuildableObjectSO buildableObjectSO = roadFixer.GetBuildableRoadSO();
            int rotation = roadFixer.GetRoadRotation();
            BuildedObject buildedObject = Instantiate(buildableObjectSO.visual, objectWorldPosition, Quaternion.Euler(0, rotation, 0)).GetComponent<BuildedObject>();
            ghostObjectList.Add(buildedObject);
        }
    }

    public void CleanOldVisual() {
        if(ghostObject != null) {
            Destroy(ghostObject);
        }
        if(ghostObjectList.Count > 0) {
            foreach(BuildedObject ghostObject in ghostObjectList) {
                ghostObject.DestroySelf();
            }
            ghostObjectList.Clear();
        }
        if(roadNodesList.Count > 0) {
            foreach(GridCell node in roadNodesList) {
                node.isTempRoad = false;
            }
            roadNodesList.Clear();
        }
    }
}