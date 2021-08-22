
namespace ME.ECS {

    public partial class World {

        private IPoolImplementation prevPools;
        private IPoolImplementation currentPools;

        partial void InitializePools() {

            this.prevPools = Pools.current;
            this.currentPools = new PoolImplementation(isNull: false);
            Pools.current = this.currentPools;
            
        }

        partial void DeInitializePools() {

            this.currentPools.Clear();
            Pools.current = this.prevPools;

        }

    }

}