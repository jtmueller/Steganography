namespace Steganography;

public static class AudioSteganography
{
    public static byte[] EncryptText(byte[] wav, string text)
    {
        var audio = new WavAudio(wav);
        string pass = audio.BitsPerSample.ToString();
        string encrypted = AESEncrypt.EncryptString(text, pass);
        OutputConsole.Write($"Text encrypted \n{encrypted}");
        if (encrypted.Length <= Math.Floor((double)(audio.TotalSamples / 8)))
        {
            var generator = new SeedURNG(audio.TotalSamples, audio.TotalSamples);
            OutputConsole.Write("Seed generated");
            OutputConsole.Write("Processing wav file...");
            uint value;
            for (int i = 0; i < encrypted.Length; i++)
            {
                value = encrypted[i];
                for (int x = 0; x < 8; x++)
                {
                    uint sample = generator.Next;
                    uint sampleValue = audio.Samples[sample];
                    sampleValue = (sampleValue & 0xFFFFFFFE) | ((value >> x) & 1);
                    audio.Samples[sample] = sampleValue;
                }

            }
            value = 0;
            for (int x = 0; x < 8; x++)
            {
                uint sample = generator.Next;
                uint sampleValue = audio.Samples[sample];
                sampleValue = (sampleValue & 0xFFFFFFFE) | ((value >> x) & 1);
                audio.Samples[sample] = sampleValue;
            }
            audio.Save();
            OutputConsole.Write($"Text encrypted... used {encrypted.Length * 8} samples");
            OutputConsole.Write("Saving wav file");
            return audio.data;
        }
        else
        {
            return Array.Empty<byte>();
        }

    }

    public static byte[] EncryptTextLinear(byte[] wav, string text)
    {
        var audio = new WavAudio(wav);
        string pass = audio.BitsPerSample.ToString();
        string encrypted = AESEncrypt.EncryptString(text, pass);
        OutputConsole.Write($"Text encrypted \n{encrypted}");
        if (encrypted.Length <= Math.Floor((double)(audio.TotalSamples / 8)))
        {
            uint n = 0;
            OutputConsole.Write("Seed generated");
            OutputConsole.Write("Processing wav file...");
            uint value;
            for (int i = 0; i < encrypted.Length; i++)
            {
                value = encrypted[i];
                for (int x = 0; x < 8; x++)
                {
                    uint sample = n;
                    uint sampleValue = audio.Samples[sample];
                    sampleValue = (sampleValue & 0xFFFFFFFE) | ((value >> x) & 1);
                    audio.Samples[sample] = sampleValue;
                    n++;
                }

            }
            value = 0;
            for (int x = 0; x < 8; x++)
            {
                uint sample = n;
                uint sampleValue = audio.Samples[sample];
                sampleValue = (sampleValue & 0xFFFFFFFE) | ((value >> x) & 1);
                audio.Samples[sample] = sampleValue;
                n++;
            }
            audio.Save();
            OutputConsole.Write($"Text encrypted... used {encrypted.Length * 8} samples");
            OutputConsole.Write("Saving wav file");
            return audio.data;
        }
        else
        {
            return Array.Empty<byte>();
        }

    }

    public static string DecryptText(byte[] wav)
    {
        var audio = new WavAudio(wav);
        string text = string.Empty;
        var generator = new SeedURNG(audio.TotalSamples, audio.TotalSamples);
        string pass = audio.BitsPerSample.ToString();
        OutputConsole.Write("Processing wav file...");
        uint value;
        do
        {
            value = 0;
            for (int x = 0; x < 8; x++)
            {
                uint sample = generator.Next;
                uint sampleValue = audio.Samples[sample];
                value |= ((sampleValue & 1) << x);
            }
            if (value != 0)
                text += Convert.ToChar(value);
        } while (value != 0);
        OutputConsole.Write("Decrypting text...");
        try
        {
            return AESEncrypt.DecryptString(text, pass);
        }
        catch (Exception e)
        {
            OutputConsole.Write("Error: Text not found");
            Console.WriteLine(e.Message);
            return string.Empty;
        }
    }

