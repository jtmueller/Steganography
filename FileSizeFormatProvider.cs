namespace Steganography;

public class FileSizeFormatProvider : IFormatProvider, ICustomFormatter
{
    public static string GetFileSize(long bytes) => string.Format(new FileSizeFormatProvider(), "{0:fs}", bytes);

    public object? GetFormat(Type? formatType)
    {
        if (formatType == typeof(ICustomFormatter)) return this;
        return null;
    }

    private const string FileSizeFormat = "fs";
    private const decimal OneKiloByte = 1024M;
    private const decimal OneMegaByte = OneKiloByte * 1024M;
    private const decimal OneGigaByte = OneMegaByte * 1024M;

    public string Format(string? format, object? arg, IFormatProvider? formatProvider)
    {
        if (format == null || !format.StartsWith(FileSizeFormat))
        {
            return DefaultFormat(format, arg, formatProvider);
        }

        if (arg is string)
        {
            return DefaultFormat(format, arg, formatProvider);
        }

        decimal size;

        try
        {
            size = Convert.ToDecimal(arg);
        }
        catch (InvalidCastException)
        {
            return DefaultFormat(format, arg, formatProvider);
        }

        string suffix;
        if (size > OneGigaByte)
        {
            size /= OneGigaByte;
            suffix = "GB";
        }
        else if (size > OneMegaByte)
        {
            size /= OneMegaByte;
            suffix = "MB";
        }
        else if (size > OneKiloByte)
        {
            size /= OneKiloByte;
            suffix = "KB";
        }
        else
        {
            suffix = "B";
        }

        string precision = format[2..];
        if (string.IsNullOrEmpty(precision)) precision = "2";
        return string.Format("{0:N" + precision + "} {1}", size, suffix);

    }

    private static string DefaultFormat(string? format, object? arg, IFormatProvider? formatProvider)
    {
        if (arg is IFormattable formattableArg)
        {
            return formattableArg.ToString(format, formatProvider);
        }
        return arg?.ToString() ?? string.Empty;
    }
}
