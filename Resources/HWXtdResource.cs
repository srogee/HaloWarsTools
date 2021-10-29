using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Numerics;

namespace HaloWarsTools
{
    public class HWXtdResource : HWBinaryResource
    {
        public GenericMesh Mesh => ValueCache.Get(ImportMesh);
        public Bitmap AmbientOcclusionTexture => ValueCache.Get(() => ExtractEmbeddedDXT5A(GetFirstChunkOfType(HWBinaryResourceChunkType.XTD_AOChunk)));
        public Bitmap OpacityTexture => ValueCache.Get(() => ExtractEmbeddedDXT5A(GetFirstChunkOfType(HWBinaryResourceChunkType.XTD_AlphaChunk)));

        public static new HWXtdResource FromFile(HWContext context, string filename) {
            return GetOrCreateFromFile(context, filename) as HWXtdResource;
        }

        private Bitmap ExtractEmbeddedDXT5A(HWBinaryResourceChunk chunk) {
            // Get raw embedded DXT5 texture from resource file
            int size = (int)chunk.Size;

            byte[] data = new byte[size];
            Buffer.BlockCopy(RawBytes, (int)chunk.Offset, data, 0, size);
            byte[] dxt5CompressedData = new byte[data.Length * 2];
            byte[] blankSections = new byte[] { 0, 0, 0, 0 };
            for (int i = 0; i < data.Length; i += 4) {
                Buffer.BlockCopy(data, i, dxt5CompressedData, i * 2, 4);
                Buffer.BlockCopy(blankSections, 0, dxt5CompressedData, i * 2 + 4, 4);
            }

            int width = (int)Math.Sqrt(data.Length * 2);
            int height = width;

            //Decompress DXT5 texture and turn it into a Bitmap
            byte[] uncompressedColorData = new byte[width * height * 4];
            Dxt.DxtDecoder.DecompressDXT5(dxt5CompressedData, width, height, uncompressedColorData);
            GCHandle m_bitsHandle = GCHandle.Alloc(uncompressedColorData, GCHandleType.Pinned);
            Bitmap bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, m_bitsHandle.AddrOfPinnedObject());
            Bitmap finalBitmap = new Bitmap(width / 4, height / 4);

            for (int x = 0; x < width; x += 4) {
                for (int y = 0; y < height; y += 4) {
                    // There's still some garbage in the resulting bitmap, so try to get rid of it by using the average pixel value
                    float average = 0;
                    for (int x1 = 0; x1 < 4; x1++) {
                        for (int y1 = 0; y1 < 4; y1++) {
                            average += bitmap.GetPixel(x + x1, y + y1).A;
                        }
                    }
                    average /= 16;

                    float newAverage = 0;
                    int sampleCount = 0;
                    int threshold = 100;
                    for (int x1 = 0; x1 < 4; x1++) {
                        for (int y1 = 0; y1 < 4; y1++) {
                            byte sample = bitmap.GetPixel(x + x1, y + y1).A;
                            if (Math.Abs(sample - average) < threshold) {
                                newAverage += sample;
                                sampleCount++;
                            }
                        }
                    }

                    byte value = (byte)Math.Round(average);

                    if (sampleCount > 0) {
                        newAverage /= sampleCount;
                        value = (byte)Math.Round(newAverage);
                    }

                    finalBitmap.SetPixel(x / 4, y / 4, Color.FromArgb(255, value, value, value));
                }
            }

            m_bitsHandle.Free();
            finalBitmap.RotateFlip(RotateFlipType.Rotate90FlipX); // Matches albedo texture

            return finalBitmap;
        }

        private GenericMesh ImportMesh() {
            int stride = 1;
            MeshNormalExportMode shadingMode = MeshNormalExportMode.Unchanged;

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
