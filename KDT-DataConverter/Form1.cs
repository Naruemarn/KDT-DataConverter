using Gajatko.IniFiles;

using u8 = System.Byte;
using u16 = System.UInt16;
using s32 = System.Int32;
using u32 = System.UInt32;
using gps_time_t = System.UInt64;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;
using CSharpLib;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using ExpressEncription;



namespace RTCM3
{
    public partial class Form1 : Form
    {
        [DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);

         string Key = "TechWithVPTechWithVPTechWithVP12";
         string IV = "VivekPanchal1122";



        private BackgroundWorker extractFile;
        private long fileSize;    //the size of the zip file
        private long extractedSizeTotal;    //the bytes total extracted
        private long compressedSize;    //the size of a single compressed file
        private string compressedFileName;    //the name of the file being extracted

        string kdtfile = "";


        const byte RTCM3PREAMB = 0xD3;


        static u32[] crc24qtab = new u32[] {
            0x000000, 0x864CFB, 0x8AD50D, 0x0C99F6, 0x93E6E1, 0x15AA1A, 0x1933EC, 0x9F7F17,
            0xA18139, 0x27CDC2, 0x2B5434, 0xAD18CF, 0x3267D8, 0xB42B23, 0xB8B2D5, 0x3EFE2E,
            0xC54E89, 0x430272, 0x4F9B84, 0xC9D77F, 0x56A868, 0xD0E493, 0xDC7D65, 0x5A319E,
            0x64CFB0, 0xE2834B, 0xEE1ABD, 0x685646, 0xF72951, 0x7165AA, 0x7DFC5C, 0xFBB0A7,
            0x0CD1E9, 0x8A9D12, 0x8604E4, 0x00481F, 0x9F3708, 0x197BF3, 0x15E205, 0x93AEFE,
            0xAD50D0, 0x2B1C2B, 0x2785DD, 0xA1C926, 0x3EB631, 0xB8FACA, 0xB4633C, 0x322FC7,
            0xC99F60, 0x4FD39B, 0x434A6D, 0xC50696, 0x5A7981, 0xDC357A, 0xD0AC8C, 0x56E077,
            0x681E59, 0xEE52A2, 0xE2CB54, 0x6487AF, 0xFBF8B8, 0x7DB443, 0x712DB5, 0xF7614E,
            0x19A3D2, 0x9FEF29, 0x9376DF, 0x153A24, 0x8A4533, 0x0C09C8, 0x00903E, 0x86DCC5,
            0xB822EB, 0x3E6E10, 0x32F7E6, 0xB4BB1D, 0x2BC40A, 0xAD88F1, 0xA11107, 0x275DFC,
            0xDCED5B, 0x5AA1A0, 0x563856, 0xD074AD, 0x4F0BBA, 0xC94741, 0xC5DEB7, 0x43924C,
            0x7D6C62, 0xFB2099, 0xF7B96F, 0x71F594, 0xEE8A83, 0x68C678, 0x645F8E, 0xE21375,
            0x15723B, 0x933EC0, 0x9FA736, 0x19EBCD, 0x8694DA, 0x00D821, 0x0C41D7, 0x8A0D2C,
            0xB4F302, 0x32BFF9, 0x3E260F, 0xB86AF4, 0x2715E3, 0xA15918, 0xADC0EE, 0x2B8C15,
            0xD03CB2, 0x567049, 0x5AE9BF, 0xDCA544, 0x43DA53, 0xC596A8, 0xC90F5E, 0x4F43A5,
            0x71BD8B, 0xF7F170, 0xFB6886, 0x7D247D, 0xE25B6A, 0x641791, 0x688E67, 0xEEC29C,
            0x3347A4, 0xB50B5F, 0xB992A9, 0x3FDE52, 0xA0A145, 0x26EDBE, 0x2A7448, 0xAC38B3,
            0x92C69D, 0x148A66, 0x181390, 0x9E5F6B, 0x01207C, 0x876C87, 0x8BF571, 0x0DB98A,
            0xF6092D, 0x7045D6, 0x7CDC20, 0xFA90DB, 0x65EFCC, 0xE3A337, 0xEF3AC1, 0x69763A,
            0x578814, 0xD1C4EF, 0xDD5D19, 0x5B11E2, 0xC46EF5, 0x42220E, 0x4EBBF8, 0xC8F703,
            0x3F964D, 0xB9DAB6, 0xB54340, 0x330FBB, 0xAC70AC, 0x2A3C57, 0x26A5A1, 0xA0E95A,
            0x9E1774, 0x185B8F, 0x14C279, 0x928E82, 0x0DF195, 0x8BBD6E, 0x872498, 0x016863,
            0xFAD8C4, 0x7C943F, 0x700DC9, 0xF64132, 0x693E25, 0xEF72DE, 0xE3EB28, 0x65A7D3,
            0x5B59FD, 0xDD1506, 0xD18CF0, 0x57C00B, 0xC8BF1C, 0x4EF3E7, 0x426A11, 0xC426EA,
            0x2AE476, 0xACA88D, 0xA0317B, 0x267D80, 0xB90297, 0x3F4E6C, 0x33D79A, 0xB59B61,
            0x8B654F, 0x0D29B4, 0x01B042, 0x87FCB9, 0x1883AE, 0x9ECF55, 0x9256A3, 0x141A58,
            0xEFAAFF, 0x69E604, 0x657FF2, 0xE33309, 0x7C4C1E, 0xFA00E5, 0xF69913, 0x70D5E8,
            0x4E2BC6, 0xC8673D, 0xC4FECB, 0x42B230, 0xDDCD27, 0x5B81DC, 0x57182A, 0xD154D1,
            0x26359F, 0xA07964, 0xACE092, 0x2AAC69, 0xB5D37E, 0x339F85, 0x3F0673, 0xB94A88,
            0x87B4A6, 0x01F85D, 0x0D61AB, 0x8B2D50, 0x145247, 0x921EBC, 0x9E874A, 0x18CBB1,
            0xE37B16, 0x6537ED, 0x69AE1B, 0xEFE2E0, 0x709DF7, 0xF6D10C, 0xFA48FA, 0x7C0401,
            0x42FA2F, 0xC4B6D4, 0xC82F22, 0x4E63D9, 0xD11CCE, 0x575035, 0x5BC9C3, 0xDD8538
        };


