namespace Steganography;

using System.Diagnostics;

public partial class Form1 : Form
{
    public enum Mode
    {
        Image,
        Audio
    }

    private Image? image;
    private byte[]? audio;
    private byte[]? file;
    private string filename = string.Empty;
    private Mode currentMode;
    private readonly Stopwatch stopwatch;

    public Form1()
    {
        InitializeComponent();
        OutputConsole.Bind(console);
        random.Checked = true;
        currentMode = Mode.Image;
        stopwatch = new Stopwatch();
    }

    private void LoadImage_Click(object sender, EventArgs e)
    {
        loadDialog.FileName = "*.*";
        var res = loadDialog.ShowDialog();
        if (res == DialogResult.OK)
        {
            image?.Dispose();
            if (Path.GetExtension(loadDialog.FileName) is ".png" or ".bmp" or ".jpg")
            {
                image = Image.FromFile(loadDialog.FileName);
                imageBox.Image = image;
                OutputConsole.Write($"Image loaded \nTotal pixels = {image.Width * image.Height}");
                OutputConsole.Write($"Maximum file size for this image = {FileSizeFormatProvider.GetFileSize((image.Width * image.Height) - 2)} - (file size digits + file name character count) bytes");
                currentMode = Mode.Image;
                audio = null;
                audioLabel.Visible = false;
            }
        }
    }

