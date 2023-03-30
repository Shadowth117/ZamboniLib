// Decompiled with JetBrains decompiler
// Type: zamboni.IceFile
// Assembly: zamboni, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 73B487C9-8F41-4586-BEF5-F7D7BFBD4C55
// Assembly location: D:\Downloads\zamboni_ngs (3)\zamboni.exe

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Zamboni.IceFileFormats;

namespace Zamboni
{
    public abstract class IceFile
    {
        protected int decryptShift = 16;

        /// <summary>
        ///     Group1 Files
        /// </summary>
        public byte[][] groupOneFiles { get; set; }

        /// <summary>
        ///     Group2 Files
        /// </summary>
        public byte[][] groupTwoFiles { get; set; }

        /// <summary>
        ///     Header of Ice file
        /// </summary>
        public byte[] header { get; set; }

        protected abstract int SecondPassThreshold { get; }

        /// <summary>
        ///     Load Ice File
        /// </summary>
        /// <param name="inStream"></param>
        /// <returns></returns>
        /// <exception cref="ZamboniException"></exception>
        public static IceFile LoadIceFile(Stream inStream)
        {
            inStream.Seek(8L, SeekOrigin.Begin);
            int num = inStream.ReadByte();
            inStream.Seek(0L, SeekOrigin.Begin);
            IceFile iceFile;
            switch (num)
            {
                case 3:
                    iceFile = new IceV3File(inStream);
                    break;
                case 4:
                    iceFile = new IceV4File(inStream);
                    break;
                case 5:
                    iceFile = new IceV5File(inStream);
                    break;
                case 6:
                    iceFile = new IceV5File(inStream);
                    break;
                case 7:
                    iceFile = new IceV5File(inStream);
                    break;
                case 8:
                    iceFile = new IceV5File(inStream);
                    break;
                case 9:
                    iceFile = new IceV5File(inStream);
                    break;
                default:
                    throw new ZamboniException("Invalid version: " + num);
            }

            inStream.Dispose();
            return iceFile;
        }

        /// <summary>
        ///     Get File name from bytes
        /// </summary>
        /// <param name="fileToWrite"></param>
        /// <returns></returns>
        public static string getFileName(byte[] fileToWrite)
        {
            return getFileName(fileToWrite, -1);
        }

        /// <summary>
        ///     Get File name from bytes
        /// </summary>
        /// <param name="fileToWrite"></param>
        /// <returns></returns>
        public static string getFileName(byte[] fileToWrite, int index = -1)
        {
            //Bounds check for file
            if (fileToWrite == null || fileToWrite.Length == 0)
            {
                return "nullFile";
            }

            if (fileToWrite[0] == 'N' && fileToWrite[1] == 'I' && fileToWrite[2] == 'F' && fileToWrite[3] == 'L')
            {
                if (index == -1)
                {
                    Debug.WriteLine("No index provided, files may overwrite each other.");
                }

                return $"namelessNIFLFile_{index}.bin";
            }

            //Handle headerless files. ICE Files, as a rule, do not seem to allow upper case characters. Here, we'll assume that normal non caps ascii is allowed. Outside that range probably isn't a normal file.
            bool isNotLowerCaseOrSpecialChar = fileToWrite[0] > 126 || fileToWrite[0] < 91;
            bool isNotANumberOrSpecialChar = fileToWrite[0] > 64 || fileToWrite[0] < 32;
            if (isNotLowerCaseOrSpecialChar && isNotANumberOrSpecialChar)
            {
                if (index == -1)
                {
                    Debug.WriteLine("No index provided, files may overwrite each other.");
                }

                return $"namelessFile_{index}.bin";
            }

            int int32 = BitConverter.ToInt32(fileToWrite, 0x10);
            return Encoding.ASCII.GetString(fileToWrite, 0x40, int32).TrimEnd(new char[1]);
        }

