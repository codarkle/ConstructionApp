namespace ConstructionApp.Helpers
{
    public static class CurrencyHelper
    {
        public static string FormatEuro(float value)
        {
            var rounded = MathF.Round(value, 2);
            bool noCents = rounded % 1 == 0;
            var culture = new System.Globalization.CultureInfo("it-IT");
            var format = noCents ? "C0" : "C2";
            return rounded.ToString(format, culture);
        }
    }
}
