using System;
using System.Windows.Forms;
using System.Data;
using System.Drawing;
using DPUruNet;
using Npgsql;

using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Newtonsoft.Json;


namespace UareUSampleCSharp
{
   
    public partial class Identification : Form
    {
        /// <summary>
        /// Holds the main form with many functions common to all of SDK actions.
        /// </summary>
        public Form_Main _sender;

        private const int DPFJ_PROBABILITY_ONE = 0x7fffffff;
        private Fmd rightIndex;
        private Fmd rightThumb;
        private Fmd anyFinger;
        private int count;
        private DataSet ds = new DataSet();
        private DataTable dt = new DataTable();
        private string referral;

        public Identification()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize the form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        //private static readonly HttpClient client = new HttpClient();
        static HttpClient client = new HttpClient();

        private void Identification_Load(object sender, System.EventArgs e)
        {
            Form_Main f = new Form_Main();
            f.Close();
            txtIdentify.Text = string.Empty;
            rightIndex = null;
            rightThumb = null;
            anyFinger = null;
            count = 0;

            SendMessage(Action.SendMessage, "Place your right index finger on the reader.");

            if (!_sender.OpenReader())
            {
                this.Close();
            }

            if (!_sender.StartCaptureAsync(this.OnCaptured))
            {
                this.Close();
            }
        }

