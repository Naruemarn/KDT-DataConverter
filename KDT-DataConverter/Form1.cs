using Gajatko.IniFiles;
using System.Globalization;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;
using System.ComponentModel;


namespace RTCM3
{
    public partial class Form1 : Form
    {
        private BackgroundWorker extractFile;
        private long fileSize;    //the size of the zip file
        private long extractedSizeTotal;    //the bytes total extracted
        private long compressedSize;    //the size of a single compressed file
        private string compressedFileName;    //the name of the file being extracted

        string kdtfile = "";

        public static string pointName_ini = null;
        public static string antennaName1_ini = null;
        public static string antennaName2_ini = null;
        public static string antennaHeight_ini = null;
        public static string serialNo_ini = null;
        public static string recerveSN_ini = "";
        public static string inputType_ini = null;
        public static string dataInterval_ini = null;

        public static int save_drive = 0;

        int step = 0;
        string extractPath = "";
        string antenna_H = "";

        string outputpath = @"C:\";
        string filenameNoKDT = "";
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public Form1()
        {
            InitializeComponent();

            //Set the maximum vaue to int.MaxValue, thus, it could be more accurate
            progressBar_Individual.Maximum = int.MaxValue;
            progressBar_Total.Maximum = int.MaxValue;

            extractFile = new BackgroundWorker();
            extractFile.DoWork += ExtractFile_DoWork;
            extractFile.ProgressChanged += ExtractFile_ProgressChanged;
            extractFile.RunWorkerCompleted += ExtractFile_RunWorkerCompleted;
            extractFile.WorkerReportsProgress = true;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void ExtractFile_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Set the maximum vaue to int.MaxValue because the process is completed
            progressBar_Individual.Value = int.MaxValue;
            progressBar_Total.Value = int.MaxValue;


            try
            {                
                string[] _iniFile = Directory.GetFiles(extractPath, "*.ini");

                read_ini_config(_iniFile[0]);
                

                if (antennaName2_ini == "Antenna Bottom")
                {
                    float x = (float)(float.Parse(antennaHeight_ini, CultureInfo.InvariantCulture.NumberFormat) + 0.121);
                    antenna_H = x.ToString();
                }
                else
                {
                    antenna_H = antennaHeight_ini;
                }



                if (inputType_ini == "RTCM")// RTCM to RINEX
                {
                    string[] listfiles = Directory.GetFiles(extractPath, "*.rtcm");
                    string rtcmfile = listfiles[0];

                    ToProcess(rtcmfile, _iniFile, outputpath + @"\" + filenameNoKDT, antenna_H);
                }
                else// BINEX to RINEX
                {
                    string[] listfiles = Directory.GetFiles(extractPath, "*.BNX");
                    string binexfile = listfiles[0];

                    ToProcess(binexfile, _iniFile, outputpath + @"\" + filenameNoKDT, antenna_H);
                }

            }
            catch (Exception ex)
            {
                DialogResult res = MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void ExtractFile_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar_Individual.Value = e.ProgressPercentage;

            //calculate the totalPercent
            long totalPercent = ((long)e.ProgressPercentage * compressedSize + extractedSizeTotal * int.MaxValue) / fileSize;
            if (totalPercent > int.MaxValue)
                totalPercent = int.MaxValue;
            progressBar_Total.Value = (int)totalPercent;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void ExtractFile_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string fileName = kdtfile;

                //get the size of the zip file
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(fileName);
                fileSize = fileInfo.Length;

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                using (Ionic.Zip.ZipFile zipFile = Ionic.Zip.ZipFile.Read(fileName))
                {
                    //reset the bytes total extracted to 0
                    extractedSizeTotal = 0;
                    int fileAmount = zipFile.Count;
                    int fileIndex = 0;
                    zipFile.ExtractProgress += Zip_ExtractProgress;
                    foreach (Ionic.Zip.ZipEntry ZipEntry in zipFile)
                    {
                        fileIndex++;
                        compressedFileName = "(" + fileIndex.ToString() + "/" + fileAmount + "): " + ZipEntry.FileName;
                        //get the size of a single compressed file
                        compressedSize = ZipEntry.CompressedSize;
                        ZipEntry.Extract(extractPath, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                        //calculate the bytes total extracted
                        extractedSizeTotal += compressedSize;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void Zip_ExtractProgress(object sender, Ionic.Zip.ExtractProgressEventArgs e)
        {
            if (e.TotalBytesToTransfer > 0)
            {
                long percent = e.BytesTransferred * int.MaxValue / e.TotalBytesToTransfer;
                //Console.WriteLine("Indivual: " + percent);
                extractFile.ReportProgress((int)percent);
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        void read_ini_config(string path)
        {
            try
            {
                convertUNIXtoWINDOWS(path);

                IniFile cn_file =  IniFile.FromFile(path);
                pointName_ini = cn_file["CONFIG"]["pointName"];
                antennaName1_ini = cn_file["CONFIG"]["antennaName1"];
                antennaName2_ini = cn_file["CONFIG"]["antennaName2"];
                antennaHeight_ini = cn_file["CONFIG"]["antennaHeight"];
                serialNo_ini = cn_file["CONFIG"]["serialNo"];
                recerveSN_ini = cn_file["CONFIG"]["recerveSN"];
                inputType_ini = cn_file["CONFIG"]["outputType"];
                dataInterval_ini = cn_file["CONFIG"]["dataInterval"];

                if(recerveSN_ini == null)
                {
                    recerveSN_ini = " ";
                }

                if (textBox1.Text.Length >= 1)
                {
                    antennaName1_ini = textBox1.Text;
                }
            }
            catch (Exception h)
            {
                MessageBox.Show(h.ToString());
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        void convertUNIXtoWINDOWS(string file)
        {
            string[] lines = File.ReadAllLines(file);
            List<string> list_of_string = new List<string>();
            foreach (string line in lines)
            {
                list_of_string.Add(line.Replace("\n", "\r\n"));
            }
            File.WriteAllLines(file, list_of_string);
            
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------      
        private void Form1_Load(object sender, EventArgs e)
        {
            string[] drives = System.IO.Directory.GetLogicalDrives();       // Get Drives 

            foreach (string str in drives)
            {
                comboBox1.Items.Add(str);
            }

            Read_Drive();

            try
            {
                comboBox1.SelectedItem = drives[save_drive];
            }
            catch(Exception ex)
            {
                save_drive = 0;                
                comboBox1.SelectedItem = drives[save_drive];
            }

            Read_OutputFolder();
            label2.Text = outputpath;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        void ConvertToRinex(string filepath, string outputpathOBS, string outputNAV, string height)
        {
            string strCmdText;


            if (recerveSN_ini == "") recerveSN_ini = " ";
            if (serialNo_ini == "") serialNo_ini = " ";

            string inputType = "";

            if (inputType_ini == "RTCM")
            {
                inputType = "rtcm3";
            }
            else
            {                
                inputType = "binex";
            }

            // Run Convbin 
            strCmdText = @" -ti " + dataInterval_ini + " -r " + inputType + " -v 3.02 " + " -hm " + pointName_ini + " -hn " + pointName_ini + @" -ht RG-CORS -ho KKFC/KKFC -hd " + height + @" -ha " + @"""" + serialNo_ini + @"""" + @"/""" + antennaName1_ini + @""" -od -os -o " + outputpathOBS + " -n " + outputNAV + " " + filepath;
            System.Diagnostics.Process.Start("kdtconv.exe", strCmdText);
        }        
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        void ToProcess(string _file, string[] _iniFile, string output, string height)
        {
            string outputOBS = output + ".obs";
            string outputNAV = output + ".nav";

            if (File.Exists(outputOBS))
            {
                File.Delete(outputOBS);
            }

            if (File.Exists(outputNAV))
            {
                File.Delete(outputNAV);
            }

            ConvertToRinex(_file, outputOBS, outputNAV, height);

            label2.Text = outputOBS;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void button3_Click(object sender, EventArgs e)
        {

            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open .kdt File";
            theDialog.Filter = "files|*.kdt";
            theDialog.InitialDirectory = comboBox1.Text + @"Android\data\com.dct.rgcontroller\files";
            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                if (label2.Text != "")
                {
                    try
                    {
                        kdtfile = theDialog.FileName;

                        label1.Text = kdtfile; 

                        string f = kdtfile.Substring(0, kdtfile.Length - 4);       // remove .kdt

                        filenameNoKDT = f.Split("\\").Last();

                        extractPath = @"C:\" + filenameNoKDT;

                        if (Directory.Exists(extractPath))
                        {
                            Directory.Delete(extractPath, true);
                        }
                        else
                        {                                                       
                            extractFile.RunWorkerAsync();
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Please select output folder", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private bool IsExistProcess(string processName)
        {

            Process[] MyProcesses = Process.GetProcesses();
            foreach (Process MyProcess in MyProcesses)
            {
                if (MyProcess.ProcessName.CompareTo(processName) == 0)
                {
                    return true;

                }
            }
            return false;
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        private void timer1_Tick(object sender, EventArgs e)
        {
            switch (step)
            {
                case 0:
                    if (IsExistProcess("kdtconv"))
                    {
                        step = 1;
                    }
                    break;
                case 1:
                    if (!IsExistProcess("kdtconv")) //closed
                    {
                        try
                        {
                            if (Directory.Exists(extractPath))
                            {
                                Directory.Delete(extractPath, true);
                            }

                            timer1.Enabled = false;

                            // Replace text
                            string path = outputpath + @"\" + filenameNoKDT + ".obs";
                            ReplaceTextFile(path);

                            // Rename Extension file
                            string path_rename = outputpath + @"\" + filenameNoKDT;
                            File.Move(path_rename + ".obs", path_rename + ".24o");
                            File.Move(path_rename + ".nav", path_rename + ".24n");

                            //Move .kdt to Output Folder
                            string destination = outputpath + @"\" + filenameNoKDT + ".kdt";

                            if (File.Exists(destination))
                            {
                                File.Delete(destination);
                            }

                            File.Move(kdtfile, destination);

                            DialogResult res = MessageBox.Show("Done", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            label2.Text = outputpath;

                            timer1.Enabled = true;

                            step = 0;
                        }
                        catch (Exception ex) 
                        {
                            MessageBox.Show(ex.ToString());
                        }
                    }
                    break;
                default:
                    step = 0;
                    break;
            }


        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            save_drive = comboBox1.SelectedIndex;
            Save_Drive();
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        void Save_Drive()
        {
            RegistryKey RegKeyWrite = Registry.CurrentUser;
            RegKeyWrite = RegKeyWrite.CreateSubKey(@"Software\CSHARP\WriteRegistryValue");
            RegKeyWrite.SetValue("savedrive", save_drive);
            RegKeyWrite.Close();
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------       
        void Read_Drive()
        {
            try
            {
                RegistryKey RegKeyRead = Registry.CurrentUser;
                RegKeyRead = RegKeyRead.OpenSubKey(@"Software\CSHARP\WriteRegistryValue");

                if (RegKeyRead != null)
                {
                    Object s = RegKeyRead.GetValue("savedrive");
                    if (s != null)
                    {
                        string s_str = s.ToString();
                        save_drive = int.Parse(s_str);
                    }
                    else
                    {
                        save_drive = 0;
                    }
                    RegKeyRead.Close();
                }
                else
                {
                    save_drive = 0;
                }
            }
            catch (Exception ex)
            {

            }
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------- 
        void Save_OutputFolder()
        {
            RegistryKey RegKeyWrite = Registry.CurrentUser;
            RegKeyWrite = RegKeyWrite.CreateSubKey(@"Software\CSHARP\WriteRegistryValue");
            RegKeyWrite.SetValue("saveoutputfolder", label2.Text);
            RegKeyWrite.Close();
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------       
        void Read_OutputFolder()
        {
            try
            {
                RegistryKey RegKeyRead = Registry.CurrentUser;
                RegKeyRead = RegKeyRead.OpenSubKey(@"Software\CSHARP\WriteRegistryValue");

                if (RegKeyRead != null)
                {
                    Object s = RegKeyRead.GetValue("saveoutputfolder");
                    if (s != null)
                    {
                        outputpath = s.ToString();
                    }
                    else
                    {
                        outputpath = @"C:\";
                    }
                    RegKeyRead.Close();
                }
                else
                {
                    outputpath = @"C:\";
                }
            }
            catch (Exception ex)
            {

            }
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            folderDlg.InitialDirectory = outputpath;
            // Show the FolderBrowserDialog.  
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {                
                label2.Text = folderDlg.SelectedPath;
                outputpath = folderDlg.SelectedPath;

                Save_OutputFolder();

                Environment.SpecialFolder root = folderDlg.RootFolder;
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        public bool SearchStringInTextFile(string path)
        {
            StreamReader s = new StreamReader(path);
            string currentLine;
            string searchString = "TRIMBLE BD990";
            bool foundText = false;

            do
            {
                currentLine = s.ReadLine();
                if (currentLine != null)
                {
                    foundText = currentLine.Contains(searchString);
                }
            }
            while (currentLine != null && !foundText);

            s.Close();

            return foundText;
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        void replace_BNX_KDT(string path)
        {
            string text = File.ReadAllText(path);

            if (inputType_ini == "BINEX")
            {
                text = text.Replace(".BNX", ".KDT");
            }
            else
            {
                text = text.Replace(".rtcm", ".KDT ");
            }

            File.WriteAllText(path, text);            
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        void ReplaceTextFile(string path)
        {
            try
            {
                replace_BNX_KDT(path);

                bool res = SearchStringInTextFile(path);

                byte[] file = System.IO.File.ReadAllBytes(path);

                // Replace text to RINEX file

                byte[] RUNBY = new byte[60];        // 60 Bytes for RUN BY
                byte[] FORMAT = new byte[60];        // 60 Bytes for Format


                byte[] REC = new byte[20];          // 20 Bytes for REC #
                byte[] TYPE = new byte[20];         // 20 Bytes for TYPE 
                byte[] VERSION = new byte[20];      // 20 Bytes for TYPE 

                //---------------------------------------------------------------

                // Clear
                for (int i = 0; i < 60; i++)
                {
                    RUNBY[i] = 0x20;
                    FORMAT[i] = 0x20;
                }

                // Clear
                for (int i = 0; i < 20; i++)
                {
                    REC[i] = 0x20;
                    TYPE[i] = 0x20;
                    VERSION[i] = 0x20;
                }

                //---------------------------------------------------------------

                // RUN BY
                byte[] runby_byte = Encoding.ASCII.GetBytes("KDT-DataConverter 1.00");
                for (int i = 0; i < runby_byte.Length; i++)
                {
                    RUNBY[i] = runby_byte[i];
                }

                // FORMAT
                byte[] format_byte = Encoding.ASCII.GetBytes("format: KDT");
                for (int i = 0; i < format_byte.Length; i++)
                {
                    FORMAT[i] = format_byte[i];
                }

                // Convert string to bytes array
                byte[] rec_byte = Encoding.ASCII.GetBytes(recerveSN_ini);
                for (int i = 0; i < rec_byte.Length; i++)
                {
                    REC[i] = rec_byte[i];
                }

                //---------------------------------------------------------------

                int runby_startindex = 82;
                int format_startindex = 164;
                int rec_startindex = 656;
                int type_startindex = 676;
                int version_startindex = 696;



                if (res) // if found "TRIMBLE BD990"
                {
                    // Replace to RUN BY
                    for (int i = 0; i < 60; i++)
                    {
                        file[runby_startindex + i] = RUNBY[i];             // Start Index 82
                    }

                    // Replace to FORMAT
                    for (int i = 0; i < 60; i++)
                    {
                        file[format_startindex + i] = FORMAT[i];             // Start Index 164
                    }

                    // convert string to array
                    byte[] type_byte = Encoding.ASCII.GetBytes("P2 GNSS");
                    for (int i = 0; i < type_byte.Length; i++)
                    {
                        TYPE[i] = type_byte[i];
                    }

                    // Replace
                    for (int i = 0; i < 20; i++)
                    {
                        file[rec_startindex + i] = REC[i];              // Start Index 656
                        file[type_startindex + i] = TYPE[i];            // Start Index 676
                        file[version_startindex + i] = VERSION[i];      // Start Index 696
                    }
                }
                else
                {
                    // Replace to RUN BY
                    for (int i = 0; i < 60; i++)
                    {
                        file[runby_startindex + i] = RUNBY[i];                  // Start Index 82
                    }

                    // Replace to FORMAT
                    for (int i = 0; i < 60; i++)
                    {
                        file[format_startindex + i] = FORMAT[i];                // Start Index 164
                    }

                    // Replace to REC
                    for (int i = 0; i < 20; i++)
                    {
                        file[rec_startindex + i] = REC[i];                      // Start Index 656
                    }
                }

                //---------------------------------------------------------------

                // Overwrite
                File.WriteAllBytes(path, file);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
    }
}