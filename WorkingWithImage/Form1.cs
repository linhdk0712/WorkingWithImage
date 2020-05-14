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
            try
            {
                Directory.Delete(rootFolder + "\\cat", true);
                Directory.Delete(rootFolder + "\\gt", true);
                Directory.Delete(rootFolder + "\\ht", true);
                Directory.Delete(rootFolder + "\\goc", true);
            }
            catch (Exception)
            {

            }

        }
        private async void btnProcess_Click(object sender, EventArgs e)
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
                        await CreateFoldesWhenSmaller4(rootFolder, numberOfFolder);
                    }
                }
                if (numberOfFolder == 4)
                {
                    await CreateFoldesWhenEqual4(rootFolder);
                }
                // Creat folder
                if (numberOfFolder <= 0)
                {
                    await CreateFolderWhenNotExits(rootFolder);
                }
                rtbResult.Text += "Đã tạo xong các thư mục cần thiết.\n";
                // get all jpg images in folder
                rtbResult.Text += "Bắt đầu thực hiện xử lý ảnh.\n";
                var fileList = Directory.GetFiles(rootFolder, ".").ToList();
                List<string> listWidthGreaterHeight = new List<string>();
                List<string> listWidthSmallerHeight = new List<string>();
                List<string> listImageGoc = new List<string>();
                await Task.Run(() => {
                    foreach (var item in fileList)
                    {
                        var fileExtension = Path.GetExtension(item);
                        var fileName = Path.GetFileName(item).Replace(fileExtension, "");
                        if (fileExtension.Equals(".jpg") || fileExtension.Equals(".JPG"))
                        {
                            if (fileName.ToUpper().StartsWith("GG"))
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
                });
                int numberOfImageGioiThieu = 0;
                int numberOfImageGoc = Convert.ToInt32(GetParameter()["NUMBER_ANH_GOC"].ToString().Trim());
                int numberOfImageCat = Convert.ToInt32(GetParameter()["NUMBER_ANH_CAT"].ToString().Trim());
                int numberOfImageHoaTron = Convert.ToInt32(GetParameter()["NUMBER_ANH_HT"].ToString().Trim());
                if (listWidthSmallerHeight.Count < 40)
                {
                    listWidthSmallerHeight.AddRange(listWidthGreaterHeight);
                }

                // create cat images
                
                await CreateImages(numberOfImageCat, listWidthSmallerHeight, "cat", rootFolder);
                rtbResult.Text += "Đã taọ xong ảnh cắt.\n";
                // create hoa tron images

                await CreateImages(numberOfImageHoaTron, listWidthGreaterHeight, "ht", rootFolder);
               
                fileList.Clear();
                fileList = Directory.GetFiles(rootFolder + "\\ht", ".").ToList();
                if (listWidthGreaterHeight.Count < 30)
                {
                    numberOfImageHoaTron = 30 - listWidthGreaterHeight.Count;
                    listWidthGreaterHeight.AddRange(listWidthGreaterHeight);
                    await CreateMoreImages(fileList, rootFolder, "ht", numberOfImageHoaTron);
                }
                rtbResult.Text += "Đã taọ xong ảnh hòa trộn.\n";
                // create gioi thieu images
                numberOfImageGioiThieu = await CreateGoiThieuImages(rootFolder, listWidthGreaterHeight, numberOfImageGioiThieu);
                rtbResult.Text += "Đã taọ xong ảnh giới thiệu.\n";
                // create goc images
                if (listImageGoc.Count == 0)
                {
                    listImageGoc = listWidthSmallerHeight;
                }
                await CreateImages(numberOfImageGoc, listImageGoc, "goc", rootFolder);
                rtbResult.Text += "Đã taọ xong ảnh gốc.\n";
                // Scan folders and create more images
                // Anh cat
                fileList.Clear();
                fileList = Directory.GetFiles(rootFolder + "\\cat", ".").ToList();
                if (fileList.Count < 80)
                {

                    numberOfImageCat = 80 - fileList.Count;
                    fileList.AddRange(listWidthSmallerHeight);
                    await CreateMoreImages(fileList, rootFolder, "cat", numberOfImageCat);
                    //Rename images
                    fileList.Clear();
                    fileList = Directory.GetFiles(rootFolder + "\\cat", ".").ToList();
                    await RenameImages(fileList, rootFolder, "cat", "Thinh", "Thinh (80)");
                }
                else if (fileList.Count == 80)
                {
                    //Rename images
                    await RenameImages(fileList, rootFolder, "cat", "Thinh", "Thinh (80)");
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
                if (fileList.Count < 20)
                {
                    numberOfImageGoc = 20 - listImageGoc.Count;
                    listImageGoc.AddRange(listImageGoc);
                    await CreateMoreImages(fileList, rootFolder, "goc", numberOfImageGoc);
                }
                fileList.Clear();
                fileList = Directory.GetFiles(rootFolder + "\\goc", ".").ToList();
                await RenameImages(fileList, rootFolder, "goc", "G", "G (20)");
                rtbResult.Text += "Đã đổi xong tên ảnh gốc.\n";
                // Anh hoa tron
                fileList.Clear();
                fileList = Directory.GetFiles(rootFolder + "\\ht", ".").ToList();
                await RenameImages(fileList, rootFolder, "ht", "L2", "L2 (30)");
                rtbResult.Text += "Đã đổi xong tên ảnh hòa trộn.\n";
                // Anh gioi thieu
                fileList.Clear();
                fileList = Directory.GetFiles(rootFolder + "\\gt", ".").ToList();
                await RenameImagesGoiThieu(fileList, rootFolder, "gt", "", "");
                rtbResult.Text += "Đã đổi xong tên ảnh giới thiệu.\n";
                string rootFolderProject = txtPathProject.Text.Trim();
                DirectoryInfo directoryInfoS = new DirectoryInfo(rootFolderProject);
                if (!directoryInfoS.Exists)
                {
                    rtbResult.Text += "Không tồn tại thư mục ảnh project.\n";
                    return;
                }
                //CopyImagesCat();
                //CopyImagesGoc();
                //CopyImagesGioiThieu();
                //CopyImagesHoaTron();
                await CopyImagesCat();
                rtbResult.Text += "Đã copy xong ảnh cắt.\n";
                await CopyImagesGoc();
                rtbResult.Text += "Đã copy xong ảnh gốc.\n";
                await CopyImagesGioiThieu();
                rtbResult.Text += "Đã copy xong ảnh giới thiệu.\n";
                await CopyImagesHoaTron();
                rtbResult.Text += "Đã copy xong ảnh hòa trộn.\n";
                rtbResult.Text += "Đã copy ảnh sang thư mục ảnh Project thành công.\n";
                await LoadProjectFile();
                rtbResult.Text += "Đã tải xong files project.\n";
            }
            catch (Exception ex)
            {
                rtbResult.Text += ex.Message + ex.StackTrace;
            }

        }
        private async Task RenameImagesGoiThieu(List<string> fileList, string rootFolder, string folder, string prefixName, string lastImageName)
        {
            await Task.Run(() =>
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
            });
        }
        private async Task RenameImages(List<string> fileList, string rootFolder, string folder, string prefixName, string lastImageName)
        {
            await Task.Run(() =>
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
            });
        }
        private async Task CreateMoreImages(List<string> fileList, string rootFolder, string folder, int numberImages)
        {
            await Task.Run(() =>
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
            });
        }
        private async Task<int> CreateImages(int numberImages, List<string> listImages, string folder, string rootFolder)
        {
            int number = 0;
            await Task.Run(() =>
            {
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
            });
            return number;
        }

        private async Task<int> CreateGoiThieuImages(string rootFolder, List<string> listWidthGreaterHeight, int numberOfImageGioiThieu)
        {
            await Task.Run(() =>
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
            });

            return numberOfImageGioiThieu;
        }

        private async Task CreateFolderWhenNotExits(string rootFolder)
        {
            await Task.Run(() =>
            {

                Directory.CreateDirectory(rootFolder + "\\cat");
                Directory.CreateDirectory(rootFolder + "\\goc");
                Directory.CreateDirectory(rootFolder + "\\ht");
                Directory.CreateDirectory(rootFolder + "\\gt");

            });
        }

        private async Task CreateFoldesWhenEqual4(string rootFolder)
        {
            await Task.Run(() =>
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
            });

        }

        private async Task CreateFoldesWhenSmaller4(string rootFolder, int numberOfFolder)
        {
            await Task.Run(() =>
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
            });
        }

        private async void btnRunProject_Click(object sender, EventArgs e)
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
                await CopyImagesCat();
                await CopyImagesGoc();
                await CopyImagesGioiThieu();
                await CopyImagesHoaTron();
                await LoadProjectFile();
            }
            catch (Exception ex)
            {
                rtbResult.Text = ex.Message + ex.StackTrace;
            }
        }
        private async Task CopyImagesGoc()
        {
            Thread.Sleep(500);
            root = txtFilePath.Text.Trim();
            rootProject = txtPathProject.Text.Trim();
            var fileList = Directory.GetFiles(root + "\\goc", ".").ToList();
            int count = 0;
            await Task.Run(() =>
            {
                foreach (var item in fileList)
                {
                    var fileName = Path.GetFileName(item);
                    // Create a FileInfo  
                    System.IO.FileInfo fi = new System.IO.FileInfo(item);
                    string des = rootProject + "\\goc" + "\\" + fileName;
                    fi.CopyTo(des, true);
                    count++;
                }
            });

        }
        private async Task CopyImagesCat()
        {
            Thread.Sleep(500);
            root = txtFilePath.Text.Trim();
            rootProject = txtPathProject.Text.Trim();
            var fileList = Directory.GetFiles(root + "\\cat", ".").ToList();
            int count = 0;
            await Task.Run(() =>
            {
                foreach (var item in fileList)
                {
                    var fileName = Path.GetFileName(item);
                    // Create a FileInfo  
                    System.IO.FileInfo fi = new System.IO.FileInfo(item);
                    string des = rootProject + "\\cat" + "\\" + fileName;
                    fi.CopyTo(des, true);
                    count++;

                }
            });

        }
        private async Task CopyImagesHoaTron()
        {
            Thread.Sleep(500);
            root = txtFilePath.Text.Trim();
            rootProject = txtPathProject.Text.Trim();
            var fileList = Directory.GetFiles(root + "\\ht", ".").ToList();
            int count = 0;
            await Task.Run(() =>
            {
                foreach (var item in fileList)
                {
                    var fileName = Path.GetFileName(item);
                    // Create a FileInfo  
                    System.IO.FileInfo fi = new System.IO.FileInfo(item);
                    string des = rootProject + "\\ht" + "\\" + fileName;
                    fi.CopyTo(des, true);
                    count++;
                }
            });

        }
        private async Task CopyImagesGioiThieu()
        {
            Thread.Sleep(500);
            root = txtFilePath.Text.Trim();
            rootProject = txtPathProject.Text.Trim();
            var fileList = Directory.GetFiles(root + "\\gt", ".").ToList();
            int count = 0;
            await Task.Run(() =>
            {
                foreach (var item in fileList)
                {
                    var fileName = Path.GetFileName(item);
                    // Create a FileInfo  
                    System.IO.FileInfo fi = new System.IO.FileInfo(item);
                    string des = rootProject + "\\gt" + "\\" + fileName;
                    fi.CopyTo(des, true);
                    count++;
                }
            });

        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }
        private async Task LoadProjectFile()
        {

            dataGridView1.DataSource = null;
            var fileList = Directory.GetFiles(txtPathProject.Text.Trim(), ".").ToList();
            List<ProjectProperties> listFile = new List<ProjectProperties>();
            await Task.Run(() =>
            {
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
            });
            dataGridView1.DataSource = listFile;


        }
        private void btnRun_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count <= 0)
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
