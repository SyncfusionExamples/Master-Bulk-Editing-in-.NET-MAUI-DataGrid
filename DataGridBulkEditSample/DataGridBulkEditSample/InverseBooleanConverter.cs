using Microsoft.Maui.Controls;
using Syncfusion.Maui.DataGrid;
using Syncfusion.Maui.GridCommon.ScrollAxis;
using System;
using System.Globalization;

namespace DataGridBulkEditSample
{

    /// <summary>
    /// Converts a boolean value to its inverse and vice versa.
    /// Useful for bindings where you need to invert a boolean (e.g., visibility toggles).
    /// </summary>
    public class InverseBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to its inverse.
        /// </summary>
        /// <param name="value">The source value (expected to be a boolean).</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">Optional parameter (not used).</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>The inverted boolean value, or false if input is not a boolean.</returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return !b;
            }
            return false;
        }

        /// <summary>
        /// Converts back the inverted boolean value to its original.
        /// </summary>
        /// <param name="value">The target value (expected to be a boolean).</param>
        /// <param name="targetType">The type of the source property.</param>
        /// <param name="parameter">Optional parameter (not used).</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>The inverted boolean value, or false if input is not a boolean.</returns>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return !b;
            }
            return false;
        }
    }

}
