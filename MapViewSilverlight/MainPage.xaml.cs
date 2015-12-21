using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ROH.Web.Client.Silverlight;
using System.Xml;
using System.IO;
using System.Xml.Linq;

namespace MapViewSilverlight
{
    public partial class MainPage : UserControl
    {
        private string mapLocation;
        private Dictionary<UIElement, Point> Locations;
        private int PushPinCount = 0;

        /// <summary>
        /// Stores an instance of the wrapper class that handles mouse interactions 
        /// (panning, zooming, etc)
        /// </summary>
        private MultiScaleImageWrapper mouseInteractionWrapper;
        

        /// <summary>
        /// Indicates if the first motion has completed yet 
        /// (the initial zoom behavior is disabled)
        /// </summary>
        private bool isInitialMotionFinished;


        public MainPage()
        {
            InitializeComponent();
            mouseInteractionWrapper = new MultiScaleImageWrapper(deepZoomObject);
            deepZoomObject.ViewportChanged += new RoutedEventHandler(msi_ViewportChanged);

            Locations = new Dictionary<UIElement, Point>();


        }

        /// <summary>
        /// On Page Loaded
        /// </summary>
         void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // while 
            // ServiceReference1.GetLegendRowCompletedEventArgs
        }


        /// <summary>
        /// This is the code which locks the overlay to the underlying deep zoom image.
        /// All it really does is set the scale factor and offset of the overlay
        /// based on the current setting of the deep zoom image.
        /// </summary>
        void msi_ViewportChanged(object sender, RoutedEventArgs e)
        {
            // This event is called during animations of the image.
            // Match the scaling of the canvas with the image
            Point viewportOrigin = deepZoomObject.ViewportOrigin;
            double viewportWidth = deepZoomObject.ViewportWidth;

            // The scale factor is just the inverse of the ViewportWidth
            overlayScale.ScaleX = 1 / viewportWidth;
            overlayScale.ScaleY = 1 / viewportWidth;

            // The offset is calculated by finding the location of the origin of the dzi
            // in element coordinates.
            Point newO = LogicalToElement(new Point(), viewportOrigin, viewportWidth);
            overlayTranslate.X = newO.X;
            overlayTranslate.Y = newO.Y;
        }
        private Point LogicalToElement(Point p, Point Origin, double Width)
        {
            return new Point(((p.X - Origin.X) / Width) * deepZoomObject.ActualWidth,
            ((p.Y - Origin.Y) / Width) * deepZoomObject.ActualWidth);
        }


        //Loads the DeepZoom image
        private void image_Loaded(object sender, RoutedEventArgs e)
        {
            //DeepZoomImageTileSource src = new DeepZoomImageTileSource(new Uri(Application.Current.Host.Source,
            //"DeepZoomImage\\dzc_output.xml"));
            //DeepZoomImageTileSource src = new DeepZoomImageTileSource(new Uri(Application.Current.Host.Source, mapLocation));
            //deepZoomObject.Source = src;

            ServiceReference1.Service1Client srv = new ServiceReference1.Service1Client();
            srv.GetMapLocationCompleted += new EventHandler<ServiceReference1.GetMapLocationCompletedEventArgs>(client_GetMapLocationCompleted);
            srv.GetMapLocationAsync();
        }
        void client_GetMapLocationCompleted(object sender, ServiceReference1.GetMapLocationCompletedEventArgs e) 
        {
            deepZoomObject.Source = new DeepZoomImageTileSource(new Uri(Application.Current.Host.Source, (string)e.Result));
        }


        /// <summary>
        /// Handles the "MotionFinished" event fired by the MultiScaleImage, 
        /// but only re-enables the "UseSprings" property after the first 
        /// motion completes (a little trick to properly bypass the initial "zoom in
        /// from nowhere" animation when first loading)
        /// </summary>
        /// <param name="sender">The MultiScaleImage instance.</param>
        /// <param name="e">Unused RoutedEvent arguments.</param>
        void image_InitialMotionFinished(object sender, RoutedEventArgs e)
        {
            if (!isInitialMotionFinished)
            {
                isInitialMotionFinished = true;
                deepZoomObject.UseSprings = true;
            }
        }


        /// <summary>
        /// Method that adds a UIElement onto MultiScaleImage.
        /// </summary>
        /// <param name="x">X position on MultiScaleImage. 0 is left. 1.0 is right.</param>
        /// <param name="y">Y position on MultiScaleImage. 0 is top. 1.0 is bottom.</param>
        /// <param name="value">Object to be added.</param>
        public void Add(Double x, Double y, UIElement value)
        {
            Locations.Add(value, new Point(x, y));
            try
            {
                overlay.Children.Add(value);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            value.RenderTransform = ProjectedTranslateTransform(Locations[value]);
        }

        /// <summary>
        /// Method that adds a pushpin onto MultiScaleImage.
        /// </summary>
        /// <param name="x">X position on MultiScaleImage. 0 is left. 1.0 is right.</param>
        /// <param name="y">Y position on MultiScaleImage. 0 is top. 1.0 is bottom.</param>
        /// <param name="color">Color of the pushpin.</param>
        /// <param name="initial">Initial charactor of the pushpin.</param>
        /// <returns>PushPin object that is added.</returns>
        public PushPin AddPin(Double x, Double y, Color color, Char initial)
        {
            PushPin newPushPin = new PushPin(initial, color);
            newPushPin.Name = "PushPin" + PushPinCount++;
            newPushPin.HorizontalAlignment = HorizontalAlignment.Left;
            newPushPin.VerticalAlignment = VerticalAlignment.Top;
            newPushPin.MouseLeftButtonDown += new MouseButtonEventHandler(PushPin_MouseLeftButtonDown);
            Add(x, y, newPushPin);
            return newPushPin;
        }

        void PushPin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PushPin pinSender = (PushPin)sender;
            Point delta = new Point((deepZoomObject.Width / 2 - ((TranslateTransform)pinSender.RenderTransform).X) * deepZoomObject.ViewportWidth / (deepZoomObject.Width), ((deepZoomObject.Height / 2 - ((TranslateTransform)pinSender.RenderTransform).Y)) * deepZoomObject.ViewportWidth / (deepZoomObject.Height * deepZoomObject.SubImages[0].AspectRatio));
            deepZoomObject.ViewportOrigin = new Point(deepZoomObject.ViewportOrigin.X - delta.X, deepZoomObject.ViewportOrigin.Y - delta.Y);
        }

        // Function that converts relative location(0.0 left-top, 1.0 bottom-right) to actual location depending on MultiScaleImage's ViewportWidth, and ViewportOrigin.
        private TranslateTransform ProjectedTranslateTransform(Point value)
        {
            if (deepZoomObject.SubImages.Count > 0)
                return new TranslateTransform() { X = (deepZoomObject.Width * value.X - deepZoomObject.Width * deepZoomObject.ViewportOrigin.X) / deepZoomObject.ViewportWidth, Y = ((deepZoomObject.Width / deepZoomObject.SubImages[0].AspectRatio) * value.Y - deepZoomObject.Width * deepZoomObject.ViewportOrigin.Y) / deepZoomObject.ViewportWidth };
            else
                return null;
        }

    }
}
