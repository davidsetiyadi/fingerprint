using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Enrollment
{
    public partial class ConfirmationForm : Form
    {   
        public ConfirmationForm()
        {
            InitializeComponent();
           
        }

        private void ConfirmationForm_Load(object sender, EventArgs e)
        {
            //VerificationForm forms = new VerificationForm();
            System.Windows.Forms.Form f = System.Windows.Forms.Application.OpenForms["CaptureForm"];
           
            string message = "Thankyou ";
            string message2 = " Please Click Close Button";
            label1.Text = message + ((CaptureForm)f).textBox1.Text;
            label2.Text = message2;
           
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Close();
            //Environment.Exit(1);
            Application.Exit();
            //this.Close();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
