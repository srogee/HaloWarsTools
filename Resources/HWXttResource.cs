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

        public static new HWXttResource FromFile(HWContext context, string filename) {
            return GetOrCreateFromFile(context, filename, HWResourceType.Xtt) as HWXttResource;
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
    }
}
