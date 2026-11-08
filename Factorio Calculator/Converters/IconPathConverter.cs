
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Factorio_Calculator
{
    public class IconPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string relativePath = value as string;

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            string normalizedPath = relativePath.TrimStart('/', '\\')
                .Replace('/', Path.DirectorySeparatorChar);

            string fullPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                normalizedPath
            );

            if (!File.Exists(fullPath))
            {
                return null;
            }

            BitmapImage image = new BitmapImage();

            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(fullPath, UriKind.Absolute);
            image.EndInit();
            image.Freeze();

            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
