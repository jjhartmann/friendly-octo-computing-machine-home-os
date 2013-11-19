#region Using Region

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Controls;

#endregion

namespace HomeOS.Hub.Tools.UpdateManager.MultiWindows
{
    /// <summary>
    /// This class is used to resize the Window and Children of the MwiWindow.
    /// This should really be split up into two separate classes and made generic so
    /// that any class can be used not just a MwiChild.
    /// </summary>
    public class ResizeAdorner : Adorner
    {
        #region Private Variables

        private Rect[] mHotSpots = new Rect[12]; // number of hot-spots on an object
        private Point mCurrentPoint;             // the current point clicked
        private Point mLastPoint;                // the last point clicked
        private HotSpot mCurrentHotSpot = HotSpot.None; // the current hot-spot clicked, default none
        private bool mIsDown = false;            // true if the mouse is currently down
        private Window mWindow = null;           // contains the window object if there is one
        private double mMinWidth = 30.0d;        // the minimum width of the object
        private double mMinHeight = 30.0d;       // the minimum height of the object
        private double mMaxWidth = 300.0d;       // the maximum width of the object
        private double mMaxHeight = 300.0d;      // the maximum height of the object

        #endregion

        #region Enums

        /// <summary>
        /// An enum containing all of the hotspots found on an objectes bounding rectangle
        /// </summary>
        private enum HotSpot
        {
            TopLeft1 = 0, // vertical line of the top-left corner
            TopLeft2,     // horizontal line of the top-left corner
            Top,
            TopRight1,    // horizontal line of the top-right corner
            TopRight2,    // vertical line of the top-right corner
            Right,
            BottomRight1, // vertical line of the bottom-right corner
            BottomRight2, // horizontal line of the bottom-right corner
            Bottom,
            BottomLeft1,  // horizontal line of the bottom-left corner
            BottomLeft2,  // vertical line of the bottom-left corner
            Left,
            None          // default
        }

        #endregion 

        #region Constructors

        /// <summary>
        /// Constructor used when a attached MwiChild is being resized.
        /// </summary>
        /// <param name="adornedElement"></param>
        public ResizeAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            if (AdornedElement.GetType().Equals(typeof(MwiChild)))
            {
                mMinWidth = ((MwiChild)AdornedElement).MinSize.Width;
                mMinHeight = ((MwiChild)AdornedElement).MinSize.Height;
            }            
        }

        /// <summary>
        /// Constructor used when a detached MwiChild is being resized.
        /// </summary>
        /// <param name="adornedElement"></param>
        /// <param name="win"></param>
        public ResizeAdorner(UIElement adornedElement, Window win)
            : base(adornedElement)
        {
            mWindow = win; // the window containing the detached MwiChild

            try
            {
                mMinWidth = win.MinWidth;
                mMinHeight = win.MinHeight;
                mMaxWidth = win.MaxWidth;
                mMaxHeight = win.MaxHeight;
            }
            catch { }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// When the left mouse button is down get the current point and capture the mouse.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            if (!mIsDown && mCurrentHotSpot != HotSpot.None)
            {
                if (AdornedElement.GetType().Equals(typeof(MwiChild)))
                    ((MwiChild)AdornedElement).IsSelected = true; // promote the child to the front since it has been selected.
                mIsDown = true; // mouse button is down
                mCurrentPoint = e.GetPosition(this);
                mLastPoint = mCurrentPoint;
                this.CaptureMouse();
            }
        }

