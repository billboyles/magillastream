using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace MagillaStream.Converters
{
    public class IsEmptyToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                // Return true if count is 0 (i.e., the list is empty)
                return count == 0;
            }

            // Fallback, in case value isn't an integer
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
