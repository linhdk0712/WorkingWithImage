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

namespace WorkingWithImage
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = folderBrowserDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    txtFilePath.Text = folderBrowserDialog1.SelectedPath.ToString();
                }
            }
            catch (Exception ex)
            {
                rtbResult.Text = ex.Message + ex.StackTrace;
            }
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            rtbResult.Text = "";
            try
            {
                if (string.IsNullOrEmpty(txtFilePath.Text.Trim()))
                {
                    rtbResult.Text += "Hãy chọn thư mục ảnh cần xử lý.\n";
                    return;
                }
                string rootFolder = txtFilePath.Text.Trim();
                rtbResult.Text += "Kiểm tra các thư mục cần thiết.\n";
                // Check folder exits 
                string[] folders = Directory.GetDirectories(rootFolder);
                //Check name,rename,create new folder

                // Creat folder
                if (folders.Length <= 0)
                {
                    rtbResult.Text += "Chưa tồn tại các thư mục cần thiết.Bắt đầu tạo các thư mục này.\n";
                    Directory.CreateDirectory(rootFolder + "\\cat");
                    rtbResult.Text += "Đã tạo thư mục ảnh cắt.\n";
                    Directory.CreateDirectory(rootFolder + "\\goc");
                    rtbResult.Text += "Đã tạo thư mục ảnh gốc.\n";
                    Directory.CreateDirectory(rootFolder + "\\ht");
                    rtbResult.Text += "Đã tạo thư mục ảnh hòa trộn.\n";
                    Directory.CreateDirectory(rootFolder + "\\gt");
                    rtbResult.Text += "Đã tạo thư mục ảnh giới thiệu.\n";
                }
            }
            catch (Exception ex)
            {
                rtbResult.Text += ex.Message + ex.StackTrace;
            }
        }
    }
}
