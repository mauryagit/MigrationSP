using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
   public static class Extension
    {
       public static DataTable ToDataTable<T>(this IList<T> data)
       {
           PropertyDescriptorCollection properties =
               TypeDescriptor.GetProperties(typeof(T));
           DataTable table = new DataTable();
           foreach (PropertyDescriptor prop in properties)
           {              
               if (prop.Name != "rowID" && prop.Name != "attachementFiles")
                   table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
           }
           foreach (T item in data)
           {
               DataRow row = table.NewRow();
               foreach (PropertyDescriptor prop in properties)
               {
                   if (prop.Name != "rowID" && prop.Name != "attachementFiles")
                       row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
               }
                   table.Rows.Add(row);
              
           }
           return table;
       }
    }
}
