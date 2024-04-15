public class GridCell {

    public int x;
    public int z;
    public int gCost;
    public int hCost;
    public int fCost;
    public bool isWater;
    public bool isWalkable;
    public bool isRoad;
    public bool isTempRoad;
    public bool isFarm;
    public GridCell cameFromNode;
    BuildedObject buildedObject;

    public GridCell(int x, int z) {
        this.x = x;
        this.z = z;
    }

    public void SetBuildedObject(BuildedObject buildedObject) {
        this.buildedObject = buildedObject;
    }

    public void ClearBuildedObject() {
        this.buildedObject = null;
        this.isWalkable = true;
    }

    public bool CanBuild() {
        return buildedObject == null;
    }

    public BuildedObject GetBuildedObject() {
        return buildedObject;
    }

    public void CalcFCost() {
        fCost = gCost + hCost;
    }

    public override string ToString() {
        return x + "," + z;
    }

    public bool GetIsWalkable() {
        return isWalkable;
    }
}