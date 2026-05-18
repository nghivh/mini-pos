using System.Data;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace API.Common.Helpers
{
    public class DataConvertHelper
    {
        // ✅ 1. DataTable -> JSON
        public static string DataTableToJson(DataTable table)
        {
            var list = new List<Dictionary<string, object>>();
            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                list.Add(dict);
            }

            return JsonSerializer.Serialize(list, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }

        // ✅ 2. IDataReader -> JSON
        public static string DataReaderToJson(IDataReader reader)
        {
            var list = new List<Dictionary<string, object>>();
            while (reader.Read())
            {
                var dict = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    dict[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                list.Add(dict);
            }

            return JsonSerializer.Serialize(list, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }

        // ✅ 3. DataTable -> List<T>
        public static List<T> DataTableToList<T>(DataTable table) where T : new()
        {
            var list = new List<T>();
            foreach (DataRow row in table.Rows)
            {
                var obj = new T();
                foreach (DataColumn col in table.Columns)
                {
                    var prop = typeof(T).GetProperty(col.ColumnName);
                    if (prop != null && row[col] != DBNull.Value)
                    {
                        prop.SetValue(obj, Convert.ChangeType(row[col], prop.PropertyType));
                    }
                }
                list.Add(obj);
            }
            return list;
        }

        // ✅ 4. DataRow -> Dictionary<string, object>
        public static Dictionary<string, object> DataRowToDictionary(DataRow row)
        {
            var dict = new Dictionary<string, object>();
            foreach (DataColumn col in row.Table.Columns)
            {
                dict[col.ColumnName] = row[col];
            }
            return dict;
        }

        // ✅ 5. List<T> -> DataTable
        public static DataTable ListToDataTable<T>(List<T> list)
        {
            var table = new DataTable(typeof(T).Name);
            var props = typeof(T).GetProperties();

            foreach (var prop in props)
            {
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (var item in list)
            {
                var values = new object[props.Length];
                for (int i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(values);
            }

            return table;
        }
    }
}
