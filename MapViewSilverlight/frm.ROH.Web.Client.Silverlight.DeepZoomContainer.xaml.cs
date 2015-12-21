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

/// <summary>
/// ROH.Web.Client.Silverlight Namespace contains classes to present various informations such as map, geographical objects, polylines, pushpins and etc.
/// </summery>
namespace ROH.Web.Client.Silverlight
{
    /// <summary>
    /// DeepZoomContainer class.
    /// (ver. 1.00.00)
    /// 
    /// It's encapsulated version of <c>MultiScaleImage</c>. It can contain UIElements such as pushpins, images, and paths. Like <c>MultiScaleImage</c>, it is ViewPoint-based, and every contained objects will move when viewpoint of <c>MultiScaleImage</c> changes.
    /// 
    /// ROH Holding Company
    /// Won Dong (fkiller@gmail.com)
    /// </summary>
    public partial class DeepZoomContainer : UserControl
    {
        private Dictionary<UIElement, Point> Locations;
        private int PushPinCount = 0;
        private bool mousePressed = false;
        private Point pointPressed = new Point();
        private Point pointOrigin = new Point();
        private MouseButtonEventHandler evtMouseLeftButtonDown;
        private MouseButtonEventHandler evtMouseLeftButtonUp;
        //private MouseEventHandler evtMouseLeftLeave;
        private MouseEventHandler evtMouseMove;

        /// <summary>
        /// Property. It's <c>MultiScaleImage</c> class, which is in DeepZoomContainer. Read-only.
        /// </summary>
        public MultiScaleImage MultiScaleImage
        {
            get
            {
                return msiMap;
            }
        }

        private Thickness? p_scrolllimit = null;
        /// <summary>
        /// Property. ScrollLimit is a <c>Thickness</c> clsss that defines scroll boundary limitation.
        /// </summary>
        public Thickness? ScrollLimit
        {
            get
            {
                return p_scrolllimit;
            }
            set
            {
                p_scrolllimit = value;
                MultiScaleImage_ViewportChanged(msiMap, new RoutedEventArgs());
            }
        }

        private bool f_mousedrag;
        /// <summary>
        /// Property. It allows mouse drag action in multiscaleimage.
        /// </summary>
        public bool MouseDrag
        {
            get
            {
                return f_mousedrag;
            }
            set
            {
                f_mousedrag = value;
                if (f_mousedrag)
                {
                    msiMap.MouseLeftButtonDown += evtMouseLeftButtonDown;
                    msiMap.MouseLeftButtonUp += evtMouseLeftButtonUp;
                    this.MouseLeftButtonUp += evtMouseLeftButtonUp;
                    //msiMap.MouseLeave += evtMouseLeftLeave;
                    msiMap.MouseMove += evtMouseMove;
                }
                else
                {
                    msiMap.MouseLeftButtonDown -= evtMouseLeftButtonDown;
                    msiMap.MouseLeftButtonUp -= evtMouseLeftButtonUp;
                    this.MouseLeftButtonUp -= evtMouseLeftButtonUp;
                    //msiMap.MouseLeave -= evtMouseLeftLeave;
                    msiMap.MouseMove -= evtMouseMove;
                }
            }
        }

        /// <summary>
        /// Property. Source is the same property of <c>MultiScaleImage</c>'s.
        /// </summary>
        /// <example>
        /// <ROH:DeepZoomContainer  Source="../GeneratedImages/dzc_output.xml" HorizontalAlignment="Stretch" VerticalAlignment="Top" Height="300" Margin="0,0,0,0" x:Name="dzcTest"/>
        /// </example>
        public MultiScaleTileSource Source
        {
            get
            {
                return msiMap.Source;
            }
            set
            {
                msiMap.Source = value;
            }
        }

        /// <summary>
        /// Property. Visibility of added objects layer.
        /// </summary>
        public Visibility LayerVisibility
        {
            get
            {
                return grdLayer1.Visibility;
            }
            set
            {
                grdLayer1.Visibility = value;
            }
        }

        // Function that converts relative location(0.0 left-top, 1.0 bottom-right) to actual location depending on MultiScaleImage's ViewportWidth, and ViewportOrigin.
        private TranslateTransform ProjectedTranslateTransform(Point value)
        {
            if (msiMap.SubImages.Count > 0)
                return new TranslateTransform() { X = (msiMap.Width * value.X - msiMap.Width * msiMap.ViewportOrigin.X) / msiMap.ViewportWidth, Y = ((msiMap.Width / msiMap.SubImages[0].AspectRatio) * value.Y - msiMap.Width * msiMap.ViewportOrigin.Y) / msiMap.ViewportWidth };
            else
                return null;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DeepZoomContainer()
        {
            InitializeComponent();

            this.SizeChanged += new SizeChangedEventHandler(DeepZoomContainer_SizeChanged);
            Locations = new Dictionary<UIElement, Point>();
            msiMap.Width = this.Width;
            msiMap.Height = this.Height;
            msiMap.ViewportChanged += new RoutedEventHandler(MultiScaleImage_ViewportChanged);
            msiMap.ImageOpenSucceeded += new RoutedEventHandler(msiMap_ImageOpenSucceeded);

            evtMouseLeftButtonDown = new MouseButtonEventHandler(msiMap_MouseLeftButtonDown);
            evtMouseLeftButtonUp = new MouseButtonEventHandler(msiMap_MouseLeftButtonUp);
            //evtMouseLeftLeave = new MouseEventHandler(msiMap_MouseLeave);
            evtMouseMove = new MouseEventHandler(msiMap_MouseMove);

            MouseDrag = true;
            msiMap.UseSprings = true;
        }

        void DeepZoomContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            msiMap.Width = this.Width;
            msiMap.Height = this.Height;
        }

