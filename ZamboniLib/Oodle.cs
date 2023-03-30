// Decompiled with JetBrains decompiler
// Type: PhilLibX.Compression.Oodle
// Assembly: zamboni, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 73B487C9-8F41-4586-BEF5-F7D7BFBD4C55
// Assembly location: D:\Downloads\zamboni_ngs (3)\zamboni.exe

using System;
using System.Runtime.InteropServices;

namespace Zamboni
{
    /// <summary>
    ///     ooz.dll wrapper
    /// </summary>
    public class Oodle
    {
        public enum CompressorLevel
        {
            None,
            SuperFast,
            VeryFast,
            Fast,
            Normal,
            Optimal1,
            Optimal2,
            Optimal3,
            Optimal4,
            Optimal5
        }

        public enum CompressorType
        {
            Kraken = 8,
            Mermaid = 9,
            Selkie = 11,
            Leviathan = 13
        }

        /// <summary>
        ///     ooz.dll x86 binary filename
        /// </summary>
        private const string OOZ_X86 = "ooz.x86.dll";

        /// <summary>
        ///     ooz.dll x64 binary filename
        /// </summary>
        private const string OOZ_X64 = "ooz.x64.dll";

        public static CompressOptions GetDefaultCompressOpts(int level)
        {
            CompressOptions compress_options_level5 =
                CompressOptions.GetCompressOptions(0, 0, 0, 0x40000, 0, 0, 0x100, 4, 0, 0x400000, 1, 0);
            CompressOptions compress_options_level4 =
                CompressOptions.GetCompressOptions(0, 0, 0, 0x40000, 0, 0, 0x100, 2, 0, 0x400000, 1, 0);
            CompressOptions compress_options_level0 =
                CompressOptions.GetCompressOptions(0, 0, 0, 0x40000, 0, 0, 0x100, 1, 0, 0x400000, 0, 0);
            return level >= 5 ? compress_options_level5 :
                level >= 4 ? compress_options_level4 : compress_options_level0;
        }

        public static int GetCompressedBufferSizeNeeded(int size)
        {
            return size + (274 * ((size + 0x3FFFF) / 0x40000));
        }

