
using System;
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
        private readonly SpatialHash spatialHash;

        private ManagerType type;
        private vec2 worldSize;
        private int cellSize;

        public bool ShowColliderDebug;

        public CollisionManager(ManagerType type, int cellSize, vec2 worldSize)
        {
            this.type = type;
            this.cellSize = cellSize;
            this.worldSize = worldSize;
            if (type != ManagerType.SPACIAL_HASH)
            {
                throw new NotSupportedException($"{nameof(CollisionManager)} currently only supports {ManagerType.SPACIAL_HASH}.");
            }

            spatialHash = new SpatialHash(cellSize, worldSize);
        }

        public void AddCollider(Collider col)
        {
            spatialHash.AddCollider(col, col.Parent.GetType());
        }

        public bool IntersectsWith(Collider self, Type other, vec2? pos = null)
        {
            return spatialHash.IntersectsWith(self, other, pos);
        }

        //--------------- Spatial Hash
        //----------------------------
        private class SpatialHash
        {
            private Dictionary<int, Collider> colliders;
            private Dictionary<Type, Dictionary<int, Collider>> colliderTypes;
            private int cellSize;
            private vec2 worldSize;

            public SpatialHash(int cellSize, vec2 worldSize)
            {
                this.cellSize = cellSize;
                this.worldSize = worldSize;
                colliders = new Dictionary<int, Collider>();
                colliderTypes = new Dictionary<Type, Dictionary<int, Collider>>();
            }
            public void AddCollider(Collider col, Type parentType)
            {
                col.Update();
                colliders.Add(CalculateHash(col), col);
                var key = parentType;
                if (!colliderTypes.ContainsKey(key))
                {
                    colliderTypes.Add(key, new Dictionary<int, Collider>());
                }
                colliderTypes[key].Add(CalculateHash(col), col);
            }

            public bool IntersectsWith(Collider self, Type other, vec2? pos = null)
            {
                if (!colliderTypes.TryGetValue(other, out var otherColliders))
                {
                    return false;
                }

                foreach (var key in otherColliders.Keys)
                {
                    if (self.Intersects(otherColliders[key], pos))
                    {
                        return true;
                    }
                }
                return false;
            }

            private int CalculateHash(Collider col)
            {
                vec2 c = col.CenterPoint();
                int cellX = (int)Math.Floor(c.x / cellSize);
                int cellY = (int)Math.Floor(c.y / cellSize);

                // Encode negatives distinctly
                uint ux = (uint)(cellX >= 0 ? cellX * 2 : (-cellX * 2 - 1));
                uint uy = (uint)(cellY >= 0 ? cellY * 2 : (-cellY * 2 - 1));

                // Large primes
                const uint p1 = 73856093;
                const uint p2 = 19349663;

                // Mix and clamp to positive
                uint h = (ux * p1) ^ (uy * p2);
                return (int)(h & 0x7FFFFFFF);
            }
            private int CellCoord(float coord)
            {
                return (int)(coord / cellSize);
            }

        }
    }
}