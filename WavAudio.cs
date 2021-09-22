namespace Steganography;

public class WavAudio
{
    public byte[] data;
    private readonly byte[] header = new byte[0x2E];
    public uint Channels { get; private set; }
    public uint BitsPerSample { get; private set; }

    private readonly int start = 0x2E;
    public uint TotalSamples { get; private set; }
    public uint[] Samples = Array.Empty<uint>();
    public long BytesAvailable { get; private set; }

    public WavAudio(byte[] data)
    {
        this.data = data ?? throw new ArgumentNullException(nameof(data));
        Array.Copy(data, 0, header, 0, 0x2E);
        Channels = BitConverter.ToUInt16(data, 0x16);
        BitsPerSample = BitConverter.ToUInt16(data, 0x22);
        if (BitsPerSample is 8 or 16 or 24 or 32)
        {
            TotalSamples = (BitConverter.ToUInt32(data, 0x2A) / Channels) / (BitsPerSample / 8);
            Samples = new uint[TotalSamples];
            int i = 0;
            for (int n = 0; n < TotalSamples; n++)
            {
                Samples[n] = BitsPerSample switch
                {
                    8 => data[start + i],
                    24 => BitConverter.ToUInt32(data, start + i) & 0xFFFFFF,
                    32 => BitConverter.ToUInt32(data, start + i),
                    _ => BitConverter.ToUInt16(data, start + i),
                };
                i += (int)(BitsPerSample / 8);
            }
            BytesAvailable = (long)Math.Floor((double)(TotalSamples / 8));
        }
        else
        {
            OutputConsole.Write("This file is incompatible, bits per sample must be 8, 16, 24 or 32");
            this.data = Array.Empty<byte>();
        }
    }

    public void Save()
    {
        if (BitsPerSample is 8 or 16 or 24 or 32)
        {
            int i = 0;
            for (int n = 0; n < TotalSamples; n++)
            {
                switch (BitsPerSample)
                {
                    case 8:
                        data[start + i] = (byte)Samples[n];
                        break;
                    case 16:
                    default:
                        data[start + i] = (byte)(Samples[n] & 0xFF);
                        data[start + i + 1] = (byte)((Samples[n] >> 8) & 0xFF);
                        break;
                    case 24:
                        data[start + i] = (byte)(Samples[n] & 0xFF);
                        data[start + i + 1] = (byte)((Samples[n] >> 8) & 0xFF);
                        data[start + i + 2] = (byte)((Samples[n] >> 16) & 0xFF);
                        break;
                    case 32:
                        data[start + i] = (byte)(Samples[n] & 0xFF);
                        data[start + i + 1] = (byte)((Samples[n] >> 8) & 0xFF);
                        data[start + i + 2] = (byte)((Samples[n] >> 16) & 0xFF);
                        data[start + i + 3] = (byte)((Samples[n] >> 24) & 0xFF);
                        break;
                }
                i += (int)(BitsPerSample / 8);
            }
        }
    }
}
