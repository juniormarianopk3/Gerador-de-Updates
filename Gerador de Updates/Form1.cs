using System;
using System.IO;
using System.Windows.Forms;
using System.Security.Cryptography;
using Ionic.Zip;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gerador_de_Updates.Utils;
using System.Text.Json;

namespace Gerador_de_Updates
{
    public partial class Form1 : Form
    {


        public Form1()
        {
            InitializeComponent();
            
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            
            if (folderBrowserDialog1.ShowDialog().Equals(DialogResult.OK))
            {
                txtClientFolder.Text = folderBrowserDialog1.SelectedPath;
                if (Directory.Exists(folderBrowserDialog1.SelectedPath + "\\Updates") && File.Exists(folderBrowserDialog1.SelectedPath + "\\Update.json"))
                {
                    Directory.Delete(folderBrowserDialog1.SelectedPath + "\\Updates", true);
                    File.Delete(folderBrowserDialog1.SelectedPath + "\\Update.json");
                }

            }

            btnUpdate.Enabled = true;
            txtClientFolder.Enabled = true;
        }

        private async void CreateFileList(string pathClient)
        {
            string pastaArquivoZip = pathClient + @"\Updates\";
            Directory.CreateDirectory(pastaArquivoZip);
            DirectoryInfo dir = new DirectoryInfo(pathClient);
            try
            {
                var arquivos = new List<Arquivo>();
                    
                if (!Directory.Exists(pathClient)) return;   
               
                string[] fileListDir = Directory.GetFiles(pathClient, "*.*", SearchOption.AllDirectories);
                int totalFiles = fileListDir.Length;
                //($"Total de arquivos : {totalFiles}, Criado em {DateTime.Now}", xm.Doc.DocumentElement);

                for (int i = 0; i < totalFiles; i++)
                {
                    var i1 = i;
                    await Task.Run(() =>
                    {
                        string filePath = fileListDir[i1];
                        string fileClientPath = filePath.Replace(pathClient, string.Empty);

                        var ShaOne = new SHA1CryptoServiceProvider();
                        byte[] data = File.ReadAllBytes(filePath);
                        var DataCrip = ShaOne.ComputeHash(data);
                        string hash = BitConverter.ToString(DataCrip).Replace("-", string.Empty).ToLower();

                        FileInfo file = new FileInfo(filePath);

                        if (fileClientPath[0] == Path.DirectorySeparatorChar)
                            fileClientPath = fileClientPath.Substring(1);
                        if (fileClientPath[0] == Path.AltDirectorySeparatorChar)
                            fileClientPath = fileClientPath.Substring(1);


                        //xm.CreateAttribute(fileElement, "Sha1", hash);

                        


                        string nomeArquivo = filePath.Replace(string.Format(pathClient + @"\"), string.Empty);


                        arquivos.Add(new Arquivo()
                        {
                           Nome = nomeArquivo,
                           Hash = hash
                        }
                        );
                       
                        using (ZipFile zips = new ZipFile())
                        {
                            var subpastas = dir.GetDirectories("*", SearchOption.AllDirectories);

                            foreach (var item in subpastas)
                            {
                                Directory.CreateDirectory(pastaArquivoZip + item.FullName.Replace(string.Format(pathClient), string.Empty));
                            }
                            zips.AddFile(filePath, "");
                            zips.Save(pastaArquivoZip + nomeArquivo + ".zip");
                            if (Directory.Exists(pastaArquivoZip + "Updates"))
                                Directory.Delete(pastaArquivoZip + "Updates", true);
                            btnSelectFolder.BeginInvoke((MethodInvoker)delegate () { btnSelectFolder.Enabled = false; });

                        }


                        this.Invoke(new MethodInvoker(delegate
                        {
                            progressStatus.Value = (i1 + 1) * 100 / totalFiles;
                            lblStatus.Text = $"Status: Processando arquivo {file.Name} ({i1}/{totalFiles}) - ({progressStatus.Value}%)";
                        }));
                    });
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                };
                var jsonString = JsonSerializer.Serialize(arquivos,options);

                using (FileStream fs = File.Create(pathClient + "\\Update.json"))
                {
                    await JsonSerializer.SerializeAsync(fs, arquivos,options);
                }
                lblStatus.Text = "Status: Completo";
                btnSelectFolder.Enabled = true;
                txtClientFolder.Text = "";
                txtClientFolder.Enabled = false;
                btnUpdate.Enabled = false;
               // xm.SaveXml(Path.Combine(pastaArquivoZip + "UpdateList.xml"));
            }

            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Erro ao tentar criar arquivos de Updates",MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            lblStatus.Text = "Escolha a pasta para criar os arquivos de update";
            txtClientFolder.Enabled = false;
            btnUpdate.Enabled = false;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            CreateFileList(folderBrowserDialog1.SelectedPath);
        }

       
    }
}

