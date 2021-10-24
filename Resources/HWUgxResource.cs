using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace HaloWarsTools
{
    public class HWUgxResource : HWBinaryResource
    {
        public HWUgxResource(string filename) : base(filename) { }

        public GenericMesh Mesh => ValueCache.Get(ImportMesh);
        public string[] TextureNames => ValueCache.Get(GetTextureNames);

        public static new HWUgxResource FromFile(string filename) {
            return GetOrCreateFromFile(filename) as HWUgxResource;
        }

        private bool ShouldStopScanning(char value) {
            return value < 46 || value > 122;
        }

        private string[] GetTextureNames() {
            HWBinaryResourceChunk materialChunk = GetFirstChunkOfType(HWBinaryResourceChunkType.UGX_MaterialChunk);
            List<string> textures = new List<string>();
            StringBuilder current = new StringBuilder();
            bool scan = false;
            for (int i = (int)materialChunk.Offset; i < materialChunk.Offset + materialChunk.Size; i++) {
                char value = (char)RawBytes[i];
                if (value == '\\') {
                    scan = true;
                } else if (value == 0) {
                    if (current.Length > 1) {
                        textures.Add(current.ToString());
                    }

                    scan = false;
                    current.Clear();
                } else if (ShouldStopScanning(value)) {
                    scan = false;
                    current.Clear();
                }

                if (scan) {
                    current.Append(value);
                }
            }

            return textures.ToArray();
        }

        private string GetStringAt(int offset) {
            StringBuilder current = new StringBuilder();
            for (int i = offset; i < RawBytes.Length; i++) {
                char value = (char)RawBytes[i];
                if (value == 0) {
                    break;
                } else {
                    current.Append(value);
                }
            }

            return current.ToString();
        }

        private GenericMesh ImportMesh() {
            int offset = 0;

            offset += 4; // 4 byte magic
            int tableOffset = BinaryUtils.ReadInt32BigEndian(RawBytes, offset); offset += 4;
            offset += 4; // 4 byte reserved
            offset += 4; // file size
            short tableCount = BinaryUtils.ReadInt16BigEndian(RawBytes, offset); offset += 2;
            offset += 2; // 2 byte reserved
            offset += 4; // 4 byte reserved
            offset += 8; // 8 byte reserved

            List<MeshTableData> tableData = new List<MeshTableData>();
            offset = tableOffset;
            for (int i = 0; i < tableCount; i++) {
                offset += 4; // 4 byte reserved
                int dataType = BinaryUtils.ReadInt32BigEndian(RawBytes, offset); offset += 4;
                int dataOffset = BinaryUtils.ReadInt32BigEndian(RawBytes, offset); offset += 4;
                int dataLength = BinaryUtils.ReadInt32BigEndian(RawBytes, offset); offset += 4;

                tableData.Add(new MeshTableData(dataType, dataOffset, dataLength));

                offset += 2; // 2 byte reserved
                offset += 2; // 2 byte reserved
                offset += 2; // 2 byte reserved
                offset += 2; // 2 byte reserved
            }

            int vertStart = 0;
            int faceStart = 0;

            Dictionary<int, List<MeshPolygonInfo>> meshArr = new Dictionary<int, List<MeshPolygonInfo>>();

            for (int i = 0; i < tableCount; i++) {
                offset = tableData[i].Offset;

                switch (tableData[i].Type) {
                    case MeshDataType.MeshInfo:
                        offset += 2; // 2 byte reserved
                        offset += 2; // 2 byte reserved
                        offset += 4; // 4 byte reserved
                        offset += 48; // 48 byte reserved
                        offset += 4; // 4 byte reserved
                        offset += 4; // 4 byte reserved

                        Dictionary<MeshSubDataType, MeshTableSubData> subDataList = new Dictionary<MeshSubDataType, MeshTableSubData>();
                        for (int j = 0; j < 6; j++) {
                            // Truncating to int because there's no fucking way we need more than 2 billion bytes, none of the files are that big
                            int dataCount = (int)BinaryUtils.ReadInt64LittleEndian(RawBytes, offset); offset += 8;
                            int dataOffset = tableData[i].Offset + (int)BinaryUtils.ReadInt64LittleEndian(RawBytes, offset); offset += 8;
                            var subData = new MeshTableSubData(dataCount, dataOffset);
                            subDataList.Add((MeshSubDataType)(j + 1), subData);
                        }

                        var data = subDataList[MeshSubDataType.MeshData];
                        offset = data.Offset;
                        for (int j = 0; j < data.Count; j++) {
                            var polyInfo = new MeshPolygonInfo();
                            polyInfo.MaterialId = BinaryUtils.ReadInt32LittleEndian(RawBytes, offset); offset += 4;
                            polyInfo.PolygonId = BinaryUtils.ReadInt32LittleEndian(RawBytes, offset); offset += 4;
                            offset += 4; // 4 byte reserved
                            polyInfo.BoneId = BinaryUtils.ReadInt32LittleEndian(RawBytes, offset); offset += 4;
                            polyInfo.FaceOffset = BinaryUtils.ReadInt32LittleEndian(RawBytes, offset); offset += 4;
                            polyInfo.FaceCount = BinaryUtils.ReadInt32LittleEndian(RawBytes, offset); offset += 4;
                            polyInfo.VertOffset = BinaryUtils.ReadInt32LittleEndian(RawBytes, offset); offset += 4;
                            polyInfo.VertLength = BinaryUtils.ReadInt32LittleEndian(RawBytes, offset); offset += 4;
                            polyInfo.VertSize = BinaryUtils.ReadInt32LittleEndian(RawBytes, offset); offset += 4;
                            polyInfo.VertCount = BinaryUtils.ReadInt32LittleEndian(RawBytes, offset); offset += 4;

                            offset += 16; // 16 byte reserved

                            int nameOffset = BinaryUtils.ReadInt32LittleEndian(RawBytes, offset); offset += 4;
                            int location = BinaryUtils.ReadInt32LittleEndian(RawBytes, tableData[i].Offset + nameOffset);
                            polyInfo.Name = GetStringAt(tableData[i].Offset + nameOffset);

                            offset += 92; // 92 byte reserved

                            if (!meshArr.ContainsKey(polyInfo.PolygonId)) {
                                meshArr.Add(polyInfo.PolygonId, new List<MeshPolygonInfo>());
                            }

                            meshArr[polyInfo.PolygonId].Add(polyInfo);
                        }

                        data = subDataList[MeshSubDataType.BoneData];
                        offset = data.Offset;
                        for (int j = 0; j < data.Count; j++) {
                            int nameOffset = BinaryUtils.ReadInt32LittleEndian(RawBytes, offset); offset += 4;
                            string boneName = GetStringAt(tableData[i].Offset + nameOffset);

                            offset += 4; // 4 byte reserved

                            offset += 4; // m11
                            offset += 4; // m12
                            offset += 4; // m13
                            offset += 4; // m14

                            offset += 4; // m21
                            offset += 4; // m22
                            offset += 4; // m23
                            offset += 4; // m24

                            offset += 4; // m31
                            offset += 4; // m32
                            offset += 4; // m33
                            offset += 4; // m34

                            offset += 4; // m41
                            offset += 4; // m42
                            offset += 4; // m43
                            offset += 4; // m44

                            offset += 4; // parentID
                            offset += 4; // 4 byte reserved
                        }

                        break;
                    case MeshDataType.IndexData:
                        faceStart = offset;
                        break;
                    case MeshDataType.VertexData:
                        vertStart = offset;
                        break;
                }
            }

            Dictionary<int, GenericMaterial> materials = new Dictionary<int, GenericMaterial>();
            Dictionary<int, GenericMeshSection> sections = new Dictionary<int, GenericMeshSection>();

            var mesh = new GenericMesh();

            int vertTotal = 0;
            foreach (var entry in meshArr) {
                var polygonInfoList = entry.Value;
                for (int i = 0; i < polygonInfoList.Count; i++) {
                    var polygonInfo = polygonInfoList[i];
                    offset = polygonInfo.VertOffset + vertStart;

                    for (int j = 0; j < polygonInfo.VertCount; j++) {
                        Vector3 position = Vector3.Zero;
                        Vector3 normal = Vector3.Zero;
                        Vector3 texcoord = Vector3.Zero;
                        switch (polygonInfo.VertSize) {
                            case 0x18:
                                position.X = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                position.Y = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                position.Z = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                offset += 2; // 2 byte reserved
                                normal.X = BinaryUtils.ReadFloatLittleEndian(RawBytes, offset); offset += 4;
                                normal.Y = BinaryUtils.ReadFloatLittleEndian(RawBytes, offset); offset += 4;
                                normal.Z = BinaryUtils.ReadFloatLittleEndian(RawBytes, offset); offset += 4;
                                texcoord.X = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                texcoord.Y = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                break;
                            case 0x1c:
                                position.X = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                position.Y = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                position.Z = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                offset += 2; // 2 byte reserved
                                normal.X = BinaryUtils.ReadFloatLittleEndian(RawBytes, offset); offset += 4;
                                normal.Y = BinaryUtils.ReadFloatLittleEndian(RawBytes, offset); offset += 4;
                                normal.Z = BinaryUtils.ReadFloatLittleEndian(RawBytes, offset); offset += 4;
                                texcoord.X = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                texcoord.Y = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                offset += 2; // 2 byte reserved
                                offset += 2; // 2 byte reserved
                                break;
                            case 0x20:
                                position.X = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                position.Y = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                position.Z = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                offset += 2; // 2 byte reserved
                                normal.X = BinaryUtils.ReadFloatLittleEndian(RawBytes, offset); offset += 4;
                                normal.Y = BinaryUtils.ReadFloatLittleEndian(RawBytes, offset); offset += 4;
                                normal.Z = BinaryUtils.ReadFloatLittleEndian(RawBytes, offset); offset += 4;
                                offset += 1; // bone1
                                offset += 1; // bone2
                                offset += 1; // bone3
                                offset += 1; // bone4
                                offset += 1; // weight1
                                offset += 1; // weight2
                                offset += 1; // weight3
                                offset += 1; // weight4
                                texcoord.X = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                texcoord.Y = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                break;
                            case 0x24:
                                position.X = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                position.Y = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                position.Z = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                offset += 2; // 2 byte reserved
                                normal.X = BinaryUtils.ReadFloatLittleEndian(RawBytes, offset); offset += 4;
                                normal.Y = BinaryUtils.ReadFloatLittleEndian(RawBytes, offset); offset += 4;
                                normal.Z = BinaryUtils.ReadFloatLittleEndian(RawBytes, offset); offset += 4;
                                offset += 4; // 4 byte reserved
                                offset += 4; // 4 byte reserved
                                offset += 4; // 4 byte reserved
                                texcoord.X = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                texcoord.Y = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                break;
                            case 0x28:
                                position.X = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                position.Y = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                position.Z = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                offset += 2; // 2 byte reserved
                                normal.X = BinaryUtils.ReadFloatLittleEndian(RawBytes, offset); offset += 4;
                                normal.Y = BinaryUtils.ReadFloatLittleEndian(RawBytes, offset); offset += 4;
                                normal.Z = BinaryUtils.ReadFloatLittleEndian(RawBytes, offset); offset += 4;
                                offset += 4; // 4 byte reserved
                                offset += 4; // 4 byte reserved
                                offset += 4; // 4 byte reserved
                                texcoord.X = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                texcoord.Y = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                offset += 4; // 4 byte reserved
                                break;
                            case 0x2c:
                                position.X = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                position.Y = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                position.Z = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                offset += 2; // 2 byte reserved
                                normal.X = BinaryUtils.ReadFloatLittleEndian(RawBytes, offset); offset += 4;
                                normal.Y = BinaryUtils.ReadFloatLittleEndian(RawBytes, offset); offset += 4;
                                normal.Z = BinaryUtils.ReadFloatLittleEndian(RawBytes, offset); offset += 4;
                                offset += 4; // 4 byte reserved
                                offset += 4; // 4 byte reserved
                                offset += 4; // 4 byte reserved
                                offset += 1; // bone1
                                offset += 1; // bone2
                                offset += 1; // bone3
                                offset += 1; // bone4
                                offset += 1; // weight1
                                offset += 1; // weight2
                                offset += 1; // weight3
                                offset += 1; // weight4
                                texcoord.X = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                texcoord.Y = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                break;
                            case 0x30:
                                position.X = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                position.Y = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                position.Z = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                offset += 2; // 2 byte reserved
                                normal.X = BinaryUtils.ReadFloatLittleEndian(RawBytes, offset); offset += 4;
                                normal.Y = BinaryUtils.ReadFloatLittleEndian(RawBytes, offset); offset += 4;
                                normal.Z = BinaryUtils.ReadFloatLittleEndian(RawBytes, offset); offset += 4;
                                offset += 4; // 4 byte reserved
                                offset += 4; // 4 byte reserved
                                offset += 4; // 4 byte reserved
                                offset += 1; // bone1
                                offset += 1; // bone2
                                offset += 1; // bone3
                                offset += 1; // bone4
                                offset += 1; // weight1
                                offset += 1; // weight2
                                offset += 1; // weight3
                                offset += 1; // weight4
                                offset += 4; // 4 byte reserved
                                texcoord.X = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                texcoord.Y = BinaryUtils.ReadHalfLittleEndian(RawBytes, offset); offset += 2;
                                break;
                            default:
                                continue;
                        }

                        mesh.Vertices.Add(position);
                        mesh.Normals.Add(Vector3.Normalize(normal));
                        texcoord.Y = 1 - texcoord.Y;
                        mesh.TexCoords.Add(texcoord);
                    }

                    offset = ((polygonInfo.FaceOffset * 2) + faceStart);
                    int firstFaceIndex = mesh.Faces.Count;
                    for (int j = 0; j < polygonInfo.FaceCount; j++) {
                        int fa = vertTotal + BinaryUtils.ReadUInt16LittleEndian(RawBytes, offset); offset += 2;
                        int fb = vertTotal + BinaryUtils.ReadUInt16LittleEndian(RawBytes, offset); offset += 2;
                        int fc = vertTotal + BinaryUtils.ReadUInt16LittleEndian(RawBytes, offset); offset += 2;

                        if (!materials.ContainsKey(polygonInfo.MaterialId)) {
                            materials.Add(polygonInfo.MaterialId, new GenericMaterial("material_" + (polygonInfo.MaterialId + 1)));
                        }

                        if (!sections.ContainsKey(polygonInfo.PolygonId)) {
                            sections.Add(polygonInfo.PolygonId, new GenericMeshSection("object_" + (polygonInfo.PolygonId + 1)));
                        }

                        mesh.Faces.Add(new GenericFace(fa, fc, fb, materials[polygonInfo.MaterialId], sections[polygonInfo.PolygonId]));
                    }
                    int lastFaceIndex = mesh.Faces.Count - 1;
                    vertTotal += polygonInfo.VertCount;
                }
            }

            return mesh;
        }

        public struct MeshPolygonInfo
        {
            public int MaterialId;
            public int PolygonId;
            public int BoneId;
            public int FaceOffset;
            public int FaceCount;
            public int VertOffset;
            public int VertLength;
            public int VertSize;
            public int VertCount;
            public string Name;
        }

        public struct MeshTableData
        {
            public MeshDataType Type;
            public int Offset;
            public int Length;

            public MeshTableData(int dataType, int dataOffset, int dataLength) : this() {
                Type = (MeshDataType)dataType;
                Offset = dataOffset;
                Length = dataLength;
            }
        }

        public struct MeshTableSubData
        {
            public int Offset;
            public int Count;

            public MeshTableSubData(int dataCount, int dataOffset) : this() {
                Offset = dataOffset;
                Count = dataCount;
            }
        }

        public enum MeshDataType
        {
            MeshInfo = 0x700,
            IndexData = 0x701,
            VertexData = 0x702
        }

        public enum MeshSubDataType
        {
            MeshData = 1,
            BoneData = 2,
            LinkData = 3,
            MeshId = 4,
            MinBound = 5,
            MaxBound = 6
        }
    }
}
