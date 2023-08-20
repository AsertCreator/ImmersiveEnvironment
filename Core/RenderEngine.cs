using ImmersiveEnvironment.Tiles;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Color = Raylib_cs.Color;

namespace ImmersiveEnvironment.Core
{
    public static class RenderEngine
	{
		public static List<Chunk> ExistingChunks = new();
		public static List<Tile> RegisteredTiles = new();
		public static Camera3D Camera;
		public static int WindowSizeX = 1600;
		public static int WindowSizeY = 900;
		public static float AccelerationY = -0.01f;
		public static float VelocityY = 0.0f;
		public static Texture2D AtlasTexture;
		public static bool AbleToJump;
		public static bool Noclip;
		public unsafe static void InitializeContent()
		{
			for (int y = -2; y < 2; y++)
				for (int x = -2; x < 2; x++)
					ExistingChunks.Add(GenerateChunk(x, y, 0));

			RegisteredTiles.Add(new AirTile());
			RegisteredTiles.Add(new GrassTile());
			RegisteredTiles.Add(new StoneTile());
			RegisteredTiles.Add(new DirtTile());
			RegisteredTiles.Add(new WaterTile());

			var atlas = new Bitmap("atlas.png");

			AtlasTexture = new Texture2D()
			{
				id = LoadTexture(atlas),
				format = PixelFormat.PIXELFORMAT_UNCOMPRESSED_R8G8B8A8,
				height = atlas.Height,
				width = atlas.Width,
				mipmaps = 1
			};

			atlas.Dispose();
		}
		public static unsafe uint LoadTexture(Bitmap bmp)
		{
			byte* data = (byte*)Marshal.AllocHGlobal(bmp.Width * bmp.Height * 4).ToPointer();

			for (int y = 0; y < bmp.Height; y++)
			{
				for (int x = 0; x < bmp.Width; x++)
				{
					System.Drawing.Color col = bmp.GetPixel(x, y);
					data[(y * bmp.Width + x) * 4 + 0] = col.R;
					data[(y * bmp.Width + x) * 4 + 1] = col.G;
					data[(y * bmp.Width + x) * 4 + 2] = col.B;
					data[(y * bmp.Width + x) * 4 + 3] = col.A;
				}
			}

			uint res = Rlgl.rlLoadTexture(data, bmp.Width, bmp.Height, PixelFormat.PIXELFORMAT_UNCOMPRESSED_R8G8B8A8, 1);

			Marshal.FreeHGlobal(new IntPtr(data));

			return res;
		}
		public static void InitializeRender()
		{
			Camera = new Camera3D(
				new Vector3(0, 17, 0),
				new Vector3(10, 17, 0),
				new Vector3(0, 1, 0),
				90, CameraProjection.CAMERA_PERSPECTIVE);

			Raylib.SetTargetFPS(60);

			Raylib.InitWindow(WindowSizeX, WindowSizeY, "Immersive Environment");
			Raylib.DisableCursor();
		}
		public static Chunk? GetChunk(int px, int pz)
		{
			for (int i = 0; i < ExistingChunks.Count; i++)
			{
				var chunk = ExistingChunks[i];
				if (chunk.PositionX == px || chunk.PositionZ == pz) return chunk;
			}
			return null;
		}
		public static Chunk GenerateChunk(int px, int pz, int variant)
		{
			Chunk chunk = new Chunk();
			chunk.PositionX = px;
			chunk.PositionZ = pz;

			for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
			{
				for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
				{
					chunk.Data[x, 0, z] = 2;
					chunk.Data[x, 1, z] = 2;
					chunk.Data[x, 2, z] = 3;
					chunk.Data[x, 3, z] = 1;
				}
			}

			return chunk;
		}
		public static void RenderWorld()
		{
			Raylib.DrawCubeWires(Camera.target, 1, 1, 1, Color.GRAY);

			for (int i = 0; i < ExistingChunks.Count; i++)
			{
				var chunk = ExistingChunks[i];

				for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
				{
					for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
					{
						for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
						{
							var tileid = chunk.Data[x, y, z];
							var tile = RegisteredTiles[tileid];
							var pos = new Vector3(Chunk.CHUNK_SIZE * chunk.PositionX + x, y, Chunk.CHUNK_SIZE * chunk.PositionZ + z);

							if (!tile.IsTransparent)
							{
								if (!tile.SeeThrough)
								{
									if (RegisteredTiles[chunk.GetChunkTile(x, y + 1, z)].SeeThrough)
										DrawCubeTextureRec(AtlasTexture, new Raylib_cs.Rectangle(tile.TextureTop.X * 16, tile.TextureTop.Y * 16, 16, 16),
											pos, 1.0f, 1.0f, 1.0f, Color.WHITE, Faces.Top);

									if (RegisteredTiles[chunk.GetChunkTile(x, y - 1, z)].SeeThrough)
										DrawCubeTextureRec(AtlasTexture, new Raylib_cs.Rectangle(tile.TextureTop.X * 16, tile.TextureTop.Y * 16, 16, 16),
											pos, 1.0f, 1.0f, 1.0f, Color.GRAY, Faces.Bottom);

									if (RegisteredTiles[chunk.GetChunkTile(x, y, z + 1)].SeeThrough)
										DrawCubeTextureRec(AtlasTexture, new Raylib_cs.Rectangle(tile.TextureTop.X * 16, tile.TextureTop.Y * 16, 16, 16),
											pos, 1.0f, 1.0f, 1.0f, Color.LIGHTGRAY, Faces.Front);

									if (RegisteredTiles[chunk.GetChunkTile(x, y, z - 1)].SeeThrough)
										DrawCubeTextureRec(AtlasTexture, new Raylib_cs.Rectangle(tile.TextureTop.X * 16, tile.TextureTop.Y * 16, 16, 16),
											pos, 1.0f, 1.0f, 1.0f, Color.GRAY, Faces.Back);

									if (RegisteredTiles[chunk.GetChunkTile(x + 1, y, z)].SeeThrough)
										DrawCubeTextureRec(AtlasTexture, new Raylib_cs.Rectangle(tile.TextureTop.X * 16, tile.TextureTop.Y * 16, 16, 16),
											pos, 1.0f, 1.0f, 1.0f, Color.LIGHTGRAY, Faces.Right);

									if (RegisteredTiles[chunk.GetChunkTile(x - 1, y, z)].SeeThrough)
										DrawCubeTextureRec(AtlasTexture, new Raylib_cs.Rectangle(tile.TextureTop.X * 16, tile.TextureTop.Y * 16, 16, 16),
											pos, 1.0f, 1.0f, 1.0f, Color.LIGHTGRAY, Faces.Left);
								}
								else
								{
									if (RegisteredTiles[chunk.GetChunkTile(x, y + 1, z)].IsTransparent)
										DrawCubeTextureRec(AtlasTexture, new Raylib_cs.Rectangle(tile.TextureTop.X * 16, tile.TextureTop.Y * 16, 16, 16),
											pos, 1.0f, 1.0f, 1.0f, Color.WHITE, Faces.Top);

									if (RegisteredTiles[chunk.GetChunkTile(x, y - 1, z)].IsTransparent)
										DrawCubeTextureRec(AtlasTexture, new Raylib_cs.Rectangle(tile.TextureTop.X * 16, tile.TextureTop.Y * 16, 16, 16),
											pos, 1.0f, 1.0f, 1.0f, Color.GRAY, Faces.Bottom);

									if (RegisteredTiles[chunk.GetChunkTile(x, y, z + 1)].IsTransparent)
										DrawCubeTextureRec(AtlasTexture, new Raylib_cs.Rectangle(tile.TextureTop.X * 16, tile.TextureTop.Y * 16, 16, 16),
											pos, 1.0f, 1.0f, 1.0f, Color.LIGHTGRAY, Faces.Front);

									if (RegisteredTiles[chunk.GetChunkTile(x, y, z - 1)].IsTransparent)
										DrawCubeTextureRec(AtlasTexture, new Raylib_cs.Rectangle(tile.TextureTop.X * 16, tile.TextureTop.Y * 16, 16, 16),
											pos, 1.0f, 1.0f, 1.0f, Color.GRAY, Faces.Back);

									if (RegisteredTiles[chunk.GetChunkTile(x + 1, y, z)].IsTransparent)
										DrawCubeTextureRec(AtlasTexture, new Raylib_cs.Rectangle(tile.TextureTop.X * 16, tile.TextureTop.Y * 16, 16, 16),
											pos, 1.0f, 1.0f, 1.0f, Color.LIGHTGRAY, Faces.Right);

									if (RegisteredTiles[chunk.GetChunkTile(x - 1, y, z)].IsTransparent)
										DrawCubeTextureRec(AtlasTexture, new Raylib_cs.Rectangle(tile.TextureTop.X * 16, tile.TextureTop.Y * 16, 16, 16),
											pos, 1.0f, 1.0f, 1.0f, Color.LIGHTGRAY, Faces.Left);
								}
							}
						}
					}
				}
			}
		}
		public static void RenderOverlay()
		{
			Raylib.DrawText("+", WindowSizeX / 2 - 5, WindowSizeY / 2 - 9, 20, Color.WHITE);

			Raylib.DrawFPS(20, 20);
			Raylib.DrawText($"Velocity Y: {VelocityY}", 20, 45, 20, Color.WHITE);
			Raylib.DrawText($"Position X: {Camera.position.X}", 20, 70, 20, Color.WHITE);
			Raylib.DrawText($"Position Y: {Camera.position.Y}", 20, 95, 20, Color.WHITE);
			Raylib.DrawText($"Position Z: {Camera.position.Z}", 20, 120, 20, Color.WHITE);
		}
		public static void RenderLoop()
		{
			while (!Raylib.WindowShouldClose())
			{
				Raylib.ClearBackground(Color.SKYBLUE);

				Raylib.UpdateCamera(ref Camera, CameraMode.CAMERA_FIRST_PERSON);

				Raylib.BeginDrawing();

				Raylib.BeginMode3D(Camera);
				RenderWorld();
				Raylib.EndMode3D();

				RenderOverlay();
				Raylib.EndDrawing();

				if (Raylib.IsKeyDown(KeyboardKey.KEY_V))
				{
					Noclip = !Noclip;
				}
				if (Raylib.IsKeyDown(KeyboardKey.KEY_R))
				{
					Camera.position = new Vector3(0, 17, 0);
					Camera.target = new Vector3(10, 17, 0);
				}

				if (Raylib.IsKeyDown(KeyboardKey.KEY_SPACE) && AbleToJump && !Noclip)
				{
					Camera.position.Y += 0.1f;
					Camera.target.Y += 0.1f;
					VelocityY = 0.25f;
				}

				if (Raylib.IsKeyDown(KeyboardKey.KEY_SPACE) && Noclip)
				{
					Camera.position.Y += 0.2f;
					Camera.target.Y += 0.2f;
				}
				if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT) && Noclip)
				{
					Camera.position.Y -= 0.2f;
					Camera.target.Y -= 0.2f;
				}

