// Decompiled with JetBrains decompiler
// Type: zamboni.IceV4File
// Assembly: zamboni, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 73B487C9-8F41-4586-BEF5-F7D7BFBD4C55
// Assembly location: D:\Downloads\zamboni_ngs (3)\zamboni.exe

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace zomboni
{
    public class IceV4File : IceFile
    {
        private const uint keyconstant_1 = 1129510338;
        private const uint keyconstant_2 = 3444586398;
        private const uint keyconstant_3 = 613566757;
        public int groupOneCount = 0;
        public int groupTwoCount = 0;

        protected override int SecondPassThreshold => 102400;

        public IceV4File(Stream inFile)
        {
            byte[][] numArray = this.splitGroups(inFile);
            this.header = numArray[0];
            this.groupOneFiles = this.splitGroup(numArray[1], this.groupOneCount);
            this.groupTwoFiles = this.splitGroup(numArray[2], this.groupTwoCount);
        }

        public IceV4File(byte[] headerData, byte[][] groupOneIn, byte[][] groupTwoIn)
        {
            this.header = headerData;
            this.groupOneFiles = groupOneIn;
            this.groupTwoFiles = groupTwoIn;
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
            int num1 = openReader.ReadInt32();
            int compSize = openReader.ReadInt32();
            int num2 = num1 == 1 ? 288 : 272;
            IceV4File.BlowfishKeys blowfishKeys = this.getBlowfishKeys(openReader.ReadBytes(256), compSize);
            byte[][] numArray1 = new byte[3][];
            byte[] numArray2 = new byte[48];
            byte[] decryptedHeaderData;
            if (num1 == 1 || num1 == 9)
            {
                inFile.Seek(0L, SeekOrigin.Begin);
                byte[] numArray3 = openReader.ReadBytes(288);
                byte[] block = openReader.ReadBytes(48);
                decryptedHeaderData = new BlewFish(blowfishKeys.groupHeadersKey).decryptBlock(block);
                numArray1[0] = new byte[336];
                Array.Copy((Array)numArray3, (Array)numArray1[0], 288);
                Array.Copy((Array)decryptedHeaderData, 0, (Array)numArray1[0], 288, decryptedHeaderData.Length);
            }
            else
            {
                switch (num1)
                {
                    case 8:
                        inFile.Seek(288L, SeekOrigin.Begin);
                        decryptedHeaderData = openReader.ReadBytes(48);
                        inFile.Seek(0L, SeekOrigin.Begin);
                        numArray1[0] = openReader.ReadBytes(336);
                        break;
                    case 327680:
                        inFile.Seek(288L, SeekOrigin.Begin);
                        decryptedHeaderData = openReader.ReadBytes(48);
                        inFile.Seek(0L, SeekOrigin.Begin);
                        numArray1[0] = openReader.ReadBytes(336);
                        break;
                    default:
                        inFile.Seek(288L, SeekOrigin.Begin);
                        decryptedHeaderData = openReader.ReadBytes(48);
                        inFile.Seek(0L, SeekOrigin.Begin);
                        numArray1[0] = openReader.ReadBytes(336);
                        break;
                }
            }
            IceFile.GroupHeader[] groupHeaderArray = this.readHeaders(decryptedHeaderData);
            this.groupOneCount = (int)groupHeaderArray[0].count;
            this.groupTwoCount = (int)groupHeaderArray[1].count;
            inFile.Seek(336L, SeekOrigin.Begin);
            numArray1[1] = new byte[0];
            numArray1[2] = new byte[0];
#if DEBUG
            if(num1 == 8 || num1 == 9)
            {
                Console.WriteLine("NGS Ice detected");
            }
#endif
            if (groupHeaderArray[0].decompSize > 0U)
                numArray1[1] = this.extractGroup(groupHeaderArray[0], openReader, (uint)(num1 & 1) > 0U, blowfishKeys.groupOneBlowfish[0], blowfishKeys.groupOneBlowfish[1], num1 == 8 || num1 == 9);
            if (groupHeaderArray[1].decompSize > 0U)
                numArray1[2] = this.extractGroup(groupHeaderArray[1], openReader, (uint)(num1 & 1) > 0U, blowfishKeys.groupTwoBlowfish[0], blowfishKeys.groupTwoBlowfish[1], num1 == 8 || num1 == 9);
            return numArray1;
        }

        private IceV4File.BlowfishKeys getBlowfishKeys(byte[] magicNumbers, int compSize)
        {
            IceV4File.BlowfishKeys blowfishKeys = new IceV4File.BlowfishKeys();
            uint temp_key = (uint)((int)BitConverter.ToUInt32(((IEnumerable<byte>)new Crc32().ComputeHash(magicNumbers, 124, 96)).Reverse<byte>().ToArray<byte>(), 0) ^ (int)BitConverter.ToUInt32(magicNumbers, 108) ^ compSize ^ 1129510338);
            uint key = this.getKey(magicNumbers, temp_key);
            blowfishKeys.groupOneBlowfish[0] = this.calcBlowfishKeys(magicNumbers, key);
            blowfishKeys.groupOneBlowfish[1] = this.getKey(magicNumbers, blowfishKeys.groupOneBlowfish[0]);
            blowfishKeys.groupTwoBlowfish[0] = blowfishKeys.groupOneBlowfish[0] >> 15 | blowfishKeys.groupOneBlowfish[0] << 17;
            blowfishKeys.groupTwoBlowfish[1] = blowfishKeys.groupOneBlowfish[1] >> 15 | blowfishKeys.groupOneBlowfish[1] << 17;
            uint x = blowfishKeys.groupOneBlowfish[0] << 13 | blowfishKeys.groupOneBlowfish[0] >> 19;
            blowfishKeys.groupHeadersKey = this.ReverseBytes(x);
            return blowfishKeys;
        }

        private uint getKey(byte[] keys, uint temp_key)
        {
            uint num1 = (uint)(((int)temp_key & (int)byte.MaxValue) + 93 & (int)byte.MaxValue);
            uint num2 = (uint)((int)(temp_key >> 8) + 63 & (int)byte.MaxValue);
            uint num3 = (uint)((int)(temp_key >> 16) + 69 & (int)byte.MaxValue);
            uint num4 = (uint)((int)(temp_key >> 24) - 58 & (int)byte.MaxValue);
            return (uint)((int)(byte)(((int)keys[(int)num2] << 7 | (int)keys[(int)num2] >> 1) & (int)byte.MaxValue) << 24 | (int)(byte)(((int)keys[(int)num4] << 6 | (int)keys[(int)num4] >> 2) & (int)byte.MaxValue) << 16 | (int)(byte)(((int)keys[(int)num1] << 5 | (int)keys[(int)num1] >> 3) & (int)byte.MaxValue) << 8) | (uint)(byte)(((int)keys[(int)num3] << 5 | (int)keys[(int)num3] >> 3) & (int)byte.MaxValue);
        }

        private uint calcBlowfishKeys(byte[] keys, uint temp_key)
        {
            uint temp_key1 = 2382545500U ^ temp_key;
            uint num1 = (uint)(613566757L * (long)temp_key1 >> 32);
            uint num2 = ((temp_key1 - num1 >> 1) + num1 >> 2) * 7U;
            for (int index = (int)temp_key1 - (int)num2 + 2; index > 0; --index)
                temp_key1 = this.getKey(keys, temp_key1);
            return (uint)((int)temp_key1 ^ 1129510338 ^ -850380898);
        }

        public byte[] getRawData(bool compress, bool forceUnencrypted) => this.packFile(this.header, this.combineGroup(this.groupOneFiles), this.combineGroup(this.groupTwoFiles), this.groupOneCount, this.groupTwoCount, compress, forceUnencrypted);

        private byte[] packFile(
          byte[] headerData,
          byte[] groupOneIn,
          byte[] groupTwoIn,
          int groupOneCount,
          int groupTwoCount,
          bool compress,
          bool forceUnencrypted = false)
        {
            byte[] compressedContents1 = this.getCompressedContents(groupOneIn, compress);
            byte[] compressedContents2 = this.getCompressedContents(groupTwoIn, compress);
            int compSize = headerData.Length + compressedContents1.Length + compressedContents2.Length;
            if (forceUnencrypted)
            {
                headerData[24] = (byte)0;
                headerData[25] = (byte)0;
                headerData[26] = (byte)0;
                headerData[27] = (byte)0;
                for (int index = 0; index < 256; ++index)
                    headerData[32 + index] = (byte)0;
                headerData[320] = (byte)0;
                headerData[321] = (byte)0;
                headerData[322] = (byte)0;
                headerData[323] = (byte)0;
                headerData[324] = (byte)0;
                headerData[325] = (byte)0;
                headerData[326] = (byte)0;
                headerData[327] = (byte)0;
                compress = false;
            }
            bool boolean = BitConverter.ToBoolean(headerData, 24);
            byte[] magicNumbers = new byte[256];
            Array.Copy((Array)headerData, 32, (Array)magicNumbers, 0, 256);
            IceV4File.BlowfishKeys blowfishKeys = this.getBlowfishKeys(magicNumbers, compSize);
            byte[] numArray1 = new byte[0];
            byte[] numArray2 = new byte[0];
            byte[] numArray3 = new byte[compSize];
            int destinationIndex1 = 336;
            int destinationIndex2 = 288;
            Array.Copy((Array)BitConverter.GetBytes(groupOneIn.Length), 0, (Array)headerData, destinationIndex2, 4);
            Array.Copy((Array)BitConverter.GetBytes(groupTwoIn.Length), 0, (Array)headerData, destinationIndex2 + 16, 4);
            if (compress)
            {
                if ((uint)groupOneIn.Length > 0U)
                {
                    Array.Copy((Array)BitConverter.GetBytes(compressedContents1.Length), 0, (Array)headerData, destinationIndex2 + 4, 4);
                    int num = 4;
                    if ((uint)groupTwoIn.Length > 0U)
                        num = 2;
                    Array.Copy((Array)BitConverter.GetBytes(groupOneIn.Length - num), 0, (Array)headerData, 320, 4);
                }
                if ((uint)groupTwoIn.Length > 0U)
                {
                    Array.Copy((Array)BitConverter.GetBytes(compressedContents2.Length), 0, (Array)headerData, destinationIndex2 + 20, 4);
                    int num = 3;
                    if ((uint)groupOneIn.Length > 0U)
                        num = 5;
                    Array.Copy((Array)BitConverter.GetBytes(groupTwoIn.Length - num), 0, (Array)headerData, 324, 4);
                }
            }
            else
            {
                headerData[destinationIndex2 + 4] = (byte)0;
                headerData[destinationIndex2 + 5] = (byte)0;
                headerData[destinationIndex2 + 6] = (byte)0;
                headerData[destinationIndex2 + 7] = (byte)0;
                headerData[destinationIndex2 + 20] = (byte)0;
                headerData[destinationIndex2 + 21] = (byte)0;
                headerData[destinationIndex2 + 22] = (byte)0;
                headerData[destinationIndex2 + 23] = (byte)0;
            }
            if ((uint)compressedContents1.Length > 0U)
            {
                byte[] numArray4 = this.packGroup(compressedContents1, blowfishKeys.groupOneBlowfish[0], blowfishKeys.groupOneBlowfish[1], boolean);
                Array.Copy((Array)numArray4, 0, (Array)numArray3, destinationIndex1, numArray4.Length);
                destinationIndex1 += numArray4.Length;
            }
            if ((uint)groupTwoIn.Length > 0U)
            {
                byte[] numArray4 = this.packGroup(compressedContents2, blowfishKeys.groupTwoBlowfish[0], blowfishKeys.groupTwoBlowfish[1], boolean);
                Array.Copy((Array)numArray4, 0, (Array)numArray3, destinationIndex1, numArray4.Length);
                int num = destinationIndex1 + numArray4.Length;
            }
            Array.Copy((Array)headerData, (Array)numArray3, 336);
            if (boolean)
            {
                BlewFish blewFish = new BlewFish(blowfishKeys.groupHeadersKey);
                byte[] block = new byte[48];
                Array.Copy((Array)headerData, 288, (Array)block, 0, 48);
                Array.Copy((Array)blewFish.encryptBlock(block), 0, (Array)numArray3, 288, 48);
            }
            Array.Copy((Array)BitConverter.GetBytes(compSize), 0, (Array)numArray3, 28, 4);
            return numArray3;
        }

        private class BlowfishKeys
        {
            public uint groupHeadersKey;

            public uint[] groupOneBlowfish { get; set; } = new uint[2];

            public uint[] groupTwoBlowfish { get; set; } = new uint[2];
        }
    }
}
