// Decompiled with JetBrains decompiler
// Type: zamboni.IceV5File
// Assembly: zamboni, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 73B487C9-8F41-4586-BEF5-F7D7BFBD4C55
// Assembly location: D:\Downloads\zamboni_ngs (3)\zamboni.exe

using System;
using System.IO;
using System.Linq;

namespace Zamboni.IceFileFormats
{
    public class IceV5File : IceFile
    {
        private const uint keyconstant_1 = 1129510338;
        private const uint keyconstant_2 = 3444586398;
        private const uint keyconstant_3 = 613566757;
        private const uint keyconstant_4 = 1321528399;

        private static readonly DecryptBaseData V5Decrypt = new DecryptBaseData
        {
            KeyStartPos = 131,
            CrcStartPos = 10,
            CrcEndPos = 210,
            KeyConstTable = new byte[4] { 226, 198, 161, 243 },
            HeaderRol = 25,
            Group2Rol = 17
        };

        private static readonly DecryptBaseData V6Decrypt = new DecryptBaseData
        {
            KeyStartPos = 179,
            CrcStartPos = 80,
            CrcEndPos = 97,
            KeyConstTable = new byte[4] { 232, 174, 183, 100 },
            HeaderRol = 15,
            Group2Rol = 4
        };

        private static readonly DecryptBaseData V7Decrypt = new DecryptBaseData
        {
            KeyStartPos = 215,
            CrcStartPos = 23,
            CrcEndPos = 71,
            KeyConstTable = new byte[4] { 8, 249, 93, 253 },
            HeaderRol = 10,
            Group2Rol = 7
        };

        private static readonly DecryptBaseData V8Decrypt = new DecryptBaseData
        {
            KeyStartPos = 22,
            CrcStartPos = 84,
            CrcEndPos = 97,
            KeyConstTable = new byte[4] { 200, 170, 94, 122 },
            HeaderRol = 28,
            Group2Rol = 5
        };

        private static readonly DecryptBaseData V9Decrypt = new DecryptBaseData
        {
            KeyStartPos = 220,
            CrcStartPos = 189,
            CrcEndPos = 219,
            KeyConstTable = new byte[4] { 13, 156, 245, 147 },
            HeaderRol = 8,
            Group2Rol = 14
        };

        private readonly byte[] cryptHeaders = new byte[48];

        private readonly DecryptBaseData[] decryptionHeaders = new DecryptBaseData[5]
        {
            V5Decrypt, V6Decrypt, V7Decrypt, V8Decrypt, V9Decrypt
        };

        private readonly byte[] magicNumbers = new byte[256];

        private byte[] allHeaderData;

        private int fileSize;
        private GroupHeader[] groupHeaders = new GroupHeader[2];
        private int iceType = 5;

        public IceV5File(string filename)
        {
            loadFile(File.OpenRead(filename));
        }

        public IceV5File(Stream inStream)
        {
            loadFile(inStream);
        }

        public IceV5File(string headerFilename, string group1, string group2)
        {
            throw new NotSupportedException();
        }

        protected override int SecondPassThreshold => 153600;

        private void loadFile(Stream inStream)
        {
            allHeaderData = new BinaryReader(inStream).ReadBytes(352);
            iceType = BitConverter.ToInt32(allHeaderData, 8);
            decryptShift = iceType + 5;
            fileSize = BitConverter.ToInt32(allHeaderData, 28);
            Array.Copy(allHeaderData, 48, magicNumbers, 0, 256);
            Array.Copy(allHeaderData, 304, cryptHeaders, 0, 48);
            inStream.Seek(0L, SeekOrigin.Begin);
            byte[][] numArray = splitGroups(inStream);
            header = numArray[0];
            int int32_1 = BitConverter.ToInt32(header, 312);
            int int32_2 = BitConverter.ToInt32(header, 328);
            groupOneFiles = splitGroup(numArray[1], int32_1);
            groupTwoFiles = splitGroup(numArray[2], int32_2);
        }

        private int calculateKeyStep1()
        {
            int keyStartPos = decryptionHeaders[iceType - 5].KeyStartPos;
            int crcStartPos = decryptionHeaders[iceType - 5].CrcStartPos;
            int count = decryptionHeaders[iceType - 5].CrcEndPos - crcStartPos;
            uint temp_key = calcBlowfishKeys(magicNumbers,
                getKey(magicNumbers,
                    (uint)((int)BitConverter.ToUInt32(
                               new Crc32().ComputeHash(magicNumbers, crcStartPos, count).Reverse().ToArray(), 0) ^
                           (int)BitConverter.ToUInt32(magicNumbers, keyStartPos) ^ fileSize ^ 1129510338)));
            uint key = getKey(magicNumbers, temp_key);
            BitConverter.ToUInt32(
                BitConverter
                    .GetBytes((temp_key << decryptionHeaders[iceType - 5].HeaderRol) |
                              (temp_key >> (32 - decryptionHeaders[iceType - 5].HeaderRol))).Reverse().ToArray(), 0);
            uint num1 = (temp_key >> decryptionHeaders[iceType - 5].HeaderRol) |
                        (temp_key << (32 - decryptionHeaders[iceType - 5].HeaderRol));
            uint num2 = (key >> decryptionHeaders[iceType - 5].HeaderRol) |
                        (key << (32 - decryptionHeaders[iceType - 5].HeaderRol));
            return 0;
        }

