using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace SPGenerator {

    class GenerateSP: IDisposable {

        private DataTable table_column_data;
        private DataTable table_fk_data;

        private LoggerDelegate Logger;
        private SqlConnection DbConn;

        private string Author;
        private string DatabaseName;
        private string SaveFolder;

        private bool AddCommentHeader;
        private bool EncloseWithBrackets;
        private bool OmitDbo;
        private bool IncludeSPDescInHeader;
        private bool CreateHistoryAndTrigger;

        Dictionary<string, DbType> DbTypes = new Dictionary<string, DbType> { };

        #region private const string FK_SQL
        private const string SQL_FK = @"SELECT
                KeyName = OBJECT_NAME(constraint_object_id),
                KeyIndex = constraint_column_id,
                FromTableSchema = (SELECT schema_name(schema_id) from sys.tables WHERE sys.tables.object_id = fkc.parent_object_id),
                FromTable = OBJECT_NAME(fkc.parent_object_id),
                FromColumn = COL_NAME(fkc.parent_object_id,parent_column_id),
                ToTableSchema = (SELECT schema_name(schema_id) from sys.tables WHERE sys.tables.object_id = fkc.referenced_object_id),
                ToTable = OBJECT_NAME(fkc.referenced_object_id),
                ToColumn = COL_NAME(fkc.referenced_object_id,referenced_column_id),
                OnDelete = delete_referential_action_desc,
                OnUpdate = update_referential_action_desc
            FROM
                sys.foreign_key_columns fkc LEFT JOIN
                sys.foreign_keys ON constraint_object_id = object_id
            ORDER BY
                OBJECT_NAME(fkc.referenced_object_id), OBJECT_NAME(constraint_object_id)";
        #endregion

        #region private const string SQL_Table_Columns
        private const string SQL_Table_Columns = @"SELECT
                so.object_id,
                schema_name             = SCHEMA_NAME(so.schema_id),
                table_name              = OBJECT_NAME(so.OBJECT_ID),
                column_name = null, 
                column_id = 0,
                type = null, 
                max_length = null, 
                precision = null, 
                scale = null, 
                is_identity = null,
                is_computed = null, 
                is_primary_key = null,
                comment = se.value
            FROM
                sys.objects so
                LEFT JOIN sys.extended_properties se ON se.name = 'MS_Description' AND se.minor_id = 0 AND so.object_id = se.major_id
            WHERE
                so.type  in ('U') AND
                so.is_ms_shipped=0 AND
                OBJECT_NAME(so.object_id) NOT IN ('sysdiagrams', 'sp_alterdiagram','sp_creatediagram','sp_dropdiagram','sp_helpdiagramdefinition','sp_helpdiagrams','sp_renamediagram','sp_upgraddiagrams')
            UNION
            SELECT
                so.object_id,
                schema_name             = SCHEMA_NAME(so.schema_id),
                table_name              = OBJECT_NAME(so.OBJECT_ID),
                column_name             = sc.name, 
                sc.column_id,
                type                    = (SELECT name from sys.types st WHERE st.system_type_id = sc.system_type_id AND st.user_type_id = sc.user_type_id ), 
                max_length, 
                precision,
                scale,
                is_identity,
                is_computed,
                is_primary_key          = ISNULL(sic.is_primary_key,0),
                Comment                 = se.value
            FROM
                sys.columns sc
                LEFT JOIN sys.objects so ON so.object_id = sc.object_id
                LEFT JOIN sys.extended_properties se ON se.name = 'MS_Description' AND se.minor_id = sc.column_id AND sc.object_id = se.major_id
                LEFT JOIN (SELECT si.object_id, sic.column_id, is_primary_key FROM sys.indexes si LEFT JOIN sys.index_columns sic ON si.object_id = sic.object_id and si.index_id = sic.index_id WHERE is_disabled = 0) sic ON sic.object_id = sc.object_id AND sic.column_id = sc.column_id
                LEFT JOIN (SELECT object_id, column_id, definition FROM sys.computed_columns) scc on scc.object_id = sc.object_id AND scc.column_id = sc.column_id
            WHERE
                so.type  in ('U') AND
                so.is_ms_shipped=0 AND
                OBJECT_NAME(sc.object_id) NOT IN ('sysdiagrams', 'sp_alterdiagram','sp_creatediagram','sp_dropdiagram','sp_helpdiagramdefinition','sp_helpdiagrams','sp_renamediagram','sp_upgraddiagrams')
            ORDER BY
                table_name,
                column_id";
        #endregion

        #region public GenerateSP(...)
        public GenerateSP(SqlConnection DbConn, LoggerDelegate Logger, string Author, string DatabaseName, string SaveFolder, bool AddCommentHeader, bool EncloseWithBrackets, bool OmitDbo, bool IncludeSPDescInHeader, bool CreateHistoryAndTrigger) {
            this.Logger = Logger;
            this.DbConn = DbConn;
            if (DbConn.State != ConnectionState.Open)
                DbConn.Open();
            DbConn.ChangeDatabase(DatabaseName);

            this.SaveFolder = SaveFolder;
            this.DatabaseName = DatabaseName;
            this.Author = Author;

            this.AddCommentHeader = AddCommentHeader;
            this.EncloseWithBrackets = EncloseWithBrackets;
            this.OmitDbo = OmitDbo;
            this.IncludeSPDescInHeader = IncludeSPDescInHeader;
            this.CreateHistoryAndTrigger = CreateHistoryAndTrigger;
        }
        #endregion

        #region public void Dispose()
        public void Dispose() {
            DbConn.ChangeDatabase("master");
        }
        #endregion

        #region public void Generate();
        public void Generate() {
            read_sys_types();
            using (SqlDataAdapter adapter = new SqlDataAdapter(SQL_Table_Columns, DbConn)) {
                table_column_data = new DataTable();
                adapter.Fill(table_column_data);
            }
            using (SqlDataAdapter adapter = new SqlDataAdapter(SQL_FK, DbConn)) {
                table_fk_data = new DataTable();
                adapter.Fill(table_fk_data);
            }

            Logger("Reading Database Information ...", Color.Blue);
            // Connect to database if it is not connected
            if (DbConn.State != ConnectionState.Open)
                DbConn.Open();

            //Create output folder if not exists
            create_database_sql();

            string previous_schema = "";
            foreach (DataRow row in table_column_data.Rows) {

                if (row["column_name"].ToString() != "")
                    continue;

                string table_name = row["table_name"].ToString();
                string schema_name = row["schema_name"].ToString();

                if (table_name.ToLower().IndexOf("_hist") > -1)
                    continue;

                if (previous_schema != schema_name) {
                    create_schemabase_sql(schema_name);
                    previous_schema = schema_name;
                }

                DataTable table = new DataView(table_column_data, "schema_name='" + schema_name + "' AND table_name='" + table_name + "' AND column_name IS NOT NULL", "", DataViewRowState.CurrentRows).ToTable();

                if (CreateHistoryAndTrigger) {
                    // Create History table and its trigger
                    create_history_table(table, schema_name, table_name, row["comment"].ToString());
                    create_history_trigger(table, schema_name, table_name);
                }

                // Create Add, Edit, Delete, Get and List and ListBy procedures
                create_add_procedure(table, schema_name, table_name);
                CreateDeleteProcedure(table, schema_name, table_name);
                CreateEditProcedure(table, schema_name, table_name);
                CreateGetProcedure(table, schema_name, table_name);
                CreateListProcedure(table, schema_name, table_name);
                CreateListByFKProcedures(table, schema_name, table_name);
            }
            Logger("Done", Color.Blue);

        }
        #endregion


        #region private string enclose_with_brackets()
        private string enclose_with_brackets(string instr) {
            if (!EncloseWithBrackets || instr == "")
                return instr;
            return "[" + instr + "]";

        }
        #endregion

        #region private string omit_dbo()
        private string omit_dbo(string instr) {
            if (!OmitDbo || instr.ToLower() != "dbo")
                return instr + ".";
            return "";
        }
        #endregion

        #region private int get_max_column_name(...)
        private int get_max_column_name(DataTable table) {
            int max_name_length = 0;
            foreach (DataRow row in table.Rows) {
                int Length = enclose_with_brackets(row["column_name"].ToString()).Length;
                if (max_name_length < Length)
                    max_name_length = Length;
            }
            return max_name_length + 1;
        }
        #endregion

        #region private void read_sys_types()
        private void read_sys_types() {
            if (DbConn.State != ConnectionState.Open)
                DbConn.Open();
            DataTable table = new DataTable();
            using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT name, max_length, precision, scale FROM sys.types", DbConn)) {
                adapter.Fill(table);
                DbTypes.Clear();
                foreach (DataRow row in table.Rows) {
                    DbTypes.Add(row["name"].ToString(), new DbType(row["name"].ToString(), row["max_length"].ToInt32(), row["precision"].ToInt32(), row["scale"].ToInt32()));
                }
            }
        }
        #endregion

        #region private string data_type_string(...)
        private string data_type_string(string type, int precision, int scale, int max_length) {
            if (DbTypes.ContainsKey(type)) {
                DbType type1 = DbTypes[type];
                if (type1.max_length == max_length && type1.precision == precision && type1.scale == scale) {
                    // Types with no size
                    return type;
                } else if (precision == 0 && scale == 0) {
                    //Char Types
                    if (type == "nvarchar" && max_length != 0)
                        max_length = max_length / 2;

                    if (max_length > -1)
                        return type + "(" + max_length + ")";
                    else
                        return type + "(MAX)";
                } else {
                    //Decimal Types
                    return type + "(" + precision + ", " + scale + ")";
                }
            } else {
                // Unknown Type
                return type + "(" + max_length + ", " + precision + ", " + scale + ")";
            }
        }
        #endregion

        #region private void create_database_sql(...);
        private void create_database_sql() {
            try {
                if (!Directory.Exists(SaveFolder))
                    Directory.CreateDirectory(SaveFolder);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("USE master");
                sb.AppendLine("GO");
                sb.AppendLine("CREATE DATABASE " + DatabaseName);
                sb.AppendLine("GO");
                sb.AppendLine("USE " + DatabaseName);
                sb.AppendLine("GO");
                File.WriteAllText(SaveFolder + "databse.sql", sb.ToString());
            } catch (Exception ex) {
                if (ex is gen_exception)
                    Logger("    Error: " + ex.Message, Color.Red);
                else
                    Logger("    Error: " + ex.ToString(), Color.Red);
            }
        }
        #endregion

        #region private void create_schemabase_sql(...);
        private void create_schemabase_sql(string schema_name) {
            try {
                string schema_folder = SaveFolder + schema_name + "\\";

                if (!File.Exists(schema_folder))
                    Directory.CreateDirectory(schema_folder);
                if (schema_name == "dbo" || schema_name == "sys")
                    return;
                Logger("Creating Schema SQL for " + schema_name + " Schema", Color.Blue);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("IF NOT EXISTS ( SELECT * FROM sys.schemas WHERE name='" + schema_name + "' )");
                sb.AppendLine("    EXEC('CREATE SCHEMA " + enclose_with_brackets(schema_name) + "');' )");
                sb.AppendLine("GO");
                File.WriteAllText(schema_folder + schema_name + ".sql", sb.ToString());
            } catch (Exception ex) {
                if (ex is gen_exception)
                    Logger("    Error: " + ex.Message, Color.Red);
                else
                    Logger("    Error: " + ex.ToString(), Color.Red);
            }
        }
        #endregion

        #region private void create_history_table(...)
        private void create_history_table(DataTable table, string schema_name, string table_name, string comment) {
            try {
                string table_save_folder = SaveFolder + "\\" + schema_name + "\\Tables\\";
                if (!Directory.Exists(table_save_folder))
                    Directory.CreateDirectory(table_save_folder);

                int max_name_length = get_max_column_name(table);
                if (max_name_length < 13)
                    max_name_length = 13; // this is length of _DateCreated column that we will add later
                string table_comment = "when a record has changed in " + omit_dbo(schema_name) + table_name + " its previous version is copied here";

                StringBuilder sb = new StringBuilder();
                if (AddCommentHeader) {
                    sb.AppendLine("/* ------------------------------------------------------------------------------------------------------");
                    if (IncludeSPDescInHeader)
                        sb.AppendLine("  Description : " + table_comment.SplitAtWords().PadStringAtNewLines(16));
                    sb.AppendLine("  Created     : " + DateTime.Now.ToString("yyyy-MM-dd") + " " + Author);
                    sb.AppendLine("------------------------------------------------------------------------------------------------------ */");
                }
                sb.AppendLine("CREATE TABLE " + enclose_with_brackets(omit_dbo(schema_name)) + enclose_with_brackets(table_name + "_hist") + "(");
                sb.AppendLine("    " + enclose_with_brackets("_Id").PadRight(max_name_length) + "int IDENTITY(1,1) NOT NULL,");
                sb.AppendLine("    " + enclose_with_brackets("_Action").PadRight(max_name_length) + "char(1),");
                sb.AppendLine("    " + enclose_with_brackets("_DateCreated").PadRight(max_name_length) + "datetime DEFAULT getdate(),");
                List<string> table_column = new List<string> { };
                for (int i = 0; i < table.Rows.Count; i++) {
                    DataRow row = table.Rows[i];
                    table_column.Add("    " + enclose_with_brackets(row["column_name"].ToString()).PadRight(max_name_length) + data_type_string(row["type"].ToString(), row["precision"].ToInt32(), row["scale"].ToInt32(), row["max_length"].ToInt32()));
                }
                sb.AppendLine(string.Join("," + Environment.NewLine, table_column.ToArray()));
                sb.AppendLine("    CONSTRAINT " + enclose_with_brackets("PK_" + table_name + "_hist") + " PRIMARY KEY (" + enclose_with_brackets("_Id") + " ASC)");
                sb.AppendLine(")");
                sb.AppendLine("GO");
                sb.AppendLine();
                sb.AppendLine("-- Table Comments ---------------------------------------------------------------------------------------");
                sb.AppendLine("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'" + schema_name + "', @level1type=N'TABLE', @level1name=N'" + table_name + "_hist', @value=N'" + table_comment + "'");
                sb.AppendLine("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'" + schema_name + "', @level1type=N'TABLE', @level1name=N'" + table_name + "_hist', @level2type=N'COLUMN', @level2name=N'_Id', @value=N'Internal Histroy Record Id'");
                sb.AppendLine("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'" + schema_name + "', @level1type=N'TABLE', @level1name=N'" + table_name + "_hist', @level2type=N'COLUMN', @level2name=N'_Action', @value=N'History Action, D=Delete, E=Edit'");
                sb.AppendLine("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'" + schema_name + "', @level1type=N'TABLE', @level1name=N'" + table_name + "_hist', @level2type=N'COLUMN', @level2name=N'_DateCreated', @value=N'date that action taken place'");
                foreach (DataRow row in table.Rows) {
                    sb.AppendLine("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'" + schema_name + "', @level1type=N'TABLE', @level1name=N'" + table_name + "_hist', @level2type=N'COLUMN', @level2name=N'" + row["column_name"] + "', @value=N'" + row["comment"].ToString().Replace("'", "''") + "'");
                }
                sb.AppendLine("GO");
                sb.AppendLine();

                File.WriteAllText(table_save_folder + table_name + "_hist.sql", sb.ToString());
            } catch (Exception ex) {
                if (ex is gen_exception)
                    Logger("    Error: " + ex.Message, Color.Red);
                else
                    Logger("    Error: " + ex.ToString(), Color.Red);
            }
        }
        #endregion

        #region private void create_history_trigger(...)
        private void create_history_trigger(DataTable table, string schema_name, string table_name) {
            try {
                string trigger_save_folder = SaveFolder + "\\" + schema_name + "\\Trigger\\";
                StringBuilder sb = new StringBuilder();
                if (!Directory.Exists(trigger_save_folder))
                    Directory.CreateDirectory(trigger_save_folder);

                StringBuilder TriggerColList = new StringBuilder();
                for (int i = 0; i < table.Rows.Count; i++) {
                    DataRow row = table.Rows[i];
                    TriggerColList.Append("[" + row["column_name"] + "]");
                    if (i < table.Rows.Count - 1)
                        TriggerColList.Append(", ");
                }

                sb.AppendLine("DROP TRIGGER IF EXISTS " + enclose_with_brackets(omit_dbo(schema_name)) + "TRG_" + enclose_with_brackets(table_name) + "_UpdateDelete]");
                sb.AppendLine("GO");
                if (AddCommentHeader) {
                    sb.AppendLine("/* ------------------------------------------------------------------------------------------------------");
                    if (IncludeSPDescInHeader)
                        sb.AppendLine("  Description : When a record is edited or deleted from " + omit_dbo(schema_name) + table_name + " this will trigger");
                    sb.AppendLine("  Created     : " + DateTime.Now.ToString("yyyy-MM-dd") + " " + Author);
                    sb.AppendLine("------------------------------------------------------------------------------------------------------ */");
                }
                sb.AppendLine("CREATE TRIGGER " + enclose_with_brackets(omit_dbo(schema_name)) + "TRG_" + enclose_with_brackets(table_name) + "_UpdateDelete]");
                sb.AppendLine("   ON  " + enclose_with_brackets(omit_dbo(schema_name)) + enclose_with_brackets(table_name) + "");
                sb.AppendLine("   AFTER UPDATE, DELETE");
                sb.AppendLine("AS");
                sb.AppendLine("BEGIN");
                sb.AppendLine("    SET NOCOUNT ON;");
                sb.AppendLine("    DECLARE @Action char(1)");
                sb.AppendLine();
                sb.AppendLine("    IF EXISTS(SELECT TOP(1) * FROM inserted) BEGIN");
                sb.AppendLine("        SET @Action = 'E' -- Edit");
                sb.AppendLine("    END ELSE BEGIN");
                sb.AppendLine("        SET @Action = 'D' -- Delete");
                sb.AppendLine("    END");
                sb.AppendLine("    INSERT INTO " + enclose_with_brackets(omit_dbo(schema_name)) + enclose_with_brackets(table_name) + "_hist](");
                sb.AppendLine("        [_Action],");
                sb.AppendLine("        " + TriggerColList);
                sb.AppendLine("    ) SELECT");
                sb.AppendLine("        @Action,");
                sb.AppendLine("        " + TriggerColList);
                sb.AppendLine("    FROM deleted");
                sb.AppendLine("END");
                sb.AppendLine("GO");
                sb.AppendLine();

                File.WriteAllText(trigger_save_folder + "TRG_" + table_name + "UpdateDelete.sql", sb.ToString());
            } catch (Exception ex) {
                if (ex is gen_exception)
                    Logger("    Error: " + ex.Message, Color.Red);
                else
                    Logger("    Error: " + ex.ToString(), Color.Red);
            }
        }
        #endregion

        #region private void create_add_procedure(...);
        private void create_add_procedure(DataTable table, string schema_name, string table_name) {
            try {
                Logger("  Writing " + omit_dbo(schema_name) + table_name + "_Add ...", Color.Black);

                string proc_save_folder = SaveFolder + "\\" + schema_name + "\\StoredProcedure\\";
                if (!Directory.Exists(proc_save_folder))
                    Directory.CreateDirectory(proc_save_folder);

                string procedure_comment = "Adds a record to " + omit_dbo(schema_name) + table_name + " table";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("DROP PROCEDURE IF EXISTS " + enclose_with_brackets(omit_dbo(schema_name)) + enclose_with_brackets(table_name + "_Add"));
                sb.AppendLine("GO");

                if (AddCommentHeader) {
                    sb.AppendLine("/* ------------------------------------------------------------------------------------------------------");
                    if (IncludeSPDescInHeader)
                        sb.AppendLine("  Description : " + procedure_comment.SplitAtWords().PadStringAtNewLines(16));
                    sb.AppendLine("  Created     : " + DateTime.Now.ToString("yyyy-MM-dd") + " " + Author);
                    sb.AppendLine("------------------------------------------------------------------------------------------------------ */");
                }
                sb.AppendLine("CREATE PROCEDURE " + enclose_with_brackets(omit_dbo(schema_name)) + enclose_with_brackets(table_name + "_Add") + "(");

                // Add cannot insert into identity, calculated columns or timestamp column
                int max_name_length = get_max_column_name(new DataView(table, "is_computed = 0 AND is_identity = 0 AND type<>'timestamp'", "", DataViewRowState.CurrentRows).ToTable());

                List<string> param_columns = new List<string> { };
                foreach (DataRow row in table.Rows) {
                    if (row["is_computed"].ToBoolean())
                        continue;
                    if (row["is_identity"].ToBoolean())
                        continue;
                    if (row["type"].ToString() == "timestamp")
                        continue;
                    param_columns.Add("    @" + row["column_name"].ToString().PadRight(max_name_length) + data_type_string(row["type"].ToString(), row["precision"].ToInt32(), row["scale"].ToInt32(), row["max_length"].ToInt32()));
                }
                sb.AppendLine(string.Join("," + Environment.NewLine, param_columns.ToArray()));

                sb.AppendLine(") AS");
                sb.AppendLine();

                List<string> insert_columns = new List<string> { };
                List<string> insert_params = new List<string> { };
                bool has_identity = false;
                foreach (DataRow row in table.Rows) {
                    if (row["is_computed"].ToBoolean())
                        continue;
                    if (row["is_identity"].ToBoolean()) {
                        has_identity = true;
                        continue;
                    }
                    insert_columns.Add(enclose_with_brackets(row["column_name"].ToString()));
                    insert_params.Add(row["column_name"].ToString());
                }

                sb.AppendLine("    INSERT INTO " + enclose_with_brackets(omit_dbo(schema_name)) + enclose_with_brackets(table_name) + "(");
                sb.AppendLine("        " + string.Join(", ", insert_columns.ToArray()).SplitAtWords().PadStringAtNewLines(8));
                sb.AppendLine("    ) VALUES (");
                sb.AppendLine("        @" + string.Join(", @", insert_params.ToArray()).SplitAtWords().PadStringAtNewLines(8));
                sb.AppendLine("    )");
                sb.AppendLine();

                if (has_identity)
                    sb.AppendLine("    RETURN SCOPE_IDENTITY()");
                else
                    sb.AppendLine("    RETURN @@ROWCOUNT");

                sb.AppendLine("GO");
                sb.AppendLine();

                sb.AppendLine("-- Stored Procedure and Parameter Description ---------------------------------------------------------------");
                sb.AppendLine("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'" + schema_name + "', @level1type=N'PROCEDURE', @level1name=N'" + table_name + "_Add', @value=N'" + procedure_comment + "'");
                foreach (DataRow row in table.Rows) {
                    if (row["is_computed"].ToBoolean())
                        continue;
                    if (row["is_identity"].ToBoolean())
                        continue;
                    sb.AppendLine("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'" + schema_name + "', @level1type=N'PROCEDURE', @level1name=N'" + table_name + "_Add', @level2type=N'PARAMETER', @level2name=N'@" + row["column_name"] + "', @value=N'" + row["comment"].ToString().Replace("'", "''") + "'");
                }
                sb.AppendLine("GO");
                sb.AppendLine();
                File.WriteAllText(proc_save_folder + table_name + "_Add.sql", sb.ToString());
            } catch (Exception ex) {
                if (ex is gen_exception)
                    Logger("    Error: " + ex.Message, Color.Red);
                else
                    Logger("    Error: " + ex.ToString(), Color.Red);
            }

        }
        #endregion

        #region private void CreateEditProcedure(...);
        private void CreateEditProcedure(DataTable table, string schema_name, string table_name) {
            try {
                Logger("  Writing " + omit_dbo(schema_name) + table_name + "_Edit ...", Color.Black);

                DataTable primary_keys = new DataView(table, "is_primary_key=1", "", DataViewRowState.CurrentRows).ToTable();
                if (primary_keys.Rows.Count < 1)
                    throw new gen_exception("Table does not have primary key, will not create edit procedure");


                string proc_save_folder = SaveFolder + "\\" + schema_name + "\\StoredProcedure\\";
                if (!Directory.Exists(proc_save_folder))
                    Directory.CreateDirectory(proc_save_folder);

                string procedure_comment = "Edits a record in " + omit_dbo(schema_name) + table_name + " table.";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("DROP PROCEDURE IF EXISTS " + enclose_with_brackets(omit_dbo(schema_name)) + enclose_with_brackets(table_name + "_Edit"));
                sb.AppendLine("GO");

                if (AddCommentHeader) {
                    sb.AppendLine("/* ------------------------------------------------------------------------------------------------------");
                    if (IncludeSPDescInHeader)
                        sb.AppendLine("  Description : " + procedure_comment.SplitAtWords().PadStringAtNewLines(16));
                    sb.AppendLine("  Created     : " + DateTime.Now.ToString("yyyy-MM-dd") + " " + Author);
                    sb.AppendLine("------------------------------------------------------------------------------------------------------ */");
                }


                // This method has specific max_name_length calculation that modifies name of columns case by case, so it is not possible to use method get_max_column_name
                int max_name_length = get_max_column_name(new DataView(table, "is_computed = 0 AND type<>'timestamp'", "", DataViewRowState.CurrentRows).ToTable());
                int max_param_length = max_name_length - 1;
                foreach (DataRow row in table.Rows) {
                    if (row["is_computed"].ToBoolean())
                        continue;
                    if (row["type"].ToString() == "timestamp")
                        continue;
                    if (row["is_primary_key"].ToBoolean() && !row["is_identity"].ToBoolean()) {
                        int len = enclose_with_brackets(row["column_name"].ToString()).Length + 3;
                        if (max_param_length < len) {
                            max_param_length = len;
                        }
                        continue;
                    }
                }
                max_param_length++;

                sb.AppendLine("CREATE PROCEDURE " + enclose_with_brackets(omit_dbo(schema_name)) + enclose_with_brackets(table_name + "_Edit") + "(");
                List<string> param_columns = new List<string> { };
                List<string> key_columns = new List<string> { };
                // Edit cannot modify calculated columns or timestamp column but an identity column used as primary key is needed
                foreach (DataRow row in table.Rows) {
                    // calculated column cannot be modified
                    if (row["is_computed"].ToBoolean())
                        continue;
                    // timestamp cannot be modified
                    if (row["type"].ToString() == "timestamp")
                        continue;
                    if (row["is_primary_key"].ToBoolean() && !row["is_identity"].ToBoolean()) {
                        // A key that is not identity, can be modified
                        key_columns.Add("@" + ("Old" + row["column_name"]).PadRight(max_param_length) + data_type_string(row["type"].ToString(), row["precision"].ToInt32(), row["scale"].ToInt32(), row["max_length"].ToInt32()));
                        param_columns.Add("@" + ("New" + row["column_name"]).PadRight(max_param_length) + data_type_string(row["type"].ToString(), row["precision"].ToInt32(), row["scale"].ToInt32(), row["max_length"].ToInt32()));
                        continue;
                    } else if (row["is_primary_key"].ToBoolean()) {
                        // A key that is identity, cannot be modified
                        key_columns.Add("@" + row["column_name"].ToString().PadRight(max_param_length) + data_type_string(row["type"].ToString(), row["precision"].ToInt32(), row["scale"].ToInt32(), row["max_length"].ToInt32()));
                        continue;
                    } else if (row["is_identity"].ToBoolean()) {
                        continue;
                    }
                    param_columns.Add("@" + row["column_name"].ToString().PadRight(max_param_length) + data_type_string(row["type"].ToString(), row["precision"].ToInt32(), row["scale"].ToInt32(), row["max_length"].ToInt32()));
                }
                sb.AppendLine("    " + string.Join("," + Environment.NewLine, key_columns).PadStringAtNewLines(4) + ",");
                sb.AppendLine("    " + string.Join("," + Environment.NewLine, param_columns).PadStringAtNewLines(4));

                sb.AppendLine(") AS");
                sb.AppendLine();

                // Add Columns
                List<string> set_list = new List<string> { };
                List<string> condition_list = new List<string> { };
                foreach (DataRow row in table.Rows) {
                    if (row["is_computed"].ToBoolean())
                        continue;
                    if (row["type"].ToString() == "timestamp")
                        continue;
                    if (row["is_primary_key"].ToBoolean() && !row["is_identity"].ToBoolean()) {
                        // A key that is not identity, can be modified
                        set_list.Add(row["column_name"].ToString().PadRight(max_name_length) + "= @New" + row["column_name"].ToString());
                        condition_list.Add(row["column_name"].ToString().PadRight(max_name_length) + "= @Old" + row["column_name"].ToString());
                        continue;
                    } else if (row["is_primary_key"].ToBoolean()) {
                        // A key that is identity, cannot be modified
                        condition_list.Add(row["column_name"].ToString().PadRight(max_name_length) + "= @" + row["column_name"].ToString());
                        continue;
                    } else if (row["is_identity"].ToBoolean()) {
                        continue;
                    }
                    set_list.Add(row["column_name"].ToString().PadRight(max_name_length) + "= @" + row["column_name"].ToString());
                }
                sb.AppendLine("    UPDATE " + enclose_with_brackets(omit_dbo(schema_name)) + enclose_with_brackets(table_name) + " SET");
                sb.AppendLine("        " + string.Join("," + Environment.NewLine, set_list).PadStringAtNewLines(8));
                sb.AppendLine("    WHERE");
                sb.AppendLine("        " + string.Join("," + Environment.NewLine, condition_list).PadStringAtNewLines(8));
                sb.AppendLine();
                sb.AppendLine("    RETURN @@ROWCOUNT");
                sb.AppendLine();
                sb.AppendLine("GO");
                sb.AppendLine();

                sb.AppendLine("-- Stored Procedure and Parameter Description ---------------------------------------------------------------");
                sb.AppendLine("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'" + schema_name + "', @level1type=N'PROCEDURE', @level1name=N'" + table_name + "_Edit', @value=N'" + procedure_comment.Replace("'", "''") + "'");
                // Add key comments first
                foreach (DataRow row in table.Rows) {
                    if (row["is_computed"].ToBoolean())
                        continue;
                    if (row["type"].ToString() == "timestamp")
                        continue;
                    if (row["is_primary_key"].ToBoolean() && !row["is_identity"].ToBoolean()) {
                        sb.AppendLine("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'" + schema_name + "', @level1type=N'PROCEDURE', @level1name=N'" + table_name + "_Edit', @level2type=N'PARAMETER', @level2name=N'@Old" + row["column_name"] + "', @value=N'" + row["comment"].ToString().Replace("'", "''") + "'");
                        sb.AppendLine("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'" + schema_name + "', @level1type=N'PROCEDURE', @level1name=N'" + table_name + "_Edit', @level2type=N'PARAMETER', @level2name=N'@New" + row["column_name"] + "', @value=N'" + row["comment"].ToString().Replace("'", "''") + "'");
                        continue;
                    } else if (row["is_primary_key"].ToBoolean()) {
                        sb.AppendLine("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'" + schema_name + "', @level1type=N'PROCEDURE', @level1name=N'" + table_name + "_Edit', @level2type=N'PARAMETER', @level2name=N'@" + row["column_name"] + "', @value=N'" + row["comment"].ToString().Replace("'", "''") + "'");
                        continue;
                    } else if (row["is_identity"].ToBoolean()) {
                        continue;
                    }
                }
                foreach (DataRow row in table.Rows) {
                    if (row["is_computed"].ToBoolean())
                        continue;
                    if (row["type"].ToString() == "timestamp")
                        continue;
                    if (row["is_primary_key"].ToBoolean()) {
                        continue;
                    } else if (row["is_identity"].ToBoolean()) {
                        continue;
                    }
                    sb.AppendLine("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'" + schema_name + "', @level1type=N'PROCEDURE', @level1name=N'" + table_name + "_Edit', @level2type=N'PARAMETER', @level2name=N'@" + row["column_name"] + "', @value=N'" + row["comment"].ToString().Replace("'", "''") + "'");
                }

                sb.AppendLine("GO" + Environment.NewLine);
                File.WriteAllText(proc_save_folder + table_name + "_Edit.sql", sb.ToString());
            } catch (Exception ex) {
                if (ex is gen_exception)
                    Logger("    Error: " + ex.Message, Color.Red);
                else
                    Logger("    Error: " + ex.ToString(), Color.Red);
            }

        }
        #endregion

        #region private void CreateDeleteProcedure(...);
        private void CreateDeleteProcedure(DataTable table, string schema_name, string table_name) {
            try {
                Logger("  Writing " + omit_dbo(schema_name) + table_name + "_Delete ...", Color.Black);

                DataTable primary_keys = new DataView(table, "is_primary_key=1", "", DataViewRowState.CurrentRows).ToTable();
                if (primary_keys.Rows.Count < 1)
                    throw new gen_exception("Table does not have primary key, will not create delete procedure");

                string proc_save_folder = SaveFolder + "\\" + schema_name + "\\StoredProcedure\\";
                if (!Directory.Exists(proc_save_folder))
                    Directory.CreateDirectory(proc_save_folder);

                string procedure_comment = "Deletes a record in " + omit_dbo(schema_name) + table_name + " table.";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("DROP PROCEDURE IF EXISTS " + enclose_with_brackets(omit_dbo(schema_name)) + enclose_with_brackets(table_name + "_Delete"));
                sb.AppendLine("GO");

                if (AddCommentHeader) {
                    sb.AppendLine("/* ------------------------------------------------------------------------------------------------------");
                    if (IncludeSPDescInHeader)
                        sb.AppendLine("  Description : " + procedure_comment.SplitAtWords().PadStringAtNewLines(16));
                    sb.AppendLine("  Created     : " + DateTime.Now.ToString("yyyy-MM-dd") + " " + Author);
                    sb.AppendLine("------------------------------------------------------------------------------------------------------ */");
                }

                int max_name_length = get_max_column_name(new DataView(table, "is_primary_key=1", "", DataViewRowState.CurrentRows).ToTable());

                sb.AppendLine("CREATE PROCEDURE " + enclose_with_brackets(omit_dbo(schema_name)) + enclose_with_brackets(table_name + "_Delete") + "(");
                List<string> key_columns = new List<string> { };
                foreach (DataRow row in table.Rows) {
                    // only interested in primary key and identity
                    if (!row["is_primary_key"].ToBoolean())
                        continue;

                    key_columns.Add("@" + row["column_name"].ToString().PadRight(max_name_length) + data_type_string(row["type"].ToString(), row["precision"].ToInt32(), row["scale"].ToInt32(), row["max_length"].ToInt32()));
                }
                sb.AppendLine("    " + string.Join("," + Environment.NewLine, key_columns).PadStringAtNewLines(4));
                sb.AppendLine(") AS");
                sb.AppendLine();

                sb.AppendLine("    DELETE FROM " + enclose_with_brackets(omit_dbo(schema_name)) + enclose_with_brackets(table_name));
                sb.AppendLine("    WHERE");

                List<string> condition_list = new List<string> { };
                foreach (DataRow row in table.Rows) {
                    // only interested in primary key and identity
                    if (!row["is_primary_key"].ToBoolean())
                        continue;
                    condition_list.Add(row["column_name"].ToString().PadRight(max_name_length) + "= @" + row["column_name"]);
                }
                sb.AppendLine("        " + string.Join(" AND" + Environment.NewLine, condition_list).PadStringAtNewLines(8));
                sb.AppendLine();
                sb.AppendLine("    RETURN @@ROWCOUNT");
                sb.AppendLine();
                sb.AppendLine("GO");
                sb.AppendLine();

                sb.AppendLine("-- Stored Procedure and Parameter Description ---------------------------------------------------------------");
                sb.AppendLine("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'" + schema_name + "', @level1type=N'PROCEDURE', @level1name=N'" + table_name + "_Delete', @value=N'" + procedure_comment.Replace("'", "''") + "'");
                foreach (DataRow row in table.Rows) {
                    if (!row["is_primary_key"].ToBoolean())
                        continue;
                    sb.AppendLine("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'" + schema_name + "', @level1type=N'PROCEDURE', @level1name=N'" + table_name + "_Delete', @level2type=N'PARAMETER', @level2name=N'@" + row["column_name"] + "', @value=N'" + row["comment"].ToString().Replace("'", "''") + "'");
                }
                sb.AppendLine("GO" + Environment.NewLine);

                File.WriteAllText(proc_save_folder + table_name + "_Delete.sql", sb.ToString());
            } catch (Exception ex) {
                if (ex is gen_exception)
                    Logger("    Error: " + ex.Message, Color.Red);
                else
                    Logger("    Error: " + ex.ToString(), Color.Red);
            }
        }
        #endregion

        #region private void CreateGetProcedure(...);
        private void CreateGetProcedure(DataTable table, string schema_name, string table_name) {
            try {
                Logger("  Writing " + omit_dbo(schema_name) + table_name + "_Get ...", Color.Black);

                DataTable primary_keys = new DataView(table, "is_primary_key=1", "", DataViewRowState.CurrentRows).ToTable();
                if (primary_keys.Rows.Count < 1)
                    throw new gen_exception("Table does not have primary key, will not create delete procedure");

                string proc_save_folder = SaveFolder + "\\" + schema_name + "\\StoredProcedure\\";
                if (!Directory.Exists(proc_save_folder))
                    Directory.CreateDirectory(proc_save_folder);

                string procedure_comment = "Gets a record in " + omit_dbo(schema_name) + table_name + " table.";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("DROP PROCEDURE IF EXISTS " + enclose_with_brackets(omit_dbo(schema_name)) + enclose_with_brackets(table_name + "_Get"));
                sb.AppendLine("GO");

                if (AddCommentHeader) {
                    sb.AppendLine("/* ------------------------------------------------------------------------------------------------------");
                    if (IncludeSPDescInHeader)
                        sb.AppendLine("  Description : " + procedure_comment.SplitAtWords().PadStringAtNewLines(16));
                    sb.AppendLine("  Created     : " + DateTime.Now.ToString("yyyy-MM-dd") + " " + Author);
                    sb.AppendLine("------------------------------------------------------------------------------------------------------ */");
                }

                int max_name_length = get_max_column_name(new DataView(table, "is_primary_key=1", "", DataViewRowState.CurrentRows).ToTable());

                sb.AppendLine("CREATE PROCEDURE " + enclose_with_brackets(omit_dbo(schema_name)) + enclose_with_brackets(table_name + "_Get") + "(");
                List<string> key_columns = new List<string> { };
                foreach (DataRow row in table.Rows) {
                    // only interested in primary key and identity
                    if (!row["is_primary_key"].ToBoolean())
                        continue;

                    key_columns.Add("@" + row["column_name"].ToString().PadRight(max_name_length) + data_type_string(row["type"].ToString(), row["precision"].ToInt32(), row["scale"].ToInt32(), row["max_length"].ToInt32()));
                }
                sb.AppendLine("    " + string.Join("," + Environment.NewLine, key_columns).PadStringAtNewLines(4));
                sb.AppendLine(") AS");
                sb.AppendLine();

                sb.AppendLine("    SELECT * FROM " + enclose_with_brackets(omit_dbo(schema_name)) + enclose_with_brackets(table_name));
                sb.AppendLine("    WHERE");

                List<string> condition_list = new List<string> { };
                foreach (DataRow row in table.Rows) {
                    // only interested in primary key and identity
                    if (!row["is_primary_key"].ToBoolean())
                        continue;
                    condition_list.Add(row["column_name"].ToString().PadRight(max_name_length) + "= @" + row["column_name"]);
                }
                sb.AppendLine("        " + string.Join(" AND" + Environment.NewLine, condition_list).PadStringAtNewLines(8));
                sb.AppendLine();
                sb.AppendLine("    RETURN @@ROWCOUNT");
                sb.AppendLine();
                sb.AppendLine("GO");
                sb.AppendLine();

                sb.AppendLine("-- Stored Procedure and Parameter Description ---------------------------------------------------------------");
                sb.AppendLine("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'" + schema_name + "', @level1type=N'PROCEDURE', @level1name=N'" + table_name + "_Get', @value=N'" + procedure_comment.Replace("'", "''") + "'");
                foreach (DataRow row in table.Rows) {
                    if (!row["is_primary_key"].ToBoolean())
                        continue;
                    sb.AppendLine("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'" + schema_name + "', @level1type=N'PROCEDURE', @level1name=N'" + table_name + "_Get', @level2type=N'PARAMETER', @level2name=N'@" + row["column_name"] + "', @value=N'" + row["comment"].ToString().Replace("'", "''") + "'");
                }
                sb.AppendLine("GO" + Environment.NewLine);

                File.WriteAllText(proc_save_folder + table_name + "_Get.sql", sb.ToString());
            } catch (Exception ex) {
                if (ex is gen_exception)
                    Logger("    Error: " + ex.Message, Color.Red);
                else
                    Logger("    Error: " + ex.ToString(), Color.Red);
            }
        }
        #endregion

        #region private void CreateListProcedure(...);
        private void CreateListProcedure(DataTable table, string schema_name, string table_name) {
            try {
                Logger("  Writing " + omit_dbo(schema_name) + table_name + "_List ...", Color.Black);

                DataTable primary_keys = new DataView(table, "is_primary_key=1", "", DataViewRowState.CurrentRows).ToTable();
                if (primary_keys.Rows.Count < 1)
                    throw new gen_exception("Table does not have primary key, will not create delete procedure");

                string proc_save_folder = SaveFolder + "\\" + schema_name + "\\StoredProcedure\\";
                if (!Directory.Exists(proc_save_folder))
                    Directory.CreateDirectory(proc_save_folder);

                string procedure_comment = "Lists records from " + omit_dbo(schema_name) + table_name + " table.";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("DROP PROCEDURE IF EXISTS " + enclose_with_brackets(omit_dbo(schema_name)) + enclose_with_brackets(table_name + "_List"));
                sb.AppendLine("GO");

                if (AddCommentHeader) {
                    sb.AppendLine("/* ------------------------------------------------------------------------------------------------------");
                    if (IncludeSPDescInHeader)
                        sb.AppendLine("  Description : " + procedure_comment.SplitAtWords().PadStringAtNewLines(16));
                    sb.AppendLine("  Created     : " + DateTime.Now.ToString("yyyy-MM-dd") + " " + Author);
                    sb.AppendLine("------------------------------------------------------------------------------------------------------ */");
                }

                // Table SQL Command
                sb.AppendLine("CREATE PROCEDURE " + enclose_with_brackets(omit_dbo(schema_name)) + enclose_with_brackets(table_name + "_List") + "(");
                sb.AppendLine("    @MaxRows int");
                sb.AppendLine(") AS");
                sb.AppendLine();
                sb.AppendLine("    IF @MaxRows IS NULL");
                sb.AppendLine("        SET @MaxRows = 1000");
                sb.AppendLine();
                sb.AppendLine("    SELECT TOP(@MaxRows) *");
                sb.AppendLine("    FROM " + enclose_with_brackets(omit_dbo(schema_name)) + enclose_with_brackets(table_name));
                sb.AppendLine("    -- WHERE ");
                sb.AppendLine("    -- ORDER BY");
                sb.AppendLine();
                sb.AppendLine("    RETURN @@ROWCOUNT");
                sb.AppendLine();
                sb.AppendLine("GO");
                sb.AppendLine();

                // Add Procedure Comments
                sb.AppendLine("-- Stored Procedure and Parameter Description ---------------------------------------------------------------");
                sb.AppendLine("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'" + schema_name + "', @level1type=N'PROCEDURE', @level1name=N'" + table_name + "_List', @value=N'" + procedure_comment.Replace("'", "''") + "'");
                sb.AppendLine("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'" + schema_name + "', @level1type=N'PROCEDURE', @level1name=N'" + table_name + "_List', @level2type=N'PARAMETER', @level2name=N'@MaxRows', @value=N'Maximum rows to return'");
                sb.AppendLine("GO");
                sb.AppendLine("");

                File.WriteAllText(proc_save_folder + table_name + "_List.sql", sb.ToString());
            } catch (Exception ex) {
                if (ex is gen_exception)
                    Logger("    Error: " + ex.Message, Color.Red);
                else
                    Logger("    Error: " + ex.ToString(), Color.Red);
            }
        }
        #endregion

        #region private void CreateListByFKProcedures(...)
        private void CreateListByFKProcedures(DataTable table, string schema_name, string table_name) {
            try {
                string proc_save_folder = SaveFolder + "\\" + schema_name + "\\StoredProcedure\\";
                if (!Directory.Exists(proc_save_folder))
                    Directory.CreateDirectory(proc_save_folder);

                DataTable fk = new DataView(table_fk_data, "FromTableSchema='" + schema_name + "' AND FromTable='" + table_name + "'", "", DataViewRowState.CurrentRows).ToTable();
                foreach (DataRow row in fk.Rows) {
                    try {
                        string ref_column = row["FromColumn"].ToString();

                        Logger("  Writing " + omit_dbo(schema_name) + table_name + "_ListBy" + ref_column + " ...", Color.Black);
                        string procedure_comment = "Lists records from " + omit_dbo(schema_name) + table_name + " table by " + ref_column;

                        DataRow ref_row_details = new DataView(table_column_data, "column_name='" + ref_column + "'", "", DataViewRowState.CurrentRows).ToTable().Rows[0];

                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("DROP PROCEDURE IF EXISTS " + enclose_with_brackets(omit_dbo(schema_name)) + enclose_with_brackets(table_name + "_ListBy" + ref_column));
                        sb.AppendLine("GO");

                        if (AddCommentHeader) {
                            sb.AppendLine("/* ------------------------------------------------------------------------------------------------------");
                            if (IncludeSPDescInHeader)
                                sb.AppendLine("  Description : " + procedure_comment.SplitAtWords().PadStringAtNewLines(16));
                            sb.AppendLine("  Created     : " + DateTime.Now.ToString("yyyy-MM-dd") + " " + Author);
                            sb.AppendLine("------------------------------------------------------------------------------------------------------ */");
                        }

                        // Table SQL Command
                        sb.AppendLine("CREATE PROCEDURE " + enclose_with_brackets(omit_dbo(schema_name)) + enclose_with_brackets(table_name + "_ListBy" + ref_column) + "(");
                        sb.AppendLine("    @" + ref_column + " " + data_type_string(ref_row_details["type"].ToString(), ref_row_details["precision"].ToInt32(), ref_row_details["scale"].ToInt32(), ref_row_details["max_length"].ToInt32()) + ",");
                        sb.AppendLine("    @MaxRows int");
                        sb.AppendLine(") AS");
                        sb.AppendLine();
                        sb.AppendLine("    IF @MaxRows IS NULL");
                        sb.AppendLine("        SET @MaxRows = 1000");
                        sb.AppendLine();
                        sb.AppendLine("    SELECT TOP(@MaxRows) *");
                        sb.AppendLine("    FROM " + enclose_with_brackets(omit_dbo(schema_name)) + enclose_with_brackets(table_name));
                        sb.AppendLine("    WHERE ");
                        sb.AppendLine("        " + ref_column + " = @" + ref_column);
                        sb.AppendLine("    -- ORDER BY");
                        sb.AppendLine();
                        sb.AppendLine("    RETURN @@ROWCOUNT");
                        sb.AppendLine();
                        sb.AppendLine("GO");
                        sb.AppendLine();

                        // Add Procedure Comments
                        sb.AppendLine("-- Stored Procedure and Parameter Description ---------------------------------------------------------------");
                        sb.AppendLine("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'" + schema_name + "', @level1type=N'PROCEDURE', @level1name=N'" + table_name + "_ListBy" + ref_column + "', @value=N'" + procedure_comment.Replace("'", "''") + "'");
                        sb.AppendLine("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'" + schema_name + "', @level1type=N'PROCEDURE', @level1name=N'" + table_name + "_ListBy" + ref_column + "', @level2type=N'PARAMETER', @level2name=N'@MaxRows', @value=N'Maximum rows to return'");
                        sb.AppendLine("GO");
                        sb.AppendLine("");

                        File.WriteAllText(proc_save_folder + table_name + "_ListBy" + ref_column + ".sql", sb.ToString());
                    } catch (Exception ex) {
                        if (ex is gen_exception)
                            Logger("    Error: " + ex.Message, Color.Red);
                        else
                            Logger("    Error: " + ex.ToString(), Color.Red);
                    }

                }
            } catch (Exception ex) {
                if (ex is gen_exception)
                    Logger("    Error: " + ex.Message, Color.Red);
                else
                    Logger("    Error: " + ex.ToString(), Color.Red);
            }
        }
        #endregion
    }
}