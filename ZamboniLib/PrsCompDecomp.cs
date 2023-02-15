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
        private byte ctrlByte;
        private int ctrlByteCounter;
        private int currDecompPos;
        private byte[] decompBuffer;

        private bool getCtrlBit()
        {
            --ctrlByteCounter;
            if (ctrlByteCounter == 0)
            {
                ctrlByte = decompBuffer[currDecompPos++];
                ctrlByteCounter = 8;
            }

            bool flag = (ctrlByte & 1U) > 0U;
            ctrlByte >>= 1;
            return flag;
        }

        public static byte[] Decompress(byte[] input, uint outCount)
        {
            return new PrsCompDecomp().localDecompress(input, outCount);
        }

        public byte[] localDecompress(byte[] input, uint outCount)
        {
            byte[] outData = new byte[(int)outCount];
            decompBuffer = input;
            ctrlByte = 0;
            ctrlByteCounter = 1;
            currDecompPos = 0;
            int outIndex = 0;
            try
            {
                while (outIndex < outCount && currDecompPos < input.Length)
                {
                    while (getCtrlBit())
                    {
                        outData[outIndex++] = decompBuffer[currDecompPos++];
                    }

                    int controlOffset;
                    int controlSize;
                    if (getCtrlBit())
                    {
                        if (currDecompPos < decompBuffer.Length)
                        {
                            int data0 = decompBuffer[currDecompPos++];
                            int data1 = decompBuffer[currDecompPos++];
                            if (data0 != 0 || data1 != 0)
                            {
                                controlOffset = (data1 << 5) + (data0 >> 3) - 8192;
                                int sizeTemp = data0 & 7;
                                controlSize = sizeTemp != 0 ? sizeTemp + 2 : decompBuffer[currDecompPos++] + 10;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        controlSize = 2;
                        if (getCtrlBit())
                        {
                            controlSize += 2;
                        }

                        if (getCtrlBit())
                        {
                            ++controlSize;
                        }

                        controlOffset = decompBuffer[currDecompPos++] - 256;
                    }

                    int loadIndex = controlOffset + outIndex;
                    for (int index = 0; index < controlSize && outIndex < outData.Length; ++index)
                    {
                        outData[outIndex++] = outData[loadIndex++];
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ZamboniException(ex);
            }

            return outData;
        }

        public static byte[] compress(byte[] toCompress)
        {
            return new PrsCompressor().compress(toCompress);
        }
    }
}
