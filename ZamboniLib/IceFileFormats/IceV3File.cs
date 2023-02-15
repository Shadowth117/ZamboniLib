using System.IO;

namespace Zamboni.IceFileFormats
{
    public class IceV3File : IceFile
    {
        public int groupOneCount;
        public int groupTwoCount;

        public IceV3File(Stream inFile)
        {
            byte[][] numArray = splitGroups(inFile);
            header = numArray[0];
            groupOneFiles = splitGroup(numArray[1], groupOneCount);
            groupTwoFiles = splitGroup(numArray[2], groupTwoCount);
        }

        public IceV3File(byte[] headerData, byte[][] groupOneIn, byte[][] groupTwoIn)
        {
            header = headerData;
            groupOneFiles = groupOneIn;
            groupTwoFiles = groupTwoIn;
        }

        protected override int SecondPassThreshold => 102400;

        private byte[][] splitGroups(Stream inFile)
        {
            byte[][] numArray1 = new byte[3][];
            BinaryReader openReader = new BinaryReader(inFile);
            numArray1[0] = openReader.ReadBytes(128);

            inFile.Seek(0x10, SeekOrigin.Begin); //Skip the ICE header

            //Read group info
            stGroup groupInfo = new stGroup();
            groupInfo.group1 = new GroupHeader();
            groupInfo.group2 = new GroupHeader();
            ReadGroupInfoGroup(openReader, groupInfo.group1);
            ReadGroupInfoGroup(openReader, groupInfo.group2);
            groupInfo.group1Size = openReader.ReadUInt32();
            groupInfo.group2Size = openReader.ReadUInt32();
            groupInfo.key = openReader.ReadUInt32();
            groupInfo.key = openReader.ReadUInt32();

            //Read crypt info
            stInfo info = new stInfo();
            info.r1 = openReader.ReadUInt32();
            info.crc32 = openReader.ReadUInt32();
            info.r2 = openReader.ReadUInt32();
            info.filesize = openReader.ReadUInt32();

            //Seek past padding/unused data
            inFile.Seek(0x30, SeekOrigin.Current);

            //Generate key
            uint key = groupInfo.group1Size;
            if (key > 0)
            {
                key = ReverseBytes(key);
            }
            else if (info.r2 > 0)
            {
                key = GetKey(groupInfo);
            }

            //Group 1
            if (groupInfo.group1.decompSize > 0)
            {
                numArray1[1] = extractGroup(groupInfo.group1, openReader, (info.r2 & 1) > 0U, key, 0,
                    info.r2 == 8 || info.r2 == 9, true);
            }

            //Group 2
            if (groupInfo.group2.decompSize > 0)
            {
                numArray1[2] = extractGroup(groupInfo.group2, openReader, (info.r2 & 1) > 0U, key, 0,
                    info.r2 == 8 || info.r2 == 9, true);
            }

            groupOneCount = (int)groupInfo.group1.count;
            groupTwoCount = (int)groupInfo.group2.count;

            return numArray1;
        }

        private void ReadGroupInfoGroup(BinaryReader openReader, GroupHeader grp)
        {
            grp.decompSize = openReader.ReadUInt32();
            grp.compSize = openReader.ReadUInt32();
            grp.count = openReader.ReadUInt32();
            grp.CRC = openReader.ReadUInt32();
        }

        //uint reversal from ice.exe
        private uint bswap(uint v)
        {
            uint r = v & 0xFF;
            r <<= 8;
            v >>= 8;
            r |= v & 0xFF;
            r <<= 8;
            v >>= 8;
            r |= v & 0xFF;
            r <<= 8;
            v >>= 8;
            r |= v & 0xFF;

            return r;
        }

        private uint GetKey(stGroup group)
        {
            return group.group1.decompSize ^ group.group2.decompSize ^ group.group2Size ^ group.key ^ 0xC8D7469A;
        }

        //Structs based on ice.exe naming
        public struct group
        {
            public uint originalSize;
            public uint dataSize;
            public uint fileCount;
            public uint crc32;
        }

        public struct stGroup
        {
            public GroupHeader group1;
            public GroupHeader group2;
            public uint group1Size;
            public uint group2Size;
            public uint key;
            public uint reserve;
        }

        public struct stInfo
        {
            public uint r1;
            public uint crc32;
            public uint r2;
            public uint filesize;
        }
    }
}