        void msiMap_MouseMove(object sender, MouseEventArgs e)
        {
            if (mousePressed)
            {
                msiMap.ViewportOrigin = new Point(pointOrigin.X - (e.GetPosition(msiMap).X - pointPressed.X) * msiMap.ViewportWidth / msiMap.Width, pointOrigin.Y - ((e.GetPosition(msiMap).Y - pointPressed.Y) * msiMap.ViewportWidth) / (msiMap.Height * msiMap.SubImages[0].AspectRatio));
                CheckLimit();
                Refresh();
            }
        }

        void msiMap_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mousePressed = false;
            msiMap.UseSprings = true;
        }

        void msiMap_MouseLeave(object sender, MouseEventArgs e)
        {
            mousePressed = false;
            msiMap.UseSprings = true;
        }

        void msiMap_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mousePressed = true;
            msiMap.UseSprings = false;
            pointPressed = e.GetPosition(msiMap);
            pointOrigin = msiMap.ViewportOrigin;
        }

        void msiMap_ImageOpenSucceeded(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            foreach (UIElement i in grdLayer1.Children)
            {
                TransformGroup transformFinal = new TransformGroup();
                if (i.GetType() == typeof(PushPin)) i.RenderTransform = ProjectedTranslateTransform(Locations[i]);
                else
                {
                    transformFinal.Children.Add(new ScaleTransform() { ScaleX = 1.0 / msiMap.ViewportWidth, ScaleY = 1.0 / msiMap.ViewportWidth });
                    transformFinal.Children.Add(ProjectedTranslateTransform(Locations[i]));
                    i.RenderTransform = transformFinal;
                }
            }
        }

        private void CheckLimit()
        {
            if (ScrollLimit != null)
            {
                Point pointLimited = new Point(msiMap.ViewportOrigin.X, msiMap.ViewportOrigin.Y);
                Thickness Boundary = new Thickness(ScrollLimit.Value.Left,
                    ScrollLimit.Value.Top,
                    ScrollLimit.Value.Right - msiMap.ViewportWidth,
                    (1.0 / msiMap.SubImages[0].AspectRatio) * (ScrollLimit.Value.Bottom - msiMap.ViewportWidth));
                if (Boundary.Right > Boundary.Left)
                {
                    if (msiMap.ViewportOrigin.X < Boundary.Left) pointLimited.X = Boundary.Left;
                    if (msiMap.ViewportOrigin.X > Boundary.Right) pointLimited.X = Boundary.Right;
                }
                else
                {
                    pointLimited.X = Boundary.Left;
                }
                if (Boundary.Bottom > Boundary.Top)
                {
                    if (msiMap.ViewportOrigin.Y < Boundary.Top) pointLimited.Y = Boundary.Top;
                    if (msiMap.ViewportOrigin.Y > Boundary.Bottom) pointLimited.Y = Boundary.Bottom;
                }
                else
                {
                    pointLimited.Y = Boundary.Top;
                }
                if (pointLimited != msiMap.ViewportOrigin)
                {
                    bool tempUseSpring = msiMap.UseSprings;
                    msiMap.UseSprings = false;
                    msiMap.ViewportOrigin = pointLimited;
                    msiMap.UseSprings = tempUseSpring;
                }
            }
        }

        void MultiScaleImage_ViewportChanged(object sender, RoutedEventArgs e)
        {
            CheckLimit();
            Refresh();
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
                grdLayer1.Children.Add(value);
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
            Point delta = new Point((msiMap.Width / 2 - ((TranslateTransform)pinSender.RenderTransform).X) * msiMap.ViewportWidth / (msiMap.Width), ((msiMap.Height / 2 - ((TranslateTransform)pinSender.RenderTransform).Y)) * msiMap.ViewportWidth / (msiMap.Height * msiMap.SubImages[0].AspectRatio));
            msiMap.ViewportOrigin = new Point(msiMap.ViewportOrigin.X - delta.X, msiMap.ViewportOrigin.Y - delta.Y);
        }


        //Loads the DeepZoom image
        private void image_Loaded(object sender, RoutedEventArgs e)
        {
            DeepZoomImageTileSource src = new DeepZoomImageTileSource(new Uri(Application.Current.Host.Source,
                //"/GeneratedImages/dzc_output.xml"));

            "C:/Users/Svetoslav/Documents/Expression/Deep Zoom Composer Projects/UntitledProject2/Exported Data/tiff/GeneratedImages/dzc_output.xml"));
            //"C:\\Users\\Svetoslav\\Documents\\Expression\\Deep Zoom Composer Projects\\UntitledProject1\\Exported Data\\preview\\GeneratedImages\\dzc_output.xml"));
            msiMap.Source = src;
        }

    }
}