﻿namespace Steganography;

public static class FileCipher //Using code from my file ciphering project https://github.com/rubendal/File-Cipher
{
    private static byte[] pad = Array.Empty<byte>();
    private static int seed;
    private static int x;
    private static int previous;

    private static void PreparePad(int l)
    {
        if (l > pad.Length)
        {
            Array.Resize(ref pad, l);
            //Build pad
            for (int i = previous; i < l; i++)
            {
                do
                {
                    x = (int)((0x13793A1F2 + (x >> 5) * 0xFF7AB) & 0xFFFFFFFF); //Temporary RNG formula, needs improvement...
                } while ((x & 0xFF) != 0); //Avoid making xor with 0
                pad[i] = (byte)(x & 0xFF);
            }
            previous = l;
        }
    }

    public static byte[] CipherFile(byte[] file, int key)
    {
        byte[] newFile = new byte[file.Length];
        seed = key;
        previous = 0;
        PreparePad(file.Length);
        for (int i = 0; i < file.Length; i++)
        {
            newFile[i] = (byte)(file[i] ^ pad[i]);
        }
        return newFile;
    }
}
