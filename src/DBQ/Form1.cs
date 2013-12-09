using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBQ
{
    public partial class Form1 : Form
    {        
        string xmlConfig = "config.xml";

        string Server = "";
        string DB = "";
        string User = "";
        string Password = "";
        string Value = "";
        string Parameter = "";
        string Table = "";
        uint DBTimeOut = 0;
        bool IsLike = false;

        class BrowserOptions
        {
            public const string NOPAGE = "localhost";
            public const string OUTBROWSER = "chrome.exe";
        };

        SqlCommand sCommand;
        SqlDataAdapter sAdapter;
        SqlCommandBuilder sBuilder;
        DataSet sDs;
        DataTable sTable;

        CancellationTokenSource cts = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void bClose_Click(object sender, EventArgs e)
        {
            DisposeAllTables();
            Close();
        }

        void MakeConnectString()
        {            
            Server = tServer.Text;
            DB = tBD.Text;
            Table = tTable.Text;
            User = tUser.Text;
            Password = tPassword.Text;
            Value = tValue.Text;
            IsLike = cbIsLike.Checked;

            GetParameter();
        }

        private void GetParameter()
        {
            bool flag = false;
            foreach (var el in cbParameter.Items)
            {
                if (cbParameter.Text == el.ToString())
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
                Parameter = cbParameter.Text;
            else
            {
                Parameter = "";
                cbParameter.Text = "";
            }
        }

        void MakeDisConnectString()
        {
            Server = "";
            DB = "";
            Table = "";
            User = "";
            Password = "";
            Value = "";
            Parameter = "";
        }

        private void MakeDefaultConnect()
        {
            try
            {
                ReadXmlConfig xml = new ReadXmlConfig();
                string server = "";
                string bd = "";
                string table = "";
                string user = "";
                string password = "";
                xml.GetParameters(xmlConfig,
                                  ref server,
                                  ref bd,
                                  ref table,
                                  ref user,
                                  ref password);
                tServer.Text = server;
                tBD.Text = bd;
                tTable.Text = table;
                tUser.Text = user;
                tPassword.Text = password;
                DBTimeOut = (uint)TimeOut.Value;
            }
            catch(Exception)
            {
                tServer.Text = "";
                tBD.Text = "";
                tTable.Text = "";
                tUser.Text = "";
                tPassword.Text = "";
                DBTimeOut = 5;
            }
        }

        private void bDefault_Click(object sender, EventArgs e)
        {
            MakeDefaultConnect();
        }

        private void bDelete_Click(object sender, EventArgs e)
        {
            try
            {
                var t = dataGridView1.SelectedRows[0].Index;
            }
            catch (Exception)
            {
                MessageBox.Show("Выберите строку для удаления.",
                                "Информационое сообщение",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Удалить запись?",
                                "Информационое сообщение",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Information) ==
                                DialogResult.No)
            {
                return;
            }

            try
            {
                dataGridView1.Rows.RemoveAt(dataGridView1.SelectedRows[0].Index);
                sAdapter.Update(sTable);
            }
            catch (Exception err)
            {
                MessageBox.Show("Произошла ошибка во время удаления записи: \n\n" +
                                err.Message, "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetConnectionString()
        {
            string connectionString =
                "Data Source=" + Server + ";" +
                "Initial Catalog=" + DB + ";" +
                "User ID=" + User + ";" +
                "Password=" + Password + ";" +
                "Connect Timeout=" + DBTimeOut.ToString() + ";";
            return connectionString;
        }

        private async void bFind_Click(object sender, EventArgs e)
        {
            InitializeStartData();
            
            if (Parameter == "" || Value == "")
            {
                MessageBox.Show("Введите данные для поиска.",
                                 "Информационое сообщение",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Information);
                DeInitializeData();
                return;
            }
            
            string sql = "";
            if(IsLike)
                sql = "select " + "*" + " from " + Table +
                      " where " + Parameter + " like '%" + Value + "%'";
            else
                sql = "select " + "*" + " from " + Table +
                      " where " + Parameter + " = '" + Value + "'";

            int tmp = await Task<int>.Run(() =>
            {
                int result = 0;
                GetData(sql);
                return
                    result;
            });

            DeInitializeData();
        }

        delegate void SetListCallback(List<string> list);

        private void SetList(List<string> list)
        {
            if (this.cbParameter.InvokeRequired)
            {
                SetListCallback d = new SetListCallback(SetList);
                this.Invoke(d, new object[] { list });
            }
            else
            {
                this.cbParameter.Items.Clear();
                int N = list.Count;
                foreach (var el in list)
                {
                    cbParameter.Items.Add(el);
                }
            }
        }

        delegate void SetDataGridCallback(DataTableCollection data);

        private void SetDataGrid(DataTableCollection data)
        {
            if (this.cbParameter.InvokeRequired)
            {
                SetDataGridCallback d = new SetDataGridCallback(SetDataGrid);
                this.Invoke(d, new object[] { data });
            }
            else
            {
                if (data != null)
                    dataGridView1.DataSource = data[0];
                else
                    dataGridView1.DataSource = data;
            }
        }

        private void GetData(string sql)
        {
            try
            {
                try
                {
                    SetDataGrid(null);

                    SqlConnection connection = new SqlConnection(GetConnectionString());
                    connection.Open();
                    sCommand = new SqlCommand(sql, connection);
                    sAdapter = new SqlDataAdapter(sCommand);
                    sBuilder = new SqlCommandBuilder(sAdapter);
                    sDs = new DataSet();
                    sAdapter.Fill(sDs);
                    sTable = sDs.Tables[0];
                    connection.Close();

                    cts.Token.ThrowIfCancellationRequested();

                    SetDataGrid(sDs.Tables);

                    var list = new List<string>();
                    int N = sTable.Columns.Count;
                    for(int i = 0; i < N; ++i)
                    {
                        list.Add(sTable.Columns[i].ColumnName);
                    }
                    SetList(list);
                }
                catch (SqlException)
                {
                    throw;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("Произошла ошибка во время работы программы: \n\n" +
                                err.Message, "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeInitializeData()
        {
            ProgressBar.Visible = false;
            MakeDisConnectString();
            bCancel.Visible = false;
            if (cts != null)
            {
                cts.Dispose();
                cts = null;
            }

            //DisposeAllTables();
        }

        private void DisposeAllTables()
        {
            ClearTables(sCommand);
            ClearTables(sAdapter);
            ClearTables(sBuilder);
            ClearTables(sDs);
            ClearTables(sTable);
        }

        private void ClearTables(IDisposable o)
        {
            if (o != null)
            {
                o.Dispose();
                o = null;
            }
        }

        private void InitializeStartData()
        {
            SetUnlockTrue();
            MakeConnectString();
            ProgressBar.Visible = true;
            bCancel.Visible = true;
            cts = new CancellationTokenSource();
            
            /*dataGridView1.DataSource = null;
            DisposeAllTables();*/
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            InitializeStartData();
            string sql = "select " + "*" + " from " + Table;

            int tmp = await Task<int>.Run(() =>
            {
                int result = 0;
                GetData(sql);
                return
                    result;
            });

            DeInitializeData();
        }

        private void bSave_Click(object sender, EventArgs e)
        {
            if (isDataSource())
            {
                if (MessageBox.Show("Внести изменения в БД? ",
                                    "Информационое сообщение",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) ==
                                    DialogResult.No)
                {
                    SetUnlockTrue();
                    button1_Click(sender, e);
                    return;
                }

                try
                {
                    sAdapter.Update(sTable);
                }
                catch (Exception err)
                {
                    MessageBox.Show("Произошла ошибка во время удаления записи: \n\n" +
                                    err.Message, "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    SetUnlockTrue();
                }
            }
        }

        private void SetUnlockTrue()
        {
            bUnlock.Visible = true;
            bSave.Visible = false;
            dataGridView1.ReadOnly = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MakeDefaultConnect();
            button1_Click(sender, e);
        }

        private void bUnlock_Click(object sender, EventArgs e)
        {
            if (isDataSource())
            {
                bUnlock.Visible = false;
                bSave.Visible = true;
                dataGridView1.ReadOnly = false;
            }
        }

        private bool isDataSource()
        {
            if(dataGridView1.DataSource != null)
                return true;
            else
            return false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            cts.Cancel();
        }

        private async void bGetUrl_Click(object sender, EventArgs e)
        {
            ClearWebBrowser();

            if (!isDataSource())
                return;

            int index = -1;

            try
            {
                var t = dataGridView1.SelectedRows[0].Index;

                GetParameter();
                index = cbParameter.Items.IndexOf(Parameter);
                if (Parameter == "" || index < 0)
                    throw(new Exception(""));
            }
            catch (Exception)
            {
                MessageBox.Show("Выберите строку и параметр (столбец) для парсинга.",
                                "Информационое сообщение",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                return;
            }

            string url = "";

            try
            {
                url = await GetXmlString(index, url);
            }
            catch (Exception err)
            {
                MessageBox.Show("Произошла ошибка во время парсинга записи: \n\n" +
                                err.Message, "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            StartBrowser(url);
        }

        private void ClearWebBrowser()
        {
            //tbUrl.Clear();
            //Navigate(BrowserOptions.NOPAGE);
        }

        private async Task<string> GetXmlString(int index, string url)
        {
            int tmp = await Task<int>.Run(() =>
            {
                int result = 0;

                var row = dataGridView1.CurrentRow;
                string file = row.Cells[index].Value.ToString();

                var xmlReader = new ReadXMLData();
                xmlReader.GetParameters(file, ref url);

                return
                    result;
            });
            return url;
        }

        private void StartBrowser(string url)
        {
            tbUrl.Text = url;
            if (isOutBrowser.Checked)
            {
                var program = new ExecuteObject();
                program.RunObject(BrowserOptions.OUTBROWSER, url, false);
            }
            else
            {
                if (Navigate(url) != true)
                {
                    MessageBox.Show("Не корректный url",
                                    "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                    throw (new Exception("Не корректный url."));
                }

                while (WebBrowserView.ReadyState != WebBrowserReadyState.Complete)
                    Application.DoEvents();
            }
        }

        private bool Navigate(String address)
        {
            if (String.IsNullOrEmpty(address)) return false;
            if (address.Equals("about:blank")) return false;
            if (!address.StartsWith("http://") &&
                !address.StartsWith("https://"))
            {
                address = "http://" + address;
            }

            try
            {
                WebBrowserView.Navigate(new Uri(address));
                return true;
            }
            catch (System.UriFormatException err)
            {
                throw (new Exception("Ошибка в uri. \n" + err.Message));
            }
            catch (Exception err)
            {
                throw (new Exception("Ошибка при переходе к документу. \n" + err.Message));
                //return false;
            }
        }
    }
}
