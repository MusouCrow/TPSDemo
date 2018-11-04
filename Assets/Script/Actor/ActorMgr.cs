using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace Game.Actor {
    using Network;

    public static class ActorMgr {
        private static GameObject shooterPrefab = Resources.Load("Prefab/Shooter") as GameObject;

        private static GameObject NewShooter(string addr) {
            var go = GameObject.Instantiate(shooterPrefab);
            go.name = addr;

            return go;
        }

        public static void Start(string leftAddr, string rightAddr, string selfAddr) {
            var a = ActorMgr.NewShooter(leftAddr);
            var b = ActorMgr.NewShooter(rightAddr);
            var self = selfAddr == leftAddr ? a : b;
            var camera = GameObject.FindWithTag("MainCamera");
            var con = camera.GetComponent<ParentConstraint>();
            var source = new ConstraintSource(){
                sourceTransform = self.transform,
                weight = 1
            };

            con.translationOffsets = new Vector3[]{camera.transform.position};
            con.rotationOffsets = new Vector3[]{camera.transform.rotation.eulerAngles};
            con.AddSource(source);
        }

        public static void Input(string addr, InputData data) {
            GameObject obj = GameObject.Find(addr);
            obj.GetComponent<Input>().SetData(data);
        }
    }
}