        public static string pointName_ini = null;
        public static string antennaName1_ini = null;
        public static string antennaName2_ini = null;
        public static string antennaHeight_ini = null;
        public static string serialNo_ini = null;
        public static string recerveSN_ini = null;
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
                    string[] _rtcmFile = Directory.GetFiles(extractPath, "*.rtcm");
                    //ToProcess(_rtcmFile, _iniFile, extractPath, antenna_H);
                    ToProcess(_rtcmFile, _iniFile, outputpath + @"\" + filenameNoKDT, antenna_H);
                }
                else// BINEX to RINEX
                {
                    string[] _binexFile = Directory.GetFiles(extractPath, "*.BNX");
                    string filetype = _binexFile[0].Substring(_binexFile[0].Length - 6);
                    //ToProcess(_binexFile, _iniFile, extractPath, antenna_H);
                    ToProcess(_binexFile, _iniFile, outputpath + @"\" + filenameNoKDT, antenna_H);
                }

            }
            catch (Exception ex)
            {
                DialogResult res = MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            //MessageBox.Show("Done!");
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void ExtractFile_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //textBox_ExtractFile.Text = compressedFileName;

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
        public static u32 crc24q(u8[] buf, u32 len, u32 crc)
        {
            for (u32 i = 0; i < len; i++)
                crc = ((crc << 8) & 0xFFFFFF) ^ crc24qtab[(crc >> 16) ^ buf[i]];
            return crc;
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        static double ROUND(double x)
        {
            return ((s32)Math.Floor((x) + 0.5));
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        static double ROUND_U(double x)
        {
            return ((u32)Math.Floor((x) + 0.5));
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        static void setbitu(u8[] buff, u32 pos, u32 len, double data)
        {
            setbitu(buff, pos, len, (u32)data);
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        static void setbitu(u8[] buff, u32 pos, u32 len, u32 data)
        {
            u32 mask = 1u << (int)(len - 1);

            if (len <= 0 || 32 < len) return;

            for (u32 i = pos; i < pos + len; i++, mask >>= 1)
            {
                if ((data & mask) > 0)
                    buff[i / 8] |= (byte)(1u << (int)(7 - i % 8));
                else
                    buff[i / 8] &= (byte)(~(1u << (int)(7 - i % 8)));
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        static void setbits(u8[] buff, u32 pos, u32 len, double data)
        {
            setbits(buff, pos, len, (s32)data);
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        static void setbits(u8[] buff, u32 pos, u32 len, s32 data)
        {
            if (data < 0)
                data |= (1 << (int)(len - 1));
            else
                data &= (~(1 << (int)(len - 1)));   /* set sign bit */
            setbitu(buff, pos, len, (u32)data);
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        static void set38bits(u8[] buff, uint pos, double value)
        {
            int word_h = (int)Math.Floor(value / 64.0);
            uint word_l = (uint)(value - word_h * 64.0);
            setbits(buff, pos, 32, word_h);
            setbitu(buff, pos + 32, 6, word_l);
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        static uint getbitu(u8[] buff, u32 pos, u32 len)
        {
            uint bits = 0;
            u32 i;
            for (i = pos; i < pos + len; i++)
                bits = (uint)((bits << 1) + ((buff[i / 8] >> (int)(7 - i % 8)) & 1u));
            return bits;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        static int getbits(u8[] buff, u32 pos, u32 len)
        {
            uint bits = getbitu(buff, pos, len);
            if (len <= 0 || 32 <= len || !((bits & (1u << (int)(len - 1))) != 0))
                return (int)bits;
            return (int)(bits | (~0u << (int)len)); /* extend sign */
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        static double getbits_38(u8[] buff, uint pos)
        {
            return (double)getbits(buff, pos, 32) * 64.0 + getbitu(buff, pos + 32, 6);
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public class rtcmpreamble
        {
            public u8 preamble = RTCM3PREAMB;
            public u8 resv1;
            public u16 length;

            public void Read(byte[] buffer)
            {
                uint i = 0;

                preamble = (byte)getbitu(buffer, i, 8); i += 8;
                resv1 = (byte)getbitu(buffer, i, 6); i += 6;
                length = (u16)getbitu(buffer, i, 10); i += 10;
            }

            public byte[] Write(byte[] buffer)
            {
                uint i = 0;

                setbitu(buffer, i, 8, RTCM3PREAMB); i += 8;
                setbitu(buffer, i, 6, resv1); i += 6;
                setbitu(buffer, i, 10, length); i += 10;

                return buffer;
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public class rtcmheader
        {
            public u16 messageno;
            public u16 refstationid;
            public u32 epoch;
            public u8 sync;
            public u8 nsat;
            public u8 smoothind;
            public u8 smoothint;

            public void Read(byte[] buffer)
            {
                u32 i = 24;

                messageno = (u16)getbitu(buffer, i, 12); i += 12;        /* message no */
                refstationid = (u16)getbitu(buffer, i, 12); i += 12;        /* ref station id */
                epoch = (u32)getbitu(buffer, i, 30); i += 30;       /* gps epoch time */
                sync = (u8)getbitu(buffer, i, 1); i += 1;          /* synchronous gnss flag */
                nsat = (u8)getbitu(buffer, i, 5); i += 5;          /* no of satellites */
                smoothind = (u8)getbitu(buffer, i, 1); i += 1;          /* smoothing indicator */
                smoothint = (u8)getbitu(buffer, i, 3); i += 3;          /* smoothing interval */
            }

            public byte[] Write(byte[] buffer)
            {
                u32 i = 24;

                setbitu(buffer, i, 12, messageno); i += 12;        /* message no */
                setbitu(buffer, i, 12, refstationid); i += 12;        /* ref station id */
                setbitu(buffer, i, 30, epoch); i += 30;       /* gps epoch time */
                setbitu(buffer, i, 1, sync); i += 1;          /* synchronous gnss flag */
                setbitu(buffer, i, 5, nsat); i += 5;          /* no of satellites */
                setbitu(buffer, i, 1, smoothind); i += 1;          /* smoothing indicator */
                setbitu(buffer, i, 3, smoothint); i += 3;          /* smoothing interval */
                return buffer;
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public class type1006
        {
            public u16 staid = 1;
            public u8 itrf = 0;
            public u8 gpsind = 1;
            public u8 glonassind = 0;
            public u8 galileoind = 0;
            public u8 refstatind = 0;
            public double rr0;
            public u8 oscind = 1;
            public u8 resv = 0;
            public double rr1;
            public u8 quatcycind = 0;
            public double rr2;
            public u16 anth;

            public double[] ecefposition
            {
                get
                {
                    return new double[]
                    {
                        rr0 * 0.0001,
                        rr1 * 0.0001,
                        rr2 * 0.0001
                    };
                }
                set
                {
                    rr0 = value[0] / 0.0001;
                    rr1 = value[1] / 0.0001;
                    rr2 = value[2] / 0.0001;
                }
            }

            public void Read(byte[] buffer)
            {
                uint i = 24 + 12;

                staid = (u16)getbitu(buffer, i, 12); i += 12;
                itrf = (u8)getbitu(buffer, i, 6); i += 6 + 4;
                rr0 = getbits_38(buffer, i); i += 38 + 2;
                rr1 = getbits_38(buffer, i); i += 38 + 2;
                rr2 = getbits_38(buffer, i); i += 38;
                anth = (u16)getbitu(buffer, i, 16); i += 16;
            }

            public uint Write(byte[] buffer, u32 len)
            {
                uint i = 24;

                setbitu(buffer, i, 12, 1006); i += 12; /* message no */
                setbitu(buffer, i, 12, staid); i += 12; /* ref station id */
                setbitu(buffer, i, 6, 0); i += 6; /* itrf realization year */
                setbitu(buffer, i, 1, 1); i += 1; /* gps indicator */
                setbitu(buffer, i, 1, 1); i += 1; /* glonass indicator */
                setbitu(buffer, i, 1, 0); i += 1; /* galileo indicator */
                setbitu(buffer, i, 1, 0); i += 1; /* ref station indicator */
                set38bits(buffer, i, ecefposition[0] / 0.0001); i += 38; /* antenna ref point ecef-x */
                setbitu(buffer, i, 1, 1); i += 1; /* oscillator indicator */
                setbitu(buffer, i, 1, 0); i += 1; /* reserved */
                set38bits(buffer, i, ecefposition[1] / 0.0001); i += 38; /* antenna ref point ecef-y */
                setbitu(buffer, i, 2, 0); i += 2; /* quarter cycle indicator */
                set38bits(buffer, i, ecefposition[2] / 0.0001); i += 38; /* antenna ref point ecef-z */


                u16 new_anth;
                string cleanAmount;

                if (antennaName2_ini == "Antenna Bottom")
                {
                    float x = (float)(float.Parse(antennaHeight_ini, CultureInfo.InvariantCulture.NumberFormat) + 0.121);
                    cleanAmount = antennaHeight_ini.Replace(".", string.Empty);
                    new_anth = Convert.ToUInt16(cleanAmount);
                }
                else
                {
                    cleanAmount = antennaHeight_ini.Replace(".", string.Empty);
                    new_anth = Convert.ToUInt16(cleanAmount);
                }

                setbitu(buffer, i, 16, new_anth); i += 16; /* antenna height */


                //new CRC
                u32 new_crc =  crc24q(buffer, len, 0);
                setbitu(buffer, i, 24, new_crc); i += 24;

                return i;
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
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

            // Set color GridView
            //dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.Blue;
            //dataGridView1.EnableHeadersVisualStyles = false;

            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            Read_OutputFolder();
            label2.Text = outputpath;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void AppendAllBytes(string path, byte[] bytes , u32 len)
        {
            int i = Convert.ToInt32(len);

            using (var stream = new FileStream(path, FileMode.Append))
            {
                stream.Write(bytes, 0, i);
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        void ConvertToRinex(string filepath, string outputpathOBS, string outputNAV, string height)
        {
            string strCmdText;


            if (recerveSN_ini == "") recerveSN_ini = " ";
            if (serialNo_ini == "") serialNo_ini = " ";

            if (inputType_ini == "RTCM")
            {
                strCmdText = @" -ti " + dataInterval_ini + " -r rtcm3 -v 3.02 -hr " + @"""" + recerveSN_ini + @"""" + @"/""" + "P2 GNSS" + @"/""" + " -hm " + pointName_ini + " -hn " + pointName_ini + @" -ht RG-CORS -ho KKFC/KKFC -hd " + height + @" -ha " + @"""" + serialNo_ini + @"""" + @"/""" + antennaName1_ini + @""" -od -os -o " + outputpathOBS + " -n " + outputNAV + " " + filepath;
                System.Diagnostics.Process.Start("convbin.exe", strCmdText);
            }
            else
            {

                strCmdText = @" -ti " + dataInterval_ini + " -r binex -v 3.02 -hr " + @"""" + recerveSN_ini + @"""" + @"/""" + "P2 GNSS" + @"/""" + " -hm " + pointName_ini + " -hn " + pointName_ini + @" -ht RG-CORS -ho KKFC/KKFC -hd " + height + @" -ha " + @"""" + serialNo_ini + @"""" + @"/""" + antennaName1_ini + @""" -od -os -o " + outputpathOBS + " -n " + outputNAV + " " + filepath;
                System.Diagnostics.Process.Start("convbin.exe", strCmdText);
            }            
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /*void RTCM555(string[] _iniFile, string[] _rtcmFile)
        {
            label1.Text = _rtcmFile[0];


            string ff = _rtcmFile[0].Substring(0, _rtcmFile[0].Length - 5);
            string newFileName = ff + "_new.rtcm";

            string outputOBS = ff + "_new.obs";
            string outputNAV = ff + "_new.nav";

            if (File.Exists(newFileName))
            {
                File.Delete(newFileName);
            }

            if (File.Exists(outputOBS))
            {
                File.Delete(outputOBS);
            }

            if (File.Exists(outputNAV))
            {
                File.Delete(outputNAV);
            }



            //string filename = Path.Combine(dir, "Config.ini");

            read_ini_config(_iniFile[0]);


            byte[] data = File.ReadAllBytes(_rtcmFile[0]);

            int step = 0;

            byte[] packet = new u8[1024 * 4];
            u32 len = 0;
            int a = 0;
            rtcmpreamble pre;

            for (Int64 i = 0; i < data.Length; i++)
            {
                switch (step)
                {
                    case 0:
                        Array.Clear(packet, 0, packet.Length);

                        if (data[i] == RTCM3PREAMB)     //D3
                        {
                            step++;
                            packet[0] = data[i];
                        }
                        break;
                    case 1:
                        packet[1] = data[i];
                        step++;
                        break;
                    case 2:
                        packet[2] = data[i];
                        step++;
                        pre = new rtcmpreamble();
                        pre.Read(packet);
                        len = pre.length;
                        a = 0;
                        // reset on oversize packet
                        if (len > packet.Length)
                            step = 0;
                        break;
                    case 3:
                        if (a < (len))
                        {
                            packet[a + 3] = data[i];
                            a++;
                        }
                        else
                        {
                            step++;
                            goto case 4;
                        }
                        break;
                    case 4:
                        packet[len + 3] = data[i];

                        step++;
                        break;
                    case 5:
                        packet[len + 3 + 1] = data[i];
                        step++;
                        break;
                    case 6:
                        packet[len + 3 + 2] = data[i];

                        len = len + 3;

                        u32 crc = crc24q(packet, len, 0);
                        u32 crcpacket = getbitu(packet, len * 8, 24);

                        //Console.WriteLine(crc.ToString("X") + " " + crcpacket.ToString("X"));

                        if (crc == crcpacket)
                        {
                            rtcmheader head = new rtcmheader();
                            head.Read(packet);

                            if (head.messageno == 1006)
                            {
                                type1006 tp = new type1006();

                                tp.Read(packet);

                                //string res = ByteArrayToString(packet);

                                tp.Write(packet, len);

                                //string new_res = ByteArrayToString(packet);                                   
                            }

                            // New file (new Antenna Height)
                            AppendAllBytes(newFileName, packet, len + 3);


                            // Input 'Binex'
                            //convbin.exe - ti 30 - r binex - hm PointName - hn PointName - ht RG - CORS - ho KKFC / KKFC - ha "Antenna No." / "C220GR2" - od - os - o OBS.obs - n NAVFILE.nav AMA12080.BNX

                            // Input 'RTCM3'
                            //convbin.exe - ti 30 - r rtcm3 - hm PointName - hn PointName - ht RG - CORS - ho KKFC / KKFC - ha "Antenna No." / "C220GR2" - od - os - o OBS.obs - n NAVFILE.nav test.rtcm

                        }


                        step = 0;
                        break;
                }
            }

            ConvertRTCM2Rinex(newFileName, outputOBS, outputNAV);
            label2.Text = outputOBS;
        }*/
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        void ToProcess(string[] _file, string[] _iniFile, string output, string height)
        {
            //label1.Text = _file[0];

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

            ConvertToRinex(_file[0], outputOBS, outputNAV, height);

            label2.Text = outputOBS;
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void button3_Click(object sender, EventArgs e)
        {

            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open .kdt File";
            theDialog.Filter = "files|*.kdt";
            //theDialog.InitialDirectory = @"C:\"; 
            theDialog.InitialDirectory = comboBox1.Text + @"Android\data\com.dct.rgcontroller\files";
            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                if (label2.Text != "")
                {
                    try
                    {
                        kdtfile = theDialog.FileName;

                        label1.Text = kdtfile; 

                        //extractPath = kdtfile.Substring(0, kdtfile.Length - 4);       // remove .kdt

                        string f = kdtfile.Substring(0, kdtfile.Length - 4);       // remove .kdt

                        filenameNoKDT = f.Split("\\").Last();

                        extractPath = @"C:\" + filenameNoKDT;

                        if (Directory.Exists(extractPath))
                        {
                            Directory.Delete(extractPath, true);
                        }
                        else
                        {
                            //Unzip
                            //System.IO.Compression.ZipFile.ExtractToDirectory(kdtfile, extractPath);

                            
                            extractFile.RunWorkerAsync();

                            //try
                            //{
                            //    string[] _iniFile = Directory.GetFiles(extractPath, "*.ini");

                            //    read_ini_config(_iniFile[0]);


                            //    label1.Text = extractPath + ".kdt";


                            //    if (antennaName2_ini == "Antenna Bottom")
                            //    {
                            //        float x = (float)(float.Parse(antennaHeight_ini, CultureInfo.InvariantCulture.NumberFormat) + 0.121);
                            //        antenna_H = x.ToString();
                            //    }
                            //    else
                            //    {
                            //        antenna_H = antennaHeight_ini;
                            //    }



                            //    if (inputType_ini == "RTCM")// RTCM to RINEX
                            //    {
                            //        string[] _rtcmFile = Directory.GetFiles(extractPath, "*.rtcm");
                            //        //ToProcess(_rtcmFile, _iniFile, extractPath, antenna_H);
                            //        ToProcess(_rtcmFile, _iniFile, outputpath + @"\" + filenameNoKDT, antenna_H);
                            //    }
                            //    else// BINEX to RINEX
                            //    {
                            //        string[] _binexFile = Directory.GetFiles(extractPath, "*.BNX");
                            //        string filetype = _binexFile[0].Substring(_binexFile[0].Length - 6);
                            //        //ToProcess(_binexFile, _iniFile, extractPath, antenna_H);
                            //        ToProcess(_binexFile, _iniFile, outputpath + @"\" + filenameNoKDT, antenna_H);
                            //    }
                            //}
                            //catch (Exception ex)
                            //{
                            //    DialogResult res = MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            //}
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
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void timer1_Tick(object sender, EventArgs e)
        {
            switch (step)
            {
                case 0:
                    if (IsExistProcess("convbin"))
                    {
                        step = 1;
                    }
                    break;
                case 1:
                    if (!IsExistProcess("convbin")) //closed
                    {
                        try
                        {

                            Directory.Delete(extractPath, true);

                            //string fn = outputpath + @"\" + filenameNoKDT + ".obs";
                            //replaceTextfile(fn, "TRIMBLE BD990", "P2 GNSS      ");
                            //replaceTextfile(fn, "TRIMBLE BD990       5.41,24/APR/2019", "P2 GNSS");


                            //Move .kdt to Output Folder
                            string destination = outputpath + @"\" + filenameNoKDT + ".kdt";
                            File.Move(kdtfile, destination);

                            DialogResult res = MessageBox.Show("Done", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            label2.Text = outputpath;
                            step = 0;
                        }
                        catch { }
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
            //button1.PerformClick();
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


        private static string formatFileNumberForSort(string inVal)
        {
            int o;
            if (int.TryParse(Path.GetFileName(inVal), out o))
            {
                Console.WriteLine(string.Format("{0:0000000000}", o));
                return string.Format("{0:0000000000}", o);
            }
            else
                return inVal;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                dataGridView1.Rows.Clear();
                dataGridView1.Refresh();

                string usbdrive = comboBox1.Text + @"Android\data\com.digicon.rtcm3\files";
                string[] files = Directory.GetFiles(usbdrive, "*.kdt").OrderByDescending(f => f.Length).ToArray();

                string[] fs = new string[files.Length];

                // Get File Size 
                for (int i = 0; i < files.Length; i++)
                {

                    FileInfo info = new FileInfo(files[i]);
                    fs[i] = info.FormatBytes();
                }

                String[] fn = new String[files.Length];

                // Get Filename only
                for (int i = 0; i < files.Length; i++)
                {
                    fn[i] = files[i].Split("\\").Last();
                }

                //--------------------------------------------------------------------------------------------------

                for (int i = 0; i < files.Length; i++)
                {
                    dataGridView1.Rows.Add();
                    dataGridView1.AllowUserToAddRows = false;
                    dataGridView1.AllowUserToDeleteRows = false;

                    dataGridView1.Rows[i].Cells[0].Value = fn[i];
                    dataGridView1.Rows[i].Cells[1].Value = fs[i];
                }
            }
            catch(Exception ex)
            {
                dataGridView1.Rows.Clear();
                dataGridView1.Refresh();
                MessageBox.Show("Wrong USB Drive", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            // Show Line Number
            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                // right alignment might actually make more sense for numbers
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, this.Font, SystemBrushes.ControlText, headerBounds, centerFormat);
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
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Run")
            {
                if (label2.Text != "")
                {
                    try
                    {

                        DataGridViewRow row = this.dataGridView1.Rows[e.RowIndex];
                        string file_ = row.Cells[0].Value.ToString();
                        string filename = comboBox1.Text + @"Android\data\com.digicon.rtcm3\files\" + file_;

                        extractPath = filename.Substring(0, filename.Length - 4);

                        filenameNoKDT = file_.Substring(0, file_.Length - 4);


                        if (Directory.Exists(extractPath))
                        {
                            Directory.Delete(extractPath, true);
                        }
                        else
                        {
                            System.IO.Compression.ZipFile.ExtractToDirectory(filename, extractPath);




                            try
                            {
                                string[] _iniFile = Directory.GetFiles(extractPath, "*.ini");

                                read_ini_config(_iniFile[0]);

                                label1.Text = extractPath + ".kdt";


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
                                    string[] _rtcmFile = Directory.GetFiles(extractPath, "*.rtcm");
                                    ToProcess(_rtcmFile, _iniFile, outputpath + @"\" + filenameNoKDT, antenna_H);
                                }
                                else// BINEX to RINEX
                                {
                                    string[] _binexFile = Directory.GetFiles(extractPath, "*.BNX");
                                    string filetype = _binexFile[0].Substring(_binexFile[0].Length - 6);
                                    ToProcess(_binexFile, _iniFile, outputpath + @"\" + filenameNoKDT, antenna_H);
                                }
                            }
                            catch (Exception ex)
                            {
                                DialogResult res = MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
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
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        private void FileDecrypt(string inputFileName, string outputFileName, string password)
        {
            byte[] passwords = Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[32];
            using (FileStream fsCrypt = new FileStream(inputFileName, FileMode.Open))
            {
                fsCrypt.Read(salt, 0, salt.Length);
                RijndaelManaged AES = new RijndaelManaged();
                AES.KeySize = 256;//aes 256 bit encryption c#
                AES.BlockSize = 128;//aes 128 bit encryption c#
                var key = new Rfc2898DeriveBytes(passwords, salt, 50000);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);
                AES.Padding = PaddingMode.PKCS7;
                AES.Mode = CipherMode.CFB;
                using (CryptoStream cryptoStream = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (FileStream fsOut = new FileStream(outputFileName, FileMode.Create))
                    {
                        int read;
                        byte[] buffer = new byte[1048576];
                        while ((read = cryptoStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fsOut.Write(buffer, 0, read);
                        }
                    }
                }
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------

        private void button4_Click(object sender, EventArgs e)
        {
            //string password = "my cool password";
            //GCHandle gch = GCHandle.Alloc(password, GCHandleType.Pinned);
            //FileDecrypt(@"C:\Output\0001-220905102558.zip.aes", @"C:\Output\0001-220905102558.zip", password);
            //ZeroMemory(gch.AddrOfPinnedObject(), password.Length * 2);
            //gch.Free();




            ExpressEncription.AESEncription.AES_Decrypt(@"C:\Output\0001-220905102558.zip.aes", "my cool password");
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        void replaceTextfile(string file, string old_, string new_)
        {
            string text = File.ReadAllText(file);
            text = text.Replace(old_, new_);
            File.WriteAllText(file, text);
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        private void button5_Click(object sender, EventArgs e)
        {
            extractPath = @"E:\Android\data\com.dct.rgcontroller\files\kaitekitest1-220831030000";          // for test only , don't zip no .kdt folder only

            filenameNoKDT = extractPath.Split("\\").Last();


                try
                {
                    string[] _iniFile = Directory.GetFiles(extractPath, "*.ini");

                    read_ini_config(_iniFile[0]);


                    label1.Text = extractPath + ".kdt";


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
                        string[] _rtcmFile = Directory.GetFiles(extractPath, "*.rtcm");
                        //ToProcess(_rtcmFile, _iniFile, extractPath, antenna_H);
                        ToProcess(_rtcmFile, _iniFile, outputpath + @"\" + filenameNoKDT, antenna_H);
                    }
                    else// BINEX to RINEX
                    {
                        string[] _binexFile = Directory.GetFiles(extractPath, "*.BNX");
                        string filetype = _binexFile[0].Substring(_binexFile[0].Length - 6);
                        //ToProcess(_binexFile, _iniFile, extractPath, antenna_H);
                        ToProcess(_binexFile, _iniFile, outputpath + @"\" + filenameNoKDT, antenna_H);
                    }
                }
                catch (Exception ex)
                {
                    DialogResult res = MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }                       
            
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        private void button6_Click(object sender, EventArgs e)
        {
            string text = "";

            string path = @"E:\test1234\2022-08-25_14-13-44.obs";
            //Read all text
            Application.DoEvents();
            progressBar1.Value = 0;
            using (StreamReader sr = new StreamReader(path))
            {
                Stream baseStream = sr.BaseStream;
                long length = baseStream.Length;
                string line;
               
                while ((line = sr.ReadLine()) != null)
                {
                    text += line;
                    progressBar1.Value = Convert.ToInt32((double)baseStream.Position / length * 100);
                    Application.DoEvents();
                }
                int x = text.Length;
            }

            //Replace , 
            text = text.Replace("TRIMBLE BD990", "P2 GNSS      ");

        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        void replacetextfile_(string path)
        {
            string text = "";

            //Read all text
            Application.DoEvents();
            progressBar1.Value = 0;
            using (StreamReader sr = new StreamReader(path))
            {
                Stream baseStream = sr.BaseStream;
                long length = baseStream.Length;
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    text += line;
                    progressBar1.Value = Convert.ToInt32((double)baseStream.Position / length * 100);
                    Application.DoEvents();
                }
                int x = text.Length;
            }

            //Replace , 
            text = text.Replace("TRIMBLE BD990", "P2 GNSS      ");
        }

    }
}