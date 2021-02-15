#if UNITY_MATHEMATICS
using RandomState = System.UInt32;
#else
using RandomState = UnityEngine.Random.State;
#endif

namespace ME.ECS {

    public abstract class State : IPoolableRecycle {

        [ME.ECS.Serializer.SerializeField]
        public Tick tick;
        [ME.ECS.Serializer.SerializeField]
        public RandomState randomState;

        // [ME.ECS.Serializer.SerializeField]
        public FiltersStorage filters;
        [ME.ECS.Serializer.SerializeField]
        internal StructComponentsContainer structComponents;
        [ME.ECS.Serializer.SerializeField]
        internal Storage storage;
        
        /// <summary>
        /// Return most unique hash
        /// </summary>
        /// <returns></returns>
        public virtual int GetHash() {

            return this.tick ^ this.structComponents.count ^ this.randomState.GetHashCode() ^ this.storage.GetHashCode();//^ this.structComponents.GetCustomHash();

        }

        public virtual void Initialize(World world, bool freeze, bool restore) {
            
            world.Register(ref this.filters, freeze, restore);
            world.Register(ref this.structComponents, freeze, restore);
            world.Register(ref this.storage, freeze, restore);

        }

        public virtual void CopyFrom(State other) {
            
            this.tick = other.tick;
            this.randomState = other.randomState;

            this.filters.CopyFrom(other.filters);
            this.structComponents.CopyFrom(other.structComponents);
            this.storage.CopyFrom(other.storage);

        }

        public virtual void OnRecycle() {
            
            WorldUtilities.Release(ref this.filters);
            WorldUtilities.Release(ref this.structComponents);
            WorldUtilities.Release(ref this.storage);

        }

        public virtual byte[] Serialize<T>() where T : State {
         
            var serializers = ME.ECS.Serializer.ECSSerializers.GetSerializers();
            serializers.Add(new ME.ECS.Serializer.BufferArraySerializer());
            return ME.ECS.Serializer.Serializer.Pack((T)this, serializers);
            
        }

        public virtual void Deserialize<T>(byte[] bytes) where T : State {
            
            var serializers = ME.ECS.Serializer.ECSSerializers.GetSerializers();
            serializers.Add(new ME.ECS.Serializer.BufferArraySerializer());
            ME.ECS.Serializer.Serializer.Unpack(bytes, serializers, (T)this);
            
        }

    }

}