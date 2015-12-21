using System;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Input;

namespace MapViewSilverlight
{
    /// <summary>
    /// Provides mouse panning and zooming support for a MultiScaleImage.
    /// </summary>
    public class MultiScaleImageWrapper
    {
        /// <summary>
        /// Specifies the zoom factor when the MultiScaleImage receives a mouse click event.
        /// </summary>
        private const double ZOOM_FACTOR_CLICK = 2;

        /// <summary>
        /// Specifies the zoom factor when the MultiScaleImage receives a mouse wheel event.
        /// </summary>
        private const double ZOOM_FACTOR_WHEEL = 1.25;

        /// <summary>
        /// Listens to mouse wheel events raised by the HTML document.
        /// </summary>
        private static MouseWheelListener mouseWheelListener = new MouseWheelListener();

        /// <summary>
        /// Indicates whether or not the mouse is over the MultiScaleImage.
        /// </summary>
        private bool isMouseOver;

        /// <summary>
        /// Indicates whether or not the left mouse button is pressed for the MultiScaleImage.
        /// </summary>
        private bool isMouseDown;

        /// <summary>
        /// Indicates whether or not the mouse is being dragged over the MultiScaleImage.
        /// </summary>
        private bool isMouseDrag;

        /// <summary>
        /// Specifies the initial mouse drag origin for the MultiScaleImage.
        /// </summary>
        private Point dragOrigin;

        /// <summary>
        /// Specifies the initial mouse drag position for the MultiScaleImage.
        /// </summary>
        private Point dragPosition;

        /// <summary>
        /// Specifies the current mouse cursor position for the MultiScaleImage.
        /// </summary>
        private Point cursorPosition;

        /// <summary>
        /// Gets the wrapped MultiScaleImage instance. 
        /// </summary>
        public MultiScaleImage Image { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MultiScaleImageWrapper class to provide mouse panning and zooming support for the specified MultiScaleImage.
        /// </summary>
        /// <param name="image">The MultiScaleImage instance to wrap.</param>
        public MultiScaleImageWrapper(MultiScaleImage image)
        {
            Image = image;
            Image.MouseEnter += Image_MouseEnter;
            Image.MouseLeave += Image_MouseLeave;
            Image.MouseMove += Image_MouseMove;
            Image.MouseLeftButtonDown += Image_MouseLeftButtonDown;
            Image.MouseLeftButtonUp += Image_MouseLeftButtonUp;
            mouseWheelListener.MouseWheel += Image_MouseWheel;
        }

        /// <summary>
        /// Handles the MouseEnter event for the MultiScaleImage. Sets the isMouseOver flag to true and saves the currentPosition of the cursor.
        /// </summary>
        /// <param name="sender">The wrapped MultiScaleImage instance.</param>
        /// <param name="e">The mouse event arguments.</param>
        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            isMouseOver = true;
            cursorPosition = e.GetPosition(Image);
        }

        /// <summary>
        /// Handles the MouseLeave event for the MultiScaleImage. Sets the isMouseOver flag to false.
        /// </summary>
        /// <param name="sender">The wrapped MultiScaleImage instance.</param>
        /// <param name="e">The mouse event arguments.</param>
        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            isMouseOver = false;
        }

        /// <summary>
        /// Handles the MouseMove event for the MultiScaleImage. Saves the currentPosition of the cursor and pans when the mouse is dragged.
        /// </summary>
        /// <param name="sender">The wrapped MultiScaleImage instance.</param>
        /// <param name="e">The mouse event arguments.</param>
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            cursorPosition = e.GetPosition(Image);

