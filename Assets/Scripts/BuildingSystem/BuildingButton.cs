using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour {

    [SerializeField] BuildableObjectSO buildableObjectSO;

    void Start() {
        //gameObject.GetComponent<Image>().sprite = buildableObjectSO.sprite;
        gameObject.GetComponent<Button>().onClick.AddListener(OnButtonClick);

    }

    void OnButtonClick() {
        BuildingSystem.instance.SetBuildableObject(buildableObjectSO);
    }
}