        /// <summary>
        /// When the left mouse button is released then stop the mouse capture. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            if (mIsDown)
            {
                mIsDown = false; // mouse button is no longer down 
                Mouse.Capture(null);
            }
        }

        /// <summary>
        /// When the mouse is moved and while the left mouse button is down, check to see if any hotspot is selected. 
        /// If not then select a hotspot if one is found and change the cursor accrodingly. If one is selected then
        /// update the objects size and location.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            Point p = e.GetPosition(this);
            if (!mIsDown)
            {                
                mCurrentHotSpot = HotSpot.None;
                for (int i = 0; i < mHotSpots.Length; i++)
                {
                    if (mHotSpots[i].Contains(p))
                    {
                        mCurrentHotSpot = (HotSpot)i;
                        UpdateCursor(mCurrentHotSpot);
                        break;
                    }
                }
            }
            else
            {
                UpdateSize(mCurrentHotSpot, p);
            }
        }

        /// <summary>
        /// Calculates the hotspots that make up the bounding rectangle. Draws the bounding rectangle as Transparent
        /// rectangles to the screen.
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            // Edge and size where intended to be used to denote the size of the bounding line and the length of the 
            // corners. However as things where updated size and edge took on different meanings and now are used as a
            // ratio to get the bounding rectangle to fit the current object.
            double size = 4.0d;
            double edge = 6.0d;

            Rect rect = new Rect(AdornedElement.DesiredSize); 
            if (mWindow != null)
            {
                rect = new Rect(new Size(mWindow.Width, mWindow.Height));
                size = 8.0d;
                edge = 3.0d;
            }
            
            mHotSpots[(int)HotSpot.TopLeft1] = new Rect(new Point(-size / 2.0d, size / 2.0d), new Size(Math.Max(size, 0), Math.Max(edge * size, 0)));
            mHotSpots[(int)HotSpot.TopLeft2] = new Rect(new Point(-size / 2.0d, -size / 2.0d), new Size(Math.Max(edge * size, 0), Math.Max(size, 0)));
            mHotSpots[(int)HotSpot.Top] = new Rect(new Point(size * (edge - 0.5d), -size / 2.0d), new Size(Math.Max(rect.Width - (2.0d * (size * (edge - 0.5d))), 0), Math.Max(size, 0)));
            mHotSpots[(int)HotSpot.TopRight1] = new Rect(new Point(rect.Width - (size * (edge - 0.5d)), -size / 2.0d), new Size(Math.Max(edge * size, 0), Math.Max(size, 0)));
            mHotSpots[(int)HotSpot.TopRight2] = new Rect(new Point(rect.Width - size / 2.0d, size / 2.0d), new Size(Math.Max(size, 0), Math.Max(edge * size, 0)));
            mHotSpots[(int)HotSpot.Right] = new Rect(new Point(rect.Width - (size / 2.0d), size * (edge + 0.5d)), new Size(Math.Max(size, 0), Math.Max(rect.Height - (2.0d * (size * (edge + 0.5d))), 0)));
            mHotSpots[(int)HotSpot.BottomRight1] = new Rect(new Point(rect.Width - size / 2.0d, rect.Height - (size * (edge + 0.5d))), new Size(Math.Max(size, 0), Math.Max(edge * size, 0)));
            mHotSpots[(int)HotSpot.BottomRight2] = new Rect(new Point(rect.Width - (size * (edge - 0.5d)), rect.Height - size / 2.0d), new Size(Math.Max(edge * size, 0), Math.Max(size, 0)));
            mHotSpots[(int)HotSpot.Bottom] = new Rect(new Point((size * (edge - 0.5d)), rect.Height - size / 2.0d), new Size(Math.Max(rect.Width - (2.0d * (size * (edge - 0.5d))), 0), Math.Max(size, 0)));
            mHotSpots[(int)HotSpot.BottomLeft1] = new Rect(new Point(-size / 2.0d, rect.Height - size / 2.0d), new Size(Math.Max(edge * size, 0), Math.Max(size, 0)));
            mHotSpots[(int)HotSpot.BottomLeft2] = new Rect(new Point(-size / 2.0d, rect.Height - (size * (edge + 0.5d))), new Size(Math.Max(size, 0), Math.Max(edge * size, 0)));
            mHotSpots[(int)HotSpot.Left] = new Rect(new Point(-size / 2.0d, (size * (edge + 0.5d))), new Size(Math.Max(size, 0), Math.Max(rect.Height - (2.0d * (size * (edge + 0.5d))), 0)));
                    
            for (int i = 0; i < mHotSpots.Length; i++)
            {
                drawingContext.DrawRectangle(new SolidColorBrush(Colors.Transparent), null, mHotSpots[i]);
                //drawingContext.DrawRectangle(new SolidColorBrush(i % 2 == 0 ? Colors.Red : Colors.White), null, mHotSpots[i]); // used to see the bounding rectangle visually
            }
        }

        #endregion 

        #region Private Methods

        /// <summary>
        /// Update the size and location of the object based on the hotspot that is selected.
        /// </summary>
        /// <param name="hspot"></param>
        /// <param name="p"></param>
        private void UpdateSize(HotSpot hspot, Point p)
        {
            double width = (double)AdornedElement.GetValue(WidthProperty);
            double height = (double)AdornedElement.GetValue(HeightProperty);
            double left = Canvas.GetLeft(AdornedElement);
            double top = Canvas.GetTop(AdornedElement);
            if (mWindow != null)
            {
                width = mWindow.Width;
                height = mWindow.Height;
                left = mWindow.Left;
                top = mWindow.Top;                
            }
            
            switch (hspot)
            {
                case HotSpot.TopLeft1:
                case HotSpot.TopLeft2:      
                    UpdateWidth(width + (mCurrentPoint.X - p.X), left + (p.X - mCurrentPoint.X));
                    UpdateHeight(height + (mCurrentPoint.Y - p.Y), top + (p.Y - mCurrentPoint.Y));
                    break;
                case HotSpot.Top:                    
                    UpdateHeight(height + (mCurrentPoint.Y - p.Y), top + (p.Y - mCurrentPoint.Y));
                    break;
                case HotSpot.TopRight1:
                case HotSpot.TopRight2:
                    UpdateWidth(width - (mLastPoint.X - p.X), p);
                    UpdateHeight(height + (mCurrentPoint.Y - p.Y), top + (p.Y - mCurrentPoint.Y));
                    mLastPoint = p; 
                    break;
                case HotSpot.Right:
                    UpdateWidth(width - (mLastPoint.X - p.X), p);
                    mLastPoint = p; 
                    break;
                case HotSpot.BottomRight1:
                case HotSpot.BottomRight2:
                    UpdateWidth(width - (mLastPoint.X - p.X), p);
                    UpdateHeight(height - (mLastPoint.Y - p.Y), p);
                    mLastPoint = p; 
                    break;
                case HotSpot.Bottom:
                    UpdateHeight(height - (mLastPoint.Y - p.Y), p);
                    mLastPoint = p; 
                    break;
                case HotSpot.BottomLeft1:
                case HotSpot.BottomLeft2:
                    UpdateWidth(width + (mCurrentPoint.X - p.X), left + (p.X - mCurrentPoint.X));
                    UpdateHeight(height - (mLastPoint.Y - p.Y), p);
                    mLastPoint = p; 
                    break;
                case HotSpot.Left:
                    UpdateWidth(width + (mCurrentPoint.X - p.X), left + (p.X - mCurrentPoint.X));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Updates the cursor based on the hotspot that is currently selected or hovered over.
        /// </summary>
        /// <param name="hspot"></param>
        private void UpdateCursor(HotSpot hspot)
        {
            switch (hspot)
            {
                case HotSpot.TopLeft1:
                case HotSpot.TopLeft2:
                    this.Cursor = Cursors.SizeNWSE;
                    break;
                case HotSpot.Top:
                    this.Cursor = Cursors.SizeNS;
                    break;
                case HotSpot.TopRight1:
                case HotSpot.TopRight2:
                    this.Cursor = Cursors.SizeNESW;
                    break;
                case HotSpot.Right:
                    this.Cursor = Cursors.SizeWE;
                    break;
                case HotSpot.BottomRight1:
                case HotSpot.BottomRight2:
                    this.Cursor = Cursors.SizeNWSE;
                    break;
                case HotSpot.Bottom:
                    this.Cursor = Cursors.SizeNS;
                    break;
                case HotSpot.BottomLeft1:
                case HotSpot.BottomLeft2:
                    this.Cursor = Cursors.SizeNESW;
                    break;
                case HotSpot.Left:
                    this.Cursor = Cursors.SizeWE;
                    break;
                default:
                    this.Cursor = Cursors.Arrow;
                    break;
            }
        }

        /// <summary>
        /// Updates the width to the newwidth provided based on the current mouse point.
        /// </summary>
        /// <param name="newwidth"></param>
        /// <param name="p"></param>
        private void UpdateWidth(double newwidth, Point p)
        {
            if (AdornedElement.GetType().Equals(typeof(MwiChild)))
                mMaxWidth = ((MwiChild)AdornedElement).MwiParent.ActualWidth - 3 - Canvas.GetLeft(AdornedElement);

            // if within the min and max width then update the width to the new width
            if ((newwidth >= mMinWidth && p.X >= mMinWidth) &&
                (newwidth <= mMaxWidth && p.X <= mMaxWidth))
            {
                if (mWindow != null)
                    mWindow.Width = newwidth;
                else                    
                    AdornedElement.SetValue(MwiChild.WidthProperty, newwidth);
            }
            else // otherwise set the width to the min or max bound accordingly
            {
                if (mWindow != null)
                {
                    if (!(newwidth <= mMaxWidth && p.X <= mMaxWidth))
                        mWindow.Width = mMaxWidth;
                    else
                        mWindow.Width = mMinWidth;
                }
                else
                {
                    if (!(newwidth <= mMaxWidth && p.X <= mMaxWidth))
                        AdornedElement.SetValue(MwiChild.WidthProperty, mMaxWidth);
                    else
                        AdornedElement.SetValue(MwiChild.WidthProperty, mMinWidth);
                }
            }
        }

        /// <summary>
        /// Update the width to the newwidth and the left location to newleft.
        /// </summary>
        /// <param name="newwidth"></param>
        /// <param name="newleft"></param>
        private void UpdateWidth(double newwidth, double newleft)
        {
            // if within the min width and a location greater than or equal to zero then update the width and the left location
            if ((newwidth >= mMinWidth) &&
                (newleft >= 0))
            {
                if (mWindow != null)
                {
                    mWindow.SetValue(Window.WidthProperty, newwidth); 
                    mWindow.SetValue(Window.LeftProperty, newleft); 
                }
                else
                {
                    Canvas.SetLeft(AdornedElement, newleft);
                    AdornedElement.SetValue(MwiChild.WidthProperty, newwidth);
                }                
            }
            else // otherwise set the width the the min or max bound accordingly
            {
                if (mWindow != null)
                {
                    double width = mWindow.Width;
                    double left = mWindow.Left;
                    if (!(newleft >= 0))
                    {
                        mWindow.Left = 0;
                        mWindow.Width = left + width;
                    }
                    else
                    {
                        mWindow.Left += (mWindow.Width - mMinWidth);
                        mWindow.Width = mMinWidth;
                    }
                }
                else
                {
                    double width = (double)AdornedElement.GetValue(WidthProperty);
                    double left = Canvas.GetLeft(AdornedElement);
                    if (!(newleft >= 0))
                    {
                        Canvas.SetLeft(AdornedElement, 0);
                        AdornedElement.SetValue(MwiChild.WidthProperty, left + width);
                    }
                    else
                    {
                        Canvas.SetLeft(AdornedElement, left + (width - mMinWidth));
                        AdornedElement.SetValue(MwiChild.WidthProperty, mMinWidth);
                    }
                }                
            }
        }

        /// <summary>
        /// Update the height with the newheight based on the current mouse point.
        /// </summary>
        /// <param name="newheight"></param>
        /// <param name="p"></param>
        private void UpdateHeight(double newheight, Point p)
        {
            if (AdornedElement.GetType().Equals(typeof(MwiChild)))
                mMaxHeight = ((MwiChild)AdornedElement).MwiParent.ActualHeight - 3 - Canvas.GetTop(AdornedElement) ;

            // if within the min and max height then update the height to the new height
            if ((newheight >= mMinHeight && p.Y >= mMinHeight) && 
                (newheight <= mMaxHeight && p.Y <= mMaxHeight))
            {
                if (mWindow != null)
                    mWindow.Height = newheight;
                else
                    AdornedElement.SetValue(MwiChild.HeightProperty, newheight);
            }
            else // otherwise set the height to the min or max bound accordingly
            {
                if (mWindow != null)
                {
                    if (!(newheight <= mMaxHeight && p.Y <= mMaxHeight))
                        mWindow.Height = mMaxHeight;
                    else
                        mWindow.Height = mMinHeight;
                }
                else
                {
                    if (!(newheight <= mMaxHeight && p.Y <= mMaxHeight))
                        AdornedElement.SetValue(MwiChild.HeightProperty, mMaxHeight);
                    else
                        AdornedElement.SetValue(MwiChild.HeightProperty, mMinHeight);
                }
            }
        }

        /// <summary>
        /// Update the height to the newheight and the top location to newtop.
        /// </summary>
        /// <param name="newwidth"></param>
        /// <param name="newleft"></param>
        private void UpdateHeight(double newheight, double newtop)
        {
            // if within the min height and a location greater than or equal to zero then update the height and the top location
            if ((newheight >= mMinHeight) &&
                (newtop >= 0))
            {
                if (mWindow != null)
                {                    
                    mWindow.Height = newheight;
                    mWindow.Top = newtop;
                }
                else
                {
                    Canvas.SetTop(AdornedElement, newtop);
                    AdornedElement.SetValue(MwiChild.HeightProperty, newheight);
                }
            }
            else // otherwise set the height the the min or max bound accordingly
            {
                if (mWindow != null)
                {
                    double height = mWindow.Height;
                    double top = mWindow.Top;
                    if (!(newtop >= 0))
                    {
                        mWindow.Top = 0;
                        mWindow.Height = top + height;
                    }
                    else
                    {
                        mWindow.Top += mWindow.Height - mMinHeight;
                        mWindow.Height = mMinHeight;
                    }
                }
                else
                {
                    double height = (double)AdornedElement.GetValue(HeightProperty);
                    double top = Canvas.GetTop(AdornedElement);
                    if (!(newtop >= 0))
                    {
                        Canvas.SetTop(AdornedElement, 0);
                        AdornedElement.SetValue(MwiChild.HeightProperty, top + height);
                    }
                    else
                    {
                        Canvas.SetTop(AdornedElement, top + (height - mMinHeight));
                        AdornedElement.SetValue(MwiChild.HeightProperty, mMinHeight);
                    }
                }
            }            
        }

        #endregion
    }
}
