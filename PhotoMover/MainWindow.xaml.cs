using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//using System.Windows.Forms;
using System.IO.Compression;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System.ComponentModel;
using System.Threading;
using System.Drawing;


namespace PhotoMover
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            //progressbar1.Minimum = 0;
            main = this;
            //pictureBox1.Source = Properties.Resources.camera_clipart;
            //progressbar1.Maximum = 100;

            //Setup worker
            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(Worker_DoWork);
            worker.ProgressChanged += new ProgressChangedEventHandler(Worker_ProgressChanged);
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);
            //worker.ReportProgress(0);
            //progressbar1.Value = 0;
        }
        internal static MainWindow main;
        internal string Status
        {
            get { return file_Count.Content.ToString(); }
            set { Dispatcher.Invoke(new Action(() => { file_Count.Content = value; })); }
        }

        private static readonly Photo _mover = new Photo();
        public BackgroundWorker worker;
        string sourcePath;
        string destinationPath;
        string sort_photos = "No";
        string sort_videos = "Yes";
        public static readonly List<string> ImageExtensions = new List<string>();
        public static readonly List<string> VideoExtensions = new List<string>();

        private void Source_button_Click(object sender, RoutedEventArgs e)
        {

            VistaFolderBrowserDialog browser = new VistaFolderBrowserDialog();
            browser.Description = "Please select the source folder";
            if (browser.ShowDialog() == true)
            {
                sourcePath = browser.SelectedPath;
                Textbox_source.Text = sourcePath;
            }
        }

        private void Destination_button_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog browser = new VistaFolderBrowserDialog();
            browser.Description = "Please select the destination folder";
            if (browser.ShowDialog() == true)
            {
                destinationPath = browser.SelectedPath;
                Textbox_destination.Text = destinationPath;
            }
        }

        private void button_move_Click(object sender, RoutedEventArgs e)
        {
            if (sourcePath == null)
            {
                MessageBox.Show("Please Select a Source Folder");
            }
            else if (destinationPath == null)
            {
                MessageBox.Show("Please Select a Destination Folder");
            }
            else if(pictures_CheckBox.IsChecked==false && movies_checkBox.IsChecked==false)
            {
                MessageBox.Show("Please Select what to organize");
            }
            else
            {
                worker.RunWorkerAsync();
                //startStatus();
            }

        }

        /*public void startStatus()
        {
            var progress = new Progress<int>(update => progressbar1.Value = update);
            try
            {
                _mover.getStatus(progress);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                progressbar1.Value = 100;
            }
        }*/

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                //progressbar1.Value = 0;
                //worker.ReportProgress(0);
                _mover.getFileExtentions(ImageExtensions, VideoExtensions);
                string[] dirFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories); //putting all files including subdirectories into an array
                _mover.moveFiles(sender, e, dirFiles, destinationPath, sort_photos, sort_videos);
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error trying to Move files: " + ex);
            }
        }
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //progressbar1.Value = e.ProgressPercentage;
            //progressbar1.Value = e.ProgressPercentage -1;
            if (e.ProgressPercentage != 0)
                progressbar1.Value = e.ProgressPercentage - 1;
            progressbar1.Value = e.ProgressPercentage;
            if (progressbar1.Maximum == e.ProgressPercentage)
                progressbar1.Value = 0;
        }
        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("The task has been cancelled");
            }
            else if (e.Error != null) 
                MessageBox.Show(e.Error.Message);
            else
            {
                //MessageBox.Show("Completed moving files");
                progressbar1.Value = 0;
                return;
            }

        }
        private void Cancel_button_Click(object sender, RoutedEventArgs e)
        {
            worker.CancelAsync();
        }

        private void movies_checkBox_Checked(object sender, RoutedEventArgs e)
        {
            sort_videos = "Yes";
        }

        private void pictures_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            sort_photos = "Yes";
        }

        private void photos_listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (JPEG.IsChecked==true)
              //  ImageExtensions.Add(JPEG.Content.ToString());
        }

        private void JPG_Checked(object sender, RoutedEventArgs e)
        {
            if (JPG.IsChecked == true)
                ImageExtensions.Add(JPG.Content.ToString());
            else
                ImageExtensions.Remove(JPG.Content.ToString());
        }

        private void JPE_Checked(object sender, RoutedEventArgs e)
        {
            if (JPE.IsChecked == true)
                ImageExtensions.Add(JPE.Content.ToString());
            else
                ImageExtensions.Remove(JPE.Content.ToString());
        }

        private void JPEG_Checked(object sender, RoutedEventArgs e)
        {
            if (JPEG.IsChecked == true)
                ImageExtensions.Add(JPEG.Content.ToString());
            else
                ImageExtensions.Remove(JPEG.Content.ToString());
        }

        private void BMP_Checked(object sender, RoutedEventArgs e)
        {
            if (BMP.IsChecked == true)
                ImageExtensions.Add(BMP.Content.ToString());
            else
                ImageExtensions.Remove(BMP.Content.ToString());
        }

        private void GIF_Checked(object sender, RoutedEventArgs e)
        {
            if (GIF.IsChecked == true)
                ImageExtensions.Add(GIF.Content.ToString());
            else
                ImageExtensions.Remove(GIF.Content.ToString());
        }

        private void PNG_Checked(object sender, RoutedEventArgs e)
        {
            if (PNG.IsChecked == true)
                ImageExtensions.Add(PNG.Content.ToString());
            else
                ImageExtensions.Remove(PNG.Content.ToString());
        }

        private void EPS_Checked(object sender, RoutedEventArgs e)
        {
            if (EPS.IsChecked == true)
                ImageExtensions.Add(EPS.Content.ToString());
            else
                ImageExtensions.Remove(EPS.Content.ToString());
        }

        private void HEIF_Checked(object sender, RoutedEventArgs e)
        {
            if (HEIF.IsChecked == true)
                ImageExtensions.Add(HEIF.Content.ToString());
            else
                ImageExtensions.Remove(HEIF.Content.ToString());
        }

        private void HEIC_Checked(object sender, RoutedEventArgs e)
        {
            if (HEIC.IsChecked == true)
                ImageExtensions.Add(HEIC.Content.ToString());
            else
                ImageExtensions.Remove(HEIC.Content.ToString());
        }

        private void ICO_Checked(object sender, RoutedEventArgs e)
        {
            if (ICO.IsChecked == true)
                ImageExtensions.Add(ICO.Content.ToString());
            else
                ImageExtensions.Remove(ICO.Content.ToString());
        }

        private void JFIF_Checked(object sender, RoutedEventArgs e)
        {
            if (JFIF.IsChecked == true)
                ImageExtensions.Add(JFIF.Content.ToString());
            else
                ImageExtensions.Remove(JFIF.Content.ToString());
        }

        private void Netpbm_Checked(object sender, RoutedEventArgs e)
        {
            if (Netpbm.IsChecked == true)
                ImageExtensions.Add(Netpbm.Content.ToString());
            else
                ImageExtensions.Remove(Netpbm.Content.ToString());
        }

        private void PCX_Checked(object sender, RoutedEventArgs e)
        {
            if (PCX.IsChecked == true)
                ImageExtensions.Add(PCX.Content.ToString());
            else
                ImageExtensions.Remove(PCX.Content.ToString());
        }

        private void PSD_Checked(object sender, RoutedEventArgs e)
        {
            if (PSD.IsChecked == true)
                ImageExtensions.Add(PSD.Content.ToString());
            else
                ImageExtensions.Remove(PSD.Content.ToString());
        }

        private void TGA_Checked(object sender, RoutedEventArgs e)
        {
            if (TGA.IsChecked == true)
                ImageExtensions.Add(TGA.Content.ToString());
            else
                ImageExtensions.Remove(TGA.Content.ToString());
        }

        private void TIFF_Checked(object sender, RoutedEventArgs e)
        {
            if (TIFF.IsChecked == true)
                ImageExtensions.Add(TIFF.Content.ToString());
            else
                ImageExtensions.Remove(TIFF.Content.ToString());
        }

        private void WebP_Checked(object sender, RoutedEventArgs e)
        {
            if (WebP.IsChecked == true)
                ImageExtensions.Add(WebP.Content.ToString());
            else
                ImageExtensions.Remove(WebP.Content.ToString());
        }

        private void ARW_Checked(object sender, RoutedEventArgs e)
        {
            if (ARW.IsChecked == true)
                ImageExtensions.Add(ARW.Content.ToString());
            else
                ImageExtensions.Remove(ARW.Content.ToString());
        }

        private void CR2_Checked(object sender, RoutedEventArgs e)
        {
            if (CR2.IsChecked == true)
                ImageExtensions.Add(CR2.Content.ToString());
            else
                ImageExtensions.Remove(CR2.Content.ToString());
        }

        private void NEF_Checked(object sender, RoutedEventArgs e)
        {
            if (NEF.IsChecked == true)
                ImageExtensions.Add(NEF.Content.ToString());
            else
                ImageExtensions.Remove(NEF.Content.ToString());
        }

        private void ORF_Checked(object sender, RoutedEventArgs e)
        {
            if (ORF.IsChecked == true)
                ImageExtensions.Add(ORF.Content.ToString());
            else
                ImageExtensions.Remove(ORF.Content.ToString());
        }

        private void RW2_Checked(object sender, RoutedEventArgs e)
        {
            if (RW2.IsChecked == true)
                ImageExtensions.Add(RW2.Content.ToString());
            else
                ImageExtensions.Remove(RW2.Content.ToString());
        }

        private void RWL_Checked(object sender, RoutedEventArgs e)
        {
            if (RWL.IsChecked == true)
                ImageExtensions.Add(RWL.Content.ToString());
            else
                ImageExtensions.Remove(RWL.Content.ToString());
        }

        private void SRW_Checked(object sender, RoutedEventArgs e)
        {
            if (RWL.IsChecked == true)
                ImageExtensions.Add(SRW.Content.ToString());
            else
                ImageExtensions.Remove(SRW.Content.ToString());
        }

        private void AAE_Checked(object sender, RoutedEventArgs e)
        {
            if (AAE.IsChecked == true)
                ImageExtensions.Add(AAE.Content.ToString());
            else
                ImageExtensions.Remove(AAE.Content.ToString());
        }

        private void AVCI_Checked(object sender, RoutedEventArgs e)
        {
            if (AVCI.IsChecked == true)
                VideoExtensions.Add(AVCI.Content.ToString());
            else
                VideoExtensions.Remove(AVCI.Content.ToString());
        }

        private void AVI_Checked(object sender, RoutedEventArgs e)
        {
            if (AVI.IsChecked == true)
                VideoExtensions.Add(AVI.Content.ToString());
            else
                VideoExtensions.Remove(AVI.Content.ToString());
        }

        private void MOV_Checked(object sender, RoutedEventArgs e)
        {
            if (MOV.IsChecked == true)
                VideoExtensions.Add(MOV.Content.ToString());
            else
                VideoExtensions.Remove(MOV.Content.ToString());
        }

        private void MP4_Checked(object sender, RoutedEventArgs e)
        {
            if (MP4.IsChecked == true)
                VideoExtensions.Add(MP4.Content.ToString());
            else
                VideoExtensions.Remove(MP4.Content.ToString());
        }

        private void M4V_Checked(object sender, RoutedEventArgs e)
        {
            if (M4V.IsChecked == true)
                VideoExtensions.Add(M4V.Content.ToString());
            else
                VideoExtensions.Remove(M4V.Content.ToString());
        }

        private void MPG_Checked(object sender, RoutedEventArgs e)
        {
            if (MPG.IsChecked == true)
                VideoExtensions.Add(MPG.Content.ToString());
            else
                VideoExtensions.Remove(MPG.Content.ToString());
        }

        private void _3GP_Checked(object sender, RoutedEventArgs e)
        {
            if (_3GP.IsChecked == true)
                VideoExtensions.Add(_3GP.Content.ToString());
            else
                VideoExtensions.Remove(_3GP.Content.ToString());
        }

        private void _2GP_Checked(object sender, RoutedEventArgs e)
        {
            if (_2GP.IsChecked == true)
                VideoExtensions.Add(_2GP.Content.ToString());
            else
                VideoExtensions.Remove(_2GP.Content.ToString());
        }

        private void _3G2_Checked(object sender, RoutedEventArgs e)
        {
            if (_3G2.IsChecked == true)
                VideoExtensions.Add(_3G2.Content.ToString());
            else
                VideoExtensions.Remove(_3G2.Content.ToString());
        }

    }
}
