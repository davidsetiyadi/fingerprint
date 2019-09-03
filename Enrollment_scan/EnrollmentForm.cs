using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;

namespace Enrollment
{
	/* NOTE: This form is inherited from the CaptureForm,
		so the VisualStudio Form Designer may not load it properly
		(at least until you build the project).
		If you want to make changes in the form layout - do it in the base CaptureForm.
		All changes in the CaptureForm will be reflected in all derived forms 
		(i.e. in the EnrollmentForm and in the VerificationForm)
	*/
	public class EnrollmentForm : CaptureForm
	{   
		public delegate void OnTemplateEventHandler(DPFP.Template template);

		public event OnTemplateEventHandler OnTemplate;

		protected override void Init()
		{
			base.Init();
			base.Text = "Fingerprint Enrollment";
			Enroller = new DPFP.Processing.Enrollment();			// Create an enrollment.
			UpdateStatus();
		}

		protected override void Process(DPFP.Sample Sample)
		{   
             

			base.Process(Sample);

			// Process the sample and create a feature set for the enrollment purpose.
			DPFP.FeatureSet features = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Enrollment);

			// Check quality of the sample and add to enroller if it's good
			if (features != null) try
			{
				MakeReport("The fingerprint feature set was created.");
				Enroller.AddFeatures(features);		// Add feature set to template.
			}
			finally {
				UpdateStatus();

				// Check if template has been created.
				switch(Enroller.TemplateStatus)
				{
					case DPFP.Processing.Enrollment.Status.Ready:	// report success and stop capturing
						OnTemplate(Enroller.Template);
						SetPrompt("Click Close, and then click Fingerprint Verification.");
                        /*string connstring = "Host=localhost;Username=postgres;Password=posgrest;Database=SGEEDE";
                        // Making connection with Npgsql provider
                        NpgsqlConnection conn = new NpgsqlConnection(connstring);
                        conn.Open();
                        string sql = "update res_partner set comment = '123123123123123' where name = 'Agrolait';";
                        //cmd.CommandText = "INSERT INTO Emp_T(EmpID, EmpName, EmpTemplate) VALUES('" + textBox1.Text + "','" + textBox2.Text + "','" + template + "')";
                        // data adapter making request from our connection
                        NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
                        // i always reset DataSet before i do something with it.... i don't know why :-)
                        ds.Reset();

                        // filling DataSet with result from NpgsqlDataAdapter
                        da.Fill(ds);
                        // since it C# DataSet can handle multiple tables, we will select first
                        //dt = ds.Tables[0];
                        // connect grid to DataTable
                        //dataGridView1.DataSource = dt;
                        // since we only showing the result we don't need connection anymore
                        conn.Close(); */

						Stop();
						break;

					case DPFP.Processing.Enrollment.Status.Failed:	// report failure and restart capturing
						Enroller.Clear();
						Stop();
						UpdateStatus();
						OnTemplate(null);
						Start();
						break;
				}
			}
		}

		private void UpdateStatus()
		{
			// Show number of samples needed.
			SetStatus(String.Format("Fingerprint samples needed: {0}", Enroller.FeaturesNeeded));
		}

		private DPFP.Processing.Enrollment Enroller;

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // EnrollmentForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(581, 354);
            this.Name = "EnrollmentForm";
            this.Load += new System.EventHandler(this.EnrollmentForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void EnrollmentForm_Load(object sender, EventArgs e)
        {

        }
	}
}
