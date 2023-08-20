using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ImmersiveEnvironment.Tiles
{
    public class AirTile : Tile
    {
        public override uint ID => 0;
        public override Vector2 TextureTop => new Vector2(0, 0);
        public override Vector2 TextureSide => new Vector2(0, 0);
        public override bool IsCollidable => false;
        public override bool IsTransparent => true;
    }
}
