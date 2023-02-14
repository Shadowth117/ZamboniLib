// Decompiled with JetBrains decompiler
// Type: zamboni.IceV4File
// Assembly: zamboni, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 73B487C9-8F41-4586-BEF5-F7D7BFBD4C55
// Assembly location: D:\Downloads\zamboni_ngs (3)\zamboni.exe

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Zamboni.IceFileFormats
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
            byte[][] numArray = splitGroups(inFile);
            header = numArray[0];
            groupOneFiles = splitGroup(numArray[1], groupOneCount);
            groupTwoFiles = splitGroup(numArray[2], groupTwoCount);
        }

        public IceV4File(byte[] headerData, byte[][] groupOneIn, byte[][] groupTwoIn)
        {
            header = headerData;
            groupOneFiles = groupOneIn;
            groupTwoFiles = groupTwoIn;
            groupOneCount = groupOneIn.Length;
            groupTwoCount = groupTwoIn.Length;
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
            BlowfishKeys blowfishKeys = getBlowfishKeys(openReader.ReadBytes(256), compSize);
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
                Array.Copy(numArray3, numArray1[0], 288);
                Array.Copy(decryptedHeaderData, 0, numArray1[0], 288, decryptedHeaderData.Length);
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
            GroupHeader[] groupHeaderArray = readHeaders(decryptedHeaderData);
            groupOneCount = (int)groupHeaderArray[0].count;
            groupTwoCount = (int)groupHeaderArray[1].count;
            inFile.Seek(336L, SeekOrigin.Begin);
            numArray1[1] = new byte[0];
            numArray1[2] = new byte[0];

            if (groupHeaderArray[0].decompSize > 0U)
                numArray1[1] = extractGroup(groupHeaderArray[0], openReader, (uint)(num1 & 1) > 0U, blowfishKeys.groupOneBlowfish[0], blowfishKeys.groupOneBlowfish[1], num1 == 8 || num1 == 9);
            if (groupHeaderArray[1].decompSize > 0U)
                numArray1[2] = extractGroup(groupHeaderArray[1], openReader, (uint)(num1 & 1) > 0U, blowfishKeys.groupTwoBlowfish[0], blowfishKeys.groupTwoBlowfish[1], num1 == 8 || num1 == 9);

            return numArray1;
        }

        private BlowfishKeys getBlowfishKeys(byte[] magicNumbers, int compSize)
        {
            BlowfishKeys blowfishKeys = new BlowfishKeys();
            uint temp_key = (uint)((int)BitConverter.ToUInt32(((IEnumerable<byte>)new Crc32().ComputeHash(magicNumbers, 124, 96)).Reverse().ToArray(), 0) ^ (int)BitConverter.ToUInt32(magicNumbers, 108) ^ compSize ^ 1129510338);
            uint key = getKey(magicNumbers, temp_key);
            blowfishKeys.groupOneBlowfish[0] = calcBlowfishKeys(magicNumbers, key);
            blowfishKeys.groupOneBlowfish[1] = getKey(magicNumbers, blowfishKeys.groupOneBlowfish[0]);
            blowfishKeys.groupTwoBlowfish[0] = blowfishKeys.groupOneBlowfish[0] >> 15 | blowfishKeys.groupOneBlowfish[0] << 17;
            blowfishKeys.groupTwoBlowfish[1] = blowfishKeys.groupOneBlowfish[1] >> 15 | blowfishKeys.groupOneBlowfish[1] << 17;
            uint x = blowfishKeys.groupOneBlowfish[0] << 13 | blowfishKeys.groupOneBlowfish[0] >> 19;
            blowfishKeys.groupHeadersKey = ReverseBytes(x);
            return blowfishKeys;
        }

        private uint getKey(byte[] keys, uint temp_key)
        {
            uint num1 = (uint)(((int)temp_key & byte.MaxValue) + 93 & byte.MaxValue);
            uint num2 = (uint)((int)(temp_key >> 8) + 63 & byte.MaxValue);
            uint num3 = (uint)((int)(temp_key >> 16) + 69 & byte.MaxValue);
            uint num4 = (uint)((int)(temp_key >> 24) - 58 & byte.MaxValue);
            return (uint)((byte)((keys[(int)num2] << 7 | keys[(int)num2] >> 1) & byte.MaxValue) << 24 | (byte)((keys[(int)num4] << 6 | keys[(int)num4] >> 2) & byte.MaxValue) << 16 | (byte)((keys[(int)num1] << 5 | keys[(int)num1] >> 3) & byte.MaxValue) << 8) | (byte)((keys[(int)num3] << 5 | keys[(int)num3] >> 3) & byte.MaxValue);
        }

        private uint calcBlowfishKeys(byte[] keys, uint temp_key)
        {
            uint temp_key1 = 2382545500U ^ temp_key;
            uint num1 = (uint)(613566757L * temp_key1 >> 32);
            uint num2 = ((temp_key1 - num1 >> 1) + num1 >> 2) * 7U;
            for (int index = (int)temp_key1 - (int)num2 + 2; index > 0; --index)
                temp_key1 = getKey(keys, temp_key1);
            return (uint)((int)temp_key1 ^ 1129510338 ^ -850380898);
        }

        public byte[] getRawData(bool compress, bool forceUnencrypted, Oodle.CompressorLevel compressorLevel) => packFile(header, combineGroup(groupOneFiles), combineGroup(groupTwoFiles), groupOneCount, groupTwoCount, compress, forceUnencrypted, compressorLevel);
        public byte[] getRawData(bool compress, bool forceUnencrypted) => packFile(header, combineGroup(groupOneFiles), combineGroup(groupTwoFiles), groupOneCount, groupTwoCount, compress, forceUnencrypted, Oodle.CompressorLevel.Fast);

        private byte[] packFile(
          byte[] headerData,
          byte[] groupOneIn,
          byte[] groupTwoIn,
          int groupOneCount,
          int groupTwoCount,
          bool compress,
          bool forceUnencrypted = false,
          Oodle.CompressorLevel compressorLevel = Oodle.CompressorLevel.Fast)
        {
            //Set group data in ICE header
            Array.Copy(BitConverter.GetBytes(groupOneIn.Length), 0, headerData, 0x120, 0x4);
            Array.Copy(BitConverter.GetBytes(groupTwoIn.Length), 0, headerData, 0x130, 0x4);
            Array.Copy(BitConverter.GetBytes(groupOneCount), 0, headerData, 0x128, 0x4);
            Array.Copy(BitConverter.GetBytes(groupTwoCount), 0, headerData, 0x138, 0x4);
            Array.Copy(BitConverter.GetBytes(groupOneIn.Length), 0, headerData, 0x140, 0x4);
            Array.Copy(BitConverter.GetBytes(groupTwoIn.Length), 0, headerData, 0x144, 0x4);

            byte[] compressedContents1 = getCompressedContents(groupOneIn, compress, compressorLevel);
            byte[] compressedContents2 = getCompressedContents(groupTwoIn, compress, compressorLevel);
            int compSize = headerData.Length + compressedContents1.Length + compressedContents2.Length;

            //Set main CRC (Should be done after potential compression, but before encryption)
            List<byte> crcCombo = new List<byte>(compressedContents1);
            crcCombo.AddRange(compressedContents2);
            var mainCrc = new Crc32Alt().GetCrc32(crcCombo.ToArray());
            crcCombo.Clear();
            Array.Copy(BitConverter.GetBytes(mainCrc), 0, headerData, 0x14, 0x4);

            if(compress)
            {
                forceUnencrypted = true;
            }
            if (forceUnencrypted)
            {
                //Set encryption flag to 0
                /*
                headerData[24] = 0;
                headerData[25] = 0;
                headerData[26] = 0;
                headerData[27] = 0;*/
                
                //Set array to 0
                for (int index = 0; index < 256; ++index)
                    headerData[32 + index] = 0;

                //Set encrypted group 1 size to 0
                headerData[0x140] = 0;
                headerData[0x141] = 0;
                headerData[0x142] = 0;
                headerData[0x143] = 0;

                //Set encrypted group 2 size to 0
                headerData[0x144] = 0;
                headerData[0x145] = 0;
                headerData[0x146] = 0;
                headerData[0x147] = 0;
                //compress = false;
            }
            bool useEncryption = forceUnencrypted ? false : BitConverter.ToBoolean(headerData, 0x18);
            byte[] magicNumbers = new byte[0x100];
            Array.Copy(headerData, 0x20, magicNumbers, 0, 0x100);
            BlowfishKeys blowfishKeys = getBlowfishKeys(magicNumbers, compSize);
            byte[] numArray1 = new byte[0];
            byte[] numArray2 = new byte[0];
            byte[] outBytes = new byte[compSize];
            int destinationIndex1 = 0x150;
            int destinationIndex2 = 0x120;
            Array.Copy(BitConverter.GetBytes(groupOneIn.Length), 0, headerData, destinationIndex2, 4);
            Array.Copy(BitConverter.GetBytes(groupTwoIn.Length), 0, headerData, destinationIndex2 + 0x10, 4);
            if (compress)
            {
                if ((uint)groupOneIn.Length > 0U)
                {
                    Array.Copy(BitConverter.GetBytes(compressedContents1.Length), 0, headerData, destinationIndex2 + 4, 4);
                    int num = 4;
                    if ((uint)groupTwoIn.Length > 0U)
                        num = 2;
                    //Array.Copy(BitConverter.GetBytes(groupOneIn.Length - num), 0, headerData, 0x140, 4);
                }
                if ((uint)groupTwoIn.Length > 0U)
                {
                    Array.Copy(BitConverter.GetBytes(compressedContents2.Length), 0, headerData, destinationIndex2 + 0x14, 4);
                    int num = 3;
                    if ((uint)groupOneIn.Length > 0U)
                        num = 5;
                    //Array.Copy(BitConverter.GetBytes(groupTwoIn.Length - num), 0, headerData, 0x144, 4);
                }
            }
            else
            {
                headerData[destinationIndex2 + 4] = 0;
                headerData[destinationIndex2 + 5] = 0;
                headerData[destinationIndex2 + 6] = 0;
                headerData[destinationIndex2 + 7] = 0;
                headerData[destinationIndex2 + 0x14] = 0;
                headerData[destinationIndex2 + 0x15] = 0;
                headerData[destinationIndex2 + 0x16] = 0;
                headerData[destinationIndex2 + 0x17] = 0;
            }
            if ((uint)compressedContents1.Length > 0U)
            {
                byte[] numArray4 = packGroup(compressedContents1, blowfishKeys.groupOneBlowfish[0], blowfishKeys.groupOneBlowfish[1], useEncryption);
                Array.Copy(numArray4, 0, outBytes, destinationIndex1, numArray4.Length);
                destinationIndex1 += numArray4.Length;
            }
            if ((uint)compressedContents2.Length > 0U)
            {
                byte[] numArray4 = packGroup(compressedContents2, blowfishKeys.groupTwoBlowfish[0], blowfishKeys.groupTwoBlowfish[1], useEncryption);
                Array.Copy(numArray4, 0, outBytes, destinationIndex1, numArray4.Length);
                int num = destinationIndex1 + numArray4.Length;
            }

            //CRC32 for groups
            Array.Copy(BitConverter.GetBytes(new Crc32Alt().GetCrc32(compressedContents1)), 0, headerData, 0x12C, 0x4);
            Array.Copy(BitConverter.GetBytes(new Crc32Alt().GetCrc32(compressedContents2)), 0, headerData, 0x13C, 0x4);

            Array.Copy(headerData, outBytes, 0x150);

            //Only necessary when encrypted for classic compression
            if (useEncryption && compress == false)
            {
                BlewFish blewFish = new BlewFish(blowfishKeys.groupHeadersKey);
                byte[] block = new byte[0x30];
                Array.Copy(headerData, 0x120, block, 0, 0x30);
                Array.Copy(blewFish.encryptBlock(block), 0, outBytes, 0x120, 0x30);
            }
            Array.Copy(BitConverter.GetBytes(compSize), 0, outBytes, 28, 4);
            return outBytes;
        }

        private class BlowfishKeys
        {
            public uint groupHeadersKey;

            public uint[] groupOneBlowfish { get; set; } = new uint[2];

            public uint[] groupTwoBlowfish { get; set; } = new uint[2];
        }
    }
}
