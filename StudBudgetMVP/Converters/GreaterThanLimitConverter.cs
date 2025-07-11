﻿using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace StudBudgetMVP.Converters;

public class GreaterThanLimitConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal total && parameter is decimal limit)
            return total > limit;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}