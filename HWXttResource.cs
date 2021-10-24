using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;

namespace HaloWarsTools
{
    public class HWXttResource : HWBinaryResource
    {
        public Bitmap AlbedoTexture => ValueCache.Get(() => ExtractEmbeddedDXT1(GetFirstChunkOfType(HWBinaryResourceChunkType.XTT_AtlasChunkAlbedo)));
        public GenericMesh Mesh => ValueCache.Get(ImportMesh);

        public HWXttResource(string filename) : base(filename) {
            Type = HWResourceType.Xtt;
        }

        private Bitmap ExtractEmbeddedDXT1(HWBinaryResourceChunk chunk) {
            // Get raw embedded DXT1 texture from resource file
            int size = BinaryUtils.ReadInt32BigEndian(RawBytes, (int)chunk.Offset);
            byte[] dxt1CompressedAlbedo = new byte[size];
            Buffer.BlockCopy(RawBytes, (int)chunk.Offset + 16, dxt1CompressedAlbedo, 0, size);

            // Decompress DXT1 texture and turn it into a Bitmap
            int width = BinaryUtils.ReadInt32BigEndian(RawBytes, (int)chunk.Offset + 4);
            int height = BinaryUtils.ReadInt32BigEndian(RawBytes, (int)chunk.Offset + 8);
            byte[] uncompressedColorData = new byte[width * height * 4];
            Dxt.DxtDecoder.DecompressDXT1(dxt1CompressedAlbedo, width, height, uncompressedColorData);
            GCHandle m_bitsHandle = GCHandle.Alloc(uncompressedColorData, GCHandleType.Pinned);
            Bitmap bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, m_bitsHandle.AddrOfPinnedObject());
            return bitmap;
        }

        private GenericMesh ImportMesh() {
            int stride = 1;
            MeshNormalExportMode shadingMode = MeshNormalExportMode.CalculateNormalsSmoothShaded;

            HWBinaryResourceChunk headerChunk = GetFirstChunkOfType(HWBinaryResourceChunkType.XTD_XTDHeader);
            float tileScale = BinaryUtils.ReadFloatBigEndian(RawBytes, (int)headerChunk.Offset + 12);
            HWBinaryResourceChunk atlasChunk = GetFirstChunkOfType(HWBinaryResourceChunkType.XTD_AtlasChunk);

            int gridSize = (int)Math.Round(Math.Sqrt((atlasChunk.Size - 32) / 8)); // Subtract the min/range vector sizes, divide by position + normal size, and sqrt for grid size
            int positionOffset = (int)atlasChunk.Offset + 32;
            int normalOffset = positionOffset + gridSize * gridSize * 4;

            if (gridSize % stride != 0) {
                throw new Exception($"Grid size {gridSize} is not evenly divisible by stride {stride} - choose a different stride value.");
            }

            GenericMesh mesh = new GenericMesh(new MeshExportOptions(Matrix4x4.Identity, shadingMode, false, false));
            GenericMaterial material = new GenericMaterial("mat");
            GenericMeshSection section = new GenericMeshSection("terrain");

            // These are stored as ZYX, 4 bytes per component
            Vector3 PosCompMin = BinaryUtils.ReadVector3BigEndian(RawBytes, (int)atlasChunk.Offset).ReverseComponents();
            Vector3 PosCompRange = BinaryUtils.ReadVector3BigEndian(RawBytes, (int)atlasChunk.Offset + 16).ReverseComponents();

            // Read vertex offsets/normals and add them to the mesh
            for (int x = 0; x < gridSize; x += stride) {
                for (int z = 0; z < gridSize; z += stride) {
                    int index = ConvertGridPositionToIndex(new Tuple<int, int>(x, z), gridSize);
                    int offset = index * 4;

                    // Get offset position and normal for this vertex
                    Vector3 position = ReadVector3Compressed(positionOffset + offset) * PosCompRange - PosCompMin;

                    // Positions are relative to the terrain grid, so shift them by the grid position
                    position += new Vector3(x, 0, z) * tileScale;
                    mesh.Vertices.Add(ConvertToUE4PositionVectorTerrain(position));

                    Vector3 normal = ConvertToUE4DirectionVector(Vector3.Normalize(ReadVector3Compressed(normalOffset + offset) * 2.0f - Vector3.One));
                    mesh.Normals.Add(normal);

                    // Simple UV based on original, non-warped terrain grid
                    Vector3 texCoord = new Vector3(x / ((float)gridSize - 1), 1 - (z / ((float)gridSize - 1)), 0);
                    mesh.TexCoords.Add(texCoord);
                }
            }

            // Generate faces based on terrain grid
            for (int x = 0; x < gridSize - stride; x += stride) {
                for (int z = 0; z < gridSize - stride; z += stride) {
                    int a = GetVertexID(x, z, gridSize, stride);
                    int b = GetVertexID(x + stride, z, gridSize, stride);
                    int c = GetVertexID(x + stride, z + stride, gridSize, stride);
                    int d = GetVertexID(x, z + stride, gridSize, stride);

                    mesh.Faces.Add(GenericFace.ReverseWinding(new GenericFace(a, c, b, material, section)));
                    mesh.Faces.Add(GenericFace.ReverseWinding(new GenericFace(a, d, c, material, section)));
                }
            }

            return mesh;
        }

        private static int GetVertexID(int x, int z, int gridSize, int stride) {
            return ConvertGridPositionToIndex(new Tuple<int, int>(x / stride, z / stride), gridSize / stride);
        }

        private static Tuple<int, int> ConvertIndexToGridPosition(int index, int gridSize) {
            return new Tuple<int, int>(index % gridSize, index / gridSize);
        }

        private static int ConvertGridPositionToIndex(Tuple<int, int> gridPosition, int gridSize) {
            return gridPosition.Item2 * gridSize + gridPosition.Item1;
        }

        private Vector3 ReadVector3Compressed(int offset) {
            // Inexplicably, position and normal vectors are encoded inside 4 bytes. ~10 bits per component
            // This seems okay for directions, but positions suffer from stairstepping artifacts
            uint kBitMask10 = (1 << 10) - 1;
            uint v = BinaryUtils.ReadUInt32LittleEndian(RawBytes, offset);
            uint x = (v >> 0) & kBitMask10;
            uint y = (v >> 10) & kBitMask10;
            uint z = (v >> 20) & kBitMask10;
            return new Vector3(x, y, z) / kBitMask10;
        }

        private static Vector3 ConvertToUE4PositionVector(Vector3 vector) {
            return new Vector3(vector.Z, vector.X, vector.Y) * 100;
        }

        private static Vector3 ConvertToUE4PositionVectorTerrain(Vector3 vector) {
            return new Vector3(vector.X, -vector.Z, vector.Y) * 100;
        }

        private static Vector3 ConvertToUE4DirectionVector(Vector3 vector) {
            return new Vector3(vector.Z, vector.X, vector.Y);
        }
    }
}