        private uint getKey(byte[] keys, uint temp_key)
        {
            uint num1 = (uint)((((int)temp_key & byte.MaxValue) + decryptionHeaders[iceType - 5].KeyConstTable[0]) &
                               byte.MaxValue);
            uint num2 = (uint)(((int)(temp_key >> 8) + decryptionHeaders[iceType - 5].KeyConstTable[1]) &
                               byte.MaxValue);
            uint num3 =
                (uint)(((int)(temp_key >> 16) + decryptionHeaders[iceType - 5].KeyConstTable[2]) & byte.MaxValue);
            uint num4 =
                (uint)(((int)(temp_key >> 24) + decryptionHeaders[iceType - 5].KeyConstTable[3]) & byte.MaxValue);
            byte num5 = (byte)(decryptionHeaders[iceType - 5].KeyConstTable[1] & 7U);
            byte num6 = (byte)(decryptionHeaders[iceType - 5].KeyConstTable[3] & 7U);
            byte num7 = (byte)(decryptionHeaders[iceType - 5].KeyConstTable[0] & 7U);
            byte num8 = (byte)(decryptionHeaders[iceType - 5].KeyConstTable[2] & 7U);
            return (uint)(
                       ((byte)(((keys[(int)num3] << num8) | (keys[(int)num3] >> (8 - num8))) & byte.MaxValue) << 24) |
                       ((byte)(((keys[(int)num1] << num7) | (keys[(int)num1] >> (8 - num7))) & byte.MaxValue) << 16) |
                       ((byte)(((keys[(int)num2] << num5) | (keys[(int)num2] >> (8 - num5))) & byte.MaxValue) << 8)) |
                   (byte)(((keys[(int)num4] << num6) | (keys[(int)num4] >> (8 - num6))) & byte.MaxValue);
        }

        private uint calcBlowfishKeys(byte[] keys, uint temp_key)
        {
            uint temp_key1 = 2382545500U ^ temp_key;
            uint num1 = (uint)((1321528399L * temp_key1) >> 32);
            uint num2 = (temp_key1 - num1) >> 1;
            uint num3 = (num1 >> 2) * 13U;
            for (int index = (int)temp_key1 - (int)num3 + 3; index > 0; --index)
            {
                temp_key1 = getKey(keys, temp_key1);
            }

            return (uint)((int)temp_key1 ^ 1129510338 ^ -850380898);
        }

        private byte[][] splitGroups(Stream inFile)
        {
            BinaryReader openReader = new BinaryReader(inFile);
            openReader.ReadBytes(4);
            openReader.ReadInt32();
            openReader.ReadInt32();
            openReader.ReadInt32();
            openReader.ReadInt32();
            openReader.ReadInt32();
            openReader.ReadInt32();
            openReader.ReadInt32();
            inFile.Seek(48L, SeekOrigin.Begin);
            byte[] numArray1 = openReader.ReadBytes(256);
            byte[] block = openReader.ReadBytes(48);
            inFile.Seek(0L, SeekOrigin.Begin);
            byte[] numArray2 = openReader.ReadBytes(304);
            int keyStartPos = decryptionHeaders[iceType - 5].KeyStartPos;
            int crcStartPos = decryptionHeaders[iceType - 5].CrcStartPos;
            int count = decryptionHeaders[iceType - 5].CrcEndPos - crcStartPos;
            uint temp_key =
                (uint)((int)BitConverter.ToUInt32(
                           new Crc32().ComputeHash(numArray1, crcStartPos, count).Reverse().ToArray(), 0) ^
                       (int)BitConverter.ToUInt32(numArray1, keyStartPos) ^ fileSize ^ 1129510338);
            uint key1 = getKey(numArray1, temp_key);
            uint num = calcBlowfishKeys(numArray1, key1);
            uint key2 = getKey(numArray1, num);
            uint key3 = ReverseBytes((num << decryptionHeaders[iceType - 5].Group2Rol) |
                                     (num >> (32 - decryptionHeaders[iceType - 5].Group2Rol)));
            uint groupOneTempKey = (num << decryptionHeaders[iceType - 5].HeaderRol) |
                                   (num >> (32 - decryptionHeaders[iceType - 5].HeaderRol));
            uint groupTwoTempKey = (key2 << decryptionHeaders[iceType - 5].HeaderRol) |
                                   (key2 >> (32 - decryptionHeaders[iceType - 5].HeaderRol));
            byte[] decryptedHeaderData = new BlewFish(key3).decryptBlock(block);
            groupHeaders = readHeaders(decryptedHeaderData);
            inFile.Seek(352L, SeekOrigin.Begin);
            byte[][] numArray3 = new byte[3][] { new byte[352], new byte[0], new byte[0] };
            Array.Copy(numArray2, numArray3[0], 304);
            Array.Copy(decryptedHeaderData, 0, numArray3[0], 304, 48);
            if (groupHeaders[0].decompSize > 0U)
            {
                numArray3[1] = extractGroup(groupHeaders[0], openReader, true, num, key2, false);
            }

            if (groupHeaders[1].decompSize > 0U)
            {
                numArray3[2] = extractGroup(groupHeaders[1], openReader, true, groupOneTempKey, groupTwoTempKey, false);
            }

            return numArray3;
        }

        private struct DecryptBaseData
        {
            public byte KeyStartPos;
            public byte CrcStartPos;
            public byte CrcEndPos;
            public byte[] KeyConstTable;
            public byte HeaderRol;
            public byte Group2Rol;
        }
    }
}
