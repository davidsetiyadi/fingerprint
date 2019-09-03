using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Npgsql;
using System.Data.SqlClient;
using DPUruNet;

namespace Enrollment
{
	/* NOTE: This form is a base for the EnrollmentForm and the VerificationForm,
		All changes in the CaptureForm will be reflected in all its derived forms.
	*/
	public partial class CaptureForm : Form, DPFP.Capture.EventHandler
	{
        private DataSet ds = new DataSet();
        private DataTable dt = new DataTable();
        //public DataRow row_david = new DataRow();
       
		public CaptureForm()
		{
			InitializeComponent();
            
            
		}

		protected virtual void Init()
		{
            try
            {
                Capturer = new DPFP.Capture.Capture();				// Create a capture operation.

                if ( null != Capturer )
                    Capturer.EventHandler = this;					// Subscribe for capturing events.
                else
                    SetPrompt("Can't initiate capture operation!");
            }
            catch
            {               
                MessageBox.Show("Can't initiate capture operation!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);            
            }
		}

		protected virtual void Process(DPFP.Sample Sample)
		{
			// Draw fingerprint sample image.
			DrawPicture(ConvertSampleToBitmap(Sample));
		}

		protected void Start()
		{
            if (null != Capturer)
            {
                try
                {
                    Capturer.StartCapture();
                    SetPrompt("Using the fingerprint reader, scan your fingerprint.");
                }
                catch
                {
                    SetPrompt("Can't initiate capture!");
                }
            }
		}

		protected void Stop()
		{
            if (null != Capturer)
            {
                try
                {
                    Capturer.StopCapture();
                }
                catch
                {
                    SetPrompt("Can't terminate capture!");
                }
            }
		}
		
	#region Form Event Handlers:

		private void CaptureForm_Load(object sender, EventArgs e)
		{
			Init();
			Start();												// Start capture operation.
            // read from database
            
            // string connstring = "Host=localhost;Username=postgres;Password=posgrest;Database=SGEEDE";
            // Making connection with Npgsql provider
            //NpgsqlConnection conn = new NpgsqlConnection(connstring);
            //conn.Open();
            //string sql = "select name,fingerprint from res_partner where customer = True;";
            // data adapter making request from our connection
            /*NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            // i always reset DataSet before i do something with it.... i don't know why :-)
            ds.Reset();
            // filling DataSet with result from NpgsqlDataAdapter
            da.Fill(ds);
            // since it C# DataSet can handle multiple tables, we will select first
            dt = ds.Tables[0];
            // connect grid to DataTable
            //dataGridView1.DataSource = dt;
            // since we only showing the result we don't need connection anymore
            
            conn.Close(); 
            //=============++++++++++++======================

            foreach (DataRow row in dt.Rows)
            {
                //MessageBox.Show(row["name"].ToString());
                listBox1.Items.Add(row["name"].ToString());
                
            }*/

            /*
             * 
             * DataResult<Fmd> resultConversion = null;
                IdentifyResult identifyResult = null;
                string MobileNumber = "";
                string Cnic = "";

                // Check capture quality and throw an error if bad.
                if (!this.CheckCaptureResult(captureResult)) return;
                // See the SDK documentation for an explanation on threshold scores.
                int thresholdScore = DPFJ_PROBABILITY_ONE * 1 / 100000;
                DataSet dataSetBiometric = DatabaseHandler.getData("select CNIC, MOBILE_NUMBER, BIOMETRIC from ACCOUNT_OPENING");
                //select CNIC, MOBILE_NUMBER, BIOMETRIC from ACCOUNT_OPENING
                Fmd[] fmds = new Fmd[dataSetBiometric.Tables[0].Rows.Count];
                for (int i = 0; i < dataSetBiometric.Tables[0].Rows.Count; i++)
                {
                    fmds[0] = Fmd.DeserializeXml(dataSetBiometric.Tables[0].Rows[i]["BIOMETRIC"].ToString());//BIOMETRIC
                    resultConversion = FeatureExtraction.CreateFmdFromFid(captureResult.Data, Constants.Formats.Fmd.ANSI);
                    identifyResult = Comparison.Identify(resultConversion.Data, 0, fmds, thresholdScore, dataSetBiometric.Tables[0].Rows.Count);
                    if (identifyResult.ResultCode == Constants.ResultCode.DP_SUCCESS)
                    {
                        MobileNumber = dataSetBiometric.Tables[0].Rows[i]["MOBILE_NUMBER"].ToString();
                        Cnic = dataSetBiometric.Tables[0].Rows[i]["CNIC"].ToString();
                        break;
                    }
                }
             * 
             * 
             */
		}

		private void CaptureForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			Stop();
		}
	#endregion

	#region EventHandler Members:

		public void OnComplete(object Capture, string ReaderSerialNumber, DPFP.Sample Sample)
		{
			MakeReport("The fingerprint sample was captured.");
			SetPrompt("Scan the same fingerprint again.");
            DataTable dt = new DataTable();
            //sd.Fill(dt);
            //MessageBox.Show("testsets");
			Process(Sample);
		}

		public void OnFingerGone(object Capture, string ReaderSerialNumber)
		{
			MakeReport("The finger was removed from the fingerprint reader.");
		}

		public void OnFingerTouch(object Capture, string ReaderSerialNumber)
		{
			MakeReport("The fingerprint reader was touched.");
		}

