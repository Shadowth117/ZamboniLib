// Decompiled with JetBrains decompiler
// Type: PhilLibX.Compression.Oodle
// Assembly: zamboni, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 73B487C9-8F41-4586-BEF5-F7D7BFBD4C55
// Assembly location: D:\Downloads\zamboni_ngs (3)\zamboni.exe

using System.Runtime.InteropServices;
using System;

namespace PhilLibX.Compression
{
    public class Oodle
    {

        private const string oozLibraryPath = "ooz";
        [DllImport("ooz", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Kraken_Decompress(
          byte[] buffer,
          uint bufferSize,
          byte[] result,
          uint outputBufferSize);

        public static unsafe byte[] Decompress(byte[] input, long decompressedLength)
        {
            byte[] result = new byte[decompressedLength];
            return Oodle.Kraken_Decompress(input, (uint)input.Length, result, (uint)decompressedLength) == 0L ? (byte[])null : result;
        }

        //This can be used if you have an official oodle dll from another game, but ooz should work fine so far.
        /*
        private const string OodleLibraryPath = "oo2core_8_win64_";
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

        public static byte[] Decompress(byte[] input, long decompressedLength)
        {
            byte[] result = new byte[decompressedLength];
            return Oodle.OodleLZ_Decompress(input, (long)input.Length, result, decompressedLength, 0, 0, 0, 0L, 0L, 0L, 0L, 0L, 0L, 3) == 0L ? (byte[])null : result;
        }*/
    }
}
