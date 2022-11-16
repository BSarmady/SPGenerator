using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SPGenerator {

    internal static class Extensions {

        #region private Dictionary<string, string> sql_data_type_to_csharp
        private static Dictionary<string, string> sql_data_type_to_sql_csharp = new Dictionary<string, string>{
            {"bit", "SqlBoolean"},
            {"tinyint", "SqlByte"},
            {"smallint", "SqlInt16"},
            {"int", "SqlInt32"},
            {"bigint", "SqlInt64"},

            {"real", "SqlSingle"},
            {"float", "SqlSingle"},
            {"decimal", "SqlDecimal"},
            {"numeric", "SqlDecimal"},
            {"smallmoney", "SqlDecimal"},
            {"money", "SqlDecimal"},

            {"time", "SqlDateTime"},
            {"date", "SqlDateTime"},
            {"smalldatetime", "SqlDateTime"},
            {"datetime", "SqlDateTime"},
            {"datetime2", "SqlDateTime"},
            {"datetimeoffset", "SqlDateTime"},
            {"timestamp", "SqlBinary"},

            {"char", "SqlString"},
            {"nchar", "SqlString"},
            {"varchar", "SqlString"},
            {"nvarchar", "SqlString"},
            {"text", "SqlString"},
            {"ntext", "SqlString"},
            {"xml", "SqlString"},

            {"uniqueidentifier", "SqlGuid"},

            {"image", "SqlBinary"},
            {"binary", "SqlBinary"},
            {"varbinary", "SqlBinary"},
            {"sql_variant", "SqlBinary"},
            /*
            {"geography", "SqlBinary"},
            {"geometry", "SqlBinary"},
            {"hierarchyid", "SqlBinary"},
            */
            
        };
        #endregion

        #region private Dictionary<string, string> csharp_data_type_to_sql
        private static Dictionary<string, string> csharp_data_type_to_sql = new Dictionary<string, string>{
            {"text", "Text"},
            {"ntext", "NText"},
            {"nvarchar", "NVarChar"},
            {"nchar", "NChar"},
            {"xml", "Xml"},
            {"varchar", "VarChar"},
            {"char", "Char"},

            {"uniqueidentifier", "UniqueIdentifier"},

            {"tinyint", "TinyInt"},
            {"smallint", "SmallInt"},
            {"int", "Int"},
            {"bigint", "BigInt"},

            {"smalldatetime", "SmallDateTime"},
            {"datetime", "DateTime"},
            {"date", "DateTime"},
            {"time", "DateTime"},

            {"real", "Real"},
            {"float", "Float"},

            {"decimal", "Decimal"},
            {"numeric", "Decimal"},
            {"money", "Money"},
            {"smallmoney", "SmallMoney"},

            {"image", "Image"},
            {"sql_variant", "Variant"},
            {"varbinary", "VarBinary"},
            {"binary", "Binary"},
            {"timestamp", "Timestamp"},
            {"sysname", "Binary"},

            {"bit", "Bit"},
        };
        #endregion

        #region private Dictionary<string, string> sql_data_type_to_csharp
        private static Dictionary<string, string> sql_data_type_to_csharp = new Dictionary<string, string>{
            {"text",                "string"},
            {"ntext",               "string"},
            {"nvarchar",            "string"},
            {"nchar",               "string"},
            {"xml",                 "string"},
            {"varchar",             "string"},
            {"char",                "string"},

            {"uniqueidentifier",    "Guid"},

            {"tinyint",             "byte"},
            {"smallint",            "short"},
            {"int",                 "int"},
            {"bigint",              "long"},

            {"smalldatetime",       "DateTime"},
            {"datetime",            "DateTime"},
            {"datetime2",           "DateTime"},
            {"date",                "DateTime"},
            {"time",                "DateTime"},
            {"timestamp",           "byte[]"},
            {"datetimeoffset",      "DateTimeOffset"},

            {"real",                "float"},
            {"float",               "float"},

            {"decimal",             "decimal"},
            {"numeric",             "decimal"},
            {"money",               "decimal"},
            {"smallmoney",          "decimal"},

            {"image",               "byte[]"},
            {"sql_variant",         "byte[]"},
            {"varbinary",           "byte[]"},
            {"binary",              "byte[]"},
            {"sysname",             "byte[]"},

            {"bit",                 "bool"},
        };
        #endregion

        #region private Dictionary<string, string> sql_data_type_to_default_value
        private static Dictionary<string, string> sql_data_type_to_default_value = new Dictionary<string, string>{
            {"text",                "\"\""},
            {"ntext",               "\"\""},
            {"nvarchar",            "\"\""},
            {"nchar",               "\"\""},
            {"xml",                 "\"\""},
            {"varchar",             "\"\""},
            {"char",                "\"\""},

            {"uniqueidentifier",    "Guid.Empty()"},

            {"tinyint",             "0b"},
            {"smallint",            "0"},
            {"int",                 "0"},
            {"bigint",              "0"},

            {"smalldatetime",       "DateTime.MinValue"},
            {"datetime",            "DateTime.MinValue"},
            {"datetime2",           "DateTime.MinValue"},
            {"date",                "DateTime.MinValue"},
            {"time",                "DateTime.MinValue"},
            {"timestamp",           "DateTime.MinValue"},
            {"datetimeoffset",      "DateTimeOffset.MinValue"},

            {"real",                "0"},
            {"float",               "0"},

            {"decimal",             "0"},
            {"numeric",             "0"},
            {"money",               "0"},
            {"smallmoney",          "0"},

            {"image",               "new byte[]{ }"},
            {"sql_variant",         "new byte[]{ }"},
            {"varbinary",           "new byte[]{ }"},
            {"binary",              "new byte[]{ }"},
            {"sysname",             "new byte[]{ }"},

            {"bit",                 "false"},
        };
        #endregion

        #region public static string to_sql_type_name(...)
        public static string to_sql_type_name(this object obj) {
            string datatype = obj.ToString().ToLower();
            if (csharp_data_type_to_sql.ContainsKey(datatype))
                return csharp_data_type_to_sql[datatype];
            throw new Exception("cannot translate " + datatype + " Type to C# type");
        }
        #endregion

        #region public static string to_csharp_sql_type_name(...)
        public static string to_csharp_sql_type_name(this object obj) {
            string datatype = obj.ToString().ToLower();
            if (sql_data_type_to_sql_csharp.ContainsKey(datatype))
                return sql_data_type_to_sql_csharp[datatype];
            throw new Exception("cannot translate " + datatype + " Type to C# type");
        }
        #endregion

        #region public static string to_csharp_type_name(...)
        public static string to_csharp_type_name(this object obj, bool nullable) {
            string datatype = obj.ToString().ToLower();
            if (sql_data_type_to_csharp.ContainsKey(datatype)) {
                string type = sql_data_type_to_csharp[datatype];
                //return type != "byte[]" && nullable ? type + "?" : type;
                return type + (nullable && type != "string" && type != "byte[]" ? "?" : "");
            }
            throw new Exception("cannot translate " + datatype + " Type to C# type");
        }
        #endregion

        #region public static string to_csharp_default_value(...)
        public static string to_csharp_default_value(this object obj) {
            string datatype = obj.ToString().ToLower();
            if (sql_data_type_to_default_value.ContainsKey(datatype)) {
                return sql_data_type_to_default_value[datatype];
            }
            throw new Exception("cannot translate " + datatype + " Type to C# type");
        }
        #endregion

        #region public static bool ToBoolean(...)
        public static bool ToBoolean(this object obj, bool Default = false) {
            try {
                return Convert.ToBoolean(obj);
            } catch {
                return Default;
            }
        }
        #endregion

        #region public static Int32 ToInt32(...)
        public static Int32 ToInt32(this object obj, Int32 Default = 0) {
            try {
                return Convert.ToInt32(obj);
            } catch {
                return Default;
            }
        }
        #endregion

        #region public static DateTime ToDateTime(...)
        public static DateTime ToDateTime(this object data, DateTime default_value) {
            try {
                return Convert.ToDateTime(data);
            } catch {
                return default_value;
            }
        }
        #endregion

        #region public static string PadStringAtNewLines(...)
        public static string PadStringAtNewLines(this object inString, int Padding = 4) {
            return inString.ToString().Trim('\n','\r',' ').Replace(Environment.NewLine, Environment.NewLine + "".PadRight(Padding));
        }
        #endregion

        #region public static string AddTrailingBackSlashes(...)
        public static string AddTrailingBackSlashes(this string inString) {
            if (inString[inString.Length - 1] == '\\')
                return inString;
            return inString + "\\";
        }
        #endregion

        #region public static string SplitAtWords(...)
        public static string SplitAtWords(this string inString, int maxlen = 120) {
            return Regex.Replace(inString, @"(.{1," + maxlen + @"})(?:\s|$)", "$1" + Environment.NewLine);
        }
        #endregion

        #region public static string ToDateTimeStr(...)
        /// <summary>
        /// Returns string representation of the date returned from table row data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="format">DateTime Format parameter</param>
        /// <returns>if value is null, it will return empty string otherwise formatted datetime string </returns>
        public static string ToDateTimeStr(this object data, string format) {
            return data == DBNull.Value ? "" : Convert.ToDateTime(data).ToString(format);
        }
        #endregion

    }
}