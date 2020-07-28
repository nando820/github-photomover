using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
//using Ookii.Dialogs.Wpf;
using System.Windows;
using System.ComponentModel;
using System.Globalization;
using System.Collections;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using PhotoMover.Helpers;

namespace PhotoMover
{
    class Photo
    {
        DirectoryInfo destRootdir;
        FileInfo mFile;
        FileInfo eFile;
        string folder;
        int filecount = 0;
        int totalFiles;
        //int completedFiles = 0;

        DateTime createdTime;
        DateTime localDate = DateTime.Now;
        string notMoved;

        private static Regex r = new Regex(":");
        public List<string> ImageExtensions;
        public List<string> MovieExtensions;
        //Ookii.Dialogs.Wpf.ProgressDialog progress_bar = new Ookii.Dialogs.Wpf.ProgressDialog();

        //Get Selected File Extentions
        public void getFileExtentions(List<string> imgExtentions, List<string> VideoExtentions)
        {
            ImageExtensions = imgExtentions;
            MovieExtensions = VideoExtentions;
        }


        //Moves pictures and videos calls checkDate method
        public void moveFiles(object sender, DoWorkEventArgs e, string[] dirFiles, string destinationDir, string sort_photos, string sort_videos)
        {

            int percentage=0;
            int completedFiles = 0;
            destRootdir = new DirectoryInfo(destinationDir);
            BackgroundWorker worker = (BackgroundWorker)sender;
            
            totalFiles = dirFiles.Length;
            MainWindow.main.Status = "Total files to move: " + totalFiles.ToString();

            foreach (string file in dirFiles)
            {
                //if cancellation is pending, cancel work.  
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                //Organize Photos
                if (ImageExtensions.Contains(Path.GetExtension(file).ToUpperInvariant())&& sort_photos=="Yes")
                {
                    filecount++;

                    percentage = (int)((filecount * 100) / totalFiles);
                    worker.ReportProgress(percentage);
                    createdTime = GetDateTakenFromImage(file);

                    var month = createdTime.ToString("MMMM", CultureInfo.InvariantCulture);
                    var year = createdTime.ToString("yyyy", CultureInfo.InvariantCulture);
                    mFile = new FileInfo(file);
                    if (createdTime == DateTime.Today)
                    {
                        folder = "\\" + "Unclassified";
                    }
                    else
                    {
                        folder = "\\" + year + "\\" + month + "\\" + "photos";
                    }

                    //create tail directory if not there
                    if (!System.IO.Directory.Exists(destinationDir + folder))
                    {
                        System.IO.Directory.CreateDirectory(destRootdir + folder);
                    }

                    // to remove name collisions
                    if (new FileInfo(destinationDir + folder + "\\" + mFile.Name).Exists == false)
                    {
                        mFile.MoveTo(destinationDir + folder + "\\" + mFile.Name);
                    }
                    // if file exists in destination folder compare files
                    else if (new FileInfo(destinationDir + folder + "\\" + mFile.Name).Exists == true)
                    {
                        eFile = new FileInfo(destinationDir + folder + "\\" + mFile.Name);
                        if(compareFiles(destinationDir + folder + "\\" + eFile.Name, file))
                        {
                            mFile.MoveTo(destinationDir + folder + "\\" + Path.GetFileNameWithoutExtension(mFile.Name)+"_" +localDate.ToString("MM_dd_yyyy_HH_mm_ss") + Path.GetExtension(mFile.Name));
                        }

                    }
                }

                //Organize Videos
                if (MovieExtensions.Contains(Path.GetExtension(file).ToUpperInvariant()) && sort_videos == "Yes")
                {
                    filecount++;

                    percentage = (int)((filecount * 100) / totalFiles);
                    worker.ReportProgress(percentage);
                    createdTime = GetDateTakenFromVideo(file);

                    var month = createdTime.ToString("MMMM", CultureInfo.InvariantCulture);
                    var year = createdTime.ToString("yyyy", CultureInfo.InvariantCulture);
                    mFile = new FileInfo(file);
                    if (createdTime == DateTime.Today)
                    {
                        folder = "\\" + "Unclassified";
                    }
                    else
                    {
                        folder = "\\" + year + "\\" + month + "\\" + "videos";
                    }

                    //create destination directory if not there
                    if (!System.IO.Directory.Exists(destinationDir + folder))
                    {
                        System.IO.Directory.CreateDirectory(destRootdir + folder);
                    }

                    // to remove name collisions
                    if(new FileInfo(destinationDir + folder + "\\" + mFile.Name).Exists == false)
                    {
                        mFile.MoveTo(destinationDir + folder + "\\" + mFile.Name);
                    }
                    // if file exists in destination folder compare files
                    else if (new FileInfo(destinationDir + folder + "\\" + mFile.Name).Exists == true)
                    {
                        eFile = new FileInfo(destinationDir + folder + "\\" + mFile.Name);
                        if (compareFiles(destinationDir + folder + "\\" + eFile.Name, file))
                        {
                            mFile.MoveTo(destinationDir + folder + "\\" + Path.GetFileNameWithoutExtension(mFile.Name) + "_" + localDate.ToString("MM_dd_yyyy_HH_mm_ss") + Path.GetExtension(mFile.Name));
                        }

                    }

                }
                //if file extension does not belong to the list of supported extensions
                else if((ImageExtensions.Contains(Path.GetExtension(file).ToUpperInvariant()))==false && (MovieExtensions.Contains(Path.GetExtension(file).ToUpperInvariant())) == false)
                {
                    notMoved = "File extension not supported";
                    Debugger(file, notMoved);
                    continue;
                }
                completedFiles++;
                MainWindow.main.Status = "Total files to move: " + totalFiles.ToString() + " Files completed: "+completedFiles;
            }
            MainWindow.main.Status = "Completed!";
            filecount = 0;
            worker.ReportProgress(100);
        }

