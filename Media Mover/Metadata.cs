using System;
using System.Collections.Generic;
using System.Text;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System.Linq;
using System.Windows;

namespace Media_Mover.Helpers
{
    class Metadata
    {
        IEnumerable<MetadataExtractor.Directory> directories;
        string dateModified = "";
        string year;
        string month;
        string day;
        string time;
        public Metadata(string filepath)
        {
            //Metadata Extractor handles metadata formats
            //Exif IPTC XMP JFIF JFXX ICC 8BIM
            //file extentions
            //JPEG PNG WebP GIF ICO BMP TIFF PSD PCX RAW CRW CR2 NEF ORF RAF RW2 RWL SRW ARW DNG X3F
            //MOV MP4 M4V 3G2 3GP 3GP
            //MP3 WAV
            //Manufacturer makernote support
            //Agfa Canon Casio Epson Fujifilm Foveon Kodak Kyocera Nikon Minolta Olympus Panasonic Pentax Sigma Sony
            directories = ImageMetadataReader.ReadMetadata(filepath);
        }

        public  DateTime extractFileModifiedDate()
        {
            var months_dictionary = new Dictionary<string, string>
            {
                {"01", "Jan"},{"02", "Feb"},{"03", "Mar"},{"04", "Apr"},{"05", "May"},{"06", "Jun"},
                {"07", "Jul"},{"08", "Aug"},{"09", "Sep"},{"10", "Oct"},{"11", "Nov" },{"12", "Dec"},
            };
            string dateTaken = "";
            string dateMod="";

            foreach (var directory in directories)
            {
                foreach (var tag in directory.Tags)
                {
                    //MessageBox.Show($"[{directory.Name}] {tag.Name} = {tag.Description}");
                    if(directory.Name == "Exif SubIFD" && tag.Name == "Date/Time Original")
                    {
                        dateModified = tag.Description;
                        year = dateModified.Substring(0,4);
                        month = dateModified.Substring(5,2);
                        day = dateModified.Substring(8, 2);
                        time = dateModified.Substring(11, 8);
                        dateTaken = year + "-" + month + "-" + day + " " + time;
                    }
                    else if (directory.Name == "File" && tag.Name == "File Modified Date")
                    {
                        dateModified = tag.Description;
                        year = dateModified.Substring(dateModified.Length - 4);
                        month = dateModified.Substring(4, 3);
                        day = dateModified.Substring(8, 2);
                        time = dateModified.Substring(11, 8);
                        dateMod = year + "-" + months_dictionary.FirstOrDefault(x => x.Value == month).Key + "-" + day + " " + time;
                    }
                }

            }
            //MessageBox.Show(dateTaken);
            //MessageBox.Show(dateMod);
            if(dateTaken!="")
                return DateTime.Parse(dateTaken);
            else
                return DateTime.Parse(dateMod);

            //Standard Extracting method
            /*foreach (var directory in directories)
              {
                  foreach (var tag in directory.Tags)
                     MessageBox.Show($"[{directory.Name}] {tag.Name} = {tag.Description}");

              if (directory.HasError)
              {
                foreach (var error in directory.Errors)
                    MessageBox.Show($"ERROR: {error}");
              }
            }*/

        }
    }
}
