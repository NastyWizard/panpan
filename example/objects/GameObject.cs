
using panpan.Scene;

namespace panpanExample
{
    public class GameObject : Entity
    {
        public TileMap? activeRoom;

        public GameObject(int x, int y)
        {
            Position.x = x;
            Position.y = y;
        }
    }
}