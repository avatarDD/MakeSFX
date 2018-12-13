using Microsoft.Win32;
using SevenZip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MakeSFX
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<FileSystemObject> fsoList;
        List<string> dlls;
        private string dllsPath = "MakeSFX.Resources";        
        private string tmpFileName = "pack.7z";
        private string RLO = "\u202e";
        private string LRO = "\u202d";
        private string iconFilePath { get; set; }
        private string startPath { get; set; }
        private Regex reFileName = new Regex(@"[^\\]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);                
        private Regex rePath = new Regex(@".*\\", RegexOptions.IgnoreCase | RegexOptions.Compiled);        
        private Regex reExec = new Regex(@"\.(exe|msi|msu|bat|cmd|com|wsf|js|jar|vba|hta)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public MainWindow()
        {
            InitializeComponent();

            fsoList = new List<FileSystemObject>();            

            dlls = new List<string>();
            dlls.Add("7zSD_RU.chm");
            dlls.Add("7zx64.dll");
            dlls.Add("7zx86.dll");            
            dlls.Add("SevenZipSharp.dll");
            dlls.Add("7z.sfx");

            foreach (string dll in dlls)
            {
                CreateFileFromResource(string.Format(@"{0}.{1}", dllsPath, dll), dll);
            }

            MakeCFG();
        }

        private static void CreateFileFromResource(string fullFileNamePath, string FileName)
        {
            Assembly curAsm = Assembly.GetExecutingAssembly();
            using (Stream stm = curAsm.GetManifestResourceStream(fullFileNamePath))
            {
                byte[] assamblyBytes = new byte[stm.Length];
                stm.Read(assamblyBytes, 0, assamblyBytes.Length);
                using (Stream newFile = new FileStream(System.IO.Path.Combine(Directory.GetCurrentDirectory(), FileName), FileMode.OpenOrCreate, FileAccess.Write))
                {
                    newFile.Write(assamblyBytes, 0, assamblyBytes.Length);
                }
            }
        }

        private void dgFiles_Drop(object sender, DragEventArgs e)
        {
            fsoList.Clear();
            string[] items = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (items == null)
                return;

            foreach (string item in items)
            {
                startPath = rePath.Match(item).Value;
                if (Directory.Exists(item))
                {
                    //это папка
                    fsoList.AddRange(GetFilesFromDir(item));
                }
                else
                {
                    //это файл
                    string fsoName = reFileName.Match(item).Value;
                    string fsoPath = startPath;
                    bool exec = (reExec.IsMatch(item)) ? true : false;
                    
                    fsoList.Add(new FileSystemObject()
                    {
                        relativePath = fsoName,
                        absolutePath = fsoPath,
                        executable =exec,                        
                    });
                }
            }
            dgFiles.DataContext = fsoList.OrderBy(a=>a.absolutePath);
            cmbxStart.DataContext = fsoList.Where(a => a.executable).Select(a => new { path = System.IO.Path.Combine(a.absolutePath, a.relativePath) }).Select(a=>a.path).OrderBy(a=>a);
        }

        private List<FileSystemObject> GetFilesFromDir(string path)
        {
            IEnumerable<string> GetFilesFromDir(string dir) =>
                Directory.EnumerateFiles(dir).Concat(
                    Directory.EnumerateDirectories(dir)
                    .SelectMany(subdir => GetFilesFromDir(subdir)));

            List<string> subFiles = GetFilesFromDir(path).ToList();
            List<FileSystemObject> subObjs = new List<FileSystemObject>();

            foreach (string subFile in subFiles)
            {
                string fsoName = subFile.Replace(startPath,"");
                string fsoPath = startPath;
                bool exec = (reExec.IsMatch(subFile)) ? true : false;
                subObjs.Add(new FileSystemObject()
                {
                    relativePath = fsoName,
                    absolutePath = fsoPath,
                    executable = exec,                        
                });
            }
            return subObjs;
        }

        private void cmbxStart_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string startFile = (cmbxStart.SelectedIndex != -1) ? cmbxStart.SelectedValue.ToString().Replace(startPath, "") : "";
            MakeCFG(startFile);
        }

        private void MakeCFG(string startFile = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(";!@Install@!UTF-8!\r\n");
            sb.Append(string.Format("RunProgram=\"{0}\"\r\n", startFile));
            sb.Append("GUIMode=\"2\"\r\n");
            sb.Append("SelfDelete=\"1\"\r\n");
            sb.Append(";!@InstallEnd@!\r\n");
            tbxCFG.Text = sb.ToString();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                btnCFG_Help_Click(this, null);
            }
            if ((Keyboard.Modifiers == ModifierKeys.Control) && (Keyboard.IsKeyDown(Key.R)))
            {
                try
                {
                    Reverse();
                }
                catch (Exception) { }
            }
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            foreach (string dll in dlls)
            {//подчищаем за собой
                try
                {
                    File.Delete(dll);                    
                }
                catch (Exception)
                {
                }
            }
        }

        private void btnMakeSFX_Click(object sender, RoutedEventArgs e)
        {
            if (dgFiles.Items.Count == 0)
                return;

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Сохранение sfx";
            dlg.InitialDirectory = Directory.GetCurrentDirectory();
            dlg.FileName = tbxOutName.Text;
            dlg.DefaultExt = ".exe";
            dlg.Filter = "Исполняемый (.exe)|*.exe";

            if (dlg.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(tbxIcon.Text))
                {
                    IconChanger.ChangeIcon("7z.sfx", tbxIcon.Text);
                }
                MakeExe(dlg.FileName);
                System.Diagnostics.Process.Start("explorer.exe", @"/select, " + dlg.FileName);
            }
        }

        private void MakeExe(string dstFileName)
        {
            Dictionary<string, string> FilesDic = fsoList.ToDictionary(a => a.relativePath, a=>string.Format("{0}{1}", a.absolutePath,a.relativePath));
            CompressByManaged7z(FilesDic, tmpFileName);
            CreateSFX(tmpFileName, dstFileName);
        }

        private void CompressByManaged7z(Dictionary<string,string> dicFiles, string outFile)
        {
            string dll = string.Empty;
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion"))
            {
                if (key != null)
                {
                    Object o = key.GetValue("BuildLabEx");
                    if (o != null)
                    {
                        if ((o as String).ToLower().Contains("amd64"))
                        {                            
                            dll = "7zx64.dll";
                        }
                        else
                        {                            
                            dll = "7zx86.dll";
                        }
                    }
                }
            }
            
            SevenZip.SevenZipCompressor.SetLibraryPath(Path.Combine(Directory.GetCurrentDirectory(), dll));

            SevenZipCompressor szc = new SevenZipCompressor()
            {
                CompressionMethod = CompressionMethod.Lzma2,
                CompressionLevel = CompressionLevel.Ultra,
                CompressionMode = CompressionMode.Create,
                DirectoryStructure = true,
                PreserveDirectoryRoot = false,                
                ArchiveFormat = OutArchiveFormat.SevenZip
            };
            szc.CompressFileDictionary(dicFiles, tmpFileName);            
        }

        private void CreateSFX(string srcFile, string dstFile)
        {
            using (Stream OutFileStream = new FileStream(dstFile, FileMode.Create, FileAccess.Write))
            {

                byte[] sfxMod = File.ReadAllBytes("7z.sfx");
                OutFileStream.Write(sfxMod, 0, sfxMod.Length);

                byte[] sfxConf = Encoding.UTF8.GetBytes(tbxCFG.Text);
                OutFileStream.Write(sfxConf, 0, sfxConf.Length);

                using (Stream arhive7z = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), tmpFileName)))
                {
                    arhive7z.CopyTo(OutFileStream);
                }
            }
            File.Delete(tmpFileName);
        }

        private void btnBrowseIcon_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Выбор иконки";
            dlg.InitialDirectory = Directory.GetCurrentDirectory();
            dlg.DefaultExt = ".ico";
            dlg.Multiselect = false;
            dlg.Filter = "Иконка (.ico)|*.ico|Все файлы (*.*)|*.*";

            if (dlg.ShowDialog() == true)
            {
                tbxIcon.Text = dlg.FileName;
            }
        }

        private void btnCFG_Help_Click(object sender, RoutedEventArgs e)
        {
            string helpFileName = "7zSD_RU.chm";
            Process.Start("hh.exe", "mk:@MSITStore:" + helpFileName + "::/examples.html");
        }

        private void Reverse()
        {
            if (tbxOutName.Text.Length == 0)
                return;

            string outFileName = "";
            string[] pth = tbxOutName.Text.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            outFileName += LRO;
            for (int i = 0; i < pth.Length - 2; i++)
            {
                outFileName += pth[i];
            }
            char[] secExt = pth[pth.Length - 2].ToCharArray();
            Array.Reverse(secExt);
            outFileName += "." + RLO + Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(secExt)) + "." + pth[pth.Length - 1];
            tbxOutName.Text = outFileName;
        }
    }
}