    public static string DecryptTextLinear(byte[] wav)
    {
        var audio = new WavAudio(wav);
        string text = string.Empty;
        uint n = 0;
        string pass = audio.BitsPerSample.ToString();
        OutputConsole.Write("Processing wav file...");
        uint value;
        do
        {
            value = 0;
            for (int x = 0; x < 8; x++)
            {
                uint sample = n;
                uint sampleValue = audio.Samples[sample];
                value |= ((sampleValue & 1) << x);
                n++;
            }
            if (value != 0)
                text += Convert.ToChar(value);
        } while (value != 0);
        OutputConsole.Write("Decrypting text...");
        try
        {
            return AESEncrypt.DecryptString(text, pass);
        }
        catch (Exception e)
        {
            OutputConsole.Write("Error: Text not found");
            Console.WriteLine(e.Message);
            return string.Empty;
        }
    }

    public static byte[] EncryptFile(byte[] wav, byte[] file, string filename)
    {
        var audio = new WavAudio(wav);
        int extraBytes = 2 + filename.Length + file.Length.ToString().Length;
        var f = new HiddenFile(file, filename);
        OutputConsole.Write($"File size: {file.Length:n0} bytes");
        f.CipherFile((int)audio.TotalSamples);
        if (file.Length <= Math.Floor((double)(audio.TotalSamples / 8)) - extraBytes)
        {
            var generator = new SeedURNG(audio.TotalSamples, audio.TotalSamples);
            OutputConsole.Write("Seed generated");
            OutputConsole.Write("Ciphering file");
            OutputConsole.Write("Processing wav file...");
            OutputConsole.Write("Writing metadata...");
            uint value;
            //Write file size
            for (int i = 0; i < file.Length.ToString().Length; i++)
            {
                value = file.Length.ToString()[i];
                for (int x = 0; x < 8; x++)
                {
                    uint sample = generator.Next;
                    uint sampleValue = audio.Samples[sample];
                    sampleValue = (sampleValue & 0xFFFFFFFE) | ((value >> x) & 1);
                    audio.Samples[sample] = sampleValue;
                }

            }
            value = '#';
            for (int x = 0; x < 8; x++)
            {
                uint sample = generator.Next;
                uint sampleValue = audio.Samples[sample];
                sampleValue = (sampleValue & 0xFFFFFFFE) | ((value >> x) & 1);
                audio.Samples[sample] = sampleValue;
            }
            //Write file name
            for (int i = 0; i < filename.Length; i++)
            {
                value = filename[i];
                for (int x = 0; x < 8; x++)
                {
                    uint sample = generator.Next;
                    uint sampleValue = audio.Samples[sample];
                    sampleValue = (sampleValue & 0xFFFFFFFE) | ((value >> x) & 1);
                    audio.Samples[sample] = sampleValue;
                }

            }
            value = 0;
            for (int x = 0; x < 8; x++)
            {
                uint sample = generator.Next;
                uint sampleValue = audio.Samples[sample];
                sampleValue = (sampleValue & 0xFFFFFFFE) | ((value >> x) & 1);
                audio.Samples[sample] = sampleValue;
            }
            //Write file content
            OutputConsole.Write("Writing file data...");
            for (int i = 0; i < file.Length; i++)
            {
                value = f.File[i];
                for (int x = 0; x < 8; x++)
                {
                    uint sample = generator.Next;
                    uint sampleValue = audio.Samples[sample];
                    sampleValue = (sampleValue & 0xFFFFFFFE) | ((value >> x) & 1);
                    audio.Samples[sample] = sampleValue;
                }

            }

        }
        else
        {
            OutputConsole.Write("Error");
            return Array.Empty<byte>();
        }
        OutputConsole.Write("Finished embedding file");
        OutputConsole.Write($"Used {(file.Length + extraBytes) * 8} samples");
        audio.Save();
        return audio.data;
    }