    private void EncryptButton_Click(object sender, EventArgs e)
    {
        if (currentMode == Mode.Image && image != null)
        {
            var encrypted = random.Checked || randomM2.Checked
                ? Steganography.InsertEncryptedTextToImage(image, textBox.Text)
                : Steganography.InsertEncryptedTextToImageLinear(image, textBox.Text);
            if (encrypted != null)
            {
                saveDialog.FileName = "*.*";
                var res = saveDialog.ShowDialog();
                if (res == DialogResult.OK)
                {
                    encrypted.Save(saveDialog.FileName);
                    OutputConsole.Write("Image saved");
                }
            }
        }
        if (currentMode == Mode.Audio && audio != null)
        {
            byte[] file = random.Checked
                ? AudioSteganography.EncryptText(audio, textBox.Text)
                : AudioSteganography.EncryptTextLinear(audio, textBox.Text);
            if (file != null)
            {
                var res = saveWav.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllBytes(saveWav.FileName, file);
                    OutputConsole.Write("Wav file saved");
                }
            }
        }
    }

    private void DecryptButton_Click(object sender, EventArgs e)
    {
        if (currentMode == Mode.Image && image != null)
        {
            string text = random.Checked || randomM2.Checked
                ? Steganography.GetDecryptedTextFromImage(image)
                : Steganography.GetDecryptedTextFromImageLinear(image);
            if (text != null)
            {
                textBox.Text = text;
                OutputConsole.Write("Text decrypted");
            }
            else
            {
                //MessageBox.Show("This image doesn't have an encrypted text or an error occurred");
            }
        }
        if (currentMode == Mode.Audio && audio != null)
        {
            string text = random.Checked ? AudioSteganography.DecryptText(audio) : AudioSteganography.DecryptTextLinear(audio);
            if (text != null)
            {
                textBox.Text = text;
                OutputConsole.Write("Text decrypted");
            }
        }
    }

    private void EncryptFile_Click(object sender, EventArgs e)
    {
        if (currentMode == Mode.Image && image is not null)
        {
            loadFileDialog.FileName = "*.*";
            var res = loadFileDialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                file = File.ReadAllBytes(loadFileDialog.FileName);
                filename = loadFileDialog.SafeFileName;
                OutputConsole.Write("Added File to buffer");
                Bitmap encrypted;
                stopwatch.Restart();
                encrypted = random.Checked
                    ? Steganography.InsertFileToImage(image, file, filename)
                    : linear.Checked
                        ? Steganography.InsertFileToImageLinear(image, file, filename)
                        : Steganography.InsertFileToImage2(image, file, filename);
                if (encrypted != null)
                {
                    stopwatch.Stop();
                    OutputConsole.Write($"Process completed in {stopwatch.Elapsed}");
                    saveDialog.FileName = "*.*";
                    var res2 = saveDialog.ShowDialog();
                    if (res2 == DialogResult.OK)
                    {
                        encrypted.Save(saveDialog.FileName);
                        OutputConsole.Write("Image saved");
                    }
                }
                stopwatch.Reset();
            }
        }
        if (currentMode == Mode.Audio && audio != null)
        {
            loadFileDialog.FileName = "*.*";
            var res = loadFileDialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                byte[] file;
                stopwatch.Restart();
                file = random.Checked
                    ? AudioSteganography.EncryptFile(audio, File.ReadAllBytes(loadFileDialog.FileName), loadFileDialog.SafeFileName)
                    : linear.Checked
                        ? AudioSteganography.EncryptFileLinear(audio, File.ReadAllBytes(loadFileDialog.FileName), loadFileDialog.SafeFileName)
                        : AudioSteganography.EncryptFile2(audio, File.ReadAllBytes(loadFileDialog.FileName), loadFileDialog.SafeFileName);
                if (file != null)
                {
                    stopwatch.Stop();
                    OutputConsole.Write($"Process completed in {stopwatch.Elapsed}");
                    var res2 = saveWav.ShowDialog();
                    if (res2 == System.Windows.Forms.DialogResult.OK)
                    {
                        File.WriteAllBytes(saveWav.FileName, file);
                        OutputConsole.Write("Wav file saved");
                    }
                }
                stopwatch.Reset();
            }
        }
    }

    private void DecryptFile_Click(object sender, EventArgs e)
    {
        if (currentMode == Mode.Image && image != null)
        {
            HiddenFile? f;
            stopwatch.Restart();
            f = random.Checked
                ? Steganography.GetFileFromImage(image)
                : linear.Checked ? Steganography.GetFileFromImageLinear(image) : Steganography.GetFileFromImage2(image);
            if (f != null)
            {
                stopwatch.Stop();
                OutputConsole.Write($"Process completed in {stopwatch.Elapsed}");
                saveFileDialog.FileName = f.Filename;
                var res = saveFileDialog.ShowDialog();
                if (res == DialogResult.OK)
                {
                    File.WriteAllBytes(saveFileDialog.FileName, f.File);
                    OutputConsole.Write("File saved");
                    if (Path.GetExtension(saveFileDialog.FileName) is ".bmp" or ".png" or ".jpg")
                    {
                        var p = new ImgPreview(Image.FromFile(saveFileDialog.FileName));
                        p.ShowDialog();
                    }
                }
            }
            else
            {
                //MessageBox.Show("This image doesn't have an encrypted text or an error occurred");
            }
            stopwatch.Reset();
        }
        if (currentMode == Mode.Audio && audio != null)
        {
            HiddenFile? file;
            stopwatch.Restart();
            file = random.Checked
                ? AudioSteganography.DecryptFile(audio)
                : linear.Checked ? AudioSteganography.DecryptFileLinear(audio) : AudioSteganography.DecryptFile2(audio);
            if (file != null)
            {
                stopwatch.Stop();
                OutputConsole.Write($"Process completed in {stopwatch.Elapsed}");
                saveFileDialog.FileName = file.Filename;
                var res = saveFileDialog.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllBytes(saveFileDialog.FileName, file.File);
                    OutputConsole.Write("File saved");
                    if (Path.GetExtension(saveFileDialog.FileName) is ".bmp" or ".png" or ".jpg")
                    {
                        var p = new ImgPreview(Image.FromFile(saveFileDialog.FileName));
                        p.ShowDialog();
                    }
                }
            }
            stopwatch.Reset();
        }
    }

    private void Console_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        int index = console.IndexFromPoint(e.Location);
        if (index != ListBox.NoMatches)
        {
            MessageBox.Show(console.Items[index].ToString());
        }
    }

    private void Random_CheckedChanged(object sender, EventArgs e)
    {
        if (random.Checked)
            OutputConsole.Write("Using random steganography algorithm");
    }

    private void Linear_CheckedChanged(object sender, EventArgs e)
    {
        if (linear.Checked)
            OutputConsole.Write("Using linear steganography algorithm (Fastest)");
    }

    private void Console_KeyDown(object sender, KeyEventArgs e)
    {
        if (console.SelectedIndex >= 0 && e.KeyCode == Keys.Delete)
        {
            console.Items.RemoveAt(console.SelectedIndex);
        }
    }

    private void LoadWavFile_Click(object sender, EventArgs e)
    {
        var res = loadWav.ShowDialog();
        if (res == DialogResult.OK)
        {
            audio = File.ReadAllBytes(loadWav.FileName);
            var wav = new WavAudio(audio);
            if (wav.data != null)
            {
                OutputConsole.Write($"Audio loaded \nSamples found: {wav.TotalSamples}");
                OutputConsole.Write($"Maximum file size for this file = {FileSizeFormatProvider.GetFileSize(wav.BytesAvailable)} - (file size digits + file name character count) bytes");
                currentMode = Mode.Audio;
                if (image != null)
                {
                    image.Dispose();
                }
                image = null;
                imageBox.Image = null;
                audioLabel.Text = $"Using Wav File: {loadWav.SafeFileName}";
                audioLabel.Visible = true;
            }
            else
            {
                audio = null;
            }
        }
    }

    private void RandomM2_CheckedChanged(object sender, EventArgs e)
    {
        //Removed
        if (randomM2.Checked)
            OutputConsole.Write("Using random method 2 steganography algorithm (Poor performance on small files, but better performance than random using bigger files) \nWorks only with files...");
    }

}
