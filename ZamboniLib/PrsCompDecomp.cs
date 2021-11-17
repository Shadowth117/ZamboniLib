// Decompiled with JetBrains decompiler
// Type: zamboni.PrsCompDecomp
// Assembly: zamboni, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 73B487C9-8F41-4586-BEF5-F7D7BFBD4C55
// Assembly location: D:\Downloads\zamboni_ngs (3)\zamboni.exe

using System;

namespace Zamboni
{
    public class PrsCompDecomp
    {
        private int ctrlByteCounter;
        private int ctrlBytePos = 0;
        private byte origCtrlByte = 0;
        private byte ctrlByte = 0;
        private byte[] decompBuffer;
        private int currDecompPos = 0;
        private int numCtrlBytes = 1;

        private bool getCtrlBit()
        {
            --ctrlByteCounter;
            if (ctrlByteCounter == 0)
            {
                ctrlBytePos = currDecompPos;
                origCtrlByte = decompBuffer[currDecompPos];
                ctrlByte = decompBuffer[currDecompPos++];
                ctrlByteCounter = 8;
                ++numCtrlBytes;
            }
            bool flag = (ctrlByte & 1U) > 0U;
            ctrlByte >>= 1;
            return flag;
        }

        public static byte[] Decompress(byte[] input, uint outCount) => new PrsCompDecomp().localDecompress(input, outCount);

        public byte[] localDecompress(byte[] input, uint outCount)
        {
            byte[] numArray = new byte[(int)outCount];
            decompBuffer = input;
            ctrlByte = 0;
            ctrlByteCounter = 1;
            numCtrlBytes = 1;
            currDecompPos = 0;
            int num1 = 0;
            try
            {
                while (num1 < outCount && currDecompPos < input.Length)
                {
                    while (getCtrlBit())
                        numArray[num1++] = decompBuffer[currDecompPos++];
                    int num2;
                    int num3;
                    if (getCtrlBit())
                    {
                        if (currDecompPos < decompBuffer.Length)
                        {
                            int num4 = decompBuffer[currDecompPos++];
                            int num5 = decompBuffer[currDecompPos++];
                            int num6 = num4;
                            int num7 = num5;
                            if (num6 != 0 || num7 != 0)
                            {
                                num2 = (num7 << 5) + (num6 >> 3) - 8192;
                                int num8 = num6 & 7;
                                num3 = num8 != 0 ? num8 + 2 : decompBuffer[currDecompPos++] + 10;
                            }
                            else
                                break;
                        }
                        else
                            break;
                    }
                    else
                    {
                        num3 = 2;
                        if (getCtrlBit())
                            num3 += 2;
                        if (getCtrlBit())
                            ++num3;
                        num2 = decompBuffer[currDecompPos++] - 256;
                    }
                    int num9 = num2 + num1;
                    for (int index = 0; index < num3 && num1 < numArray.Length; ++index)
                        numArray[num1++] = numArray[num9++];
                }
            }
            catch (Exception ex)
            {
                throw new ZamboniException(ex);
            }
            return numArray;
        }

        public static byte[] compress(byte[] toCompress) => new PrsCompressor().compress(toCompress);
    }
}