    public static byte[] EncryptFileLinear(byte[] wav, byte[] file, string filename)
    {
        var audio = new WavAudio(wav);
        int extraBytes = 2 + filename.Length + file.Length.ToString().Length;
        var f = new HiddenFile(file, filename);
        OutputConsole.Write($"File size: {file.Length:n0} bytes");
        f.CipherFile((int)audio.TotalSamples);
        if (file.Length <= Math.Floor((double)(audio.TotalSamples / 8)) - extraBytes)
        {
            uint n = 0;
            OutputConsole.Write("Ciphering file");
            OutputConsole.Write("Processing wav file...");
            OutputConsole.Write("Writing metadata...");
            uint value;
            //Write file size
            for (int i = 0; i < file.Length.ToString().Length; i++)
            {
                value = file.Length.ToString()[i];
                for (int x = 0; x < 8; x++)
                {
                    uint sample = n;
                    uint sampleValue = audio.Samples[sample];
                    sampleValue = (sampleValue & 0xFFFFFFFE) | ((value >> x) & 1);
                    audio.Samples[sample] = sampleValue;
                    n++;
                }

            }
            value = '#';
            for (int x = 0; x < 8; x++)
            {
                uint sample = n;
                uint sampleValue = audio.Samples[sample];
                sampleValue = (sampleValue & 0xFFFFFFFE) | ((value >> x) & 1);
                audio.Samples[sample] = sampleValue;
                n++;
            }
            //Write file name
            for (int i = 0; i < filename.Length; i++)
            {
                value = filename[i];
                for (int x = 0; x < 8; x++)
                {
                    uint sample = n;
                    uint sampleValue = audio.Samples[sample];
                    sampleValue = (sampleValue & 0xFFFFFFFE) | ((value >> x) & 1);
                    audio.Samples[sample] = sampleValue;
                    n++;
                }

            }
            value = 0;
            for (int x = 0; x < 8; x++)
            {
                uint sample = n;
                uint sampleValue = audio.Samples[sample];
                sampleValue = (sampleValue & 0xFFFFFFFE) | ((value >> x) & 1);
                audio.Samples[sample] = sampleValue;
                n++;
            }
            //Write file content
            OutputConsole.Write("Writing file data...");
            for (int i = 0; i < file.Length; i++)
            {
                value = f.File[i];
                for (int x = 0; x < 8; x++)
                {
                    uint sample = n;
                    uint sampleValue = audio.Samples[sample];
                    sampleValue = (sampleValue & 0xFFFFFFFE) | ((value >> x) & 1);
                    audio.Samples[sample] = sampleValue;
                    n++;
                }

            }
        }
        else
        {
            OutputConsole.Write("Error");
            return Array.Empty<byte>();
        }
        OutputConsole.Write("Finished embedding file");
        OutputConsole.Write($"Used {(file.Length + extraBytes) * 8} samples");
        audio.Save();
        return audio.data;
    }

