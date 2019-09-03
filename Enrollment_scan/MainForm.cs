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

namespace Enrollment
{
	delegate void Function();	// a simple delegate for marshalling calls from event handlers to the GUI thread

    
	public partial class MainForm : Form
	{
        private DataSet ds = new DataSet();
        private DataTable dt = new DataTable();
        //public string referral = new string();
		public MainForm()
		{
			InitializeComponent();
		}

		private void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void EnrollButton_Click(object sender, EventArgs e)
		{
			EnrollmentForm Enroller = new EnrollmentForm();
			Enroller.OnTemplate += this.OnTemplate;
            
			Enroller.ShowDialog();


		}

		private void VerifyButton_Click(object sender, EventArgs e)
		{
			VerificationForm Verifier = new VerificationForm();
            
			Verifier.Verify( Template,Template2 );
		}

		private void SaveButton_Click(object sender, EventArgs e)
		{
            //string connstring = "Host=localhost;Username=postgres;Password=posgrest;Database=SGEEDE";
            string connstring = "Host=192.168.56.102;Username=postgres;Password=posgrest;Database=QFC_ADMIN";
             // Making connection with Npgsql provider
            
             NpgsqlConnection conn = new NpgsqlConnection(connstring);
            // conn.Open();
             MemoryStream fingerprintData = new MemoryStream();
             Template.Serialize(fingerprintData);
             fingerprintData.Position = 0;
             BinaryReader br = new BinaryReader(fingerprintData);
             Byte[] bytes = br.ReadBytes((Int32)fingerprintData.Length);
             //br.Close();

            //conn.Open();

           conn.Open();
              using (var cmd = new NpgsqlCommand())
                {
                 cmd.Connection = conn;
                 cmd.CommandText = "update res_partner set fingerprint = (@p) where name = (@q);";
                 cmd.Parameters.AddWithValue("p", bytes);
                 cmd.Parameters.AddWithValue("q", listBox1.Text);
                  
                 //cmd.Parameters.AddWithValue("p", "Hello world");
                 cmd.ExecuteNonQuery();
                } 
           
             conn.Close(); 
             //sukses saved to nama partner.
             MessageBox.Show("Sukses saved fingerprint {0}", listBox1.Text);

			
		}

		private void LoadButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog open = new OpenFileDialog();
			open.Filter = "Fingerprint Template File (*.fpt)|*.fpt";
			if (open.ShowDialog() == DialogResult.OK) {
				using (FileStream fs = File.OpenRead(open.FileName)) {
					DPFP.Template template = new DPFP.Template(fs);
					OnTemplate(template);
				}
			}
		}

		private void OnTemplate(DPFP.Template template)
		{
			this.Invoke(new Function(delegate()
			{
				Template = template;
				VerifyButton.Enabled = SaveButton.Enabled = (Template != null);
				if (Template != null)
					MessageBox.Show("The fingerprint template is ready for fingerprint verification.", "Fingerprint Enrollment");
				else
					MessageBox.Show("The fingerprint template is not valid. Repeat fingerprint enrollment.", "Fingerprint Enrollment");
			}));
		}

		private DPFP.Template Template;

        private void MainForm_Load(object sender, EventArgs e)
        {
            //tempFingerPrint = Fmd.SerializeXml(resultConversion.Data);
            /*
            string connstring = "Server=192.168.100.20;Username=postgres;Password=postgres;Database=Serenity";
            // Making connection with Npgsql provider
            NpgsqlConnection conn = new NpgsqlConnection(connstring);
            conn.Open();
            string sql = "select name from res_partner where customer = True;";
            // data adapter making request from our connection
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            // i always reset DataSet before i do something with it.... i don't know why :-)
            ds.Reset();
            // filling DataSet with result from NpgsqlDataAdapter
            da.Fill(ds);
            // since it C# DataSet can handle multiple tables, we will select first
            dt = ds.Tables[0];
            // connect grid to DataTable
            // since we only showing the result we don't need connection anymore
            conn.Close();

            foreach (DataRow row in dt.Rows)
            {
                //MessageBox.Show(row["name"].ToString());
                listBox1.Items.Add(row["name"].ToString());

            }*/
        }

        private void LoadButton2_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Fingerprint Template File (*.fpt)|*.fpt";
            if (open.ShowDialog() == DialogResult.OK)
            {
                using (FileStream fs = File.OpenRead(open.FileName))
                {
                    DPFP.Template template = new DPFP.Template(fs);
                    OnTemplate2(template);
                }
            }
        }

        private void OnTemplate2(DPFP.Template template)
        {
            this.Invoke(new Function(delegate()
            {
                Template2 = template;
                VerifyButton.Enabled = SaveButton.Enabled = (Template2 != null);
                if (Template2 != null)
                    MessageBox.Show("The fingerprint template is ready for fingerprint verification.", "Fingerprint Enrollment");
                else
                    MessageBox.Show("The fingerprint template is not valid. Repeat fingerprint enrollment.", "Fingerprint Enrollment");
            }));
        }

        private DPFP.Template Template2;

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            // read from database
            string connstring = "Host=localhost;Username=postgres;Password=posgrest;Database=SGEEDE";
            // Making connection with Npgsql provider
            NpgsqlConnection conn = new NpgsqlConnection(connstring);
            conn.Open();
            string sql = "select name,fingerprint from res_partner where customer = true and fingerprint is not null limit 1;";
            // data adapter making request from our connection
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
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

            MemoryStream ms;
            
             
            Byte[] fpBtes2;
           
            foreach (DataRow row in dt.Rows)
            {
                

                fpBtes2 = Encoding.UTF8.GetBytes(row["fingerprint"].ToString());
                
                ms = new System.IO.MemoryStream(fpBtes2);
                //MessageBox.Show(ms.ToString());
                DPFP.Template template = new DPFP.Template(ms);
               
                OnTemplate(template);

            }
            
        }


	}
}