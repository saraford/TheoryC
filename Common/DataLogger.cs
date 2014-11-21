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

        public void LogExperiment(ObservableCollection<Models.Trial> Trials)
        {
            StreamWriter writer = CreateLogFile();

            if (writer == null)
            {
                return;
            }

            try
            {
                // Establish column headings
                writer.WriteLine("Trial # , Time on Target , Absolute Error , Constant Error , Variable Error ");

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

        private string SeralizedResults(int trialNumber, Models.Result Result)
        {
            //Time on Target , Absolute Error , Constant Error , Variable Error

            string str;

            str = trialNumber.ToString() + ",";
            str += Result.TimeOnTargetMs.ToString() + ",";
            str += Result.AbsoluteError.ToString() + ",";
            str += Result.ConstantError.ToString() + ",";
            str += Result.VariableError.ToString() + ",";

            return str;   
        }

        private StreamWriter CreateLogFile()
        {
            StreamWriter writer = null;

            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filepath = System.IO.Path.Combine(desktop, "TheoryC_Results");

                if (Directory.Exists(filepath) == false)
                {
                    Directory.CreateDirectory(filepath);
                }

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


    }


}
