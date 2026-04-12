namespace Academy.Shared.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ToDateTime(this string dtString)
        {
            if (DateTime.TryParse(dtString, out DateTime dt)) 
            {
                return dt;
            }
            throw new ArgumentException("Supplied string is not a valid datetime");
        }
    }
}
