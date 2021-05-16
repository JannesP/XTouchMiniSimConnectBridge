using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Utility.Wpf.Converters
{
    public class IsNotNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }
            else
            {
                if (value is string str)
                {
                    return !string.IsNullOrEmpty(str);
                }
                else
                {
                    throw new ArgumentException("This converter only supports strings.");
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}