		public void OnReaderConnect(object Capture, string ReaderSerialNumber)
		{
			MakeReport("The fingerprint reader was connected.");
		}

		public void OnReaderDisconnect(object Capture, string ReaderSerialNumber)
		{
			MakeReport("The fingerprint reader was disconnected.");
		}

		public void OnSampleQuality(object Capture, string ReaderSerialNumber, DPFP.Capture.CaptureFeedback CaptureFeedback)
		{
			if (CaptureFeedback == DPFP.Capture.CaptureFeedback.Good)
				MakeReport("The quality of the fingerprint sample is good.");
			else
				MakeReport("The quality of the fingerprint sample is poor.");
		}
	#endregion

		protected Bitmap ConvertSampleToBitmap(DPFP.Sample Sample)
		{
			DPFP.Capture.SampleConversion Convertor = new DPFP.Capture.SampleConversion();	// Create a sample convertor.
			Bitmap bitmap = null;												            // TODO: the size doesn't matter
			Convertor.ConvertToPicture(Sample, ref bitmap);									// TODO: return bitmap as a result
			return bitmap;
		}

		protected DPFP.FeatureSet ExtractFeatures(DPFP.Sample Sample, DPFP.Processing.DataPurpose Purpose)
		{
			DPFP.Processing.FeatureExtraction Extractor = new DPFP.Processing.FeatureExtraction();	// Create a feature extractor
			DPFP.Capture.CaptureFeedback feedback = DPFP.Capture.CaptureFeedback.None;
			DPFP.FeatureSet features = new DPFP.FeatureSet();
			Extractor.CreateFeatureSet(Sample, Purpose, ref feedback, ref features);			// TODO: return features as a result?
			if (feedback == DPFP.Capture.CaptureFeedback.Good)
				return features;
			else
				return null;
		}

		protected void SetStatus(string status)
		{
			this.Invoke(new Function(delegate() {
				StatusLine.Text = status;
			}));
		}

		protected void SetPrompt(string prompt)
		{   
            if (prompt == "Click Close, and then click Fingerprint Verification."){
               // MessageBox.Show("suksesss");
                //button1.Enabled = true;
            }
			this.Invoke(new Function(delegate() {
				Prompt.Text = prompt;
			}));
		}
		protected void MakeReport(string message)
		{
			this.Invoke(new Function(delegate() {
				StatusText.AppendText(message + "\r\n");
			}));
		}

        protected void MakeName(string message)
        {
            this.Invoke(new Function(delegate()
            {
                textBox1.Text = message;
                StatusText.AppendText(message + "\r\n");
            }));
        }
		private void DrawPicture(Bitmap bitmap)
		{
			this.Invoke(new Function(delegate() {
				Picture.Image = new Bitmap(bitmap, Picture.Size);	// fit the image into the picture box
			}));
		}

		private DPFP.Capture.Capture Capturer;

        private void Prompt_TextChanged(object sender, EventArgs e)
        {

        }
       
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private DPFP.Template Template;

        private void button1_Click(object sender, EventArgs e)
        {
            // read from database
            string connstring = "Host=localhost;Username=postgres;Password=posgrest;Database=SGEEDE";
            // Making connection with Npgsql provider
            NpgsqlConnection conn = new NpgsqlConnection(connstring);
            conn.Open();
           /* MemoryStream fingerprintData = new MemoryStream();
            Template.Serialize(fingerprintData);
            fingerprintData.Position = 0;
            BinaryReader br = new BinaryReader(fingerprintData);
            Byte[] bytes = br.ReadBytes((Int32)fingerprintData.Length);

            string sql = "update res_partner set fingerprint = '" + bytes + "' where name = '" + listBox1.Text + "';";
            //cmd.CommandText = "INSERT INTO Emp_T(EmpID, EmpName, EmpTemplate) VALUES('" + textBox1.Text + "','" + textBox2.Text + "','" + template + "')";
            // data adapter making request from our connection
            //NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "update res_partner set fingerprint = '" + bytes + "' where name = '" + listBox1.Text + "';";
                //cmd.Parameters.AddWithValue("p", bytes);
                //cmd.Parameters.AddWithValue("p", "Hello world");
                cmd.ExecuteNonQuery();
            } */

            // i always reset DataSet before i do something with it.... i don't know why :-)
            //ds.Reset();
            
            // filling DataSet with result from NpgsqlDataAdapter
            //da.Fill(ds);
            // since it C# DataSet can handle multiple tables, we will select first
            //dt = ds.Tables[0];
            // connect grid to DataTable
            //dataGridView1.DataSource = dt;
            // since we only showing the result we don't need connection anymore
            conn.Close(); 

            
           /* foreach (DataRow row in dt.Rows)
            {
                //MessageBox.Show(row["name"].ToString());
                listBox1.Items.Add(row["name"].ToString());
             
            }*/
            /*SqlConnection cn = new SqlConnection("Data Source=(local); Initial Catalog=master; Integrated Security=SSPI;");
            SqlCommand cmd = new SqlCommand("SELECT FIRST NAME FROM tblUser", cn);
            //rdr = cmd.ExecuteReader();
            ds.Reset();
            rdr.Fill(ds); */
            //MessageBox.Show(listBox1.Text);
           
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            ConfirmationForm Confirmation = new ConfirmationForm();

            Confirmation.ShowDialog();
        }

        

       

	}
}