        //Update Progress bar
        /*public void getStatus(IProgress<int> progress)
        {
            int percentage = 0;
            while(totalFiles>completedFiles)
            {
                percentage = (int)((filecount * 100) / totalFiles);
                progress.Report(percentage);
            }

        }*/

        //retrieves the datetime WITHOUT loading the whole image
        public static DateTime GetDateTakenFromImage(string filepath)
        {

                try
                {
                //Use Date Taken
                //Original image stream method
                /*using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                using (Image myImage = Image.FromStream(fs, false, false))
                {
                    PropertyItem propItem;
                    if (myImage.PropertyIdList.Any(p => p == 36867))
                    {
                        propItem = myImage.GetPropertyItem(36867);
                        string dateTaken = r.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
                        return DateTime.Parse(dateTaken);
                    }
                }*/

                string[] ends = { ".AAE" };
                bool extention = ends.Any(x => filepath.EndsWith(x));

                if (extention == true)
                {
                    //FileInfo imageFile = new FileInfo(filepath);
                    DateTime fileDate = File.GetCreationTime(filepath);
                    //DateTime fileDate = File.GetLastWriteTime(filepath);
                    return fileDate;
                }
                //Metadata Extractor handles
                //JPEG PNG WebP GIF ICO BMP TIFF PSD PCX RAW CRW CR2 NEF ORF RAF RW2 RWL SRW ARW DNG X3F
                Metadata fileMetaData = new Metadata(filepath);
                 return(fileMetaData.extractFileModifiedDate());                       
                 
                }
                catch(Exception e)
                {
                    Console.WriteLine("{0} Exception caught.", e);
                    return DateTime.Today;
                }

            
        }

        //Get Date created from video
        public static DateTime GetDateTakenFromVideo(string filepath)
        {

            try
            {
                string[] ends = { ".mpg"};
                bool extention = ends.Any(x => filepath.EndsWith(x));

                if (extention == true)
                {
                    //FileInfo videoFile = new FileInfo(filepath);
                    //DateTime fileDate = videoFile.CreationTime;
                    DateTime fileDate = File.GetLastWriteTime(filepath);
                    return fileDate;
                }

                else
                {
                    //Metadata Extractor handles
                    //MOV MP4 M4V 3G2 3GP 3GP
                    Metadata fileMetaData = new Metadata(filepath);
                    return(fileMetaData.extractFileModifiedDate());
                }

            }

            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
                  return DateTime.Today;
            }
            
        }

        //Compare two Files
        public bool compareFiles(string eFile, string mFile)
        {
            byte[] existing_file = System.IO.File.ReadAllBytes(eFile);
            byte[] moving_file = System.IO.File.ReadAllBytes(mFile);
            if (existing_file.Length != moving_file.Length)
            {
                return true;
            }
            else
            {
                notMoved = "Identical file exists on Destination folder";
                Debugger(mFile,notMoved);
                return false;
            }

        }

        public void Debugger(string file, string description)
        {
            string output_file = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Files_not_moved_" + localDate.ToString("MM_dd_yyyy_HH_mm_ss") + ".csv";
            using (System.IO.StreamWriter strFile = new System.IO.StreamWriter(output_file, true))
            {
               strFile.WriteLine("{0},{1}", file, description);
               strFile.Close();
            }
  
        }

    }
}
