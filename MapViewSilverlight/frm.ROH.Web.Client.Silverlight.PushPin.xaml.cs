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

namespace ROH.Web.Client.Silverlight
{
    /// <summary>
    /// PushPin Class
    /// (ver. 1.00.00)
    /// 
    /// This class has a vector image of the pushpin. Orientation of this image is set to bottom-pinpoint, so its position is exactly same as its aim point.
    /// There are 3 variables; Color, Initial, Dot, so you can modify its style using those.
    /// 
    /// ROH Holding Company
    /// Won Dong (fkiller@gmail.com)
    /// </summary>
    public partial class PushPin : UserControl
    {
        /// <summary>
        /// PushPin() Constructor
        /// </summary>
        /// <remarks>
        /// Constructor without parametors. Its default style is yellow-backgrounded dot.
        /// </remarks>
        public PushPin()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// PushPin() Constructor
        /// </summary>
        /// <remarks>
        /// Constructor with initial and location.
        /// </remarks>
        /// <param name="Initial">
        /// It indicates initial charactor of pushpin. If it's space ' ', then dot will be shown. If initial is <c>char.MinValue</c>, then nothing will be shown.
        /// </param>
        /// <param name="Location">
        /// Location of pushpin. Its orientation is set to bottom-pinpoint, so its position is exactly same as its aim point.
        /// </param>
        public PushPin(char Initial, Point Location)
        {
            this.InitializeComponent();
            txtTitle.Text = Convert.ToString(Initial);
            if (Initial != char.MinValue && Initial != ' ') txtTitle.Visibility = Visibility.Visible;
            if (Initial == ' ') shpDot.Visibility = Visibility.Visible;
            TranslateTransform matTranslate = new TranslateTransform() { X = Location.X, Y = Location.Y };
            this.RenderTransform = matTranslate;
        }

        /// <summary>
        /// PushPin() Constructor
        /// </summary>
        /// <remarks>
        /// Constructor with initial, location, and base panel.
        /// </remarks>
        /// <param name="Initial">
        /// It indicates initial charactor of pushpin. If it's space ' ', then dot will be shown. If initial is <c>char.MinValue</c>, then nothing will be shown.
        /// </param>
        /// <param name="Location">
        /// Location of pushpin. Its orientation is set to bottom-pinpoint, so its position is exactly same as its aim point.
        /// </param>
        /// <param name="Base">
        /// Panel that will be parent of this class.
        /// </param>
        public PushPin(char Initial, Point Location, Panel Base)
        {
            this.InitializeComponent();
            txtTitle.Text = Convert.ToString(Initial);
            if (Initial != char.MinValue && Initial != ' ') txtTitle.Visibility = Visibility.Visible;
            if (Initial == ' ') shpDot.Visibility = Visibility.Visible;
            Base.Children.Add(this);

            TranslateTransform matTranslate = new TranslateTransform() { X = Location.X, Y = Location.Y };
            this.RenderTransform = matTranslate;
        }

        /// <summary>
        /// PushPin() Constructor
        /// </summary>
        /// <remarks>
        /// Constructor with initial, location, and base panel.
        /// </remarks>
        /// <param name="Initial">
        /// It indicates initial charactor of pushpin. If it's space ' ', then dot will be shown. If initial is <c>char.MinValue</c>, then nothing will be shown.
        /// </param>
        /// <param name="Color">
        /// Pushpin's color. Solid color only.
        /// </param>
        public PushPin(char Initial, Color Color)
        {
            this.InitializeComponent();
            txtTitle.Text = Convert.ToString(Initial);
            if (Initial != char.MinValue && Initial != ' ') txtTitle.Visibility = Visibility.Visible;
            if (Initial == ' ') shpDot.Visibility = Visibility.Visible;
            shpPushpin.Fill = new SolidColorBrush(Color);
        }

        private double p_scale = 1.0;
        /// <summary>
        /// Property. Size of pushpin. Default value is 1.0 (Width:35, Height:63).
        /// </summary>
        public double Scale
        {
            get
            {
                return p_scale;
            }
            set
            {
                p_scale = value;
                ScaleTransform matScale = new ScaleTransform() { ScaleX = p_scale, ScaleY = p_scale };
                grdRoot.RenderTransform = matScale;
            }
        }
    }
}