    public static byte[] EncryptFile2(byte[] wav, byte[] file, string filename)
    {
        var audio = new WavAudio(wav);
        int extraBytes = 2 + filename.Length + file.Length.ToString().Length;
        var f = new HiddenFile(file, filename);
        OutputConsole.Write($"File size: {file.Length:n0} bytes");
        f.CipherFile((int)audio.TotalSamples);
        if (file.Length <= Math.Floor((double)(audio.TotalSamples / 8)) - extraBytes)
        {
            var generator = new SeedURNG(audio.TotalSamples, audio.TotalSamples, true);
            OutputConsole.Write("Seed generated");
            OutputConsole.Write("Ciphering file");
            OutputConsole.Write("Processing wav file...");
            OutputConsole.Write("Writing metadata...");
            uint value;
            //Write file size
            var lenStr = file.Length.ToString();
            for (int i = 0; i < lenStr.Length; i++)
            {
                value = lenStr[i];
                for (int x = 0; x < 8; x++)
                {
                    uint sample = generator.NextN;
                    uint sampleValue = audio.Samples[sample];
                    sampleValue = (sampleValue & 0xFFFFFFFE) | ((value >> x) & 1);
                    audio.Samples[sample] = sampleValue;
                }

            }
            value = '#';
            for (int x = 0; x < 8; x++)
            {
                uint sample = generator.NextN;
                uint sampleValue = audio.Samples[sample];
                sampleValue = (sampleValue & 0xFFFFFFFE) | ((value >> x) & 1);
                audio.Samples[sample] = sampleValue;
            }
            //Write file name
            for (int i = 0; i < filename.Length; i++)
            {
                value = filename[i];
                for (int x = 0; x < 8; x++)
                {
                    uint sample = generator.NextN;
                    uint sampleValue = audio.Samples[sample];
                    sampleValue = (sampleValue & 0xFFFFFFFE) | ((value >> x) & 1);
                    audio.Samples[sample] = sampleValue;
                }

            }
            value = 0;
            for (int x = 0; x < 8; x++)
            {
                uint sample = generator.NextN;
                uint sampleValue = audio.Samples[sample];
                sampleValue = (sampleValue & 0xFFFFFFFE) | ((value >> x) & 1);
                audio.Samples[sample] = sampleValue;
            }
            //Write file content
            OutputConsole.Write("Writing file data...");
            for (int i = 0; i < file.Length; i++)
            {
                value = f.File[i];
                for (int x = 0; x < 8; x++)
                {
                    uint sample = generator.NextN;
                    uint sampleValue = audio.Samples[sample];
                    sampleValue = (sampleValue & 0xFFFFFFFE) | ((value >> x) & 1);
                    audio.Samples[sample] = sampleValue;
                }

            }

        }
        else
        {
            OutputConsole.Write("Error");
            return Array.Empty<byte>();
        }
        OutputConsole.Write("Finished embedding file");
        OutputConsole.Write($"Used {(file.Length + extraBytes) * 8} samples");
        audio.Save();
        return audio.data;
    }