            if (isMouseDown)
            {
                isMouseDrag = true;

                Point origin = new Point();
                origin.X = dragOrigin.X - (cursorPosition.X - dragPosition.X) / Image.ActualWidth * Image.ViewportWidth;
                origin.Y = dragOrigin.Y - (cursorPosition.Y - dragPosition.Y) / Image.ActualWidth * Image.ViewportWidth;

                Image.ViewportOrigin = origin;
            }
        }

        /// <summary>
        /// Handles the MouseLeftButtonDown event for the MultiScaleImage. Saves the initial dragOrigin and dragPosition in case the user begins to pan.
        /// </summary>
        /// <param name="sender">The wrapped MultiScaleImage instance.</param>
        /// <param name="e">The mouse button event arguments.</param>
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image.CaptureMouse();
            mouseWheelListener.IsEnabled = false;

            isMouseDown = true;
            isMouseDrag = false;

            dragOrigin = Image.ViewportOrigin;
            dragPosition = e.GetPosition(Image);
        }

        /// <summary>
        /// Handles the MouseLeftButtonUp event for the MultiScaleImage. Zooms in if the user is not completing a pan and resets the mouse.
        /// </summary>
        /// <param name="sender">The wrapped MultiScaleImage instance.</param>
        /// <param name="e">The mouse button event arguments.</param>
        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!isMouseDrag && isMouseDown)
            {
                bool isShiftDown = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
                double factor = isShiftDown ? 1 / ZOOM_FACTOR_CLICK : ZOOM_FACTOR_CLICK;
                Zoom(factor, cursorPosition);
            }

            isMouseDown = false;
            isMouseDrag = false;

            Image.ReleaseMouseCapture();
            mouseWheelListener.IsEnabled = true;
        }

        /// <summary>
        /// Handles the MouseWheel event for the MultiScaleImage. Zooms in or out depending on the mouse wheel direction.
        /// </summary>
        /// <param name="sender">The MouseWheelWrapper instance.</param>
        /// <param name="e">The mouse wheel event arguments.</param>
        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!isMouseOver || e.Delta == 0)
                return;

            double factor = e.Delta > 0 ? ZOOM_FACTOR_WHEEL : 1 / ZOOM_FACTOR_WHEEL;
            Zoom(factor, cursorPosition);
            e.Handled = true;
        }

        /// <summary>
        /// Zooms in or out of the MultiScaleImage.
        /// </summary>
        /// <param name="factor">The zoom factor.</param>
        /// <param name="point">The zoom center point.</param>
        public void Zoom(double factor, Point point)
        {
            Point logicalPoint = Image.ElementToLogicalPoint(point);
            Image.ZoomAboutLogicalPoint(factor, logicalPoint.X, logicalPoint.Y);
        }

        /// <summary>
        /// Provides data for a mouse wheel event.
        /// </summary>
        private class MouseWheelEventArgs : EventArgs
        {
            /// <summary>
            /// Gets a value that specifies the delta and direction of the mouse wheel event. This value is normalized across browsers.
            /// </summary>
            public double Delta { get; private set; }

            /// <summary>
            /// Gets or sets a value that indicates whether or not the mouse wheel event has been handled.
            /// </summary>
            public bool Handled { get; set; }

            /// <summary>
            /// Initializes a new instance of the MouseWheelEventArgs class with the specified delta.
            /// </summary>
            /// <param name="delta">The delta calculated for the mouse wheel event.</param>
            public MouseWheelEventArgs(double delta)
            {
                Delta = delta;
            }
        }

        /// <summary>
        /// Provides cross-browser support for the mouse wheel.
        /// </summary>
        private class MouseWheelListener
        {
            /// <summary>
            /// Indicates whether or not the mouse wheel is enabled.
            /// </summary>
            public bool IsEnabled { get; set; }

            /// <summary>
            /// Occurs when a mouse wheel event is fired.
            /// </summary>
            public event EventHandler<MouseWheelEventArgs> MouseWheel;

            /// <summary>
            /// Initializes a new instance of the MouseWheelListener class that listens to mouse wheel events fired by the HTML document. 
            /// </summary>
            public MouseWheelListener()
            {
                if (HtmlPage.IsEnabled)
                {
                    HtmlPage.Document.AttachEvent("DOMMouseScroll", Plugin_MouseWheelFirefox);
                    HtmlPage.Document.AttachEvent("onmousewheel", Plugin_MouseWheelOther);
                    IsEnabled = true;
                }
            }

            /// <summary>
            /// Handles mouse wheel events for Firefox.
            /// </summary>
            /// <param name="sender">The HTML element for the plug-in.</param>
            /// <param name="e">The HTML event arguments.</param>
            private void Plugin_MouseWheelFirefox(object sender, HtmlEventArgs e)
            {
                if (!IsEnabled)
                {
                    e.PreventDefault();
                    return;
                }

                double delta = (double)e.EventObject.GetProperty("detail") / -3;
                MouseWheelEventArgs args = new MouseWheelEventArgs(delta);
                MouseWheel(this, args);

                if (args.Handled)
                    e.PreventDefault();
            }

            /// <summary>
            /// Handles mouse wheel events for browsers other than Firefox.
            /// </summary>
            /// <param name="sender">The HTML element for the plug-in.</param>
            /// <param name="e">The HTML event arguments.</param>
            private void Plugin_MouseWheelOther(object sender, HtmlEventArgs e)
            {
                if (!IsEnabled)
                {
                    e.EventObject.SetProperty("returnValue", false);
                    return;
                }

                double delta = (double)e.EventObject.GetProperty("wheelDelta") / 120;
                MouseWheelEventArgs args = new MouseWheelEventArgs(delta);
                MouseWheel(this, args);

                if (args.Handled)
                    e.EventObject.SetProperty("returnValue", false);
            }
        }
    }
}