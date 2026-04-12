namespace Academy.Shared.Extensions
{
    public static class IntExtensions
    {
        public static int GetDefaultIfNull(this int? input)
        {
            if (input == null) 
                return 0;
            else if (input.HasValue) 
                return input.Value;
            else 
                return 0;
        }
        public static byte GetDefaultIfNull(this byte? input)
        {
            if (input == null) 
                return 0;
            else 
                return input.Value;
        }
    }
}
