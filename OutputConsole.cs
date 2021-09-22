namespace Steganography;

public static class OutputConsole
{
    private static ListBox? list;

    public static void Bind(ListBox listbox) => Interlocked.Exchange(ref list, listbox);

    public static void Show()
    {
        if (list != null)
        {
            list.Visible = true;
        }
    }

    public static void Hide()
    {
        if (list != null)
        {
            list.Visible = false;
        }
    }

    public static void Write(string text)
    {
        if (list != null)
        {
            list.Items.Add(text);
            if (list.Items.Count > (list.Height / list.ItemHeight))
            {
                list.Items.RemoveAt(0);
            }
            list.SelectedIndex = list.Items.Count - 1;
        }
    }

    public static void Clear()
    {
        list?.Items.Clear();
    }

}
