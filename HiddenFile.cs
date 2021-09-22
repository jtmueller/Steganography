namespace Steganography;

public class HiddenFile
{
    public string Filename { get; set; }
    public byte[] File { get; set; }
    public int Size { get; set; }

    public HiddenFile(byte[] file, string filename)
    {
        File = file;
        Filename = filename;
        Size = file.Length;
    }

    public void CipherFile(int seed)
    {
        byte[] newFile = FileCipher.CipherFile(File, seed);
        File = newFile;
    }
}
