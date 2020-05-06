using System;
using System.Collections;
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
            rtbResult.Text = "Start\n";
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
                int numberOfFolder = folders.Length;
                //Check name,rename,create new folder
                if (numberOfFolder > 0)
                {
                    //If number of folder < 4 create new folder
                    if (numberOfFolder < 4)
                    {
                        while (numberOfFolder < 4)
                        {
                            Directory.CreateDirectory(rootFolder + "\\xxxx_" + numberOfFolder);
                            numberOfFolder++;
                        }
                        folders = Directory.GetDirectories(rootFolder);
                        // Rename folders                

                        var cat = new DirectoryInfo(folders[0].ToString());
                        if (!cat.Name.Equals("cat"))
                        {
                            cat.MoveTo(rootFolder + "\\cat");
                        }
                        var gt = new DirectoryInfo(folders[1].ToString());
                        if (!gt.Name.Equals("gt"))
                        {
                            gt.MoveTo(rootFolder + "\\gt");
                        }
                        var ht = new DirectoryInfo(folders[2].ToString());
                        if (!ht.Name.Equals("ht"))
                        {
                            ht.MoveTo(rootFolder + "\\ht");
                        }
                        var goc = new DirectoryInfo(folders[3].ToString());
                        if (!goc.Name.Equals("goc"))
                        {
                            goc.MoveTo(rootFolder + "\\goc");
                        }
                    }


                }
                if (numberOfFolder == 4)
                {
                    folders = Directory.GetDirectories(rootFolder);
                    List<string> listFolders = new List<string>();
                    foreach (var item in folders)
                    {
                        var dr = new DirectoryInfo(item);
                        listFolders.Add(dr.Name);
                    }
                    // Rename folders   
                    if (!listFolders.Contains("cat"))
                    {
                        var cat = new DirectoryInfo(folders[0].ToString());
                        if (!cat.Name.Equals("cat"))
                        {
                            cat.MoveTo(rootFolder + "\\cat");
                        }
                    }
                    if (!listFolders.Contains("goc"))
                    {
                        var goc = new DirectoryInfo(folders[3].ToString());
                        if (!goc.Name.Equals("goc"))
                        {
                            goc.MoveTo(rootFolder + "\\goc");
                        }
                    }
                    if (!listFolders.Contains("gt"))
                    {
                        var gt = new DirectoryInfo(folders[1].ToString());
                        if (!gt.Name.Equals("gt"))
                        {
                            gt.MoveTo(rootFolder + "\\gt");
                        }
                    }

                    if (!listFolders.Contains("ht"))
                    {
                        var ht = new DirectoryInfo(folders[2].ToString());
                        if (!ht.Name.Equals("ht"))
                        {
                            ht.MoveTo(rootFolder + "\\ht");
                        }

                    }

                }
                // Creat folder
                if (numberOfFolder <= 0)
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
                rtbResult.Text += "Đã tạo xong các thư mục cần thiết.\n";
                // get all jpg images in folder
                rtbResult.Text += "Bắt đầu thực hiện xử lý ảnh.\n";
                var fileList = Directory.GetFiles(rootFolder, ".").ToList();
                List<string> listWidthGreaterHeight = new List<string>();
                List<string> listWidthSmallerHeight = new List<string>();
                List<string> listImageGoc = new List<string>();
                foreach (var item in fileList)
                {
                    var fileExtension = Path.GetExtension(item);
                    var fileName = Path.GetFileName(item).Replace(fileExtension,"");
                    if (fileExtension.Equals(".jpg") || fileExtension.Equals(".JPG"))
                    {
                        if (fileName.ToUpper().Contains("G"))
                        {
                            listImageGoc.Add(item);
                        }
                        else
                        {
                            Bitmap bitmap = new Bitmap(item);
                            var w = bitmap.PhysicalDimension.Width;
                            var h = bitmap.PhysicalDimension.Height;
                            if (w < h)
                            {
                                listWidthSmallerHeight.Add(item);
                            }
                            else
                            {
                                listWidthGreaterHeight.Add(item);
                            }
                            bitmap.Dispose();
                        }
                    }
                }
                int numberOfImageGioiThieu = 0;
                int numberOfImageGoc = 0;
                int numberOfImageCat = 0;
                int numberOfImageHoaTron = 0;
                if (listWidthSmallerHeight.Count < 40)
                {
                    listWidthSmallerHeight.AddRange(listWidthGreaterHeight);
                }
                while (numberOfImageGioiThieu < 2)
                {
                    Random rnd = new Random();
                    int r = rnd.Next(listWidthGreaterHeight.Count);
                    var fileName = Path.GetFileName(listWidthGreaterHeight[r]);
                    // Create a FileInfo  
                    System.IO.FileInfo fi = new System.IO.FileInfo(listWidthGreaterHeight[r]);
                    string des = rootFolder + "\\gt" + "\\" + fileName;
                    fi.CopyTo(des);
                    numberOfImageGioiThieu++;
                }
                foreach (var item in listWidthSmallerHeight)
                {
                    var fileName = Path.GetFileName(item);
                    // Create a FileInfo  
                    System.IO.FileInfo fi = new System.IO.FileInfo(item);
                    string des = rootFolder + "\\cat" + "\\" + fileName;
                    fi.CopyTo(des);
                    numberOfImageCat++;
                    if (numberOfImageCat == 80)
                    {
                        break;
                    }
                }
                foreach (var item in listWidthGreaterHeight)
                {
                    var fileName = Path.GetFileName(item);
                    // Create a FileInfo  
                    System.IO.FileInfo fi = new System.IO.FileInfo(item);
                    string des = rootFolder + "\\ht" + "\\" + fileName;
                    fi.CopyTo(des);
                    numberOfImageHoaTron++;
                    if (numberOfImageHoaTron == 30)
                    {
                        break;
                    }
                }
                foreach (var item in listImageGoc)
                {
                    var fileName = Path.GetFileName(item);
                    // Create a FileInfo  
                    System.IO.FileInfo fi = new System.IO.FileInfo(item);
                    string des = rootFolder + "\\goc" + "\\" + fileName;
                    fi.CopyTo(des);
                    numberOfImageGoc++;
                    if (numberOfImageGoc == 20)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                rtbResult.Text += ex.Message + ex.StackTrace;
            }
        }
    }
}
