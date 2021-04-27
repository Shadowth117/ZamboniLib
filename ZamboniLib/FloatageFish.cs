// Decompiled with JetBrains decompiler
// Type: zamboni.FloatageFish
// Assembly: zamboni, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 73B487C9-8F41-4586-BEF5-F7D7BFBD4C55
// Assembly location: D:\Downloads\zamboni_ngs (3)\zamboni.exe

namespace zamboni
{
    internal class FloatageFish
    {
        public static byte[] decrypt_block(byte[] data_block, uint length, uint key) => FloatageFish.decrypt_block(data_block, length, key, 16);

        public static byte[] decrypt_block(byte[] data_block, uint length, uint key, int shift)
        {
            byte num1 = (byte)(((int)(key >> shift) ^ (int)key) & (int)byte.MaxValue);
            byte[] numArray = new byte[(int)length];
            for (uint index = 0; index < length; ++index)
            {
                int num2 = data_block[(int)index] == (byte)0 ? 0 : ((int)data_block[(int)index] != (int)num1 ? 1 : 0);
                numArray[(int)index] = num2 == 0 ? data_block[(int)index] : (byte)((uint)data_block[(int)index] ^ (uint)num1);
            }
            return numArray;
        }
    }
}
