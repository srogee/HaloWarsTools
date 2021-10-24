using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace HaloWarsTools
{
    public class HWXtdResource : HWBinaryResource
    {
        public HWXtdResource(string filename) : base(filename) { }

        public Bitmap AmbientOcclusionTexture => ValueCache.Get(() => ExtractEmbeddedDXT5A(GetFirstChunkOfType(HWBinaryResourceChunkType.XTD_AOChunk)));
        public Bitmap OpacityTexture => ValueCache.Get(() => ExtractEmbeddedDXT5A(GetFirstChunkOfType(HWBinaryResourceChunkType.XTD_AlphaChunk)));

        public static new HWXtdResource FromFile(string filename) {
            return GetOrCreateFromFile(filename) as HWXtdResource;
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
    }
}