        protected byte[][] splitGroup(byte[] groupToSplit, int fileCount)
        {
            byte[][] fileArray = new byte[fileCount][];
            int sourceIndex = 0;

            //Bounds check for group
            if (groupToSplit == null || groupToSplit.Length == 0)
            {
                return fileArray;
            }

            //Handle headerless files. ICE Files, as a rule, do not seem to allow upper case characters. Here, we'll assume that normal non caps ascii is allowed. Outside that range probably isn't a normal file.
            bool isNotLowerCaseOrSpecialChar = groupToSplit[0] > 126 || groupToSplit[0] < 91;
            bool isNotANumberOrSpecialChar = groupToSplit[0] > 64 || groupToSplit[0] < 32;
            if (groupToSplit[0] == 'N' && groupToSplit[1] == 'I' && groupToSplit[2] == 'F' && groupToSplit[3] == 'L')
            {
                for (int index = 0; index < fileCount && sourceIndex < groupToSplit.Length; ++index)
                {
                    if (groupToSplit[sourceIndex] == 'N' && groupToSplit[sourceIndex + 1] == 'I' &&
                        groupToSplit[sourceIndex + 2] == 'F' && groupToSplit[sourceIndex + 3] == 'L')
                    {
                        int size = BitConverter.ToInt32(groupToSplit, sourceIndex + 0x14); //Main NIFL Size
                        int nof0Size = BitConverter.ToInt32(groupToSplit, sourceIndex + size + 0x4) + 0x8; //NOF0 size
                        nof0Size += 0x10 - (nof0Size % 0x10); //Add padding bytes
                        size += nof0Size + 0x10; //Add in NOF0 size and NEND bytes

                        fileArray[index] = new byte[size];
                        Array.Copy(groupToSplit, sourceIndex, fileArray[index], 0, size);
                        sourceIndex += size;
                    }
                    else
                    {
                        int namelessSize = groupToSplit.Length - sourceIndex;
                        fileArray[index] = new byte[namelessSize];

                        if (fileCount > index + 1)
                        {
                            Debug.WriteLine(
                                $"Unhandled file count {fileCount}, outputting nameless file for index {index} and remaining file data.");
                        }

                        Array.Copy(groupToSplit, sourceIndex, fileArray[index], 0, namelessSize);
                        return fileArray;
                    }
                }

                return fileArray;
            }

            if (isNotLowerCaseOrSpecialChar && isNotANumberOrSpecialChar)
            {
                fileArray[0] = groupToSplit;
                if (fileCount > 1)
                {
                    Debug.WriteLine($"Unhandled file count {fileCount}, outputting only one nameless file.");
                }

                return fileArray;
            }

            for (int index = 0; index < fileCount && sourceIndex < groupToSplit.Length; ++index)
            {
                int size = BitConverter.ToInt32(groupToSplit, sourceIndex + 4);
                fileArray[index] = new byte[size];
                Array.Copy(groupToSplit, sourceIndex, fileArray[index], 0, size);
                sourceIndex += size;
            }

            return fileArray;
        }

        protected byte[] combineGroup(byte[][] filesToJoin, bool headerLess = true)
        {
            List<byte> outBytes = new List<byte>();
            for (int i = 0; i < filesToJoin.Length; i++)
            {
                //Apply file padding as we need it.
                int iceFileSize = BitConverter.ToInt32(filesToJoin[i], 0x4);
                int potentialPadding = 0x10 - (iceFileSize % 0x10);

                if (potentialPadding > 0 && potentialPadding != 0x10)
                {
                    Array.Copy(BitConverter.GetBytes(iceFileSize + potentialPadding), 0, filesToJoin[i], 0x4, 0x4);
                    outBytes.AddRange(filesToJoin[i]);
                    outBytes.AddRange(new byte[potentialPadding]);
                }
                else
                {
                    outBytes.AddRange(filesToJoin[i]);
                }
            }

            return outBytes.ToArray();
        }

        protected byte[] decryptGroup(byte[] buffer, uint key1, uint key2, bool v3Decrypt)
        {
            byte[] block1 = new byte[buffer.Length];
            if (v3Decrypt == false)
            {
                block1 = FloatageFish.decrypt_block(buffer, (uint)buffer.Length, key1, decryptShift);
            }
            else
            {
                Array.Copy(buffer, 0, block1, 0, buffer.Length);
            }

            byte[] block2 = new BlewFish(ReverseBytes(key1)).decryptBlock(block1);
            byte[] numArray = block2;
            if (block2.Length <= SecondPassThreshold && v3Decrypt == false)
            {
                numArray = new BlewFish(ReverseBytes(key2)).decryptBlock(block2);
            }

            return numArray;
        }

