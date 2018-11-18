using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace Game.Actor {
    public static class ActorMgr {
        private static GameObject playerPrefab = Resources.Load("Prefab/Shooter") as GameObject;
        private static Dictionary<int, GameObject> playerMap = new Dictionary<int, GameObject>();

        public static GameObject NewPlayer(int connectionId, Vector3 position, bool isLocal) {
            var obj = GameObject.Instantiate(ActorMgr.playerPrefab, position, Quaternion.identity);
            obj.name = connectionId.ToString();
            obj.GetComponent<Identity>().connectionId = connectionId;
            ActorMgr.playerMap.Add(connectionId, obj);
            
            if (isLocal) {
                var camera = GameObject.FindWithTag("MainCamera");
                var con = camera.GetComponent<ParentConstraint>();
                var source = new ConstraintSource(){
                    sourceTransform = obj.transform,
                    weight = 1
                };

                con.translationOffsets = new Vector3[]{camera.transform.position};
                con.rotationOffsets = new Vector3[]{camera.transform.rotation.eulerAngles};
                con.AddSource(source);
            }

            return obj;
        }

        public static bool DelPlayer(int connectionId) {
            if (!ActorMgr.playerMap.ContainsKey(connectionId)) {
                return false;
            }

            GameObject.Destroy(ActorMgr.playerMap[connectionId]);
            ActorMgr.playerMap.Remove(connectionId);

            return true;
        }

        public static PlayerData[] ToPlayerDatas() {
            var playerDatas = new PlayerData[ActorMgr.playerMap.Values.Count];
            int i = 0;

            foreach (var p in ActorMgr.playerMap.Values) {
                playerDatas[i] = p.GetComponent<Identity>().ToPlayerData();
                i++;
            }

            return playerDatas;
        }

        public static void Input(Snapshot snapshot) {
            var identity = ActorMgr.playerMap[snapshot.connectionId].GetComponent<Identity>();
            identity.RunEvent(snapshot);
        }

        public static GameObject GetPlayer(int connectionId) {
            return ActorMgr.playerMap[connectionId];
        }
    }
}
