using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HaloWarsTools
{
    public enum HWResourceType
    {
        None,
        Xtt, // Terrain Mesh/Albedo
        Xtd, // Terrain Opacity/AO
        Scn, // Scenario
        Sc2, // Scenario
        Sc3, // Scenario
        Gls, // Scenario Lighting
        Ugx, // Mesh
    }

    public enum HWResourceAuditEntryType
    {
        Created,
        Accessed,
        Loaded,
        Unloaded
    }

    public enum HWBinaryResourceChunkType : ulong
    {
        Unknown = 0,

        XTD_XTDHeader = 0x1111,
        XTD_TerrainChunk = 0x2222,
        XTD_AtlasChunk = 0x8888,
        XTD_TessChunk = 0xAAAA,
        XTD_LightingChunk = 0xBBBB,
        XTD_AOChunk = 0xCCCC,
        XTD_AlphaChunk = 0xDDDD,

        XTT_TerrainAtlasLinkChunk = 0x2222,
        XTT_AtlasChunkAlbedo = 0x6666,
        XTT_RoadChunk = 0x8888,
        XTT_FoliageHeaderChunk = 0xAAAA,
        XTT_FoliageQNChunk = 0xBBBB,

        XSD_XSDHeader = 0x1111,
        XSD_SimHeights = 0x2222, // HALF [NumCacheFriendlyXVerts^2]
        XSD_Obstructions = 0x4444, // BYTE [NumXDataTiles^2]
        XSD_TileTypes = 0x8888, // BYTE [NumXDataTiles^2]
        XSD_CamHeights = 0xAAAA,
        XSD_FlightHeights = 0xABBB,
        XSD_Buildable = 0xCCCC, // BYTE [NumXDataTiles^2]

        UGX_CachedDataChunk = 0x00000700,
        UGX_IndexBufferChunk = 0x00000701,
        UGX_VertexBufferChunk = 0x00000702,
        UGX_GrxChunk = 0x00000703, // granny_file_info
        UGX_MaterialChunk = 0x00000704,
        UGX_TreeChunk = 0x00000705,
        UGX_CachedDataSignatureHW1_BE = 0xC2340004,
        UGX_CachedDataSignatureHW1_LE = 0x040034C2,
        UGX_CachedDataSignatureHW2 = 0x060034C2, // 0xC2340006
    }

    public class HWBinaryResourceChunk
    {
        public HWBinaryResourceChunkType Type;
        public uint Offset;
        public uint Size;
    }

    public struct HWResourceAuditEntry
    {
        public HWResource Resource;
        public DateTime When;
        public HWResourceAuditEntryType Type;

        public HWResourceAuditEntry(HWResource resource, HWResourceAuditEntryType type) {
            When = DateTime.Now;
            Resource = resource;
            Type = type;
        }

        public override string ToString() {
            string text = Type switch {
                HWResourceAuditEntryType.Created => $"Created resource \"{Resource.UserFriendlyName}\"",
                HWResourceAuditEntryType.Accessed => $"Accessed resource \"{Resource.UserFriendlyName}\"",
                _ => null
            };

            return $"{When.ToShortTimeString()}: {text}";
        }
    }

    public class HWResource {
        private static Dictionary<string, HWResource> ResourcesByFilename = new Dictionary<string, HWResource>();
        private static List<HWResourceAuditEntry> AuditLog = new List<HWResourceAuditEntry>();


        protected LazyValueCache ValueCache;

        private static void AuditResource(HWResource resource, HWResourceAuditEntryType type) {
            AuditLog.Add(new HWResourceAuditEntry(resource, type));
        }

        public string Filename;
        public HWResourceType Type = HWResourceType.None;
        public bool IsLoaded = false;

        public string UserFriendlyName
        {
            get
            {
                return Filename;
            }
        }

        protected HWResource(string filename) {
            Filename = filename;
            ValueCache = new LazyValueCache();
        }

        public static HWResource GetOrCreateResource(string filename) {
            if (!ResourcesByFilename.ContainsKey(filename)) {
                ResourcesByFilename.Add(filename, CreateResource(filename));
                AuditResource(ResourcesByFilename[filename], HWResourceAuditEntryType.Created);
            } else {
                AuditResource(ResourcesByFilename[filename], HWResourceAuditEntryType.Accessed);
            }

            return ResourcesByFilename[filename];
        }

        private static HWResource CreateResource(string filename) {
            string extension = Path.GetExtension(filename).ToLowerInvariant();
            return extension switch {
                ".xtt" => new HWXttResource(filename),
                ".xtd" => new HWXtdResource(filename),
                ".scn" => new HWScnResource(filename),
                ".sc2" => new HWSc2Resource(filename),
                ".sc3" => new HWSc3Resource(filename),
                ".gls" => new HWGlsResource(filename),
                ".ugx" => new HWUgxResource(filename),
                _ => null
            };
        }
    }

    public class HWBinaryResource : HWResource
    {
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

        public HWBinaryResource(string filename) : base(filename) { }

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

    public class HWXmlResource : HWResource
    {
        protected XElement XmlData => ValueCache.Get(() => XElement.Load(Filename));

        public HWXmlResource(string filename) : base(filename) { }

        protected static Vector3 DeserializeVector3(string value) {
            string[] components = value.Split(",");
            return new Vector3(float.Parse(components[0]), float.Parse(components[1]), float.Parse(components[2]));
        }
    }

    public class HWUgxResource : HWBinaryResource
    {
        public HWUgxResource(string filename) : base(filename) {
            Type = HWResourceType.Ugx;
        }
    }

    public class HWScnResource : HWXmlResource
    {
        public HWScnResource(string filename) : base(filename) {
            Type = HWResourceType.Scn;
        }
    }

    public class HWSc2Resource : HWXmlResource
    {
        public HWSc2Resource(string filename) : base(filename) {
            Type = HWResourceType.Sc2;
        }
    }

    public class HWSc3Resource : HWXmlResource
    {
        public HWSc3Resource(string filename) : base(filename) {
            Type = HWResourceType.Sc3;
        }
    }
}
