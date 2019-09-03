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

       
		internal void SaveButton_Click(object sender, EventArgs e)
		{
            
            //string connstring = "Host=206.189.159.57;Username=sgeede_fingerprint;Password=we@re$g33d3;Database=SERENITY_PRODUCTION";
            string connstring = "Host=192.168.100.20;Username=postgres;Password=we@re$g33d3;Database=Serenity";
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
             System.Windows.Forms.Form f = System.Windows.Forms.Application.OpenForms["Confirmation_form"];

             string textbox11 = ((Confirmation_form)f).textBox1.Text;

              using (var cmd = new NpgsqlCommand())
                {
                 cmd.Connection = conn;
                 cmd.CommandText = "update hr_employee set fingerprint = (@p),state='verify',register_date = (@r)  where name_related = (@q);";
                 cmd.Parameters.AddWithValue("p", bytes);
                 cmd.Parameters.AddWithValue("q", textbox11);

                 cmd.Parameters.AddWithValue("r", DateTime.Now.AddHours(-7).ToString("yyyy-MM-dd HH:mm:ss"));
                  
                 //cmd.Parameters.AddWithValue("p", "Hello world");
                 try
                 {
                     
                     //if (listBox1.Text.ToString() != "")
                     if (textbox11 != "")
                     {
                         cmd.ExecuteNonQuery();
                         //MessageBox.Show(listBox1.Text.ToString());
                     }
                 }
                 catch
                 {
                 }
                } 
             
             // since we only showing the result we don't need connection anymore
             conn.Close(); 
             //sukses saved to nama partner.
             //string message = "Sukses saved fingerprint {0}", listBox1.Text;
             //MessageBox.Show(message);  
             string message = "Sukses saved fingerprint to ";
             string selected = textbox11;
             message += selected;
             //string s = String.Join("Sukses saved fingerprint", "1212");
             //MessageBox.Show("Sukses saved fingerprint {0}", listBox1.Text);
             //MessageBox.Show(message);
             if (textbox11 != "")
             {
                 //MessageBox.Show(message);
                 //SaveButton.Enabled = false;
                 //string message = "Do you want to close this window?";
                 string title = "Close Window";
                 //MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                 DialogResult result = MessageBox.Show(message, title);
                 //MessageBox.Show(result);
                 
                 if (result == DialogResult.OK)
                 {
                     this.Close();                     
                 }
                 else
                 {
                     // Do something  
                     this.Close();
                 }  

             }
             if (textbox11 == "")
             {
                 //MessageBox.Show("Please select name from the list");
             }
			/*SaveFileDialog save = new SaveFileDialog();
			save.Filter = "Fingerprint Template File (*.fpt)|*.fpt";
			if (save.ShowDialog() == DialogResult.OK) {
				using (FileStream fs = File.Open(save.FileName, FileMode.Create, FileAccess.Write)) {
					Template.Serialize(fs);
				}
			}*/
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
                {
                    //MessageBox.Show("The fingerprint template is ready for fingerprint verification.", "Fingerprint Enrollment");

                    Confirmation_form Confirmation = new Confirmation_form();                        
                    Confirmation.TopMost = true;
                    Confirmation.ShowDialog();
                }
                else
                {
                    MessageBox.Show("The fingerprint template is not valid. Repeat fingerprint enrollment.", "Fingerprint Enrollment");
                }
			}));
		}

        private void Click_save(String test)
        {
            //testttttt
            MessageBox.Show("testst");
        }
		private DPFP.Template Template;

        private void MainForm_Load(object sender, EventArgs e)
        {
            /* string connstring = "Server=192.168.100.78;Username=postgres;Password=postgres;Database=SERENITY_PRODUCTION";
            //string connstring = "Server=localhost;Username=postgres;Password=postgres;Database=SGEEDE";
            // Making connection with Npgsql provider
            NpgsqlConnection conn = new NpgsqlConnection(connstring);
            conn.Open();
            string sql = "select name_related from hr_employee order by id desc;";
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

            foreach (DataRow row in dt.Rows)
            {
                listBox1.Items.Add(row["name_related"].ToString());

            }*/
            EnrollButton.PerformClick();
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
            /*
            // read from database
            string connstring = "Host=localhost;Username=postgres;Password=posgrest;Database=SGEEDE";
            // Making connection with Npgsql provider
            NpgsqlConnection conn = new NpgsqlConnection(connstring);
            conn.Open();
            string sql = "select name,fingerprint from res_partner where customer = true and fingerprint is not null limit 1;";
            // data adapter making request from our connection
             * */
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

            MemoryStream ms;            
             
            Byte[] fpBtes2;
           
            foreach (DataRow row in dt.Rows)
            {
                
                fpBtes2 = Encoding.UTF8.GetBytes(row["fingerprint"].ToString());
                ms = new System.IO.MemoryStream(fpBtes2);
                DPFP.Template template = new DPFP.Template(ms);
                OnTemplate(template);
            
            }*/
            
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            //
            
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            /* if (e.KeyChar == (char)13)
            {
                // Enter key pressed
               
                for (int i = listBox1.Items.Count - 1; i >= 0; i--)
                {
                    listBox1.Items.RemoveAt(i);
                   
                }
                string connstring = "Server=192.168.100.78;Username=postgres;Password=postgres;Database=SERENITY_PRODUCTION";
                // Making connection with Npgsql provider
                NpgsqlConnection conn = new NpgsqlConnection(connstring);
                conn.Open();
                //MessageBox.Show(textBox1.Text.ToString());
                string sql = "select name_related from hr_employee where name_related ilike '%" + textBox1.Text.ToString() + "%' order by id desc;";
                //string sql = "select name_related from hr_employee where name_related like 'yay%';";
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

                foreach (DataRow row in dt.Rows)
                {
                    //MessageBox.Show(row["name"].ToString());
                    listBox1.Items.Add(row["name_related"].ToString());

                }

            }*/
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnrollmentForm Enroller = new EnrollmentForm();
            Enroller.OnTemplate += this.OnTemplate;
            Enroller.ShowDialog();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

       

	}
}