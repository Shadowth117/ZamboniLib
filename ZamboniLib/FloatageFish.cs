namespace Zamboni
{
    public class FloatageFish
    {
        public static byte[] decrypt_block(byte[] data_block, uint length, uint key)
        {
            /*
            byte xor_byte = (byte)((( key >> 16 ) ^ key) & 0xFF);
            byte[] to_return = new byte[length];

	        for ( uint i = 0; i < length; ++i )
            {
                if (data_block[i] != 0 && data_block[i] != xor_byte)
                    to_return[i] = (byte)(data_block[i] ^ xor_byte);
                else
                    to_return[i] = data_block[i];
            }*/
            return decrypt_block(data_block, length, key, 16);
        }

        public static byte[] decrypt_block(byte[] data_block, uint length, uint key, int shift)
        {
            byte xor_byte = (byte)(((key >> shift) ^ key) & 0xFF);
            byte[] to_return = new byte[length];

            for (uint i = 0; i < length; ++i)
            {
                if (data_block[i] != 0 && data_block[i] != xor_byte)
                {
                    to_return[i] = (byte)(data_block[i] ^ xor_byte);
                }
                else
                {
                    to_return[i] = data_block[i];
                }
            }

            return to_return;
        }
    }
}