        /// <summary>
        /// Handler for when a fingerprint is captured.
        /// </summary>
        /// <param name="captureResult">contains info and data on the fingerprint capture</param>
        private void OnCaptured(CaptureResult captureResult)
        {
            try
            {
                // Check capture quality and throw an error if bad.
                if (!_sender.CheckCaptureResult(captureResult)) return;

                SendMessage(Action.SendMessage, "A finger was captured.");

                DataResult<Fmd> resultConversion = FeatureExtraction.CreateFmdFromFid(captureResult.Data, Constants.Formats.Fmd.ANSI);
                if (captureResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                {
                    _sender.Reset = true;
                    throw new Exception(captureResult.ResultCode.ToString());
                }

                if (count == 0)
                {
                    rightIndex = resultConversion.Data;
                    anyFinger = resultConversion.Data;
                    count += 1;
                    count = 0;
                    SendMessage(Action.SendMessage, "Press Sign Out Button to Verify.");

                }
                else if (count == 1)
                {
                    rightThumb = resultConversion.Data;
                    count += 1;
                    SendMessage(Action.SendMessage, "Now place any finger on the reader.");
                }
                else if (count == 10)
                {
                    anyFinger = resultConversion.Data;
                    Fmd[] fmds = new Fmd[2];
                    fmds[0] = rightIndex;
                    fmds[1] = rightThumb;

                    // See the SDK documentation for an explanation on threshold scores.
                    int thresholdScore = DPFJ_PROBABILITY_ONE * 1 / 100000;

                    IdentifyResult identifyResult = Comparison.Identify(anyFinger, 0, fmds, thresholdScore, 2);
                    if (identifyResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                    {
                        _sender.Reset = true;
                        throw new Exception(identifyResult.ResultCode.ToString());
                    }

                    SendMessage(Action.SendMessage, "Identification resulted in the following number of matches: " + identifyResult.Indexes.Length.ToString());
                    SendMessage(Action.SendMessage, "Place your right index finger on the reader.");
                    //SendMessage(Action.SendMessage, Fmd.SerializeXml(anyFinger) ); //data ini di simpan di server
                    // read from database

                    string connstring = "Host=192.168.56.102;Username=david;Password=david;Database=CHOCO";
                    // Making connection with Npgsql provider
                    NpgsqlConnection conn = new NpgsqlConnection(connstring);
                    conn.Open();              
                    
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "update hr_employee set finger2 = '" + Fmd.SerializeXml(anyFinger) + "' where id = 3;";
                        //string sql = "update hr_employee set finger2 = '" + Fmd.SerializeXml(anyFinger) + "' where id = 1;";
                        //cmd.CommandText = "update hr_employee set is_helper = true where id = 1";

                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("sukesess");
                    conn.Close();
                    //get from server 

                    
                    // data adapter making request from our connection
                    count = 0;
                }
            }
            catch (Exception ex)
            {
                // Send error message, then close form
                SendMessage(Action.SendMessage, "Error:  " + ex.Message);                
            }
        }

        /// <summary>
        /// Close window.
        /// </summary>
        private void btnBack_Click(System.Object sender, System.EventArgs e)
        {
            //this.Close();
            System.Windows.Forms.Application.Exit();
        }

        /// <summary>
        /// Close window.
        /// </summary>
        private void Identification_Closed(object sender, System.EventArgs e)
        {
            _sender.CancelCaptureAndCloseReader(this.OnCaptured);
        }

        #region SendMessage
        private enum Action
        {
            SendMessage
        }
        private delegate void SendMessageCallback(Action action, string payload);
        private void SendMessage(Action action, string payload)
        {
            try
            {
                if (this.txtIdentify.InvokeRequired)
                {
                    SendMessageCallback d = new SendMessageCallback(SendMessage);
                    this.Invoke(d, new object[] { action, payload });
                }
                else
                {
                    switch (action)
                    {
                        case Action.SendMessage:
                            txtIdentify.Text += payload + "\r\n\r\n";
                            txtIdentify.SelectionStart = txtIdentify.TextLength;
                            txtIdentify.ScrollToCaret();
                            break;
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        #endregion

        private void BtnTest_Click(object sender, EventArgs e)
        {
            string connstring = "Host=192.168.56.102;Username=david;Password=david;Database=CHOCO";
            // Making connection with Npgsql provider
            NpgsqlConnection conn = new NpgsqlConnection(connstring);
            conn.Open();
            string sql = "select id,fingerprint,name from hr_employee where fingerprint is not null;";
            // data adapter making request from our connection
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);

            ds.Reset();
            // filling DataSet with result from NpgsqlDataAdapter
            da.Fill(ds);
            // since it C# DataSet can handle multiple tables, we will select first
            dt = ds.Tables[0];
            
            foreach (DataRow dr in dt.Rows)
            {
                //test any finger closes with ?
                //MessageBox.Show("test12"+dr["id"].ToString());
                //anyFinger = resultConversion.Data;

                Fmd[] fmds = new Fmd[2];


                fmds[0] = Fmd.DeserializeXml( dr["fingerprint"].ToString() );
                
                //Fmd.SerializeXml(anyFinger)
                //fmds[1] = rightThumb;

                // See the SDK documentation for an explanation on threshold scores.
                int thresholdScore = DPFJ_PROBABILITY_ONE * 1 / 100000;

                IdentifyResult identifyResult = Comparison.Identify(anyFinger, 0, fmds, thresholdScore, 2);
                if (identifyResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                {
                    _sender.Reset = true;
                    throw new Exception(identifyResult.ResultCode.ToString());
                }

                SendMessage(Action.SendMessage, "Identification resulted in the following number of matches: " + identifyResult.Indexes.Length.ToString());
                SendMessage(Action.SendMessage, "Place your right index finger on the reader.");
                SendMessage(Action.SendMessage, dr["name"].ToString());
                if (identifyResult.Indexes.Length.ToString() == "1")
                {
                    MessageBox.Show(dr["name"].ToString());

                    string connstring2 = "Host=192.168.56.102;Username=david;Password=david;Database=CHOCO";
                    // Making connection with Npgsql provider
                    NpgsqlConnection conn2 = new NpgsqlConnection(connstring2);
                    conn2.Open();
                    //MessageBox.Show("Saved2");
                    /*vals = {
                        'employee_id': id,
				        'check_in': action_date,
			        }*/
                    //ToString("MM/dd/yyyy HH:mm")
                    //ToString("yyyy/MM/dd HH:mm:ss")
                    //MessageBox.Show(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                    double hours = -7;
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn2;
                        //cmd.CommandText = "insert into hr_attendance (employee_id, check_in) values ('" + dr["id"].ToString() + "','" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "' ) ;";
                        cmd.CommandText = "update hr_attendance set check_out = '" + DateTime.Now.AddHours(hours).ToString("yyyy/MM/dd HH:mm:ss") + "' where employee_id = " + dr["id"].ToString() + " ;";
                        //cmd.CommandText = "update hr_employee set is_helper = true where id = 1";
                        cmd.ExecuteNonQuery();
                    }
                    conn2.Close();
                    
                    //MyMain();
                    conn.Close();
                    break;                  

                }              

            }

            conn.Close();
        }
        //using var client = new HttpClient();
        static async Task MyMain()
        {
            /*string myJson = "{'jsonrpc': '2.0','params': {'login': 'demo','password': 'demo','db': '13_GBR_Demo_2'}}";
            //string myJson = "{'Username': 'myusername','Password':'pass'}";
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(
                    "http://192.168.56.102:8069/web/session/authenticate/",
                     new StringContent(myJson, Encoding.UTF8, "application/json"));
            }*/
            //client.DefaultRequestHeaders
            // Add request body
            
            /*var person = new Person();
            person.Name = "John Doe";
            person.Occupation = "gardener";
            //MessageBox.Show("stestsetset");
            var json = JsonConvert.SerializeObject(person);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var url = "http://192.168.56.102:8069/";
            

            var response = await client.PostAsync(url, data);*/

            /*string result = response.Content.ReadAsStringAsync().Result;
            MessageBox.Show(result);
            Console.WriteLine(result);*/
        }

    }
    public class Student
    {
        public int Id { get; set; }
        public string login { get; set; }
        public string password { get; set; }
        public string db { get; set; }
    }
    class Person
    {
        public string Name { get; set; }
        public string Occupation { get; set; }

        public override string ToString()
        {
            return $"{Name}: {Occupation}";
        }
    }
}