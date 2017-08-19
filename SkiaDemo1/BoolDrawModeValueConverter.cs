using System;
using System.Globalization;
using Xamarin.Forms;

namespace SkiaDemo1
{
	public class BoolDrawModeValueConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool bVal = (bool)value;
			return ((bVal) ? "Drawing" : "Panning");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
