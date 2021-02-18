﻿#if ENABLE_IL2CPP
#define INLINE_METHODS
#endif

namespace ME.ECS {

    using ME.ECS.Collections;

    public class IsBitmask : System.Attribute { }

    public enum ComponentLifetime : byte {

        Infinite = 0,

        NotifyAllSystemsBelow = 1,
        NotifyAllSystems = 2,

        NotifyAllModulesBelow = 3,
        NotifyAllModules = 4,

    }

    public interface IStructComponent { }

    public interface IStructCopyableBase { }

    public interface IStructCopyable<T> : IStructComponent, IStructCopyableBase where T : IStructCopyable<T> {

        void CopyFrom(in T other);
        void OnRecycle();

    }

    public interface IStructRegistryBase {

        IStructComponent GetObject(Entity entity);
        bool SetObject(Entity entity, IStructComponent data);
        bool RemoveObject(Entity entity);
        bool HasType(System.Type type);

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public abstract class StructRegistryBase : IStructRegistryBase, IPoolableRecycle {

        public World world {
            get {
                return Worlds.currentWorld;
            }
        }

        public abstract int GetTypeBit();
        public abstract int GetAllTypeBit();

        public abstract bool HasType(System.Type type);
        public abstract IStructComponent GetObject(Entity entity);
        public abstract bool SetObject(Entity entity, IStructComponent data);
        public abstract bool RemoveObject(Entity entity);

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public abstract void Merge();

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public abstract void UseLifetimeStep(World world, byte step);

        public abstract void CopyFrom(StructRegistryBase other);

        public abstract void CopyFrom(in Entity from, in Entity to);

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public abstract bool Validate(int capacity);

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public abstract bool Validate(in Entity entity);

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public abstract bool Has(in Entity entity);

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public abstract bool Remove(in Entity entity, bool clearAll = false);

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public abstract void OnRecycle();

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public abstract void Recycle();

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public abstract StructRegistryBase Clone();

        public abstract int GetCustomHash();

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class StructComponents<TComponent> : StructRegistryBase where TComponent : struct, IStructComponent {

        [ME.ECS.Serializer.SerializeField]
        internal BufferArraySliced<TComponent> components;
        [ME.ECS.Serializer.SerializeField]
        internal BufferArray<byte> componentsStates;
        [ME.ECS.Serializer.SerializeField]
        internal ListCopyable<int> lifetimeIndexes;

        internal TComponent emptyComponent;

        public override int GetTypeBit() {

            return WorldUtilities.GetComponentTypeId<TComponent>();

        }

        public override int GetAllTypeBit() {

            return WorldUtilities.GetAllComponentTypeId<TComponent>();

        }

        public override int GetCustomHash() {

            var hash = 0;
            if (typeof(TComponent) == typeof(ME.ECS.Transform.Position)) {

                for (int i = 0; i < this.components.Length; ++i) {

                    var p = (ME.ECS.Transform.Position)(object)this.components[i];
                    hash ^= (int)(p.x * 100000f);
                    hash ^= (int)(p.y * 100000f);
                    hash ^= (int)(p.z * 100000f);

                }

            }

            return hash;

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public override void Merge() {

            if (WorldUtilities.IsComponentAsTag<TComponent>() == false) this.components = this.components.Merge();

        }

        public override void Recycle() {

            PoolRegistries.Recycle(this);

        }

        public override StructRegistryBase Clone() {

            var reg = this.SpawnInstance();
            this.Merge();
            reg.CopyFrom(this);
            return reg;

        }

        protected virtual StructRegistryBase SpawnInstance() {

            return PoolRegistries.Spawn<TComponent>();

        }

        public override void OnRecycle() {

            this.components = this.components.Dispose();
            PoolArray<byte>.Recycle(ref this.componentsStates);
            if (this.lifetimeIndexes != null) PoolListCopyable<int>.Recycle(ref this.lifetimeIndexes);

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public override void UseLifetimeStep(World world, byte step) {

            if (this.lifetimeIndexes != null) {

                for (int i = 0; i < this.lifetimeIndexes.Count; ++i) {

                    var id = this.lifetimeIndexes[i];
                    this.UseLifetimeStep(id, world, step);

                }

                this.lifetimeIndexes.Clear();

            }

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        private void UseLifetimeStep(int id, World world, byte step) {

            ref var state = ref this.componentsStates.arr[id];
            if (state - 1 == step) {

                var entity = world.GetEntityById(id);
                if (entity.generation == 0) return;

                state = 0;
                if (WorldUtilities.IsComponentAsTag<TComponent>() == false) this.components[id] = default;
                if (world.currentState.filters.HasInAnyFilter<TComponent>() == true) {

                    world.currentState.storage.archetypes.Remove<TComponent>(in entity);
                    world.UpdateFilterByStructComponent<TComponent>(in entity);

                }

                --world.currentState.structComponents.count;
                #if ENTITY_ACTIONS
                world.RaiseEntityActionOnRemove<TComponent>(in entity);
                #endif

            }

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public override bool Validate(int capacity) {

            var result = false;
            if (WorldUtilities.IsComponentAsTag<TComponent>() == false) {

                if (ArrayUtils.Resize(capacity, ref this.components) == true) {

                    // Add into dirty map
                    result = true;

                }

            }

            if (ArrayUtils.WillResize(capacity, ref this.componentsStates) == true) {

                ArrayUtils.Resize(capacity, ref this.componentsStates);

            }

            this.world.currentState.storage.archetypes.Validate(capacity);

            return result;

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public override bool Validate(in Entity entity) {

            var result = false;
            var index = entity.id;
            if (WorldUtilities.IsComponentAsTag<TComponent>() == false) {

                if (ArrayUtils.Resize(index, ref this.components) == true) {

                    // Add into dirty map
                    result = true;

                }

            }

            if (ArrayUtils.WillResize(index, ref this.componentsStates) == true) {

                ArrayUtils.Resize(index, ref this.componentsStates);

            }

            this.world.currentState.storage.archetypes.Validate(in entity);

            if (this.world.currentState.filters.HasInAnyFilter<TComponent>() == true && this.world.HasData<TComponent>(in entity) == true) {

                this.world.currentState.storage.archetypes.Set<TComponent>(in entity);

            }

            return result;

        }

        public override bool HasType(System.Type type) {

            return typeof(TComponent) == type;

        }

        public override IStructComponent GetObject(Entity entity) {

            #if WORLD_EXCEPTIONS
            if (entity.IsAlive() == false) {
                
                EmptyEntityException.Throw(entity);
                
            }
            #endif

            //var bucketId = this.GetBucketId(in entity.id, out var index);
            var index = entity.id;
            //this.CheckResize(in index);
            var bucketState = this.componentsStates.arr[index];
            if (bucketState > 0) {

                if (WorldUtilities.IsComponentAsTag<TComponent>() == false) {

                    var bucket = this.components[index];
                    return bucket;

                } else {

                    return this.emptyComponent;

                }

            }

            return null;

        }

        public override bool SetObject(Entity entity, IStructComponent data) {

            #if WORLD_EXCEPTIONS
            if (entity.IsAlive() == false) {
                
                EmptyEntityException.Throw(entity);
                
            }
            #endif

            //var bucketId = this.GetBucketId(in entity.id, out var index);
            var index = entity.id;
            //this.CheckResize(in index);
            if (WorldUtilities.IsComponentAsTag<TComponent>() == false) {

                ref var bucket = ref this.components[index];
                bucket = (TComponent)data;

            }

            ref var bucketState = ref this.componentsStates.arr[index];
            if (bucketState == 0) {

                bucketState = 1;

                var componentIndex = WorldUtilities.GetComponentTypeId<TComponent>();
                if (this.world.currentState.filters.allFiltersArchetype.HasBit(componentIndex) == true) this.world.currentState.storage.archetypes.Set<TComponent>(in entity);

                return true;

            }

            return false;

        }

        public override bool RemoveObject(Entity entity) {

            #if WORLD_EXCEPTIONS
            if (entity.IsAlive() == false) {
                
                EmptyEntityException.Throw(entity);
                
            }
            #endif

            //var bucketId = this.GetBucketId(in entity.id, out var index);
            var index = entity.id;
            //this.CheckResize(in index);
            ref var bucketState = ref this.componentsStates.arr[index];
            if (bucketState > 0) {

                if (WorldUtilities.IsComponentAsTag<TComponent>() == false) this.RemoveData(in entity);
                bucketState = 0;

                var componentIndex = WorldUtilities.GetComponentTypeId<TComponent>();
                if (this.world.currentState.filters.allFiltersArchetype.HasBit(componentIndex) == true) this.world.currentState.storage.archetypes.Remove<TComponent>(in entity);

                return true;

            }

            return false;

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public virtual void RemoveData(in Entity entity) {

            this.components[entity.id] = default;

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public override bool Has(in Entity entity) {

            #if WORLD_EXCEPTIONS
            if (entity.generation == 0) {
                
                EmptyEntityException.Throw(entity);
                
            }
            #endif

            //var bucketId = this.GetBucketId(in entity.id, out var index);
            //var index = entity.id;
            //this.CheckResize(in index);
            return this.componentsStates.arr[entity.id] > 0;

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public override bool Remove(in Entity entity, bool clearAll = false) {

            #if WORLD_EXCEPTIONS
            if (entity.IsAlive() == false) {
                
                EmptyEntityException.Throw(entity);
                
            }
            #endif

            //var bucketId = this.GetBucketId(in entity.id, out var index);
            var index = entity.id;
            //this.CheckResize(in index);
            ref var bucketState = ref this.componentsStates.arr[index];
            if (bucketState > 0) {

                if (WorldUtilities.IsComponentAsTag<TComponent>() == false) this.RemoveData(in entity);
                bucketState = 0;

                if (clearAll == true) {

                    this.world.currentState.storage.archetypes.RemoveAll<TComponent>(in entity);

                } else {

                    this.world.currentState.storage.archetypes.Remove<TComponent>(in entity);

                }

                return true;

            }

            return false;

        }

        public override void CopyFrom(in Entity from, in Entity to) {

            if (typeof(TComponent) == typeof(ME.ECS.Views.ViewComponent)) {

                var view = from.GetData<ME.ECS.Views.ViewComponent>(createIfNotExists: false);
                if (view.viewInfo.entity == from) {

                    to.InstantiateView(view.viewInfo.prefabSourceId);

                }

                return;

            }

            this.componentsStates.arr[to.id] = this.componentsStates.arr[from.id];
            if (WorldUtilities.IsComponentAsTag<TComponent>() == false) this.components[to.id] = this.components[from.id];
            if (this.componentsStates.arr[from.id] > 0) {

                if (this.world.currentState.filters.HasInAnyFilter<TComponent>() == true) this.world.currentState.storage.archetypes.Set<TComponent>(in to);

            } else {

                if (this.world.currentState.filters.HasInAnyFilter<TComponent>() == true) this.world.currentState.storage.archetypes.Remove<TComponent>(in to);

            }

        }

        public override void CopyFrom(StructRegistryBase other) {

            var _other = (StructComponents<TComponent>)other;
            ArrayUtils.Copy(in _other.componentsStates, ref this.componentsStates);
            ArrayUtils.Copy(_other.lifetimeIndexes, ref this.lifetimeIndexes);
            if (WorldUtilities.IsComponentAsTag<TComponent>() == false) ArrayUtils.Copy(in _other.components, ref this.components);

        }

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public sealed class StructComponentsCopyable<TComponent> : StructComponents<TComponent> where TComponent : struct, IStructComponent, IStructCopyable<TComponent> {

        private struct CopyItem : IArrayElementCopyWithIndex<TComponent> {

            public BufferArray<byte> states;

            #if INLINE_METHODS
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            #endif
            public void Copy(int index, TComponent @from, ref TComponent to) {

                if (this.states.arr != null && index >= 0 && index < this.states.Length && this.states.arr[index] > 0) {

                    to.CopyFrom(from);

                }

            }

            #if INLINE_METHODS
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            #endif
            public void Recycle(int index, ref TComponent item) {

                if (this.states.arr != null && index >= 0 && index < this.states.Length && this.states.arr[index] > 0) {

                    item.OnRecycle();
                    item = default;

                }

            }

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public override void RemoveData(in Entity entity) {

            this.components[entity.id].OnRecycle();
            this.components[entity.id] = default;

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        protected override StructRegistryBase SpawnInstance() {

            return PoolRegistries.SpawnCopyable<TComponent>();

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public override void CopyFrom(StructRegistryBase other) {

            var _other = (StructComponents<TComponent>)other;
            ArrayUtils.Copy(in _other.componentsStates, ref this.componentsStates);
            ArrayUtils.Copy(_other.lifetimeIndexes, ref this.lifetimeIndexes);
            if (WorldUtilities.IsComponentAsTag<TComponent>() == false)
                ArrayUtils.CopyWithIndex(_other.components, ref this.components, new CopyItem() { states = _other.componentsStates });

        }

    }

    public interface IStructComponentsContainer {

        BufferArray<StructRegistryBase> GetAllRegistries();

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public struct StructComponentsContainer : IStructComponentsContainer {

        internal interface ITask {

            Entity entity { get; }
            void Execute();
            void Recycle();
            ITask Clone();
            void CopyFrom(ITask other);

        }

        internal class NextFrameTask<TComponent> : ITask, System.IEquatable<NextFrameTask<TComponent>> where TComponent : struct, IStructComponent {

            public Entity entity { get; set; }
            public TComponent data;
            public World world;
            public ComponentLifetime lifetime;

            public void Execute() {

                if (this.entity.IsAlive() == true) {

                    this.world.SetData(this.entity, in this.data, this.lifetime);

                }

            }

            public void CopyFrom(ITask other) {

                var _other = (NextFrameTask<TComponent>)other;
                this.entity = _other.entity;
                this.data = _other.data;
                this.world = _other.world;
                this.lifetime = _other.lifetime;

            }

            public void Recycle() {

                this.world = null;
                this.data = default;
                this.entity = default;
                this.lifetime = default;
                PoolClass<NextFrameTask<TComponent>>.Recycle(this);

            }

            public ITask Clone() {

                var copy = PoolClass<NextFrameTask<TComponent>>.Spawn();
                copy.CopyFrom(this);
                return copy;

            }

            public bool Equals(NextFrameTask<TComponent> other) {

                if (other == null) return false;
                return this.entity == other.entity && this.lifetime == other.lifetime;

            }

        }

        [ME.ECS.Serializer.SerializeField]
        internal CCList<ITask> nextFrameTasks;
        [ME.ECS.Serializer.SerializeField]
        internal CCList<ITask> nextTickTasks;

        [ME.ECS.Serializer.SerializeField]
        internal BufferArray<StructRegistryBase> list;
        [ME.ECS.Serializer.SerializeField]
        internal int count;
        [ME.ECS.Serializer.SerializeField]
        private bool isCreated;

        private System.Collections.Generic.List<int> dirtyMap;

        public bool IsCreated() {

            return this.isCreated;

        }

        public void Initialize(bool freeze) {

            this.nextFrameTasks = PoolCCList<ITask>.Spawn();
            this.nextTickTasks = PoolCCList<ITask>.Spawn();
            this.dirtyMap = PoolList<int>.Spawn(10);

            ArrayUtils.Resize(100, ref this.list);
            this.isCreated = true;

        }

        public void Merge() {

            if (this.dirtyMap == null) return;

            for (int i = 0, count = this.dirtyMap.Count; i < count; ++i) {

                var idx = this.dirtyMap[i];
                this.list.arr[idx].Merge();

            }

            this.dirtyMap.Clear();

        }

        public BufferArray<StructRegistryBase> GetAllRegistries() {

            return this.list;

        }

        public int Count {
            get {
                return this.count;
            }
        }

        public int GetCustomHash() {

            var hash = 0;
            for (int i = 0; i < this.list.Length; ++i) {

                hash ^= this.list.arr[i].GetCustomHash();

            }

            return hash;

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public void SetEntityCapacity(int capacity) {

            // Update all known structs
            for (int i = 0, length = this.list.Length; i < length; ++i) {

                var item = this.list.arr[i];
                if (item != null) {

                    if (item.Validate(capacity) == true) {

                        this.dirtyMap.Add(i);

                    }

                }

            }

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public void OnEntityCreate(in Entity entity) {

            // Update all known structs
            for (int i = 0, length = this.list.Length; i < length; ++i) {

                var item = this.list.arr[i];
                if (item != null) {

                    if (item.Validate(in entity) == true) {

                        this.dirtyMap.Add(i);

                    }

                }

            }

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public void RemoveAll(in Entity entity) {

            for (int i = 0; i < this.nextFrameTasks.Count; ++i) {

                if (this.nextFrameTasks[i] == null) continue;

                if (this.nextFrameTasks[i].entity == entity) {

                    this.nextFrameTasks[i].Recycle();
                    this.nextFrameTasks.RemoveAt(i);
                    --i;

                }

            }

            for (int i = 0; i < this.nextTickTasks.Count; ++i) {

                if (this.nextTickTasks[i] == null) continue;

                if (this.nextTickTasks[i].entity == entity) {

                    this.nextTickTasks[i].Recycle();
                    this.nextTickTasks.RemoveAt(i);
                    --i;

                }

            }

            for (int i = 0, length = this.list.Length; i < length; ++i) {

                var item = this.list.arr[i];
                if (item != null) {

                    //var sw = new System.Diagnostics.Stopwatch();
                    //sw.Start();
                    item.Validate(in entity);
                    item.Remove(in entity, clearAll: true);
                    //sw.Stop();
                    //if (sw.ElapsedMilliseconds > 10) UnityEngine.Debug.Log("REMOVE " + sw.ElapsedMilliseconds + " :: " + item.GetType());

                }

            }

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public void Validate<TComponent>(in Entity entity, bool isTag = false) where TComponent : struct, IStructComponent {

            var code = WorldUtilities.GetAllComponentTypeId<TComponent>();
            this.Validate<TComponent>(code, isTag);
            var reg = (StructComponents<TComponent>)this.list.arr[code];
            reg.Validate(in entity);

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public void ValidateCopyable<TComponent>(in Entity entity, bool isTag = false) where TComponent : struct, IStructComponent, IStructCopyable<TComponent> {

            var code = WorldUtilities.GetAllComponentTypeId<TComponent>();
            this.ValidateCopyable<TComponent>(code, isTag);
            var reg = (StructComponentsCopyable<TComponent>)this.list.arr[code];
            reg.Validate(in entity);

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public void Validate<TComponent>(bool isTag = false) where TComponent : struct, IStructComponent {

            var code = WorldUtilities.GetAllComponentTypeId<TComponent>();
            if (isTag == true) WorldUtilities.SetComponentAsTag<TComponent>();
            this.Validate<TComponent>(code, isTag);

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public void ValidateCopyable<TComponent>(bool isTag = false) where TComponent : struct, IStructComponent, IStructCopyable<TComponent> {

            var code = WorldUtilities.GetAllComponentTypeId<TComponent>();
            if (isTag == true) WorldUtilities.SetComponentAsTag<TComponent>();
            this.ValidateCopyable<TComponent>(code, isTag);

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        private void ValidateCopyable<TComponent>(int code, bool isTag) where TComponent : struct, IStructComponent, IStructCopyable<TComponent> {

            if (ArrayUtils.WillResize(code, ref this.list) == true) {

                ArrayUtils.Resize(code, ref this.list);

            }

            if (this.list.arr[code] == null) {

                var instance = PoolRegistries.SpawnCopyable<TComponent>();
                this.list.arr[code] = instance;

            }

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        private void Validate<TComponent>(int code, bool isTag) where TComponent : struct, IStructComponent {

            if (ArrayUtils.WillResize(code, ref this.list) == true) {

                ArrayUtils.Resize(code, ref this.list);

            }

            if (this.list.arr[code] == null) {

                var instance = PoolRegistries.Spawn<TComponent>();
                this.list.arr[code] = instance;

            }

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public bool HasBit(in Entity entity, int bit) {

            if (bit < 0 || bit >= this.list.Length) return false;
            var reg = this.list.arr[bit];
            if (reg == null) return false;
            return reg.Has(in entity);

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public void OnRecycle() {

            for (int i = 0; i < this.nextFrameTasks.array.Length; ++i) {

                if (this.nextFrameTasks.array[i] == null) continue;

                for (int j = 0; j < this.nextFrameTasks.array[i].Length; ++j) {

                    if (this.nextFrameTasks.array[i][j] == null) continue;

                    this.nextFrameTasks.array[i][j].Recycle();

                }

            }

            PoolCCList<ITask>.Recycle(ref this.nextFrameTasks);

            for (int i = 0; i < this.nextTickTasks.array.Length; ++i) {

                if (this.nextTickTasks.array[i] == null) continue;

                for (int j = 0; j < this.nextTickTasks.array[i].Length; ++j) {

                    if (this.nextTickTasks.array[i][j] == null) continue;

                    this.nextTickTasks.array[i][j].Recycle();

                }

            }

            PoolCCList<ITask>.Recycle(ref this.nextTickTasks);

            for (int i = 0; i < this.list.Length; ++i) {

                if (this.list.arr[i] != null) {

                    this.list.arr[i].OnRecycle();
                    PoolRegistries.Recycle(this.list.arr[i]);
                    this.list.arr[i] = null;

                }

            }

            PoolArray<StructRegistryBase>.Recycle(ref this.list);

            if (this.dirtyMap != null) PoolList<int>.Recycle(ref this.dirtyMap);

            this.count = default;
            this.isCreated = default;

        }

        public void CopyFrom(in Entity from, in Entity to) {

            for (int i = 0; i < this.list.Count; ++i) {

                var reg = this.list.arr[i];
                if (reg != null) reg.CopyFrom(in from, in to);

            }

        }

        private struct CopyTask : IArrayElementCopy<ITask> {

            public void Copy(ITask @from, ref ITask to) {

                if (from == null && to == null) return;

                if (from == null && to != null) {

                    to.Recycle();
                    to = null;

                } else if (to == null) {

                    to = from.Clone();

                } else {

                    to.CopyFrom(from);

                }

            }

            public void Recycle(ITask item) {

                item.Recycle();

            }

        }

        private struct CopyRegistry : IArrayElementCopy<StructRegistryBase> {

            public void Copy(StructRegistryBase @from, ref StructRegistryBase to) {

                if (from == null && to == null) return;

                if (from == null && to != null) {

                    to.Recycle();
                    to = null;

                } else if (to == null) {

                    to = from.Clone();

                } else {

                    from.Merge();
                    to.CopyFrom(from);

                }

            }

            public void Recycle(StructRegistryBase item) {

                PoolRegistries.Recycle(item);

            }

        }

        #if ECS_COMPILE_IL2CPP_OPTIONS
        [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
         Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
        #endif
        public void CopyFrom(StructComponentsContainer other) {

            //this.OnRecycle();

            this.count = other.count;
            this.isCreated = other.isCreated;

            {

                for (int i = 0; i < this.nextFrameTasks.array.Length; ++i) {

                    if (this.nextFrameTasks.array[i] == null) continue;

                    for (int j = 0; j < this.nextFrameTasks.array[i].Length; ++j) {

                        if (this.nextFrameTasks.array[i][j] == null) continue;

                        this.nextFrameTasks.array[i][j].Recycle();

                    }

                }

                PoolCCList<ITask>.Recycle(ref this.nextFrameTasks);

                for (int i = 0; i < this.nextTickTasks.array.Length; ++i) {

                    if (this.nextTickTasks.array[i] == null) continue;

                    for (int j = 0; j < this.nextTickTasks.array[i].Length; ++j) {

                        if (this.nextTickTasks.array[i][j] == null) continue;

                        this.nextTickTasks.array[i][j].Recycle();

                    }

                }

                PoolCCList<ITask>.Recycle(ref this.nextTickTasks);

                this.nextFrameTasks = PoolCCList<ITask>.Spawn();
                this.nextFrameTasks.InitialCopyOf(other.nextFrameTasks);
                for (int i = 0; i < other.nextFrameTasks.array.Length; ++i) {

                    if (other.nextFrameTasks.array[i] == null) {

                        this.nextFrameTasks.array[i] = null;
                        continue;

                    }

                    for (int j = 0; j < other.nextFrameTasks.array[i].Length; ++j) {

                        var item = other.nextFrameTasks.array[i][j];
                        if (item == null) {

                            this.nextFrameTasks.array[i][j] = null;
                            continue;

                        }

                        var copy = item.Clone();
                        this.nextFrameTasks.array[i][j] = copy;

                    }

                }

                this.nextTickTasks = PoolCCList<ITask>.Spawn();
                this.nextTickTasks.InitialCopyOf(other.nextTickTasks);
                for (int i = 0; i < other.nextTickTasks.array.Length; ++i) {

                    if (other.nextTickTasks.array[i] == null) {

                        this.nextTickTasks.array[i] = null;
                        continue;

                    }

                    for (int j = 0; j < other.nextTickTasks.array[i].Length; ++j) {

                        var item = other.nextTickTasks.array[i][j];
                        if (item == null) {

                            this.nextTickTasks.array[i][j] = null;
                            continue;

                        }

                        var copy = item.Clone();
                        this.nextTickTasks.array[i][j] = copy;

                    }

                }

            }

            //ArrayUtils.Copy(other.nextFrameTasks, ref this.nextFrameTasks, new CopyTask());
            //ArrayUtils.Copy(other.nextTickTasks, ref this.nextTickTasks, new CopyTask());
            ArrayUtils.Copy(other.list, ref this.list, new CopyRegistry());

        }

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public partial class World {

        public ref StructComponentsContainer GetStructComponents() {

            return ref this.currentState.structComponents;

        }

        partial void OnSpawnStructComponents() { }

        partial void OnRecycleStructComponents() { }

        partial void SetEntityCapacityPlugin1(int capacity) {

            this.currentState.structComponents.SetEntityCapacity(capacity);

        }

        partial void CreateEntityPlugin1(Entity entity) {

            this.currentState.structComponents.OnEntityCreate(in entity);

        }

        partial void DestroyEntityPlugin1(Entity entity) {

            this.currentState.structComponents.RemoveAll(in entity);

        }

        public void Register(ref StructComponentsContainer componentsContainer, bool freeze, bool restore) {

            if (componentsContainer.IsCreated() == false) {

                //componentsContainer = new StructComponentsContainer();
                componentsContainer.Initialize(freeze);

            }

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public bool HasSharedData<TComponent>() where TComponent : struct, IStructComponent {

            return this.HasData<TComponent>(in this.sharedEntity);

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public ref TComponent GetSharedData<TComponent>(bool createIfNotExists = true) where TComponent : struct, IStructComponent {

            return ref this.GetData<TComponent>(in this.sharedEntity, createIfNotExists);

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public void SetSharedData<TComponent>(in TComponent data) where TComponent : struct, IStructComponent {

            this.SetData(in this.sharedEntity, data);

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public void SetSharedData<TComponent>(in TComponent data, ComponentLifetime lifetime) where TComponent : struct, IStructComponent {

            this.SetData(in this.sharedEntity, data, lifetime);

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public void RemoveSharedData<TComponent>() where TComponent : struct, IStructComponent {

            this.RemoveData<TComponent>(in this.sharedEntity);

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public bool HasDataBit(in Entity entity, int bit) {

            return this.currentState.structComponents.HasBit(in entity, bit);

        }

        public void ValidateData<TComponent>(in Entity entity, bool isTag = false) where TComponent : struct, IStructComponent {

            this.currentState.structComponents.Validate<TComponent>(in entity, isTag);

        }

        public void ValidateDataCopyable<TComponent>(in Entity entity, bool isTag = false) where TComponent : struct, IStructComponent, IStructCopyable<TComponent> {

            this.currentState.structComponents.ValidateCopyable<TComponent>(in entity, isTag);

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public bool HasData<TComponent>(in Entity entity) where TComponent : struct, IStructComponent {

            #if WORLD_EXCEPTIONS
            if (entity.IsAlive() == false) {
                
                EmptyEntityException.Throw(entity);
                
            }
            #endif

            return this.currentState.structComponents.list.arr[WorldUtilities.GetAllComponentTypeId<TComponent>()].Has(in entity);

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public ref 
            #if UNITY_EDITOR
            readonly
            #endif
            TComponent ReadData<TComponent>(in Entity entity) where TComponent : struct, IStructComponent {

            #if WORLD_EXCEPTIONS
            if (entity.IsAlive() == false) {
                
                EmptyEntityException.Throw(entity);
                
            }
            #endif

            // Inline all manually
            var reg = (StructComponents<TComponent>)this.currentState.structComponents.list.arr[WorldUtilities.GetAllComponentTypeId<TComponent>()];
            if (WorldUtilities.IsComponentAsTag<TComponent>() == true) return ref reg.emptyComponent;
            return ref reg.components[entity.id];

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public ref TComponent GetData<TComponent>(in Entity entity, bool createIfNotExists = true) where TComponent : struct, IStructComponent {

            #if WORLD_EXCEPTIONS
            if (entity.IsAlive() == false) {
                
                EmptyEntityException.Throw(entity);
                
            }
            #endif

            // Inline all manually
            var reg = (StructComponents<TComponent>)this.currentState.structComponents.list.arr[WorldUtilities.GetAllComponentTypeId<TComponent>()];
            ref var state = ref reg.componentsStates.arr[entity.id];
            var incrementVersion = (this.HasStep(WorldStep.LogicTick) == true || this.HasResetState() == false);
            if (state == 0 && createIfNotExists == true) {

                #if WORLD_EXCEPTIONS
                if (this.HasStep(WorldStep.LogicTick) == false && this.HasResetState() == true) {

                    OutOfStateException.ThrowWorldStateCheck();

                }
                #endif

                incrementVersion = true;
                state = 1;
                if (this.currentState.filters.HasInAnyFilter<TComponent>() == true) {

                    this.currentState.storage.archetypes.Set<TComponent>(in entity);
                    this.UpdateFilterByStructComponent<TComponent>(in entity);

                }

                System.Threading.Interlocked.Increment(ref this.currentState.structComponents.count);
                #if ENTITY_ACTIONS
                this.RaiseEntityActionOnAdd<TComponent>(in entity);
                #endif

            }

            if (WorldUtilities.IsComponentAsTag<TComponent>() == true) return ref reg.emptyComponent;
            if (incrementVersion == true) {

                this.currentState.storage.versions.Increment(in entity);
                this.UpdateFilterByStructComponentVersioned<TComponent>(in entity);

            }

            return ref reg.components[entity.id];

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public ref byte SetData<TComponent>(in Entity entity) where TComponent : struct, IStructComponent {

            #if WORLD_STATE_CHECK
            if (this.HasStep(WorldStep.LogicTick) == false && this.HasResetState() == true) {

                OutOfStateException.ThrowWorldStateCheck();
                
            }
            #endif

            #if WORLD_EXCEPTIONS
            if (entity.IsAlive() == false) {
                
                EmptyEntityException.Throw(entity);
                
            }
            #endif

            // Inline all manually
            var reg = (StructComponents<TComponent>)this.currentState.structComponents.list.arr[WorldUtilities.GetAllComponentTypeId<TComponent>()];
            if (WorldUtilities.IsComponentAsTag<TComponent>() == false) reg.RemoveData(in entity);
            ref var state = ref reg.componentsStates.arr[entity.id];
            if (state == 0) {

                state = 1;
                if (this.currentState.filters.HasInAnyFilter<TComponent>() == true) {

                    this.currentState.storage.archetypes.Set<TComponent>(in entity);
                    this.UpdateFilterByStructComponent<TComponent>(in entity);

                }

                System.Threading.Interlocked.Increment(ref this.currentState.structComponents.count);

            }
            #if ENTITY_ACTIONS
            this.RaiseEntityActionOnAdd<TComponent>(in entity);
            #endif
            this.currentState.storage.versions.Increment(in entity);
            this.UpdateFilterByStructComponentVersioned<TComponent>(in entity);

            return ref state;

        }

        /// <summary>
        /// Lifetime default is Infinite
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="data"></param>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public ref byte SetData<TComponent>(in Entity entity, in TComponent data) where TComponent : struct, IStructComponent {

            #if WORLD_STATE_CHECK
            if (this.HasStep(WorldStep.LogicTick) == false && this.HasResetState() == true) {

                OutOfStateException.ThrowWorldStateCheck();
                
            }
            #endif

            #if WORLD_EXCEPTIONS
            if (entity.IsAlive() == false) {
                
                EmptyEntityException.Throw(entity);
                
            }
            #endif

            // Inline all manually
            var reg = (StructComponents<TComponent>)this.currentState.structComponents.list.arr[WorldUtilities.GetAllComponentTypeId<TComponent>()];
            if (WorldUtilities.IsComponentAsTag<TComponent>() == false) reg.components[entity.id] = data;
            ref var state = ref reg.componentsStates.arr[entity.id];
            if (state == 0) {

                state = 1;
                if (this.currentState.filters.HasInAnyFilter<TComponent>() == true) {

                    this.currentState.storage.archetypes.Set<TComponent>(in entity);
                    this.UpdateFilterByStructComponent<TComponent>(in entity);

                }

                System.Threading.Interlocked.Increment(ref this.currentState.structComponents.count);
                //this.AddComponentToFilter(entity);

            }
            #if ENTITY_ACTIONS
            this.RaiseEntityActionOnAdd<TComponent>(in entity);
            #endif
            this.currentState.storage.versions.Increment(in entity);
            this.UpdateFilterByStructComponentVersioned<TComponent>(in entity);

            return ref state;

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        private void PlayTasksForFrame() {

            if (this.currentState.structComponents.nextFrameTasks.Count > 0) {

                for (int i = 0; i < this.currentState.structComponents.nextFrameTasks.Count; ++i) {

                    var task = this.currentState.structComponents.nextFrameTasks[i];
                    if (task == null) continue;

                    task.Execute();
                    task.Recycle();

                }

                this.currentState.structComponents.nextFrameTasks.ClearNoCC();

            }

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        private void PlayTasksForTick() {

            if (this.currentState.structComponents.nextTickTasks.Count > 0) {

                for (int i = 0; i < this.currentState.structComponents.nextTickTasks.Count; ++i) {

                    var task = this.currentState.structComponents.nextTickTasks[i];
                    if (task == null) continue;

                    task.Execute();
                    task.Recycle();

                }

                this.currentState.structComponents.nextTickTasks.ClearNoCC();

            }

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        private void UseLifetimeStep(ComponentLifetime step) {

            var bStep = (byte)step;
            for (int i = 0; i < this.currentState.structComponents.list.Length; ++i) {

                ref var reg = ref this.currentState.structComponents.list.arr[i];
                if (reg == null) continue;

                reg.UseLifetimeStep(this, bStep);

            }

        }

        private void AddToLifetimeIndex<TComponent>(in Entity entity) where TComponent : struct, IStructComponent {

            ref var r = ref this.currentState.structComponents.list.arr[WorldUtilities.GetAllComponentTypeId<TComponent>()];
            var reg = (StructComponents<TComponent>)r;
            if (reg.lifetimeIndexes == null) reg.lifetimeIndexes = PoolListCopyable<int>.Spawn(10);
            reg.lifetimeIndexes.Add(entity.id);

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public void SetData<TComponent>(in Entity entity, ComponentLifetime lifetime) where TComponent : struct, IStructComponent {

            #if WORLD_STATE_CHECK
            if (this.HasStep(WorldStep.LogicTick) == false && this.HasResetState() == true) {

                OutOfStateException.ThrowWorldStateCheck();
                
            }
            #endif

            #if WORLD_EXCEPTIONS
            if (entity.IsAlive() == false) {
                
                EmptyEntityException.Throw(entity);
                
            }
            #endif

            if (lifetime == ComponentLifetime.NotifyAllModules ||
                lifetime == ComponentLifetime.NotifyAllSystems) {

                if (this.HasData<TComponent>(in entity) == true) return;

                var task = PoolClass<StructComponentsContainer.NextFrameTask<TComponent>>.Spawn();
                task.world = this;
                task.entity = entity;
                task.data = default;

                if (lifetime == ComponentLifetime.NotifyAllModules) {

                    task.lifetime = ComponentLifetime.NotifyAllModulesBelow;

                    this.currentState.structComponents.nextFrameTasks.Add(task);

                } else if (lifetime == ComponentLifetime.NotifyAllSystems) {

                    task.lifetime = ComponentLifetime.NotifyAllSystemsBelow;

                    this.currentState.structComponents.nextTickTasks.Add(task);

                }

            } else {

                ref var state = ref this.SetData<TComponent>(in entity);

                if (lifetime == ComponentLifetime.Infinite) return;
                state = (byte)(lifetime + 1);

                this.AddToLifetimeIndex<TComponent>(in entity);

            }

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public void SetData<TComponent>(in Entity entity, in TComponent data, ComponentLifetime lifetime) where TComponent : struct, IStructComponent {

            #if WORLD_STATE_CHECK
            if (this.HasStep(WorldStep.LogicTick) == false && this.HasResetState() == true) {

                OutOfStateException.ThrowWorldStateCheck();
                
            }
            #endif

            #if WORLD_EXCEPTIONS
            if (entity.IsAlive() == false) {
                
                EmptyEntityException.Throw(entity);
                
            }
            #endif

            if (lifetime == ComponentLifetime.NotifyAllModules ||
                lifetime == ComponentLifetime.NotifyAllSystems) {

                if (this.HasData<TComponent>(in entity) == true) return;

                var task = PoolClass<StructComponentsContainer.NextFrameTask<TComponent>>.Spawn();
                task.world = this;
                task.entity = entity;
                task.data = data;

                if (lifetime == ComponentLifetime.NotifyAllModules) {

                    task.lifetime = ComponentLifetime.NotifyAllModulesBelow;

                    this.currentState.structComponents.nextFrameTasks.Add(task);

                } else if (lifetime == ComponentLifetime.NotifyAllSystems) {

                    task.lifetime = ComponentLifetime.NotifyAllSystemsBelow;

                    if (this.currentState.structComponents.nextTickTasks.Contains(task) == false) this.currentState.structComponents.nextTickTasks.Add(task);

                }

            } else {

                ref var state = ref this.SetData(in entity, in data);

                if (lifetime == ComponentLifetime.Infinite) return;
                state = (byte)(lifetime + 1);

                this.AddToLifetimeIndex<TComponent>(in entity);

            }

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public void RemoveData(in Entity entity) {

            #if WORLD_STATE_CHECK
            if (this.HasStep(WorldStep.LogicTick) == false && this.HasResetState() == true) {
                
                OutOfStateException.ThrowWorldStateCheck();
                
            }
            #endif

            #if WORLD_EXCEPTIONS
            if (entity.IsAlive() == false) {
                
                EmptyEntityException.Throw(entity);
                
            }
            #endif

            var changed = false;
            for (int i = 0; i < this.currentState.structComponents.list.Length; ++i) {

                var reg = this.currentState.structComponents.list.arr[i];
                if (reg != null && reg.Remove(in entity, false) == true) {

                    var bit = reg.GetTypeBit();
                    if (this.currentState.filters.allFiltersArchetype.HasBit(bit) == true) this.currentState.storage.archetypes.Remove(in entity, bit);
                    System.Threading.Interlocked.Decrement(ref this.currentState.structComponents.count);
                    changed = true;

                }

            }

            if (changed == true) {

                this.currentState.storage.versions.Increment(in entity);
                this.RemoveComponentFromFilter(in entity);

            }

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        public void RemoveData<TComponent>(in Entity entity) where TComponent : struct, IStructComponent {

            #if WORLD_STATE_CHECK
            if (this.HasStep(WorldStep.LogicTick) == false && this.HasResetState() == true) {
                
                OutOfStateException.ThrowWorldStateCheck();
                
            }
            #endif

            #if WORLD_EXCEPTIONS
            if (entity.IsAlive() == false) {
                
                EmptyEntityException.Throw(entity);
                
            }
            #endif

            var reg = (StructComponents<TComponent>)this.currentState.structComponents.list.arr[WorldUtilities.GetAllComponentTypeId<TComponent>()];
            ref var state = ref reg.componentsStates.arr[entity.id];
            if (state > 0) {

                state = 0;
                this.currentState.storage.versions.Increment(in entity);
                this.UpdateFilterByStructComponentVersioned<TComponent>(in entity);
                if (WorldUtilities.IsComponentAsTag<TComponent>() == false) reg.RemoveData(in entity);
                if (this.currentState.filters.HasInAnyFilter<TComponent>() == true) {

                    this.currentState.storage.archetypes.Remove<TComponent>(in entity);
                    this.UpdateFilterByStructComponent<TComponent>(in entity);

                }

                System.Threading.Interlocked.Decrement(ref this.currentState.structComponents.count);
                #if ENTITY_ACTIONS
                this.RaiseEntityActionOnRemove<TComponent>(in entity);
                #endif

            }

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        internal void SetData(in Entity entity, in IStructComponent data, int dataIndex, int componentIndex) {

            #if WORLD_STATE_CHECK
            if (this.HasStep(WorldStep.LogicTick) == false && this.HasResetState() == true) {

                OutOfStateException.ThrowWorldStateCheck();
                
            }
            #endif

            #if WORLD_EXCEPTIONS
            if (entity.IsAlive() == false) {
                
                EmptyEntityException.Throw(entity);
                
            }
            #endif

            // Inline all manually
            ref var reg = ref this.currentState.structComponents.list.arr[dataIndex];
            if (reg.SetObject(entity, data) == true) {

                this.currentState.storage.versions.Increment(in entity);
                if (this.currentState.filters.allFiltersArchetype.HasBit(componentIndex) == true) this.currentState.storage.archetypes.Set(in entity, componentIndex);
                System.Threading.Interlocked.Increment(ref this.currentState.structComponents.count);

            }

        }

        #if INLINE_METHODS
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        #endif
        internal void RemoveData(in Entity entity, int dataIndex, int componentIndex) {

            #if WORLD_STATE_CHECK
            if (this.HasStep(WorldStep.LogicTick) == false && this.HasResetState() == true) {
                
                OutOfStateException.ThrowWorldStateCheck();
                
            }
            #endif

            #if WORLD_EXCEPTIONS
            if (entity.IsAlive() == false) {
                
                EmptyEntityException.Throw(entity);
                
            }
            #endif

            var reg = this.currentState.structComponents.list.arr[dataIndex];
            if (reg.RemoveObject(entity) == true) {

                this.currentState.storage.versions.Increment(in entity);
                if (this.currentState.filters.allFiltersArchetype.HasBit(componentIndex) == true) this.currentState.storage.archetypes.Remove(in entity, componentIndex);
                System.Threading.Interlocked.Decrement(ref this.currentState.structComponents.count);

            }

        }

    }

}