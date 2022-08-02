namespace ME.ECS.Collections.MemoryAllocator {

    using Collections.V3;
    using Unity.Collections.LowLevel.Unsafe;

    public unsafe struct SparseSetData {

        public void* densePtr;
        public MemArrayAllocator<int> sparse;

        public ref T Get<T>(MemoryAllocator allocator, int index) where T : struct {

            var idx = this.sparse[in allocator, index];
            if (idx == 0) {
                throw new OutOfBoundsException();
            }
            return ref UnsafeUtility.ArrayElementAsRef<T>(this.densePtr, idx);
            
        }

        public ref T Get<T>(in MemoryAllocator allocator, int index) where T : struct {

            var idx = this.sparse[in allocator, index];
            if (idx == 0) {
                throw new OutOfBoundsException();
            }
            return ref UnsafeUtility.ArrayElementAsRef<T>(this.densePtr, idx);
            
        }

    }
    
    public struct SparseSet<T> where T : struct {

        [ME.ECS.Serializer.SerializeField]
        private MemArraySlicedAllocator<T> dense;
        [ME.ECS.Serializer.SerializeField]
        private MemArrayAllocator<int> sparse;
        [ME.ECS.Serializer.SerializeField]
        private Stack<int> freeIndexes;

        public bool isCreated;
        public int Length => this.sparse.Length;
        
        public SparseSet(ref MemoryAllocator allocator, int length) {

            this.isCreated = true;
            this.dense = new MemArraySlicedAllocator<T>(ref allocator, length);
            this.sparse = default;
            this.freeIndexes = default;
            this.Validate(ref allocator, length);

        }

        public unsafe SparseSetData GetData(in MemoryAllocator allocator) {

            return new SparseSetData() {
                densePtr = this.dense.GetUnsafePtr(in allocator),
                sparse = this.sparse,
            };

        }

        public T ReadDense(in MemoryAllocator allocator, int sparseIndex) {

            return this.dense[in allocator, sparseIndex];

        }
        
        public ref T GetDense(in MemoryAllocator allocator, int sparseIndex) {

            return ref this.dense[in allocator, sparseIndex];

        }

        public MemArrayAllocator<int> GetSparse() {

            return this.sparse;

        }

        public SparseSet<T> Merge(ref MemoryAllocator allocator) {

            this.dense = this.dense.Merge(ref allocator);
            return this;

        }
        
        public void Validate(ref MemoryAllocator allocator, int capacity) {

            if (this.freeIndexes.isCreated == false) this.freeIndexes = new Stack<int>(ref allocator, 10);
            this.sparse.Resize(ref allocator, capacity);

        }

        public SparseSet<T> Dispose(ref MemoryAllocator allocator) {
            
            this.freeIndexes.Dispose(ref allocator);
            this.sparse.Dispose(ref allocator);
            this.dense.Dispose(ref allocator);
            return this;

        }

        public void Set(ref MemoryAllocator allocator, int fromEntityId, int toEntityId, in T data) {

            for (int i = fromEntityId; i <= toEntityId; ++i) {
                this.Set(ref allocator, i, in data);
            }
            
        }

        public int Set(ref MemoryAllocator allocator, int entityId, in T data) {

            ref var idx = ref this.sparse[in allocator, entityId];
            if (idx == 0) {
                if (this.freeIndexes.Count(in allocator) > 0) {
                    idx = this.freeIndexes.Pop(in allocator);
                } else {
                    idx = this.dense.Length + 1;
                }
            }

            this.dense.Resize(ref allocator, idx + 1, out _);
            this.dense[in allocator, idx] = data;
            return idx;

        }

        public ref T Get(ref MemoryAllocator allocator, int entityId) {
            
            var idx = this.sparse[in allocator, entityId];
            if (idx == 0) idx = this.Set(ref allocator, entityId, default);
            return ref this.dense[in allocator, idx];

        }

        public void Remove(ref MemoryAllocator allocator, int entityId) {
            
            ref var idx = ref this.sparse[in allocator, entityId];
            this.dense[in allocator, idx] = default;
            this.freeIndexes.Push(ref allocator, idx);
            idx = 0;
            
        }

        public void Remove(ref MemoryAllocator allocator, int entityId, int length) {

            for (int i = entityId; i < length; ++i) {
                this.Remove(ref allocator, i);
            }
            
        }

    }

}