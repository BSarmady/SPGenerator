using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace SPGenerator {

    delegate void LoggerDelegate(string Message, Color? color = null);

    public partial class frmMain: Form {

        private const string AppName = "SPGenerator";
        private const string AppRegistryKey = @"Software\JGhost\SPGenerator";
        private const string Key = "Sd6Ci36qT3t6VTUv/62TGQ==";
        private bool Running = false;
        private SqlConnection DbConn = new SqlConnection();
        private List<ServerSettings> Servers;
        private string LastSelectedDatabase = "";

        #region public frmMain()
        public frmMain() {
            InitializeComponent();
        }
        #endregion

        #region private void Log(...)
        private void Log(string Message, Color? color) {
            if (this.InvokeRequired) {
                this.Invoke((MethodInvoker) delegate {
                    _Log(Message, color);
                });
            } else {
                _Log(Message, color);
            }
        }
        private void _Log(string Message, Color? color) {
            if (txtLog.IsDisposed)
                return;
            txtLog.SelectionColor = color == null ? Color.Black : color.Value;
            txtLog.AppendText(Message + "\n");
            Control control = this.ActiveControl;
            txtLog.Focus();
            // Return focus back to the control that had focus before
            control.Focus();
            Application.DoEvents();
        }
        #endregion

        #region private void ReadDatabases()
        private void ReadDatabases() {
            using (SqlDataAdapter adapter = new SqlDataAdapter("sp_databases", DbConn)) {
                DataTable table = new DataTable();
                adapter.Fill(table);
                cbDatabase.Items.Clear();
                if (table != null && table.Rows.Count > 0) {
                    foreach (DataRow row in table.Rows) {
                        cbDatabase.Items.Add(row["database_name"].ToString());
                    }
                }
            }
            if (cbDatabase.Items.Count > 0)
                cbDatabase.SelectedIndex = 0;
            if (LastSelectedDatabase != "" && cbDatabase.Items.IndexOf(LastSelectedDatabase) > -1)
                cbDatabase.SelectedIndex = cbDatabase.Items.IndexOf(LastSelectedDatabase);
        }
        #endregion

        #region private void frmMain_Shown(...)
        private void frmMain_Shown(object sender, EventArgs e) {
            this.Icon = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
            grpOperation.Enabled = false;
            grpDbConn.Enabled = true;

            edtAuthor.Text = Reg.Read(AppRegistryKey, "Author", edtAuthor.Text);
            edtSaveFolder.Text = Reg.Read(AppRegistryKey, "SaveFolder", Application.StartupPath.AddTrailingBackSlashes());
            cbAddCommentHeader.Checked = Reg.Read(AppRegistryKey, "AddCommentHeader", 1) == 1;
            cbEncloseWithBrackets.Checked = Reg.Read(AppRegistryKey, "EncloseWithBrackets", 1) == 1;
            cbOmitDbo.Checked = Reg.Read(AppRegistryKey, "OmitDbo", 1) == 1;
            cbIncludeSPDescInHeader.Checked = Reg.Read(AppRegistryKey, "IncludeSPDescInHeader", 1) == 1;
            cbCreateHistoryAndTrigger.Checked = Reg.Read(AppRegistryKey, "CreateHistoryAndTrigger", 1) == 1;
            cbSavelLog.Checked = Reg.Read(AppRegistryKey, "SavelLog", 0) == 1;

            Servers = ServerList.List(AppRegistryKey);
            foreach (ServerSettings settings in Servers)
                cbServers.Items.Add(settings.Server);
            if (cbServers.Items.Count > 0)
                cbServers.SelectedIndex = 0;
        }
        #endregion

        #region private void frmMain_FormClosed(...)
        private void frmMain_FormClosed(object sender, FormClosedEventArgs e) {
            Reg.Write(AppRegistryKey, "Author", edtAuthor.Text);
            Reg.Write(AppRegistryKey, "SaveFolder", edtSaveFolder.Text);
            Reg.Write(AppRegistryKey, "AddCommentHeader", cbAddCommentHeader.Checked ? 1 : 0);
            Reg.Write(AppRegistryKey, "EncloseWithBrackets", cbEncloseWithBrackets.Checked ? 1 : 0);
            Reg.Write(AppRegistryKey, "OmitDbo", cbOmitDbo.Checked ? 1 : 0);
            Reg.Write(AppRegistryKey, "IncludeSPDescInHeader", cbIncludeSPDescInHeader.Checked ? 1 : 0);
            Reg.Write(AppRegistryKey, "CreateHistoryAndTrigger", cbCreateHistoryAndTrigger.Checked ? 1 : 0);
            Reg.Write(AppRegistryKey, "SavelLog", cbSavelLog.Checked ? 1 : 0);
        }
        #endregion

        #region private void cbServers_SelectedIndexChanged(...)
        private void cbServers_SelectedIndexChanged(object sender, EventArgs e) {
            // Read database configuration when selected server changes
            if (cbServers.Text != "") {
                foreach (ServerSettings setting in Servers) {
                    if (cbServers.Text == setting.Server) {
                        cbAuthentication.SelectedIndex = setting.Authentication;
                        edtUserName.Text = setting.Username;
                        try {
                            edtPassword.Text = Crypto.AES.Decrypt(setting.Password, Key);
                        } catch {
                            edtPassword.Text = "";
                        }
                        LastSelectedDatabase = setting.LastDatabaseName;
                    }
                }
            }
        }
        #endregion

        #region private void cbAuthentication_SelectedIndexChanged(...)
        private void cbAuthentication_SelectedIndexChanged(object sender, EventArgs e) {
            if (cbAuthentication.SelectedIndex == 0) {
                edtPassword.Enabled = true;
                edtUserName.Enabled = true;
                lblPassword.Enabled = true;
                lblUserName.Enabled = true;
            } else {
                edtPassword.Enabled = false;
                edtUserName.Enabled = false;
                lblPassword.Enabled = false;
                lblUserName.Enabled = false;
            }
        }
        #endregion

        #region private void btnBrowse_Click(...)
        private void btnBrowse_Click(object sender, EventArgs e) {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            if (Directory.Exists(edtSaveFolder.Text)) {
                dialog.SelectedPath = edtSaveFolder.Text;
            }
            if (dialog.ShowDialog(this) == DialogResult.OK) {
                edtSaveFolder.Text = dialog.SelectedPath.AddTrailingBackSlashes();
            }
        }
        #endregion

        #region private void btnConnect_Click(...)
        private void btnConnect_Click(object sender, EventArgs e) {
            try {
                if (DbConn.State != ConnectionState.Open) {
                    Log("Trying to connect to " + cbServers.Text + " ...", Color.Blue);
                    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                    if (cbAuthentication.SelectedIndex == 0) {
                        builder.DataSource = cbServers.Text;
                        builder.UserID = edtUserName.Text;
                        builder.Password = edtPassword.Text;
                        builder.IntegratedSecurity = false;

                    } else {
                        builder["integrated Security"] = true;
                    }
                    builder["Initial Catalog"] = "master";
                    DbConn.ConnectionString = builder.ConnectionString;
                    DbConn.Open();

                    #region Save connected server to registry
                    ServerList.Update(AppRegistryKey, new ServerSettings(
                        cbServers.Text,
                        cbAuthentication.SelectedIndex,
                        edtUserName.Text,
                        Crypto.AES.Encrypt(edtPassword.Text, Key)
                    ));
                    Servers = ServerList.List(AppRegistryKey);
                    #endregion

                    grpOperation.Enabled = true;
                    ReadDatabases();

                    cbServers.Enabled = false;
                    cbAuthentication.Enabled = false;
                    edtUserName.Enabled = false;
                    edtPassword.Enabled = false;
                    lblUserName.Enabled = false;
                    lblPassword.Enabled = false;
                    lblAuthentication.Enabled = false;
                    lblServers.Enabled = false;

                    Log("  Connected", Color.Black);
                    btnConnect.Text = "Disconnect";
                } else {
                    DbConn.Close();
                    btnConnect.Text = "Connect";
                    Log("Server Disconnected", Color.Black);
                    grpOperation.Enabled = false;

                    lblAuthentication.Enabled = true;
                    lblServers.Enabled = true;
                    cbServers.Enabled = true;
                    cbAuthentication.Enabled = true;
                    cbAuthentication_SelectedIndexChanged(null, null);
                }
            } catch (Exception ex) {
                Log(ex.Message, Color.Red);
            }
        }
        #endregion

        #region private async void btnRun_Click(...)
        private async void btnRun_Click(object sender, EventArgs e) {
            try {
                if (Running)
                    return;
                Running = true;
                grpOperation.Enabled = false;
                txtLog.Clear();
                edtSaveFolder.Text = edtSaveFolder.Text.AddTrailingBackSlashes();

                #region Save connected server to registry
                ServerList.Update(AppRegistryKey, new ServerSettings(
                    cbServers.Text,
                    cbAuthentication.SelectedIndex,
                    edtUserName.Text,
                    Crypto.AES.Encrypt(edtPassword.Text, Key),
                    cbDatabase.Text
                ));
                Servers = ServerList.List(AppRegistryKey);

                Reg.Write(AppRegistryKey, "SaveFolder", edtSaveFolder.Text);
                Reg.Write(AppRegistryKey, "Author", edtAuthor.Text);
                Reg.Write(AppRegistryKey, "AddCommentHeader", cbAddCommentHeader.Checked ? 1 : 0);
                Reg.Write(AppRegistryKey, "EncloseWithBrackets", cbEncloseWithBrackets.Checked ? 1 : 0);
                Reg.Write(AppRegistryKey, "OmitDbo", cbOmitDbo.Checked ? 1 : 0);
                Reg.Write(AppRegistryKey, "IncludeSPDescInHeader", cbIncludeSPDescInHeader.Checked ? 1 : 0);
                Reg.Write(AppRegistryKey, "CreateHistoryAndTrigger", cbCreateHistoryAndTrigger.Checked ? 1 : 0);
                Reg.Write(AppRegistryKey, "SavelLog", cbSavelLog.Checked ? 1 : 0);
                #endregion

                string author = edtAuthor.Text;
                string database = cbDatabase.SelectedItem.ToString();
                string saveFolder = edtSaveFolder.Text + cbDatabase.Text.AddTrailingBackSlashes();

                bool AddCommentHeader = cbAddCommentHeader.Checked;
                bool EncloseWithBrackets = cbEncloseWithBrackets.Checked;
                bool OmitDbo = cbOmitDbo.Checked;
                bool IncludeSPDescInHeader = cbIncludeSPDescInHeader.Checked;
                bool CreateHistoryAndTrigger = cbCreateHistoryAndTrigger.Checked;
                await Task.Run(() => {
                    using (GenerateSP generateSP = new GenerateSP(DbConn, Log, author, database, saveFolder, AddCommentHeader, EncloseWithBrackets, OmitDbo, IncludeSPDescInHeader, CreateHistoryAndTrigger)) {
                        generateSP.Generate();
                    }
                });
                if (cbSavelLog.Checked)
                    txtLog.SaveFile(saveFolder + "SPGenerator.log.doc");
            } catch (Exception ex) {
                Log(ex.Message, Color.Red);
            } finally {
                grpOperation.Enabled = true;
                Running = false;
            }
        }
        #endregion

        private void cbAddCommentHeader_CheckedChanged(object sender, EventArgs e) {
            cbIncludeSPDescInHeader.Enabled = cbAddCommentHeader.Checked;
        }
    }
}

