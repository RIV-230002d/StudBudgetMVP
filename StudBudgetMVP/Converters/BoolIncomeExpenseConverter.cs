using System.Globalization;

namespace StudBudgetMVP.Converters
{
    public class BoolIncomeExpenseConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
            => (bool)value ? "Доход:" : "Фактический расход:";
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }
}
