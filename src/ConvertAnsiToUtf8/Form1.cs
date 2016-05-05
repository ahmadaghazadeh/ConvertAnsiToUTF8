using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConvertAnsiToUtf8
{
    
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        
        private void button1_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            progressBar1.Value = 0;
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            
            DialogResult result = fbd.ShowDialog();


            if (!string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
     
                String[] files = System.IO.Directory.GetFiles(fbd.SelectedPath, "*."+ ebExt.Text.Trim().ToLower(), System.IO.SearchOption.AllDirectories);

                progressBar1.Maximum = files.Length;
                int index = 1;
                foreach (var file in files)
                {
                       
                        byte[] ansiBytes;
                        using (var reader = new System.IO.StreamReader(file, true))
                        {
                                 ansiBytes = File.ReadAllBytes(file);
                        }
                        if (!IsUTF8Bytes(ansiBytes))
                        {
                            System.IO.File.Move(file, file + "_");
                            var utf8String = Encoding.Default.GetString(ansiBytes);
                            File.WriteAllText(file, utf8String);
                        }
                
                    Application.DoEvents();
                    progressBar1.Value = index++;
                }

            }

            btnStart.Enabled = true;
        }




        public static System.Text.Encoding GetType(FileStream fs)
        {
            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //with BOM
            Encoding reVal = Encoding.Default;

            BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default);
            int i;
            int.TryParse(fs.Length.ToString(), out i);
            byte[] ss = r.ReadBytes(i);
            if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
            {
                reVal = Encoding.UTF8;
            }
            else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
            {
                reVal = Encoding.BigEndianUnicode;
            }
            else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
            {
                reVal = Encoding.Unicode;
            }
            r.Close();
            return reVal;

        }

        private static bool IsUTF8Bytes(byte[] data)
        {
            int charByteCounter = 1;
            byte curByte;
            for (int i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        while (((curByte <<= 1) & 0x80) != 0)
                        {
                            charByteCounter++;
                        }

                        if (charByteCounter == 1 || charByteCounter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1)
            {
                throw new Exception("Error byte format");
            }
            return true;
        }

    }

}
