using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ImmersiveEnvironment.Tiles
{
    public abstract class Tile
    {
        public abstract uint ID { get; }
        public abstract Vector2 TextureTop { get; }
        public abstract Vector2 TextureSide { get; }
        public abstract bool IsCollidable { get; }
        public abstract bool IsTransparent { get; }
        public virtual bool SeeThrough => IsTransparent;
	}
}
