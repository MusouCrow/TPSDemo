using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace Game.Actor {
    using Network;

    public static class ActorMgr {
        private static GameObject shooterPrefab = Resources.Load("Prefab/Shooter") as GameObject;
        
        public static GameObject NewShooter(string addr, float x, float z) {
            var obj = GameObject.Instantiate(ActorMgr.shooterPrefab, new Vector3(x, 0, z), Quaternion.identity);
            obj.name = addr;
            var identity = obj.GetComponent<Identity>();
            identity.addr = addr;

            return obj;
        }

        public static void BindCamera(Transform ts) {
            var camera = GameObject.FindWithTag("MainCamera");
            var con = camera.GetComponent<ParentConstraint>();
            var source = new ConstraintSource(){
                sourceTransform = ts,
                weight = 1
            };

            con.translationOffsets = new Vector3[]{camera.transform.position};
            con.rotationOffsets = new Vector3[]{camera.transform.rotation.eulerAngles};
            con.AddSource(source);
        }

        public static void Input(string addr, InputData[] datas) {
            var obj = GameObject.Find(addr);
            obj.GetComponent<Identity>().SetInputDatas(datas);
        }
    }
}