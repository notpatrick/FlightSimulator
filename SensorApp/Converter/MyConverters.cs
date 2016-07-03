using System;
using Windows.UI.Xaml.Data;

namespace SensorApp
{
    public class TwoDecimalsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return string.Format("{0:0.00}", value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
