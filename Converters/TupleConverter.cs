using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using MagillaStream.Models;

namespace MagillaStream.Converters
{
    public class TupleConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count == 2 && values[0] is OutputGroup && values[1] is StreamTarget)
            {
                return Tuple.Create((OutputGroup)values[0], (StreamTarget)values[1]);
            }
            
            throw new InvalidOperationException("TupleConverter requires exactly two values of type OutputGroup and StreamTarget.");
        }

        public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            if (value is Tuple<OutputGroup, StreamTarget> tuple)
            {
                return new object[] { tuple.Item1, tuple.Item2 };
            }

            throw new InvalidOperationException("Value is not a tuple of OutputGroup and StreamTarget.");
        }
    }
}
