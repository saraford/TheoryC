using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TheoryC.Common
{
    class DataLogger
    {

        public static void LogExperiment(string participantID, ObservableCollection<Models.Trial> Trials)
        {
            StreamWriter writer = CreateLogFile();

            if (writer == null)
            {
                return;
            }

            try
            {
                // Put the participant ID at the top
                writer.WriteLine(participantID);

                // Establish column headings
                writer.WriteLine("Trial # , Time on Target , Absolute Error , Constant Error , Variable Error , Hand Depth StdDev , Tick Count");

                foreach (var trial in Trials)
                {
                    string strResults = SeralizedResults(trial.TrialName, trial.Results);
                    writer.WriteLine(strResults);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Yo! LogExperiment() failed " + e.Message);
            }

            writer.Close();

        }

        private static string SeralizedResults(int trialNumber, Models.Result Result)
        {
            //Time on Target , Absolute Error , Constant Error , Variable Error , Tick Count

            string str;

            str = trialNumber.ToString() + ",";
            str += Result.TimeOnTarget.ToString() + ",";
            str += Result.AbsoluteError.ToString() + ",";
            str += Result.ConstantError.ToString() + ",";
            str += Result.VariableError.ToString() + ",";
            str += Result.HandDepth.ToString() + ",";
            str += Result.TickCount.ToString() + ",";

            return str;   
        }

        private static StreamWriter CreateLogFile()
        {
            StreamWriter writer = null;

            try
            {
                string filepath = GetDesktopFolder();

                string filename = DateTime.Now.ToString("yyyyMMddHHmm") + Properties.Settings.Default.CSVextension;
                string fullpath = System.IO.Path.Combine(filepath, filename);
                writer = new StreamWriter(fullpath, false);                

            }
            catch (Exception e)
            {
                MessageBox.Show("Couldn't create data file " + e.Message);
            }

            return writer;
        }

        public static string GetDesktopFolder()
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filepath = System.IO.Path.Combine(desktop, "TheoryC_Results");

            if (Directory.Exists(filepath) == false)
            {
                Directory.CreateDirectory(filepath);
            }
            return filepath;
        }



        internal static bool ExportSettings(ObservableCollection<Models.Trial> Trials)
        {
            bool success = false; //to avoid false positives
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.InitialDirectory = desktop;
            dialog.DefaultExt = Properties.Settings.Default.CSVextension;
            dialog.Filter = "CSV Files (*.csv)|*.csv";

            // Show save file dialog box
            Nullable<bool> result = dialog.ShowDialog();

            // Process save file dialog box results
            string filename = "";
            if (result == true)
            {
                // Save document
                filename = dialog.FileName;
            }
            else
            {
                // user decided to cancel
                return false; 
            }

            try
            {
                StreamWriter writer = new StreamWriter(filename, false);

                foreach (var trial in Trials)
                {
                    string strResults = SeralizedSettings(trial);
                    writer.WriteLine(strResults);
                }

                writer.Close();
                success = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yo! Couldn't save data " + ex.Message);
            }

            return success;
        }

        private static string SeralizedSettings(Models.Trial trial)
        {
            //shapeSizeDiameter, durationSeconds, rpm

            string str;

            str = trial.ShapeSizeDiameter.ToString() + ",";
            str += trial.DurationSeconds.ToString() + ",";
            str += trial.RPMs.ToString() + ",";

            return str;
        }

    }


}
