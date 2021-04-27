using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZamboniLessCrashi
{
    public class IceHeaderStructures
    {
		public uint signature = 0x454349;  // 'ICE'
		public uint reserve1;   // reserve
		public uint version;    // version
		public uint const80 = 0x80;         // 0x80
		
		public uint constFF = 0xFF;         // 0xFF,0x350
		public uint crc32;      // CRC32
		public uint encFlag;         // 1(Encryption flag?)
		public uint filesize;   // File size

		public groupStruct group1Header;
		public groupStruct group2Header;

		public uint group1Size;      // Size of Group1
		public uint group2Size;      // Size of Group2
		public uint key;             // key
		public uint reserve2;        // reserve

		public struct groupStruct
        {
			public uint originalSize;  // Size size before compression, Size after uncompress
			public uint dataSize;      // Size before uncompress, Size after compression
			public uint fileCount;     // File count
			public uint crc32;         // CRC32
		}
	}
}
