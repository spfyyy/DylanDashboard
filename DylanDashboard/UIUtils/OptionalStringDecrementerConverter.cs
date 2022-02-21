using System.Globalization;

namespace DylanDashboard.UIUtils
{
    public class OptionalStringDecrementerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (int.TryParse(parameter as string, out int startValue) && value is string text && text.Length > 0)
            {
                return startValue - 1;
            }
            return parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
