using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;
using Npgsql;

using System.IO;
using System.Data.SqlClient;

namespace Enrollment
{
    public partial class Confirmation_form : Form
    {
        private DataSet ds = new DataSet();
        private DataTable dt = new DataTable();

        public delegate void OnClickButtonEventHandler(string testset);

        public event OnClickButtonEventHandler Click_save;

        public Confirmation_form()
        {
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            MainForm master = (MainForm)Application.OpenForms["MainForm"];
            master.SaveButton.PerformClick();
            //Application.Exit();
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //this.Close();
            Application.Exit();
        }

        private void Confirmation_form_Load(object sender, EventArgs e)
        {
            //System.Windows.Forms.Form f = System.Windows.Forms.Application.OpenForms["CaptureForm"];
            //string connstring = "Server=206.189.159.57;Username=sgeede_fingerprint;Password=;Database=SERENITY_PRODUCTION";
            //string connstring = "Server=192.168.100.20;Username=postgres;Password=postgres;Database=Serenity";
            string connstring = "Server=localhost;Username=postgres;Password=postgres;Database=SERENITY_PRODUCTION";
            // Making connection with Npgsql provider
            NpgsqlConnection conn = new NpgsqlConnection(connstring);
            conn.Open();
            string sql = "select name_related from hr_employee order by write_date desc limit 1;";
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
            string message = "Fingerprint save to ";
            foreach (DataRow row in dt.Rows)
            {
                               
                textBox1.Text = row["name_related"].ToString();
                //textBox1.Text = 

            }

            //string message = "Thankyou ";
            //string message2 = " Please Click Close Button";
            label1.Text = message;//+ ((CaptureForm)f).textBox1.Text;
            //label2.Text = message2;
            // Fingerprint saved to xxx

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Confirmation_form_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
