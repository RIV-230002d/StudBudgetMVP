﻿using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace StudBudgetMVP.Converters
{
    public class AmountColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal amount && amount > 0)
                return Colors.Green;
            return Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
