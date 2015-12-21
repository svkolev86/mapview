using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Net;
using System.Xml.Linq;
using System.Xml;
using System.Web.Hosting;
using System.Drawing;

namespace MapViewSilverlight.Web
{
    [ServiceContract(Namespace = "")]
    [SilverlightFaultBehavior]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class Service1
    {
        [OperationContract]
        public void DoWork()
        {
            // Add your operation implementation here
            return;
        }
        // Add more operations here and mark them with [OperationContract]

        public string mapLocation;

        [OperationContract]
        public string GetMapLocation()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(HostingEnvironment.MapPath("~/ClientBin") + @"\config.xml");
            mapLocation = xDoc.GetElementsByTagName("MapPath")[0].InnerText.ToString();
            return (mapLocation);
        }

        [OperationContract]
        public void GetLegendRow(string name, out string  pic, out string description)
        {
            pic = "";
            description = "";

            XmlDocument xDoc = new XmlDocument();

            xDoc.Load(HostingEnvironment.MapPath("~/ClientBin") + @"\config.xml");
            XmlNodeList elem = xDoc.GetElementsByTagName(name);
            for (int i = 1; i <= elem.Count; i++)
            {
                XmlNode node = elem.Item(i);
                switch(node.Name)
                {
                    case "picture":
                        {
                            Image im = Image.FromFile(node.InnerText.ToString());
                            //pic = convToBase64String; 
                            break;
                        }
                    case "description": ; break;
                }
                elem.Item(i).InnerText.ToString();
            }
        }
        /*
        public string ImageToBase64(Image image,
  System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }
        private string convToBase64String()
        {
            StreamResourceInfo sri = null;
            Uri uri = new Uri("Checked.png", UriKind.Relative);
            sri = Application.GetResourceStream(uri);

            BitmapImage bitmap = new BitmapImage();
            bitmap.SetSource(sri.Stream);
            WriteableBitmap wb = new WriteableBitmap(bitmap);

            MemoryStream ms = new MemoryStream();
            wb.SaveJpeg(ms, bitmap.PixelWidth, bitmap.PixelHeight, 0, 100);
            byte[] imageBytes = ms.ToArray();

            base64 = System.Convert.ToBase64String(imageBytes);

        }

        */
    }
}
