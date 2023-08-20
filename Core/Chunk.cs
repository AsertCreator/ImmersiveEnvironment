using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImmersiveEnvironment.Core
{
    public class Chunk
	{
		public const int CHUNK_SIZE = 16;
		public const int CHUNK_HEIGHT = 32;
		public int PositionX;
        public int PositionZ;
        public int[,,] Data = new int[CHUNK_SIZE, CHUNK_HEIGHT, CHUNK_SIZE];
		public int GetChunkTile(int x, int y, int z)
		{
			if (y < 0 || y >= CHUNK_HEIGHT) return 0;
			if (x < 0 || x >= CHUNK_SIZE) return 0;
			if (z < 0 || z >= CHUNK_SIZE) return 0;
			return Data[x, y, z];
		}
	}
}
