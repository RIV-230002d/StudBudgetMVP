using System.Globalization;

namespace StudBudgetMVP.Converters
{
    public class IncomeExpenseColorConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
            => (bool)value ? Colors.Green : Colors.Black;
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }
}