        [DllImport(OOZ_X86, EntryPoint = "Kraken_Decompress", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Kraken_Decompress32(
            byte[] buffer,
            uint bufferSize,
            byte[] result,
            uint outputBufferSize);

        [DllImport(OOZ_X86, EntryPoint = "Compress", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Compress32(
            int compressorId,
            byte[] src_in,
            byte[] dst_in,
            int src_size,
            int compressorLevel,
            IntPtr compressorOptions,
            IntPtr src_window_base,
            IntPtr c_void
        );

        [DllImport(OOZ_X64, EntryPoint = "Kraken_Decompress", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Kraken_Decompress64(
            byte[] buffer,
            uint bufferSize,
            byte[] result,
            uint outputBufferSize);

        [DllImport(OOZ_X64, EntryPoint = "Compress", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Compress64(
            int compressorId,
            byte[] src_in,
            byte[] dst_in,
            int src_size,
            int compressorLevel,
            IntPtr compressorOptions,
            IntPtr src_window_base,
            IntPtr c_void
        );

        /// <summary>
        ///     Decompress via Kraken
        /// </summary>
        /// <param name="input">Input binary</param>
        /// <param name="decompressedLength">output binary size</param>
        /// <returns></returns>
        /// <exception cref="ZamboniException">DLL not found</exception>
        public static byte[] Decompress(byte[] input, long decompressedLength)
        {
            byte[] result = new byte[decompressedLength];
            if (IntPtr.Size == 8)
            {
                return Kraken_Decompress64(input, (uint)input.Length, result, (uint)decompressedLength) == 0L
                    ? null
                    : result;
            }

            if (IntPtr.Size == 4)
            {
                return Kraken_Decompress32(input, (uint)input.Length, result, (uint)decompressedLength) == 0L
                    ? null
                    : result;
            }

            throw new ZamboniException("Could not load ooz. Place ooz.x86.dll and ooz.x64.dll in the same directory.");
        }

        /// <summary>
        ///     Compress via Kraken
        /// </summary>
        /// <param name="input">Input binary</param>
        /// <param name="level">Comporessor Level</param>
        /// <returns></returns>
        /// <exception cref="ZamboniException">Dll not found.</exception>
        public static byte[] Compress(byte[] input, CompressorLevel level = CompressorLevel.Fast)
        {
            CompressOptions compressOptions = GetDefaultCompressOpts((int)level);
            IntPtr compressOptionsPtr = Marshal.AllocHGlobal(Marshal.SizeOf<CompressOptions>());
            Marshal.StructureToPtr(compressOptions, compressOptionsPtr, false);

            byte[] result = new byte[GetCompressedBufferSizeNeeded(input.Length)];
            int compSize;
            if (IntPtr.Size == 8)
            {
                compSize = Compress64((int)CompressorType.Kraken, input, result, input.Length, (int)level,
                    compressOptionsPtr, IntPtr.Zero, IntPtr.Zero);
            }
            else if (IntPtr.Size == 4)
            {
                compSize = Compress32((int)CompressorType.Kraken, input, result, input.Length, (int)level,
                    compressOptionsPtr, IntPtr.Zero, IntPtr.Zero);
            }
            else
            {
                throw new ZamboniException(
                    "Could not load ooz. Place ooz.x86.dll and ooz.x64.dll in the same directory.");
            }

            Marshal.FreeHGlobal(compressOptionsPtr);

            Array.Resize(ref result, compSize);

            return result;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CompressOptions
        {
            public int unknown_0;
            public int min_match_length;
            public int seek_chunk_reset;
            public int seek_chunk_len;

            public int unknown_1;
            public int dictionary_size;
            public int space_speed_tradeoff_bytes;
            public int unknown_2;

            public int make_qhcrc;
            public int max_local_dictionary_size;
            public int make_long_range_matcher;
            public int hash_bits;

            public static CompressOptions GetCompressOptions(int Unknown_0, int Min_match_length, int Seek_chunk_reset,
                int Seek_chunk_len, int Unknown_1, int Dictionary_size, int Space_speed_tradeoff_bytes, int Unknown_2,
                int Make_qhcrc, int Max_local_dictionary_size, int Make_long_range_matcher, int Hash_bits)
            {
                CompressOptions options = new CompressOptions();
                options.unknown_0 = Unknown_0;
                options.min_match_length = Min_match_length;
                options.seek_chunk_reset = Seek_chunk_reset;
                options.seek_chunk_len = Seek_chunk_len;

                options.unknown_1 = Unknown_1;
                options.dictionary_size = Dictionary_size;
                options.space_speed_tradeoff_bytes = Space_speed_tradeoff_bytes;
                options.unknown_2 = Unknown_2;

                options.make_qhcrc = Make_qhcrc;
                options.max_local_dictionary_size = Max_local_dictionary_size;
                options.make_long_range_matcher = Make_long_range_matcher;
                options.hash_bits = Hash_bits;

                return options;
            }
        }

        //This can be used if you have an official oodle dll from another game, but ooz should work fine so far.
        private const string OodleLibraryPath = "oo2core_8_win64_";
        [DllImport("oo2core_8_win64_", CallingConvention = CallingConvention.Cdecl)]
        private static extern long OodleLZ_GetCompressedBufferSizeNeeded(
            long bufferSize);

        [DllImport("oo2core_8_win64_", CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong OodleLZ_Compress(
            CompressorType codec, 
            byte[] buffer, 
            long bufferSize, 
            byte[] result,
            CompressorLevel level,
            IntPtr opts, 
            long offs, 
            ulong unk, 
            IntPtr scr, 
            long scrSize);

        [DllImport("oo2core_8_win64_", CallingConvention = CallingConvention.Cdecl)]
        private static extern long OodleLZ_Decompress(
          byte[] buffer,
          long bufferSize,
          byte[] result,
          long outputBufferSize,
          int a,
          int b,
          int c,
          long d,
          long e,
          long f,
          long g,
          long h,
          long i,
          int ThreadModule);

        public static byte[] OodleDecompress(byte[] input, long decompressedLength)
        {
            byte[] result = new byte[decompressedLength];
            return Oodle.OodleLZ_Decompress(input, (long)input.Length, result, decompressedLength, 0, 0, 0, 0L, 0L, 0L, 0L, 0L, 0L, 3) == 0L ? (byte[])null : result;
        }

        public static byte[] OodleCompress(byte[] source, CompressorLevel level)
        {
            return OodleCompress(source, CompressorType.Kraken, level);
        }

        public static byte[] OodleCompress(byte[] source, CompressorType codec, CompressorLevel level)
        {
            byte[] result = new byte[(int)(source.Length * 1.1)];
            ulong resultSize = OodleLZ_Compress(codec, source, source.LongLength, result, level, IntPtr.Zero, 0, 0, IntPtr.Zero, 0);
            Array.Resize(ref result, (int)resultSize);
            return result;
        }
    }
}
