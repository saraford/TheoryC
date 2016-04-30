/*  Copyright (c)2015 San Jose State University - All Rights Reserved
    Licensed under the Microsoft Public License (Ms-Pl)
    Created by Sara Ford and Dr. Emily Wughalter, Dept of Kinesiology, San Jose State University */

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
            StreamWriter writer = CreateResultsLogFile(participantID);

            if (writer == null)
            {
                return;
            }

            try
            {
                // Put the participant ID at the top
                writer.WriteLine(participantID);

                // Establish column headings
                writer.WriteLine("Trial # , " + 
                                 " Time on Target , " + 
                                 " Absolute Error , " + 
                                 " Constant Error , " + 
                                 " Variable Error , " + 
                                 " Hand Depth StdDev , " + 
                                 " Lean Right Left X , " + 
                                 " Lean Forward Backward Y , " + 
                                 " Total Possible Ticks , " +
                                 " Tick Count , " + 
                                 "  , " + 
                                 " ToT1 , " +
                                 " ToT2 , " +
                                 " ToT3 , " + 
                                 " ABE1 , " +
                                 " ABE2 , " +
                                 " ABE3 , " +
                                 " CE1  , " +
                                 " CE2  , " + 
                                 " CE3  , " +
                                 " VE1  , " +
                                 " VE2  , " +
                                 " VE3  , " +
                                 " HandDepth1 , " + 
                                 " HandDepth2 , " + 
                                 " HandDepth3 , " + 
                                 " LeanLR1 , " + 
                                 " LeanLR2 , " + 
                                 " LeanLR3 , " +
                                 " LeanFB1 , " + 
                                 " LeanFB2 , " + 
                                 " LeanFB3 , " + 
                                 " Tick1 , " +
                                 " Tick2 , " +
                                 " Tick3 ," +
                                 " KinectFPS1 , " +
                                 " KinectFPS2 , " +
                                 " KinectFPS3 , " + 
                                 " Body Frames Counted , " +
                                 " Total Possible Body Frames ");

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
            str += Result.HandDepthStdDev.ToString() + ",";
            str += Result.LeanLeftRightX.ToString() + ",";
            str += Result.LeanForwardBackY.ToString() + ",";
            str += Result.TotalPossibleTicks.ToString() + ",";
            str += Result.TickCount.ToString() + ",";
            str += "  ,";
            str += Result.TimeOnTarget1.ToString() + ",";
            str += Result.TimeOnTarget2.ToString() + ",";
            str += Result.TimeOnTarget3.ToString() + ",";
            str += Result.AbsoluteError1.ToString() + ",";
            str += Result.AbsoluteError2.ToString() + ",";
            str += Result.AbsoluteError3.ToString() + ",";
            str += Result.ConstantError1.ToString() + ",";
            str += Result.ConstantError2.ToString() + ",";
            str += Result.ConstantError3.ToString() + ",";
            str += Result.VariableError1.ToString() + ",";
            str += Result.VariableError2.ToString() + ",";
            str += Result.VariableError3.ToString() + ",";
            str += Result.HandDepthStdDev1.ToString() + ",";
            str += Result.HandDepthStdDev2.ToString() + ",";
            str += Result.HandDepthStdDev3.ToString() + ",";
            str += Result.LeanLeftRightX1.ToString() + ",";
            str += Result.LeanLeftRightX2.ToString() + ",";
            str += Result.LeanLeftRightX3.ToString() + ",";
            str += Result.LeanForwardBackY1.ToString() + ",";
            str += Result.LeanForwardBackY2.ToString() + ",";
            str += Result.LeanForwardBackY3.ToString() + ",";
            str += Result.TickCount1.ToString() + ",";
            str += Result.TickCount2.ToString() + ",";
            str += Result.TickCount3.ToString() + ",";
            str += Result.KinectFPS1.ToString() + ",";
            str += Result.KinectFPS2.ToString() + ",";
            str += Result.KinectFPS3.ToString() + ",";
            str += Result.KinectBodyFramesTrial.ToString() + ",";
            str += Result.KinectTotalPossibleBodyFrames.ToString();

            return str;   
        }

        private static StreamWriter CreateResultsLogFile(string participantID)
        {
            StreamWriter writer = null;

            if (participantID == null || participantID == "")
            {
                participantID = "No Name";
            }
                
            try
            {

                string timestamp = DateTime.Now.ToString("MMM dd yyyy");
                string filenameOrg =  participantID + " " + timestamp;
                string filepath = GetDesktopFolder(); 
                string fullpath = System.IO.Path.Combine(filepath, filenameOrg + Properties.Settings.Default.CSVextension);

                string filename;
                for (int i = 1; ; ++i)
                {
                    if (!File.Exists(fullpath))
                        break;

                    filename = filenameOrg + "_" + i;
                    fullpath = Path.Combine(filepath, filename + Properties.Settings.Default.CSVextension);
                }

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
            //shapeSizeDiameter, durationSeconds, rpm, breaktime

            string str;

            str = trial.ShapeSizeDiameter.ToString() + ",";
            str += trial.DurationSeconds.ToString() + ",";
            str += trial.RPMs.ToString() + ",";
            str += trial.BreakTime.ToString() + ","; 

            return str;
        }

    }


}
