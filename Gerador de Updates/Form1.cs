using System;
using System.IO;
using System.Windows.Forms;
using System.Security.Cryptography;
using Ionic.Zip;

namespace Gerador_de_Updates
{
    public partial class Form1 : Form
    {


        public Form1()
        {
            InitializeComponent();
            lblProgresso.Text = "Escolha os arquivos para serem compactados";

        }



        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog().Equals(DialogResult.OK))
            {
                backgroundWorker1.RunWorkerAsync();
            }


        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            string pastaSelecionada = folderBrowserDialog1.SelectedPath;

            string pastaArquivoZip = pastaSelecionada + "\\Updates\\"; // Aqui vamos informar para qual pasta devemos enviar o arquivo, neste caso será uma subpasta da pasta atual (Este método pode ser feito de diversas formas, está é uma bem simples)

            if (Directory.Exists(pastaArquivoZip))
            {
                MessageBox.Show("Pasta Updates antiga excluida e arquivo UpdateList.txt", "Informativo", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Directory.Delete(pastaArquivoZip, true);
                File.Delete(pastaSelecionada + "\\UpdateList.txt");
            }

            Directory.CreateDirectory(pastaArquivoZip);
            DirectoryInfo dir = new DirectoryInfo(pastaSelecionada);

            var subpastas = dir.GetDirectories("*", SearchOption.AllDirectories);




            var Root = Directory.GetFiles(pastaSelecionada, "*.*", SearchOption.AllDirectories);
            foreach (string arquivo in Root)
            {
                try
                {

                    var ShaOne = new SHA1CryptoServiceProvider();
                    byte[] data = File.ReadAllBytes(arquivo);
                    var DataCrip = ShaOne.ComputeHash(data);
                    string hash = BitConverter.ToString(DataCrip).Replace("-", string.Empty).ToLower();

                    using (StreamWriter writer = File.AppendText(pastaArquivoZip + "\\UpdateList.txt"))
                    {
                        writer.WriteLine(arquivo.Replace(string.Format(pastaSelecionada + "\\"), string.Empty) + "," + hash);
                    }

                    string nomeArquivo = arquivo.Replace(string.Format(pastaSelecionada + "\\"), string.Empty);

                    using (ZipFile file = new ZipFile())
                    {
                        foreach (var item in subpastas)
                        {
                            Directory.CreateDirectory(pastaArquivoZip + item.FullName.Replace(string.Format(pastaSelecionada), string.Empty));
                        }
                        file.AddFile(arquivo, "");
                        file.Save(pastaArquivoZip + nomeArquivo + ".zip");
                        Directory.Delete(pastaArquivoZip + "Updates");
                        lblProgresso.BeginInvoke((MethodInvoker)delegate () { lblProgresso.Text = "Compactando arquivos, Por favor Aguarde..."; });
                        btnFileDialog.BeginInvoke((MethodInvoker)delegate () { btnFileDialog.Enabled = false; });
                        progressBar1.BeginInvoke((MethodInvoker)delegate () { progressBar1.Style = ProgressBarStyle.Marquee; });
                    }

                }

                catch (Exception ex)
                {

                    MessageBox.Show(ex.ToString());
                }


            }

        }

        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            progressBar1.BeginInvoke((MethodInvoker)delegate () { progressBar1.Style = ProgressBarStyle.Blocks; });

            if (MessageBox.Show("Arquivos criados com sucesso na Pasta ArquivoZip", "Tudo ok!", MessageBoxButtons.OK, MessageBoxIcon.Information).Equals(DialogResult.OK))
            {
                lblProgresso.Text = "Escolha os arquivos a serem compactados";
                btnFileDialog.BeginInvoke((MethodInvoker)delegate () { btnFileDialog.Enabled = true; });

            }
        }
    }
}