        public uint ReverseBytes(uint x)
        {
            x = (x >> 16) | (x << 16);
            return ((x & 4278255360U) >> 8) | (uint)(((int)x & 16711935) << 8);
        }

        protected GroupHeader[] readHeaders(byte[] decryptedHeaderData)
        {
            GroupHeader[] groupHeaderArray = new GroupHeader[2] { new GroupHeader(), null };
            groupHeaderArray[0].decompSize = BitConverter.ToUInt32(decryptedHeaderData, 0);
            groupHeaderArray[0].compSize = BitConverter.ToUInt32(decryptedHeaderData, 4);
            groupHeaderArray[0].count = BitConverter.ToUInt32(decryptedHeaderData, 8);
            groupHeaderArray[0].CRC = BitConverter.ToUInt32(decryptedHeaderData, 12);
            groupHeaderArray[1] = new GroupHeader();
            groupHeaderArray[1].decompSize = BitConverter.ToUInt32(decryptedHeaderData, 16);
            groupHeaderArray[1].compSize = BitConverter.ToUInt32(decryptedHeaderData, 20);
            groupHeaderArray[1].count = BitConverter.ToUInt32(decryptedHeaderData, 24);
            groupHeaderArray[1].CRC = BitConverter.ToUInt32(decryptedHeaderData, 28);
            return groupHeaderArray;
        }

        protected byte[] extractGroup(
            GroupHeader header,
            BinaryReader openReader,
            bool encrypt,
            uint groupOneTempKey,
            uint groupTwoTempKey,
            bool ngsMode,
            bool v3Decrypt = false)
        {
            byte[] buffer = openReader.ReadBytes((int)header.getStoredSize());
            byte[] inData = !encrypt ? buffer : decryptGroup(buffer, groupOneTempKey, groupTwoTempKey, v3Decrypt);
            return header.compSize <= 0U ? inData :
                !ngsMode ? decompressGroup(inData, header.decompSize) : decompressGroupNgs(inData, header.decompSize);
        }

        protected byte[] decompressGroup(byte[] inData, uint bufferLength)
        {
            byte[] input = new byte[inData.Length];
            Array.Copy(inData, input, input.Length);
            for (int index = 0; index < input.Length; ++index)
            {
                input[index] ^= 149;
            }

            return PrsCompDecomp.Decompress(input, bufferLength);
        }

        protected byte[] decompressGroupNgs(byte[] inData, uint bufferLength)
        {
            return Oodle.Decompress(inData, bufferLength);
        }

        protected byte[] compressGroupNgs(byte[] buffer,
            Oodle.CompressorLevel compressorLevel = Oodle.CompressorLevel.Fast)
        {
            return Oodle.OodleCompress(buffer, compressorLevel);
        }

        protected byte[] getCompressedContents(byte[] buffer, bool compress,
            Oodle.CompressorLevel compressorLevel = Oodle.CompressorLevel.Fast)
        {
            if ((uint)buffer.Length <= 0U || compress == false)
            {
                return buffer;
            }

            return compressGroupNgs(buffer, compressorLevel);
            if (!compress || (uint)buffer.Length <= 0U)
            {
                return buffer;
            }

            byte[] numArray = PrsCompDecomp.compress(buffer);
            for (int index = 0; index < numArray.Length; ++index)
            {
                numArray[index] ^= 149;
            }

            return numArray;
        }

        protected byte[] packGroup(byte[] buffer, uint key1, uint key2, bool encrypt)
        {
            if (!encrypt)
            {
                return buffer;
            }

            byte[] block = buffer;
            if (buffer.Length <= SecondPassThreshold)
            {
                block = new BlewFish(ReverseBytes(key2)).encryptBlock(buffer);
            }

            byte[] data_block = new BlewFish(ReverseBytes(key1)).encryptBlock(block);
            return FloatageFish.decrypt_block(data_block, (uint)data_block.Length, key1);
        }

        public class GroupHeader
        {
            public uint compSize;
            public uint count;
            public uint CRC;
            public uint decompSize;

            public uint getStoredSize()
            {
                return compSize > 0U ? compSize : decompSize;
            }
        }
    }
}
