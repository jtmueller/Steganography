namespace Steganography;

public partial class ImgPreview : Form
{
    public ImgPreview()
    {
        InitializeComponent();
    }

    public ImgPreview(Image image)
    {
        InitializeComponent();
        imageBox.Image = image;
        Size = image.Size;
    }

    private void ImgPreview_FormClosed(object sender, FormClosedEventArgs e) => imageBox.Image.Dispose();
}
