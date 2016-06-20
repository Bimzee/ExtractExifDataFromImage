using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExifLib;
using System.IO;
using System.Drawing.Imaging;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using System.Xml.Linq;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { FileName = txtFile.Text, Filter = "JPEG Images (*.jpg)|*.jpg" };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtFile.Text = dlg.FileName;
            }
        }

        private void btnData_Click(object sender, EventArgs e)
        {
            try
            {
                //Install-Package ExifLib
                //http://www.codeproject.com/Articles/36342/ExifLib-A-Fast-Exif-Data-Extractor-for-NET-2-0

                txtResult.Clear();
                using (var reader = new ExifReader(txtFile.Text))
                {
                    var thumbNailBytes = reader.GetJpegThumbnailBytes();

                    if (thumbNailBytes == null)
                        pictureBoxThumbnail.Image = null;
                    else
                    {
                        using (var stream = new MemoryStream(thumbNailBytes))
                            pictureBoxThumbnail.Image = Image.FromStream(stream);
                    }

                    double[] gpsLatitude;
                    string output = string.Empty;

                    if (reader.GetTagValue(ExifTags.GPSLatitude, out gpsLatitude))
                    {
                        output = string.Format("{0}: {1},{2},{3} ", "GPS Latitude", gpsLatitude[0], gpsLatitude[1], gpsLatitude[2]);

                        txtResult.AppendText(output);
                    }

                    double[] gpsLongitude;

                    if (reader.GetTagValue(ExifTags.GPSLongitude, out gpsLongitude))
                    {

                        output = string.Format("{0}: {1},{2},{3} ", "GPS Longitude", gpsLongitude[0], gpsLongitude[1], gpsLongitude[2]);

                        txtResult.AppendText(Environment.NewLine);
                        txtResult.AppendText(output);
                    }

                    DateTime datePictureTaken;
                    if (reader.GetTagValue(ExifTags.DateTimeOriginal, out datePictureTaken))
                    {
                        txtResult.AppendText(Environment.NewLine);
                        txtResult.AppendText(string.Format("{0}: {1} ", "Date ", datePictureTaken.ToShortDateString()));
                    }

                    string locationName = string.Empty;
                    if (gpsLatitude != null && gpsLongitude != null)
                    {
                        locationName = GetLocationName(gpsLatitude[2], gpsLongitude[2]);

                        txtResult.AppendText(Environment.NewLine);
                        txtResult.AppendText(string.Format("{0}: {1} ", "Location ", locationName));
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public string GetLocationName(double latitude, double longitude)

        {
            string locationName = string.Empty;

            //string baseUri = "https://maps.googleapis.com/maps/api/timezone/json?location={0},{1}&timestamp=1374868635&sensor=false";

            string baseUri = "http://dev.virtualearth.net/REST/v1/Locations/{0},{1}?key=AjzHTEndVFIIdMNhX3hDjsbfLfZPY3qU_OLueRyfZ-L9dhDxRArlhCMWRDoYcAmA";

            string requestUri = string.Format(baseUri, latitude, longitude);

            using (WebClient wc = new WebClient())
            {
                string result = wc.DownloadString(requestUri);

                var apiResult = JsonConvert.DeserializeObject<dynamic>(result);

                //locationName = apiResult.timeZoneId;
                //locationName = locationName.Split('/')[1];

                locationName = apiResult.resourceSets[0].resources[0].name;
            }

            return locationName;
        }
    }
}
