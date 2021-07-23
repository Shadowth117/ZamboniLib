using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zamboni
{
    public class IceHeaderStructures
    {
		public class IceArchiveHeader
		{
			public uint signature = 0x454349;  // 'ICE'
			public uint reserve1;   // reserve
			public uint version = 0x4;    // version. Usually we would write back to v4
			public uint const80 = 0x80;         // 0x80

			public uint constFF = 0xFF;         // 0xFF,0x350
			public uint crc32;      // CRC32
			public uint encFlag = 1;         // 1(Encryption flag?) Usually just 1. Changes to 8 or 9 for Kraken.
			public uint filesize;   // File size

			public byte[] emptyBytes = new byte[0x100];

			public groupStruct group1Header;
			public groupStruct group2Header;

			public uint group1Size;      // Size of Group1
			public uint group2Size;      // Size of Group2
			public uint key;             // key
			public uint reserve2;        // reserve

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

				outBytes.AddRange(groupStruct.GetBytes(this.group1Header));
				outBytes.AddRange(groupStruct.GetBytes(this.group2Header));
				outBytes.AddRange(BitConverter.GetBytes(group1Size));
				outBytes.AddRange(BitConverter.GetBytes(group2Size));
				outBytes.AddRange(BitConverter.GetBytes(key));
				outBytes.AddRange(BitConverter.GetBytes(reserve2));

				return outBytes.ToArray();
			}
		}

		public struct groupStruct
        {
			public uint originalSize;  // Size size before compression, Size after uncompress
			public uint dataSize;      // Size before uncompress, Size after compression
			public uint fileCount;     // File count
			public uint crc32;         // CRC32

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

		//Ice Archive files all have these before extraction. Their size can vary, but they're typically 0x50 or 0x60 bytes. When repacking, the variation is not a huge consideration.
		public class IceFileHeader
        {
			public byte[] extension = new byte[0x4]; //File extension, up to 4 bytes of utf8
			public uint fileSize;    //File size with this header
			public uint dataSize;    //File size sans the header
			public uint headerSize = 0x60;  //Header size

			public uint filenameLength; //Length of filename. Includes null character if not ending at multiple of 0x10
			public uint field_0x14 = 1;     //Always 0x1. Unknown use
			public uint reserve0;
			public uint reserve1;

			public byte[] reserveBytes = new byte[0x20];

			public byte[] fileNameBytes = new byte[0x20];

			public IceFileHeader(string fileName, uint givenFileSize)
            {
				string ext = Path.GetExtension(fileName).Replace(".", "");
				extension = Encoding.UTF8.GetBytes(ext);
				Array.Resize(ref extension, 4);
				dataSize = givenFileSize;

				//Properly write filename length
				var fileNameTemp = Path.GetFileName(fileName);
				filenameLength = (uint)fileNameTemp.Length;
				if(filenameLength % 0x10 != 0)
                {
					filenameLength += 1; //This string technically has a null character if it's not ending right at the line
                }

				var tempBytes = Encoding.UTF8.GetBytes(fileNameTemp);
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
