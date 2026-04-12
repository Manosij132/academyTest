using System.Data;
using System.Reflection;

namespace Academy.Shared.Extensions
{
    public static class DataTableExtensions
    {
        public static List<T> ToList<T>(this DataTable table) where T : new()
        {
            List<T> list = new();
            foreach (var row in table.AsEnumerable())
            {
                T obj = new T();
                foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (prop.PropertyType == typeof(DateTime?))
                    {
                        prop.SetValue(obj, row[prop.Name] != DBNull.Value ? (DateTime?)row[prop.Name] : null, null);
                    }
                    else
                    {
                        prop.SetValue(obj, Convert.ChangeType(row[prop.Name], prop.PropertyType), null);
                    }
                }
                list.Add(obj);
            }
            return list;
        }

        public static List<Dictionary<string, object>> ToListDictionary(this DataTable dataTable)
        {
            var list = new List<Dictionary<string, object>>();
            // Iterate over each row in the DataTable
            foreach (DataRow row in dataTable.Rows)
            {
                var dict = new Dictionary<string, object>();
                // Iterate over each column in the DataTable
                foreach (DataColumn column in dataTable.Columns)
                {
                    // Add the column name and its corresponding value to the dictionary
                    dict[column.ColumnName] = row[column];
                }
                // Add the dictionary to the list
                list.Add(dict);
            }
            return list;
        }
    }
}
