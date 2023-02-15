using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Zamboni.IceFileFormats
{
    public class IceHeaderStructures
    {
        public class IceArchiveHeader
        {
            /// <summary>
            ///     0x80
            /// </summary>
            public uint const80 = 0x80;

            /// <summary>
            ///     0xFF,0x350
            /// </summary>
            public uint constFF = 0xFF;

            /// <summary>
            ///     CRC32
            /// </summary>
            public uint crc32;

            /// <summary>
            ///     Padding bytes
            /// </summary>
            public byte[] emptyBytes = new byte[0x100];

            /// <summary>
            ///     1(Encryption flag?) Usually just 1. Changes to 8 or 9 for Kraken.
            /// </summary>
            public uint encFlag = 1;

            /// <summary>
            ///     File size
            /// </summary>
            public uint filesize;

            /// <summary>
            ///     Group1
            /// </summary>
            public groupStruct group1Header;

            /// <summary>
            ///     Size of Group1
            /// </summary>
            public uint group1Size;

            /// <summary>
            ///     Group2
            /// </summary>
            public groupStruct group2Header;

            /// <summary>
            ///     Size of Group2
            /// </summary>
            public uint group2Size;

            /// <summary>
            ///     key
            /// </summary>
            public uint key;

            /// <summary>
            ///     reserve
            /// </summary>
            public uint reserve1;

            /// <summary>
            ///     reserve
            /// </summary>
            public uint reserve2;

            /// <summary>
            ///     'ICE'
            /// </summary>
            public uint signature = 0x454349;

            /// <summary>
            ///     version. Usually we would write back to v4
            /// </summary>
            public uint version = 0x4;

            /// <summary>
            ///     Get header binary data
            /// </summary>
            /// <returns></returns>
            public IceArchiveHeader()
            {
            }

            //Only Kraken compression is supported
            public IceArchiveHeader(bool compress)
            {
                encFlag = (uint)(compress ? 8 : 1);
            }

            public byte[] GetBytes()
            {
                List<byte> outBytes = new List<byte>();

                outBytes.AddRange(BitConverter.GetBytes(signature));
                outBytes.AddRange(BitConverter.GetBytes(reserve1));
                outBytes.AddRange(BitConverter.GetBytes(version));
                outBytes.AddRange(BitConverter.GetBytes(const80));

                outBytes.AddRange(BitConverter.GetBytes(0xFF));
                outBytes.AddRange(BitConverter.GetBytes(crc32));
                outBytes.AddRange(BitConverter.GetBytes(encFlag));
                outBytes.AddRange(BitConverter.GetBytes(filesize));

                outBytes.AddRange(emptyBytes);

                outBytes.AddRange(groupStruct.GetBytes(group1Header));
                outBytes.AddRange(groupStruct.GetBytes(group2Header));
                outBytes.AddRange(BitConverter.GetBytes(group1Size));
                outBytes.AddRange(BitConverter.GetBytes(group2Size));
                outBytes.AddRange(BitConverter.GetBytes(key));
                outBytes.AddRange(BitConverter.GetBytes(reserve2));

                return outBytes.ToArray();
            }
        }

        /// <summary>
        ///     Group data structure
        /// </summary>
        public struct groupStruct
        {
            /// <summary>
            ///     Size size before compression, Size after uncompress
            /// </summary>
            public uint originalSize;

            /// <summary>
            ///     Size before uncompress, Size after compression
            /// </summary>
            public uint dataSize;

            /// <summary>
            ///     File count
            /// </summary>
            public uint fileCount;

            /// <summary>
            ///     CRC32
            /// </summary>
            public uint crc32;

            /// <summary>
            ///     Get group binary data
            /// </summary>
            /// <param name="gp">Group</param>
            /// <returns></returns>
            public static byte[] GetBytes(groupStruct gp)
            {
                List<byte> outBytes = new List<byte>();
                outBytes.AddRange(BitConverter.GetBytes(gp.originalSize));
                outBytes.AddRange(BitConverter.GetBytes(gp.dataSize));
                outBytes.AddRange(BitConverter.GetBytes(gp.fileCount));
                outBytes.AddRange(BitConverter.GetBytes(gp.crc32));

                return outBytes.ToArray();
            }
        }

        /// <summary>
        ///     Ice Archive files all have these before extraction.
        ///     Their size can vary, but they're typically 0x50 or 0x60 bytes.
        ///     When repacking, the variation is not a huge consideration.
        /// </summary>
        public class IceFileHeader
        {
            /// <summary>
            ///     File size sans the header
            /// </summary>
            public uint dataSize;

            /// <summary>
            ///     File extension, up to 4 bytes of utf8
            /// </summary>
            public byte[] extension = new byte[0x4];

            /// <summary>
            ///     Always 0x1. Unknown use
            /// </summary>
            public uint field_0x14 = 1;

            public byte[] fileNameBytes = new byte[0x20];

            /// <summary>
            ///     Length of filename.
            ///     Includes null character if not ending at multiple of 0x10
            /// </summary>
            public uint filenameLength;

            /// <summary>
            ///     File size with this header
            /// </summary>
            public uint fileSize;

            /// <summary>
            ///     Header size
            /// </summary>
            public uint headerSize = 0x60;

            public uint reserve0;
            public uint reserve1;

            public byte[] reserveBytes = new byte[0x20];

            public IceFileHeader(string fileName, uint givenFileSize)
            {
                string ext = Path.GetExtension(fileName).Replace(".", "");
                extension = Encoding.UTF8.GetBytes(ext);
                Array.Resize(ref extension, 4);
                dataSize = givenFileSize;

                // Properly write filename length
                string fileNameTemp = Path.GetFileName(fileName);
                filenameLength = (uint)fileNameTemp.Length;

                byte[] tempBytes = Encoding.UTF8.GetBytes(fileNameTemp);
                Array.Resize(ref tempBytes, tempBytes.Length + 1);
                fileNameBytes = new byte[0x10 - (tempBytes.Length % 0x10) + tempBytes.Length];
                Array.Copy(tempBytes, 0, fileNameBytes, 0, tempBytes.Length);
                headerSize = 0x40 + (uint)fileNameBytes.Length;
                fileSize = givenFileSize + headerSize;
            }

            public IceFileHeader()
            {
            }

            public byte[] GetBytes()
            {
                List<byte> outBytes = new List<byte>();
                outBytes.AddRange(extension);
                outBytes.AddRange(BitConverter.GetBytes(fileSize));
                outBytes.AddRange(BitConverter.GetBytes(dataSize));
                outBytes.AddRange(BitConverter.GetBytes(headerSize));

                outBytes.AddRange(BitConverter.GetBytes(filenameLength));
                outBytes.AddRange(BitConverter.GetBytes(field_0x14));
                outBytes.AddRange(BitConverter.GetBytes(reserve0));
                outBytes.AddRange(BitConverter.GetBytes(reserve1));

                outBytes.AddRange(reserveBytes);
                outBytes.AddRange(fileNameBytes);

                return outBytes.ToArray();
            }
        }
    }
}
