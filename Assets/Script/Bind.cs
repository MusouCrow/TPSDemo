using System;
using UnityEngine;
using UnityEngine.Animations;

public class Bind : MonoBehaviour {
    protected void Start() {
        var camera = GameObject.FindWithTag("MainCamera");
        var con = camera.GetComponent<ParentConstraint>();
        var source = new ConstraintSource(){
            sourceTransform = this.transform,
            weight = 1
        };
        
        con.AddSource(source);
        con.translationOffsets = new Vector3[]{new Vector3(-5, 1, 1)};
        con.rotationOffsets = new Vector3[]{new Vector3(0, 90, 0)};
    }
}
