﻿using System;
using System.Collections.Generic;
using System.Globalization;
using SSW.Consulting.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SSW.Consulting.Converters
{
    public class SkillToBadgeConverter : IValueConverter, IMarkupExtension
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;
            //ImageSource source;
            string imageResourceName = ((string)value).ToLower().Replace(" ", string.Empty).Replace(".", string.Empty).Replace("#", string.Empty);

            return $"skill_{imageResourceName}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Only one way bindings are supported for this converter");
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
