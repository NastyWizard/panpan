
using panpan.Scene;

namespace panpanExample
{
    public class ExampleGameObject : Entity
    {
        public ExampleTileMap? activeRoom;

        public ExampleGameObject(int x, int y)
        {
            Position.x = x;
            Position.y = y;
        }
    }
}