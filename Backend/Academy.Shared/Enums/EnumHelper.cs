using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academy.Shared.Enums
{
   
    public static class EnumHelper
    {
        public static List<KeyValuePair<int, string>> EnumToKeyValueList<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                       .Cast<T>()
                       .Select(e => new KeyValuePair<int, string>(Convert.ToInt32(e), e.ToString()))
                       .ToList();
        }
    }
}