				if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
				{
					SetTile((int)Camera.target.X, (int)Camera.target.Y, (int)Camera.target.Z, 2);
				}
				if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
				{
					SetTile((int)Camera.target.X, (int)Camera.target.Y, (int)Camera.target.Z, 0);
				}

				VelocityY += AccelerationY;
				var hy = GetHighestCollidableTile((int)MathF.Round(Camera.position.X), (int)MathF.Round(Camera.position.Y), (int)MathF.Round(Camera.position.Z));
				if ((Camera.position.Y + VelocityY > hy + 2 || hy == -1) && !Noclip)
				{
					Camera.position.Y += VelocityY;
					Camera.target.Y += VelocityY;
					AbleToJump = false;
				}
				else
				{
					VelocityY = 0;
					AbleToJump = true;
				}
			}

			Raylib.CloseWindow();

			Raylib.UnloadTexture(AtlasTexture);
		}
		public static int GetHighestCollidableTile(int px, int maxy, int pz)
		{
			int ax = Math.Abs(px % Chunk.CHUNK_SIZE);
			int az = Math.Abs(pz % Chunk.CHUNK_SIZE);
			int cx = px / Chunk.CHUNK_SIZE;
			int cz = pz / Chunk.CHUNK_SIZE;

			for (int i = 0; i < ExistingChunks.Count; i++)
			{
				var chunk = ExistingChunks[i];

				if (chunk.PositionX == cx && chunk.PositionZ == cz)
				{
					for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
					{
						for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
						{
							for (int y = Math.Min(maxy, Chunk.CHUNK_HEIGHT - 1); y > 0; y--)
							{
								Tile tile = RegisteredTiles[chunk.Data[ax, y, az]];
								if (tile.IsCollidable) return y;
							}
						}
					}
				}
			}

			return 0;
		}
		public static void SetTile(int px, int py, int pz, int tile)
		{
			for (int i = 0; i < ExistingChunks.Count; i++)
			{
				var chunk = ExistingChunks[i];

				for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
				{
					for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
					{
						for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
						{
							var pos = new Vector3(Chunk.CHUNK_SIZE * chunk.PositionX + x, y, Chunk.CHUNK_SIZE * chunk.PositionZ + z);

							if (pos.X == px && pos.Y == py && pos.Z == pz) chunk.Data[x, y, z] = tile;
						}
					}
				}
			}
		}
		public static int GetTile(int px, int py, int pz)
		{
			for (int i = 0; i < ExistingChunks.Count; i++)
			{
				var chunk = ExistingChunks[i];

				for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
				{
					for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
					{
						for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
						{
							var tileid = chunk.Data[x, y, z];
							var pos = new Vector3(Chunk.CHUNK_SIZE * chunk.PositionX + x, y, Chunk.CHUNK_SIZE * chunk.PositionZ + z);

							if (pos.X == px && pos.Y == py && pos.Z == pz) return tileid;
						}
					}
				}
			}
			return 0;
		}
		public static void DrawCubeTextureRec(Texture2D texture, Raylib_cs.Rectangle source, Vector3 position, float width, float height, float length, Color color, Faces f)
        {
			if (f != 0)
			{
				float x = position.X;
				float y = position.Y;
				float z = position.Z;
				float texWidth = texture.width;
				float texHeight = texture.height;

				// Set desired texture to be enabled while drawing following vertex data
				Rlgl.rlSetTexture(texture.id);

				// We calculate the normalized texture coordinates for the desired texture-source-rectangle
				// It means converting from (tex.width, tex.height) coordinates to [0.0f, 1.0f] equivalent 
				Rlgl.rlBegin(7);
				Rlgl.rlColor4ub(color.r, color.g, color.b, color.a);

				if (f.HasFlag(Faces.Front))
				{
					// Front face
					Rlgl.rlNormal3f(0.0f, 0.0f, 1.0f);
					Rlgl.rlTexCoord2f(source.x / texWidth, (source.y + source.height) / texHeight);
					Rlgl.rlVertex3f(x - width / 2, y - height / 2, z + length / 2);
					Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, (source.y + source.height) / texHeight);
					Rlgl.rlVertex3f(x + width / 2, y - height / 2, z + length / 2);
					Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, source.y / texHeight);
					Rlgl.rlVertex3f(x + width / 2, y + height / 2, z + length / 2);
					Rlgl.rlTexCoord2f(source.x / texWidth, source.y / texHeight);
					Rlgl.rlVertex3f(x - width / 2, y + height / 2, z + length / 2);
				}

				if (f.HasFlag(Faces.Back))
				{
					// Back face
					Rlgl.rlNormal3f(0.0f, 0.0f, -1.0f);
					Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, (source.y + source.height) / texHeight);
					Rlgl.rlVertex3f(x - width / 2, y - height / 2, z - length / 2);
					Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, source.y / texHeight);
					Rlgl.rlVertex3f(x - width / 2, y + height / 2, z - length / 2);
					Rlgl.rlTexCoord2f(source.x / texWidth, source.y / texHeight);
					Rlgl.rlVertex3f(x + width / 2, y + height / 2, z - length / 2);
					Rlgl.rlTexCoord2f(source.x / texWidth, (source.y + source.height) / texHeight);
					Rlgl.rlVertex3f(x + width / 2, y - height / 2, z - length / 2);
				}

				if (f.HasFlag(Faces.Top))
				{
					// Top face
					Rlgl.rlNormal3f(0.0f, 1.0f, 0.0f);
					Rlgl.rlTexCoord2f(source.x / texWidth, source.y / texHeight);
					Rlgl.rlVertex3f(x - width / 2, y + height / 2, z - length / 2);
					Rlgl.rlTexCoord2f(source.x / texWidth, (source.y + source.height) / texHeight);
					Rlgl.rlVertex3f(x - width / 2, y + height / 2, z + length / 2);
					Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, (source.y + source.height) / texHeight);
					Rlgl.rlVertex3f(x + width / 2, y + height / 2, z + length / 2);
					Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, source.y / texHeight);
					Rlgl.rlVertex3f(x + width / 2, y + height / 2, z - length / 2);
				}

				if (f.HasFlag(Faces.Bottom))
				{
					// Bottom face
					Rlgl.rlNormal3f(0.0f, -1.0f, 0.0f);
					Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, source.y / texHeight);
					Rlgl.rlVertex3f(x - width / 2, y - height / 2, z - length / 2);
					Rlgl.rlTexCoord2f(source.x / texWidth, source.y / texHeight);
					Rlgl.rlVertex3f(x + width / 2, y - height / 2, z - length / 2);
					Rlgl.rlTexCoord2f(source.x / texWidth, (source.y + source.height) / texHeight);
					Rlgl.rlVertex3f(x + width / 2, y - height / 2, z + length / 2);
					Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, (source.y + source.height) / texHeight);
					Rlgl.rlVertex3f(x - width / 2, y - height / 2, z + length / 2);
				}

				if (f.HasFlag(Faces.Right))
				{
					// Right face
					Rlgl.rlNormal3f(1.0f, 0.0f, 0.0f);
					Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, (source.y + source.height) / texHeight);
					Rlgl.rlVertex3f(x + width / 2, y - height / 2, z - length / 2);
					Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, source.y / texHeight);
					Rlgl.rlVertex3f(x + width / 2, y + height / 2, z - length / 2);
					Rlgl.rlTexCoord2f(source.x / texWidth, source.y / texHeight);
					Rlgl.rlVertex3f(x + width / 2, y + height / 2, z + length / 2);
					Rlgl.rlTexCoord2f(source.x / texWidth, (source.y + source.height) / texHeight);
					Rlgl.rlVertex3f(x + width / 2, y - height / 2, z + length / 2);
				}

				if (f.HasFlag(Faces.Left))
				{
					// Left face
					Rlgl.rlNormal3f(-1.0f, 0.0f, 0.0f);
					Rlgl.rlTexCoord2f(source.x / texWidth, (source.y + source.height) / texHeight);
					Rlgl.rlVertex3f(x - width / 2, y - height / 2, z - length / 2);
					Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, (source.y + source.height) / texHeight);
					Rlgl.rlVertex3f(x - width / 2, y - height / 2, z + length / 2);
					Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, source.y / texHeight);
					Rlgl.rlVertex3f(x - width / 2, y + height / 2, z + length / 2);
					Rlgl.rlTexCoord2f(source.x / texWidth, source.y / texHeight);
					Rlgl.rlVertex3f(x - width / 2, y + height / 2, z - length / 2);
				}

				Rlgl.rlEnd();

				Rlgl.rlSetTexture(0);
			}
        }
    }
	[Flags]
	public enum Faces
	{
		Left = 1, Right = 2, Front = 4, Top = 8, Bottom = 16, Back = 32, All = Left | Right | Front | Top | Bottom | Back
	}
}
