using System.Data.SqlClient;

namespace Quiz.Models
{
    public static class Reader_Ext
    {
        public static string SafeGetString(this SqlDataReader reader, string colName)
        {
            int colIndex = reader.GetOrdinal(colName);

            if (!reader.IsDBNull(colIndex))
            {
                return reader.GetString(colIndex);
            }
            else
            {
                return "";
            }
        }

    }
}
