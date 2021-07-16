// Decompiled with JetBrains decompiler
// Type: zamboni.IceV5File
// Assembly: zamboni, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 73B487C9-8F41-4586-BEF5-F7D7BFBD4C55
// Assembly location: D:\Downloads\zamboni_ngs (3)\zamboni.exe

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace zamboni
{
    public class IceV5File : IceFile
    {
        private static IceV5File.DecryptBaseData V5Decrypt = new IceV5File.DecryptBaseData()
        {
            KeyStartPos = 131,
            CrcStartPos = 10,
            CrcEndPos = 210,
            KeyConstTable = new byte[4]
          {
        (byte) 226,
        (byte) 198,
        (byte) 161,
        (byte) 243
          },
            HeaderRol = 25,
            Group2Rol = 17
        };
        private static IceV5File.DecryptBaseData V6Decrypt = new IceV5File.DecryptBaseData()
        {
            KeyStartPos = 179,
            CrcStartPos = 80,
            CrcEndPos = 97,
            KeyConstTable = new byte[4]
          {
        (byte) 232,
        (byte) 174,
        (byte) 183,
        (byte) 100
          },
            HeaderRol = 15,
            Group2Rol = 4
        };
        private static IceV5File.DecryptBaseData V7Decrypt = new IceV5File.DecryptBaseData()
        {
            KeyStartPos = 215,
            CrcStartPos = 23,
            CrcEndPos = 71,
            KeyConstTable = new byte[4]
          {
        (byte) 8,
        (byte) 249,
        (byte) 93,
        (byte) 253
          },
            HeaderRol = 10,
            Group2Rol = 7
        };
        private static IceV5File.DecryptBaseData V8Decrypt = new IceV5File.DecryptBaseData()
        {
            KeyStartPos = 22,
            CrcStartPos = 84,
            CrcEndPos = 97,
            KeyConstTable = new byte[4]
          {
        (byte) 200,
        (byte) 170,
        (byte) 94,
        (byte) 122
          },
            HeaderRol = 28,
            Group2Rol = 5
        };
        private static IceV5File.DecryptBaseData V9Decrypt = new IceV5File.DecryptBaseData()
        {
            KeyStartPos = 220,
            CrcStartPos = 189,
            CrcEndPos = 219,
            KeyConstTable = new byte[4]
          {
        (byte) 13,
        (byte) 156,
        (byte) 245,
        (byte) 147
          },
            HeaderRol = 8,
            Group2Rol = 14
        };
        private IceV5File.DecryptBaseData[] decryptionHeaders = new IceV5File.DecryptBaseData[5]
        {
      IceV5File.V5Decrypt,
      IceV5File.V6Decrypt,
      IceV5File.V7Decrypt,
      IceV5File.V8Decrypt,
      IceV5File.V9Decrypt
        };
        private const uint keyconstant_1 = 1129510338;
        private const uint keyconstant_2 = 3444586398;
        private const uint keyconstant_3 = 613566757;
        private const uint keyconstant_4 = 1321528399;
        private IceFile.GroupHeader[] groupHeaders = new IceFile.GroupHeader[2];
        private byte[] allHeaderData;
        private byte[] magicNumbers = new byte[256];
        private byte[] cryptHeaders = new byte[48];
        private int iceType = 5;
        private int fileSize = 0;

        protected override int SecondPassThreshold => 153600;

        public IceV5File(string filename) => this.loadFile((Stream)File.OpenRead(filename));

        public IceV5File(Stream inStream) => this.loadFile(inStream);

        private void loadFile(Stream inStream)
        {
            this.allHeaderData = new BinaryReader(inStream).ReadBytes(352);
            this.iceType = BitConverter.ToInt32(this.allHeaderData, 8);
            this.decryptShift = this.iceType + 5;
            this.fileSize = BitConverter.ToInt32(this.allHeaderData, 28);
            Array.Copy((Array)this.allHeaderData, 48, (Array)this.magicNumbers, 0, 256);
            Array.Copy((Array)this.allHeaderData, 304, (Array)this.cryptHeaders, 0, 48);
            inStream.Seek(0L, SeekOrigin.Begin);
            byte[][] numArray = this.splitGroups(inStream);
            this.header = numArray[0];
            int int32_1 = BitConverter.ToInt32(this.header, 312);
            int int32_2 = BitConverter.ToInt32(this.header, 328);
            this.groupOneFiles = this.splitGroup(numArray[1], int32_1);
            this.groupTwoFiles = this.splitGroup(numArray[2], int32_2);
        }

        public IceV5File(string headerFilename, string group1, string group2) => throw new NotSupportedException();

        private int calculateKeyStep1()
        {
            int keyStartPos = (int)this.decryptionHeaders[this.iceType - 5].KeyStartPos;
            int crcStartPos = (int)this.decryptionHeaders[this.iceType - 5].CrcStartPos;
            int count = (int)this.decryptionHeaders[this.iceType - 5].CrcEndPos - crcStartPos;
            uint temp_key = this.calcBlowfishKeys(this.magicNumbers, this.getKey(this.magicNumbers, (uint)((int)BitConverter.ToUInt32(((IEnumerable<byte>)new Crc32().ComputeHash(this.magicNumbers, crcStartPos, count)).Reverse<byte>().ToArray<byte>(), 0) ^ (int)BitConverter.ToUInt32(this.magicNumbers, keyStartPos) ^ this.fileSize ^ 1129510338)));
            uint key = this.getKey(this.magicNumbers, temp_key);
            BitConverter.ToUInt32(((IEnumerable<byte>)BitConverter.GetBytes(temp_key << (int)this.decryptionHeaders[this.iceType - 5].HeaderRol | temp_key >> 32 - (int)this.decryptionHeaders[this.iceType - 5].HeaderRol)).Reverse<byte>().ToArray<byte>(), 0);
            uint num1 = temp_key >> (int)this.decryptionHeaders[this.iceType - 5].HeaderRol | temp_key << 32 - (int)this.decryptionHeaders[this.iceType - 5].HeaderRol;
            uint num2 = key >> (int)this.decryptionHeaders[this.iceType - 5].HeaderRol | key << 32 - (int)this.decryptionHeaders[this.iceType - 5].HeaderRol;
            return 0;
        }

        private uint getKey(byte[] keys, uint temp_key)
        {
            uint num1 = (uint)(((int)temp_key & (int)byte.MaxValue) + (int)this.decryptionHeaders[this.iceType - 5].KeyConstTable[0] & (int)byte.MaxValue);
            uint num2 = (uint)((int)(temp_key >> 8) + (int)this.decryptionHeaders[this.iceType - 5].KeyConstTable[1] & (int)byte.MaxValue);
            uint num3 = (uint)((int)(temp_key >> 16) + (int)this.decryptionHeaders[this.iceType - 5].KeyConstTable[2] & (int)byte.MaxValue);
            uint num4 = (uint)((int)(temp_key >> 24) + (int)this.decryptionHeaders[this.iceType - 5].KeyConstTable[3] & (int)byte.MaxValue);
            byte num5 = (byte)((uint)this.decryptionHeaders[this.iceType - 5].KeyConstTable[1] & 7U);
            byte num6 = (byte)((uint)this.decryptionHeaders[this.iceType - 5].KeyConstTable[3] & 7U);
            byte num7 = (byte)((uint)this.decryptionHeaders[this.iceType - 5].KeyConstTable[0] & 7U);
            byte num8 = (byte)((uint)this.decryptionHeaders[this.iceType - 5].KeyConstTable[2] & 7U);
            return (uint)((int)(byte)(((int)keys[(int)num3] << (int)num8 | (int)keys[(int)num3] >> 8 - (int)num8) & (int)byte.MaxValue) << 24 | (int)(byte)(((int)keys[(int)num1] << (int)num7 | (int)keys[(int)num1] >> 8 - (int)num7) & (int)byte.MaxValue) << 16 | (int)(byte)(((int)keys[(int)num2] << (int)num5 | (int)keys[(int)num2] >> 8 - (int)num5) & (int)byte.MaxValue) << 8) | (uint)(byte)(((int)keys[(int)num4] << (int)num6 | (int)keys[(int)num4] >> 8 - (int)num6) & (int)byte.MaxValue);
        }

        private uint calcBlowfishKeys(byte[] keys, uint temp_key)
        {
            uint temp_key1 = 2382545500U ^ temp_key;
            uint num1 = (uint)(1321528399L * (long)temp_key1 >> 32);
            uint num2 = temp_key1 - num1 >> 1;
            uint num3 = (num1 >> 2) * 13U;
            for (int index = (int)temp_key1 - (int)num3 + 3; index > 0; --index)
                temp_key1 = this.getKey(keys, temp_key1);
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
            int keyStartPos = (int)this.decryptionHeaders[this.iceType - 5].KeyStartPos;
            int crcStartPos = (int)this.decryptionHeaders[this.iceType - 5].CrcStartPos;
            int count = (int)this.decryptionHeaders[this.iceType - 5].CrcEndPos - crcStartPos;
            uint temp_key = (uint)((int)BitConverter.ToUInt32(((IEnumerable<byte>)new Crc32().ComputeHash(numArray1, crcStartPos, count)).Reverse<byte>().ToArray<byte>(), 0) ^ (int)BitConverter.ToUInt32(numArray1, keyStartPos) ^ this.fileSize ^ 1129510338);
            uint key1 = this.getKey(numArray1, temp_key);
            uint num = this.calcBlowfishKeys(numArray1, key1);
            uint key2 = this.getKey(numArray1, num);
            uint key3 = this.ReverseBytes(num << (int)this.decryptionHeaders[this.iceType - 5].Group2Rol | num >> 32 - (int)this.decryptionHeaders[this.iceType - 5].Group2Rol);
            uint groupOneTempKey = num << (int)this.decryptionHeaders[this.iceType - 5].HeaderRol | num >> 32 - (int)this.decryptionHeaders[this.iceType - 5].HeaderRol;
            uint groupTwoTempKey = key2 << (int)this.decryptionHeaders[this.iceType - 5].HeaderRol | key2 >> 32 - (int)this.decryptionHeaders[this.iceType - 5].HeaderRol;
            byte[] decryptedHeaderData = new BlewFish(key3).decryptBlock(block);
            this.groupHeaders = this.readHeaders(decryptedHeaderData);
            inFile.Seek(352L, SeekOrigin.Begin);
            byte[][] numArray3 = new byte[3][]
            {
        new byte[352],
        new byte[0],
        new byte[0]
            };
            Array.Copy((Array)numArray2, (Array)numArray3[0], 304);
            Array.Copy((Array)decryptedHeaderData, 0, (Array)numArray3[0], 304, 48);
            if (this.groupHeaders[0].decompSize > 0U)
                numArray3[1] = this.extractGroup(this.groupHeaders[0], openReader, true, num, key2, false);
            if (this.groupHeaders[1].decompSize > 0U)
                numArray3[2] = this.extractGroup(this.groupHeaders[1], openReader, true, groupOneTempKey, groupTwoTempKey, false);
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
