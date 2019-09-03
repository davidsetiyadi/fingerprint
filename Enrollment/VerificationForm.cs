using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Npgsql;
using DPUruNet;


using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;



namespace Enrollment
{
	/* NOTE: This form is inherited from the CaptureForm,
		so the VisualStudio Form Designer may not load it properly
		(at least until you build the project).
		If you want to make changes in the form layout - do it in the base CaptureForm.
		All changes in the CaptureForm will be reflected in all derived forms 
		(i.e. in the EnrollmentForm and in the VerificationForm)
	*/
	public class VerificationForm : CaptureForm
	{

        private DataSet ds = new DataSet();
        private DataTable dt = new DataTable();
        public void Verify(DPFP.Template template, DPFP.Template template2)
		{
			Template = template;
            Template2 = template2;
			ShowDialog();
		}

		protected override void Init()
		{
			base.Init();
			base.Text = "Fingerprint Verification";
			Verificator = new DPFP.Verification.Verification();		// Create a fingerprint template verificator
			UpdateStatus(0);
		}

		protected override void Process(DPFP.Sample Sample)
		{
			base.Process(Sample);

			// Process the sample and create a feature set for the enrollment purpose.
			DPFP.FeatureSet features = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Verification);

			// Check quality of the sample and start verification if it's good
			// TODO: move to a separate task
            
			if (features != null)
			{
				// Compare the feature set with our template
				DPFP.Verification.Verification.Result result = new DPFP.Verification.Verification.Result();
				
                //template dibuat switch case
                // ambil dari database , nama dan template
                // upload data finger dan data id , id[n] = Fingerprint
                //Indexes , 
                string connstring = "Host=192.168.56.102;Username=david;Password=david;Database=QFC_ADMIN";
                // Making connection with Npgsql provider
                NpgsqlConnection conn = new NpgsqlConnection(connstring);
                conn.Open();
                string sql = "select name,fingerprint from res_partner where customer = True and fingerprint is not null;";
                // data adapter making request from our connection
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);

                //datataba
               /* DataResult<Fmd> resultConversion = null;
                IdentifyResult identifyResult = null;
                string MobileNumber = "";
                string Cnic = ""; */

                // Check capture quality and throw an error if bad.
               // if (!this.CheckCaptureResult(captureResult)) return;
                // See the SDK documentation for an explanation on threshold scores.
                //int thresholdScore = DPFJ_PROBABILITY_ONE * 1 / 100000;

                // https://stackoverflow.com/questions/55838416/matching-fingerprint-with-a-database-sourceafis
                //https://stackoverflow.com/questions/41742993/digital-persona-finger-print-verification-from-database-not-working

                //DataSet dataSetBiometric = DatabaseHandler.getData("select CNIC, MOBILE_NUMBER, BIOMETRIC from ACCOUNT_OPENING");
                //select CNIC, MOBILE_NUMBER, BIOMETRIC from ACCOUNT_OPENING
                ds.Reset();
                // filling DataSet with result from NpgsqlDataAdapter
                da.Fill(ds);
                // since it C# DataSet can handle multiple tables, we will select first
                dt = ds.Tables[0];
                
                foreach ( DataRow dr in dt.Rows)
                {
                    byte[] _img = (byte[])dr["fingerprint"];
                    MemoryStream ms = new MemoryStream(_img);
                    DPFP.Template Template = new DPFP.Template();
                    Template.DeSerialize(ms);
                    DPFP.Verification.Verification Verificator = new DPFP.Verification.Verification();

                    base.Process(Sample);

                    DPFP.FeatureSet features2 = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Verification);
                    if (features2 != null)
                    {
                        DPFP.Verification.Verification.Result result2 = new DPFP.Verification.Verification.Result();
                        Verificator.Verify(features2, Template, ref result2);
                        UpdateStatus(result.FARAchieved);
                        if (result2.Verified)
                        {
                            MakeReport("The fingerprint was VERIFIED.");
                            MakeReport(dr["name"].ToString());
                            MessageBox.Show(dr["name"].ToString());
                        }
                        else
                        {
                            MakeReport("The fingerprint was NOT VERIFIED.");
                        }
                    }

                }
                /*Fmd[] fmds = new Fmd[ds.Tables[0].Rows.Count];
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    fmds[0] = Fmd.DeserializeXml(ds.Tables[0].Rows[i]["BIOMETRIC"].ToString());//BIOMETRIC
                    resultConversion = FeatureExtraction.CreateFmdFromFid(captureResult.Data, Constants.Formats.Fmd.ANSI);
                    identifyResult = Comparison.Identify(resultConversion.Data, 0, fmds, thresholdScore, dataSetBiometric.Tables[0].Rows.Count);
                    if (identifyResult.ResultCode == Constants.ResultCode.DP_SUCCESS)
                    {
                        MobileNumber = dataSetBiometric.Tables[0].Rows[i]["MOBILE_NUMBER"].ToString();
                        Cnic = dataSetBiometric.Tables[0].Rows[i]["CNIC"].ToString();
                        break;
                    }
                }*/

                //Verificator.Verify(features, Template, ref result);

                //DPFP.Verification.Verification.Result result2 = new DPFP.Verification.Verification.Result();
               // Verificator.Verify(features, Template2, ref result2);

				//UpdateStatus(result.FARAchieved);

               // if (result.Verified)
               //     MakeReport("The fingerprint was VERIFIED RIGHT.");

				//else if (result.Verified)
				//	MakeReport("The fingerprint was VERIFIED LEFT.");
				//else
				//	MakeReport("The fingerprint was NOT VERIFIED.");
			}
		}

		private void UpdateStatus(int FAR)
		{
          
			// Show "False accept rate" value
			SetStatus(String.Format("False Accept Rate (FAR) = {0}", FAR));
		}
        //private string Template_list;
		private DPFP.Template Template;
        private DPFP.Template Template2;
		private DPFP.Verification.Verification Verificator;

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // VerificationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(581, 354);
            this.Name = "VerificationForm";
            this.Load += new System.EventHandler(this.VerificationForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void VerificationForm_Load(object sender, EventArgs e)
        {

            /*DataResult<Fmd> resultConversion = null;
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
            }*/

            

        }

	}
}