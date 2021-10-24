using System;
using System.IO;
using System.Linq;

namespace HaloWarsTools
{
    public class HWBinaryResource : HWResource
    {
        protected HWBinaryResource(string filename) : base(filename) { }

        protected byte[] RawBytes => ValueCache.Get(() => File.ReadAllBytes(Filename));

        protected HWBinaryResourceChunk[] Chunks => ValueCache.Get(() => {
            uint headerSize = BinaryUtils.ReadUInt32BigEndian(RawBytes, 4);
            ushort numChunks = BinaryUtils.ReadUInt16BigEndian(RawBytes, 16);
            int chunkHeaderSize = 24;

            var chunks = new HWBinaryResourceChunk[numChunks];
            for (int i = 0; i < chunks.Length; i++) {
                int offset = (int)headerSize + i * chunkHeaderSize;

                chunks[i] = new HWBinaryResourceChunk() {
                    Type = ParseChunkType(BinaryUtils.ReadUInt64BigEndian(RawBytes, offset)),
                    Offset = BinaryUtils.ReadUInt32BigEndian(RawBytes, offset + 8),
                    Size = BinaryUtils.ReadUInt32BigEndian(RawBytes, offset + 12)
                };
            }

            return chunks;
        });

        protected static HWBinaryResourceChunkType ParseChunkType(ulong type) {
            if (Enum.TryParse(type.ToString(), out HWBinaryResourceChunkType result)) {
                return result;
            }

            return HWBinaryResourceChunkType.Unknown;
        }

        protected HWBinaryResourceChunk[] GetAllChunksOfType(HWBinaryResourceChunkType type) {
            return Chunks.Where(chunk => chunk.Type == type).ToArray();
        }

        protected HWBinaryResourceChunk GetFirstChunkOfType(HWBinaryResourceChunkType type) {
            return Chunks.FirstOrDefault(chunk => chunk.Type == type);
        }
    }
}