    public static HiddenFile? DecryptFile(byte[] wav)
    {
        try
        {
            var audio = new WavAudio(wav);
            string text = string.Empty;
            var generator = new SeedURNG(audio.TotalSamples, audio.TotalSamples);
            OutputConsole.Write("Seed generated");
            OutputConsole.Write("Processing wav file...");
            OutputConsole.Write("Reading metadata...");
            uint value = 0;
            do
            {
                value = 0;
                for (int x = 0; x < 8; x++)
                {
                    uint sample = generator.Next;
                    uint sampleValue = audio.Samples[sample];
                    value |= ((sampleValue & 1) << x);
                }
                if (value != '#')
                    text += Convert.ToChar(value);
            } while (value != '#' && char.IsNumber((char)value));
            int filesize = int.Parse(text);
            OutputConsole.Write($"Extracted file size: {filesize:n0} bytes");
            text = string.Empty;
            do
            {
                value = 0;
                for (int x = 0; x < 8; x++)
                {
                    uint sample = generator.Next;
                    uint sampleValue = audio.Samples[sample];
                    value |= ((sampleValue & 1) << x);
                }
                if (value != 0)
                    text += Convert.ToChar(value);
            } while (value != 0);
            string filename = text;
            OutputConsole.Write($"Extracted file name: {filename}");
            byte[] file = new byte[filesize];
            for (int i = 0; i < filesize; i++)
            {
                value = 0;
                for (int x = 0; x < 8; x++)
                {
                    uint sample = generator.Next;
                    uint sampleValue = audio.Samples[sample];
                    value |= ((sampleValue & 1) << x);
                }
                file[i] = (byte)value;
            }
            OutputConsole.Write("Extracted file content");
            var f = new HiddenFile(file, filename);
            OutputConsole.Write("Ciphering file...");
            f.CipherFile((int)audio.TotalSamples);
            return f;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static HiddenFile? DecryptFileLinear(byte[] wav)
    {
        try
        {
            var audio = new WavAudio(wav);
            string text = string.Empty;
            uint n = 0;
            OutputConsole.Write("Processing wav file...");
            OutputConsole.Write("Reading metadata...");
            uint value = 0;
            do
            {
                value = 0;
                for (int x = 0; x < 8; x++)
                {
                    uint sample = n;
                    uint sampleValue = audio.Samples[sample];
                    value |= ((sampleValue & 1) << x);
                    n++;
                }
                if (value != '#')
                    text += Convert.ToChar(value);
            } while (value != '#' && char.IsNumber((char)value));
            int filesize = int.Parse(text);
            OutputConsole.Write($"Extracted file size: {filesize:n0} bytes");
            text = string.Empty;
            do
            {
                value = 0;
                for (int x = 0; x < 8; x++)
                {
                    uint sample = n;
                    uint sampleValue = audio.Samples[sample];
                    value |= ((sampleValue & 1) << x);
                    n++;
                }
                if (value != 0)
                    text += Convert.ToChar(value);
            } while (value != 0);
            string filename = text;
            OutputConsole.Write($"Extracted file name: {filename}");
            byte[] file = new byte[filesize];
            for (int i = 0; i < filesize; i++)
            {
                value = 0;
                for (int x = 0; x < 8; x++)
                {
                    uint sample = n;
                    uint sampleValue = audio.Samples[sample];
                    value |= ((sampleValue & 1) << x);
                    n++;
                }
                file[i] = (byte)value;
            }
            OutputConsole.Write("Extracted file content");
            var f = new HiddenFile(file, filename);
            OutputConsole.Write("Ciphering file...");
            f.CipherFile((int)audio.TotalSamples);
            return f;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static HiddenFile? DecryptFile2(byte[] wav)
    {
        try
        {
            var audio = new WavAudio(wav);
            string text = string.Empty;
            var generator = new SeedURNG(audio.TotalSamples, audio.TotalSamples, true);
            OutputConsole.Write("Seed generated");
            OutputConsole.Write("Processing wav file...");
            OutputConsole.Write("Reading metadata...");
            uint value = 0;
            do
            {
                value = 0;
                for (int x = 0; x < 8; x++)
                {
                    uint sample = generator.NextN;
                    uint sampleValue = audio.Samples[sample];
                    value |= ((sampleValue & 1) << x);
                }
                if (value != '#')
                    text += Convert.ToChar(value);
            } while (value != '#' && char.IsNumber((char)value));
            int filesize = int.Parse(text);
            OutputConsole.Write($"Extracted file size: {filesize:n0} bytes");
            text = string.Empty;
            do
            {
                value = 0;
                for (int x = 0; x < 8; x++)
                {
                    uint sample = generator.NextN;
                    uint sampleValue = audio.Samples[sample];
                    value |= ((sampleValue & 1) << x);
                }
                if (value != 0)
                    text += Convert.ToChar(value);
            } while (value != 0);
            string filename = text;
            OutputConsole.Write($"Extracted file name: {filename}");
            byte[] file = new byte[filesize];
            for (int i = 0; i < filesize; i++)
            {
                value = 0;
                for (int x = 0; x < 8; x++)
                {
                    uint sample = generator.NextN;
                    uint sampleValue = audio.Samples[sample];
                    value |= ((sampleValue & 1) << x);
                }
                file[i] = (byte)value;
            }
            OutputConsole.Write("Extracted file content");
            var f = new HiddenFile(file, filename);
            OutputConsole.Write("Ciphering file...");
            f.CipherFile((int)audio.TotalSamples);
            return f;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
