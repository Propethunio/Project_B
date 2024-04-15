using UnityEngine;

public class Water : MonoBehaviour {

    public static Water instance { get; private set; }

    void Awake() {
        if(instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    public void ChangeWaterScale(float width, float height) {
        gameObject.transform.localScale = new Vector3(width, 1f, height);
    }
}