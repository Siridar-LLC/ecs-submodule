#if VIEWS_MODULE_SUPPORT
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS.Views {

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class PoolGameObject<T> where T : Component, IViewBase, IViewRespawnTime {

        private Dictionary<ViewId, HashSet<T>> prefabToInstances = new Dictionary<ViewId, HashSet<T>>();

        private static float GetCurrentTime() {

            //return (float)Worlds.currentWorld.GetTimeSinceStart();
            return Time.realtimeSinceStartup;

        }
        
        public void Clear() {

            foreach (var instance in this.prefabToInstances) {

                foreach (var view in instance.Value) {

                    if (view != null) UnityObjectUtils.Destroy(view.gameObject);

                }
                
            }
            this.prefabToInstances.Clear();
            
        }
        
        public T Spawn(T source, ViewId sourceId, in Entity targetEntity) {

            T instance = default;
            var found = false;
            var key = sourceId;
            HashSet<T> list;
            if (this.prefabToInstances.TryGetValue(key, out list) == true) {

                if (list.Count > 0) {

                    if (source is IViewRespawnTime sourceRespawn && sourceRespawn.hasCache == true) {

                        foreach (var item in list) {
                            
                            if (item is IViewRespawnTime itemRespawn && item.entity.id == targetEntity.id) {

                                instance = item;
                                list.Remove(instance);
                                found = true;
                                break;
                                
                            }
                            
                        }

                        if (found == false) {

                            foreach (var item in list) {

                                if (item is IViewRespawnTime itemRespawn && itemRespawn.respawnTime <= PoolGameObject<T>.GetCurrentTime()) {

                                    instance = item;
                                    list.Remove(instance);
                                    found = true;
                                    break;

                                }

                            }

                        }

                    } else {

                        foreach (var item in list) {

                            instance = item;
                            list.Remove(instance);
                            found = true;
                            break;

                        }

                    }

                }

            } else {
                
                list = new HashSet<T>();
                this.prefabToInstances.Add(key, list);
                
            }

            if (found == false) {

                var go = GameObject.Instantiate(source);
                instance = go.GetComponent<T>();

            }

            var instanceInternal = (IViewBaseInternal)instance;
            instanceInternal.Setup(instance.world, new ViewInfo(instance.entity, key, instance.creationTick));
            instance.gameObject.SetActive(true);
            return instance;

        }

        public void Recycle(ref T instance, float timeout) {
            
            var key = instance.prefabSourceId;
            HashSet<T> list;
            if (this.prefabToInstances.TryGetValue(key, out list) == true) {

                if (instance != null) {

                    if (instance.gameObject != null) instance.gameObject.SetActive(false);
                    if (instance is IViewRespawnTime respawnTimeInstance) {
                    
                        respawnTimeInstance.respawnTime = PoolGameObject<T>.GetCurrentTime() + timeout;

                    }
                    list.Add(instance);

                }

            } else {

                UnityObjectUtils.Destroy(instance.gameObject);

            }

            instance = null;

        }
        
    }

}
#endif