
using GlmSharp;
using panpan.Scene;
using SDL3;

namespace panpan.Collision
{
    public class CollisionManager: Component
    {
        public enum ManagerType
        {
            SPACIAL_HASH,
            QUADTREE,
            INVALID
        }

        private List<Collider> colliders;
        private SpatialHash spatialHash;
        private Quadtree quadtree;

        private ManagerType type;
        private vec2 worldSize;
        private int cellSize;

        public CollisionManager(ManagerType type, int cellSize, vec2 worldSize)
        {
            this.type = type;
            this.cellSize = cellSize;
            this.worldSize = worldSize;
        }

        public void AddCollider(Collider col)
        {
            colliders.Add(col);
        }

        //----------------------------
        //------------------ Quad Tree
        //----------------------------
        private class Quadtree
        {

        }

        //----------------------------
        //--------------- Spatial Hash
        //----------------------------
        private class SpatialHash
        {
            private Dictionary<int, Collider> colliders;
            private int cellSize;
            private vec2 worldSize;

            public SpatialHash(int cellSize, vec2 worldSize)
            {
                this.cellSize = cellSize;
                this.worldSize = worldSize;
            }
            public void AddCollider(Collider col)
            {
                colliders.Add(CalculateHash(col),col);
            }

            private int CalculateHash(Collider col)
            {
                vec2 c = col.CenterPoint();
                c.x = CellCoord(c.x);
                c.y = CellCoord(c.y);
                int h = (int)Math.Pow(c.x * 92837111, c.y * 689287499);
                return h % cellSize;
            }
            private int CellCoord(float coord)
            {
                return (int)(coord / cellSize);
            }
        }
    }
}