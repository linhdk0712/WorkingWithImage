using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WorkingWithImage
{
    public partial class Form1 : Form
    {
        private string root = "";
        private string rootProject = "";
        static object syncObj = new object();
        public Form1()
        {
            InitializeComponent();
            txtPathProject.Text = GetParameter()["PATH_PROJECT"].ToString();

        }
        private Hashtable GetParameter()
        {
            Hashtable hashtable = new Hashtable();
            hashtable.Add("PATH_PROJECT", System.Configuration.ConfigurationManager.AppSettings["PATH_PROJECT"].ToString().Trim());
            hashtable.Add("NUMBER_ANH_CAT", System.Configuration.ConfigurationManager.AppSettings["NUMBER_ANH_CAT"].ToString().Trim());
            hashtable.Add("NUMBER_ANH_GOC", System.Configuration.ConfigurationManager.AppSettings["NUMBER_ANH_GOC"].ToString().Trim());
            hashtable.Add("NUMBER_ANH_HT", System.Configuration.ConfigurationManager.AppSettings["NUMBER_ANH_HT"].ToString().Trim());
            hashtable.Add("NUMBER_ANH_GT", System.Configuration.ConfigurationManager.AppSettings["NUMBER_ANH_GT"].ToString().Trim());
            return hashtable;
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
        private void RemoveImages()
        {

            string rootFolder = txtFilePath.Text.Trim();
            Directory.Delete(rootFolder + "\\cat", true);
            Directory.Delete(rootFolder + "\\gt", true);
            Directory.Delete(rootFolder + "\\ht", true);
            Directory.Delete(rootFolder + "\\goc", true);

        }
        private void btnProcess_Click(object sender, EventArgs e)
        {
           
            timer1.Start();
            rtbResult.Text = "";
            rtbResult.Text += "Start\n";
            RemoveImages();
            try
            {
                if (string.IsNullOrEmpty(txtFilePath.Text.Trim()))
                {
                    rtbResult.Text += "Hãy chọn thư mục ảnh cần xử lý.\n";
                    return;
                }
                string rootFolder = txtFilePath.Text.Trim();
                DirectoryInfo directoryInfo = new DirectoryInfo(rootFolder);
                if (!directoryInfo.Exists)
                {
                    rtbResult.Text += "Không tồn tại thư mục ảnh của khách hàng.\n";
                }
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
                        CreateFoldesWhenSmaller4(rootFolder, numberOfFolder);
                    }
                }
                if (numberOfFolder == 4)
                {
                    CreateFoldesWhenEqual4(rootFolder);
                }
                // Creat folder
                if (numberOfFolder <= 0)
                {
                    CreateFolderWhenNotExits(rootFolder);
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
                    var fileName = Path.GetFileName(item).Replace(fileExtension, "");
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
                int numberOfImageGoc = 20;
                int numberOfImageCat = 80;
                int numberOfImageHoaTron = 30;
                if (listWidthSmallerHeight.Count < 40)
                {
                    listWidthSmallerHeight.AddRange(listWidthGreaterHeight);
                }

                // create cat images
                CreateImages(numberOfImageCat, listWidthSmallerHeight, "cat", rootFolder);
                // create hoa tron images

                CreateImages(numberOfImageHoaTron, listWidthGreaterHeight, "ht", rootFolder);
                fileList.Clear();
                fileList = Directory.GetFiles(rootFolder + "\\ht", ".").ToList();
                if (listWidthGreaterHeight.Count < 30)
                {
                    numberOfImageHoaTron = 30 - listWidthGreaterHeight.Count;
                    listWidthGreaterHeight.AddRange(listWidthGreaterHeight);
                    CreateMoreImages(fileList, rootFolder, "ht", numberOfImageHoaTron);
                }
                // create gioi thieu images
                numberOfImageGioiThieu = CreateGoiThieuImages(rootFolder, listWidthGreaterHeight, numberOfImageGioiThieu);
                // create goc images
                if (listImageGoc.Count == 0)
                {
                    listImageGoc = listWidthSmallerHeight;
                }
                CreateImages(numberOfImageGoc, listImageGoc, "goc", rootFolder);
                rtbResult.Text += "Đã xử lý xong.\n";
                // Scan folders and create more images
                // Anh cat
                fileList.Clear();
                fileList = Directory.GetFiles(rootFolder + "\\cat", ".").ToList();
                if (fileList.Count < 80)
                {

                    numberOfImageCat = 80 - fileList.Count;
                    fileList.AddRange(listWidthSmallerHeight);
                    CreateMoreImages(fileList, rootFolder, "cat", numberOfImageCat);
                    //Rename images
                    fileList.Clear();
                    fileList = Directory.GetFiles(rootFolder + "\\cat", ".").ToList();
                    RenameImages(fileList, rootFolder, "cat", "Thinh", "Thinh (80)");
                }
                else if (fileList.Count == 80)
                {
                    //Rename images
                    RenameImages(fileList, rootFolder, "cat", "Thinh", "Thinh (80)");
                }
                else
                {
                    //Delete images
                    //Rename images

                }
                rtbResult.Text += "Đã đổi xong tên ảnh cắt.\n";
                // Anh goc
                fileList.Clear();
                fileList = Directory.GetFiles(rootFolder + "\\goc", ".").ToList();
                RenameImages(fileList, rootFolder, "goc", "G", "G (20)");
                rtbResult.Text += "Đã đổi xong tên ảnh gốc.\n";
                // Anh hoa tron
                fileList.Clear();
                fileList = Directory.GetFiles(rootFolder + "\\ht", ".").ToList();
                RenameImages(fileList, rootFolder, "ht", "L2", "L2 (30)");
                rtbResult.Text += "Đã đổi xong tên ảnh hòa trộn.\n";
                // Anh gioi thieu
                fileList.Clear();
                fileList = Directory.GetFiles(rootFolder + "\\gt", ".").ToList();
                RenameImagesGoiThieu(fileList, rootFolder, "gt", "", "");
                rtbResult.Text += "Đã đổi xong tên ảnh giới thiệu.\n";
                string rootFolderProject = txtPathProject.Text.Trim();
                DirectoryInfo directoryInfoS = new DirectoryInfo(rootFolderProject);
                if (!directoryInfoS.Exists)
                {
                    rtbResult.Text += "Không tồn tại thư mục ảnh project.\n";
                    return;
                }
                CopyImagesCat();
                CopyImagesGoc();
                CopyImagesGioiThieu();
                CopyImagesHoaTron();
                rtbResult.Text += "Đã copy ảnh sang thư mục ảnh Project thành công.\n";
                LoadProjectFile();
            }
            catch (Exception ex)
            {
                rtbResult.Text += ex.Message + ex.StackTrace;
            }
          
        }
        private void RenameImagesGoiThieu(List<string> fileList, string rootFolder, string folder, string prefixName, string lastImageName)
        {
            if (fileList.Count > 2)
            {
                return;
            }
            var fileExtensionCD = Path.GetExtension(fileList[0]);
            var fileExtensionCR = Path.GetExtension(fileList[0]);
            // Create a FileInfo  
            System.IO.FileInfo fiCD = new System.IO.FileInfo(fileList[0]);
            System.IO.FileInfo fiCR = new System.IO.FileInfo(fileList[1]);
            string desCD = rootFolder + "\\" + folder + "\\CD" + fileExtensionCD;
            string desCR = rootFolder + "\\" + folder + "\\CR" + fileExtensionCR;
            fiCD.MoveTo(desCD);
            fiCR.MoveTo(desCR);
        }
        private void RenameImages(List<string> fileList, string rootFolder, string folder, string prefixName, string lastImageName)
        {
            int cout = 1;
            for (int i = 0; i < fileList.Count; i++)
            {
                var fileExtension = Path.GetExtension(fileList[i]);
                var fileName = Path.GetFileName(fileList[i]).Replace(fileExtension, "");
                if (fileName.Equals(prefixName))
                {
                    break;
                }
                // Create a FileInfo  
                System.IO.FileInfo fi = new System.IO.FileInfo(fileList[i]);
                string des = rootFolder + "\\" + folder + "\\" + prefixName + " (" + cout + ")" + fileExtension;
                if ((prefixName + " (" + cout + ")").Equals(lastImageName))
                {
                    des = rootFolder + "\\" + folder + "\\" + prefixName + fileExtension;
                }
                fi.MoveTo(des);
                cout++;
            }
        }
        private void CreateMoreImages(List<string> fileList, string rootFolder, string folder, int numberImages)
        {
            int cout = 0;
            foreach (var item in fileList)
            {
                var fileExtension = Path.GetExtension(item);
                // Create a FileInfo  
                System.IO.FileInfo fi = new System.IO.FileInfo(item);
                string des = rootFolder + "\\" + folder + "\\" + "z_" + cout + fileExtension;
                fi.CopyTo(des, true);
                cout++;
                if (cout == numberImages)
                {
                    break;
                }
            }
        }
        private int CreateImages(int numberImages, List<string> listImages, string folder, string rootFolder)
        {
            int number = 0;
            foreach (var item in listImages)
            {
                var fileName = Path.GetFileName(item);
                // Create a FileInfo  
                System.IO.FileInfo fi = new System.IO.FileInfo(item);
                string des = rootFolder + "\\" + folder + "\\" + fileName;
                fi.CopyTo(des, true);
                number++;
                if (number == numberImages)
                {
                    break;
                }
            }
            return number;
        }

        private int CreateGoiThieuImages(string rootFolder, List<string> listWidthGreaterHeight, int numberOfImageGioiThieu)
        {
            while (numberOfImageGioiThieu < 2)
            {
                Random rnd = new Random();
                int r = rnd.Next(listWidthGreaterHeight.Count);
                var fileName = Path.GetFileName(listWidthGreaterHeight[r]);
                // Create a FileInfo  
                System.IO.FileInfo fi = new System.IO.FileInfo(listWidthGreaterHeight[r]);
                string des = rootFolder + "\\gt" + "\\" + fileName;
                fi.CopyTo(des, true);
                numberOfImageGioiThieu++;
                listWidthGreaterHeight.RemoveAt(r);
            }

            return numberOfImageGioiThieu;
        }

        private void CreateFolderWhenNotExits(string rootFolder)
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

        private void CreateFoldesWhenEqual4(string rootFolder)
        {
            string[] folders = Directory.GetDirectories(rootFolder);
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

        private void CreateFoldesWhenSmaller4(string rootFolder, int numberOfFolder)
        {
            string[] folders;
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

        private void btnRunProject_Click(object sender, EventArgs e)
        {
            try
            {
                string rootFolderProject = txtPathProject.Text.Trim();
                DirectoryInfo directoryInfo = new DirectoryInfo(rootFolderProject);
                if (!directoryInfo.Exists)
                {
                    rtbResult.Text += "Không tồn tại thư mục ảnh project.\n";
                    return;
                }
                CopyImagesCat();
                CopyImagesGoc();
                CopyImagesGioiThieu();
                CopyImagesHoaTron();

            }
            catch (Exception ex)
            {
                rtbResult.Text = ex.Message + ex.StackTrace;
            }
        }
        private void CopyImagesGoc()
        {
            Thread.Sleep(500);
            root = txtFilePath.Text.Trim();
            rootProject = txtPathProject.Text.Trim();
            var fileList = Directory.GetFiles(root + "\\goc", ".").ToList();
            int count = 0;
            foreach (var item in fileList)
            {
                var fileName = Path.GetFileName(item);
                // Create a FileInfo  
                System.IO.FileInfo fi = new System.IO.FileInfo(item);
                string des = rootProject + "\\goc" + "\\" + fileName;
                fi.CopyTo(des, true);
                count++;
            }
            rtbResult.Text += "Đã copy " + count + " ảnh goc thành công.\n";
        }
        private void CopyImagesCat()
        {
            Thread.Sleep(500);
            root = txtFilePath.Text.Trim();
            rootProject = txtPathProject.Text.Trim();
            var fileList = Directory.GetFiles(root + "\\cat", ".").ToList();
            int count = 0;
            foreach (var item in fileList)
            {
                var fileName = Path.GetFileName(item);
                // Create a FileInfo  
                System.IO.FileInfo fi = new System.IO.FileInfo(item);
                string des = rootProject + "\\cat" + "\\" + fileName;
                fi.CopyTo(des, true);
                count++;
                
            }
            rtbResult.Text += "Đã copy "+count+" ảnh cat thành công.\n";
        }
        private void CopyImagesHoaTron()
        {
            Thread.Sleep(500);
            root = txtFilePath.Text.Trim();
            rootProject = txtPathProject.Text.Trim();
            var fileList = Directory.GetFiles(root + "\\ht", ".").ToList();
            int count = 0;
            foreach (var item in fileList)
            {
                var fileName = Path.GetFileName(item);
                // Create a FileInfo  
                System.IO.FileInfo fi = new System.IO.FileInfo(item);
                string des = rootProject + "\\ht" + "\\" + fileName;
                fi.CopyTo(des, true);
                count++;
            }
            rtbResult.Text += "Đã copy " + count + " ảnh hoa tron thành công.\n";
        }
        private void CopyImagesGioiThieu()
        {
            Thread.Sleep(500);
            root = txtFilePath.Text.Trim();
            rootProject = txtPathProject.Text.Trim();
            var fileList = Directory.GetFiles(root + "\\gt", ".").ToList();
            int count = 0;
            foreach (var item in fileList)
            {
                var fileName = Path.GetFileName(item);
                // Create a FileInfo  
                System.IO.FileInfo fi = new System.IO.FileInfo(item);
                string des = rootProject + "\\gt" + "\\" + fileName;
                fi.CopyTo(des, true);
                count++;
            }
            rtbResult.Text += "Đã copy " + count + " ảnh gioi thieu thành công.\n";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
           
        }
        private void LoadProjectFile()
        {
            dataGridView1.DataSource = null;
            var fileList = Directory.GetFiles(txtPathProject.Text.Trim(), ".").ToList();
           
            List<ProjectProperties> listFile = new List<ProjectProperties>();
            foreach (var item in fileList)
            {
                var fileExtension = Path.GetExtension(item);
                var fileName = Path.GetFileName(item);
                if (fileExtension.Equals(".psh"))
                {
                    var project = new ProjectProperties();
                    project.Name = fileName;
                    project.Path = item;
                    listFile.Add(project);
                }
            }
            dataGridView1.DataSource = listFile;
            
        }
        private void btnRun_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count <=0)
            {
                MessageBox.Show("Chưa chọn Project để chạy");
                return;
            }
            try
            {
                string path = dataGridView1.SelectedCells[0].Value.ToString();                
                Process.Start(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
            }
        }
    }
    public class ProjectProperties
    {
        public ProjectProperties()
        {

        }
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
