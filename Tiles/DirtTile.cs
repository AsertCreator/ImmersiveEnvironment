﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ImmersiveEnvironment.Tiles
{
    public class DirtTile : Tile
    {
        public override uint ID => 3;
        public override Vector2 TextureTop => new Vector2(2, 0);
        public override Vector2 TextureSide => new Vector2(2, 0);
        public override bool IsCollidable => true;
        public override bool IsTransparent => false;
    }
}
