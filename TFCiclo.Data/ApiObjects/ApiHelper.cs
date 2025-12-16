
namespace TFCiclo.Data.ApiObjects
{
    public class ApiHelper
    {
        #region Métodos static
        public static string Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value ?? string.Empty;

            return value.Length <= maxLength
                ? value
                : value.Substring(0, maxLength);
        }
        #endregion
    }
}
