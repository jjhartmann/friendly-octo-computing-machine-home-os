#region Using Region

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Media;
using System.Reflection;
using System.Windows.Documents;

#endregion

namespace HomeOS.Hub.Tools.UpdateManager.MultiWindows
{
    /// <summary>
    /// A canvas which is used to drag element around. An attached property, IsDraggable, is used to determine which portion of
    /// an element cues dragging. Then the entire element is dragged when the mouse is moved. In the future it would be nice to
    /// move the IsDraggable property to this class and add another attachable property denoting the parent element which should be 
    /// dragged.
    /// </summary>
    public class DragCanvas : Canvas
    {
        #region Private Methods

        private bool isDown = false;       // true if mouse is down
        private Point startpoint;          // the point which the mouse down was captured
        private UIElement originalElement; // placeholder for the object being clicked
        private bool isDragging = false;   // true if object is currently being dragged
        private double originalLeft;       // the original elements left position
        private double originalTop;        // the original elements top position
        private bool isdrag = false;       // true if element contains the IsDraggable attached property

        #endregion 

        #region Constructor

        /// <summary>
        /// The DragCanvas Constructor
        /// </summary>
        public DragCanvas()
            : base()
        { }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Used to find DependencyObject with an IsDraggable attached property
        /// </summary>
        /// <param name="htr"></param>
        /// <returns></returns>
        protected HitTestResultBehavior MyHitTestResult(HitTestResult htr)
        {
            return HitTestResultBehavior.Stop; 
        }

        /// <summary>
        /// Used to find DependencyObject with an IsDraggable attached property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected HitTestFilterBehavior MyHitTestFilter(DependencyObject obj)
        {
            isdrag = (bool)obj.GetValue(MwiChild.IsDraggableProperty);

            if (isdrag)
                return HitTestFilterBehavior.ContinueSkipSelf;
            else
                return HitTestFilterBehavior.Continue;
        }

        /// <summary>
        /// Handles the mouse left button down click event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            if (!e.Source.Equals(this)) // do drag if the source is the DragCanvas
            {
                isDown = true;          // mouse is down, save the click point
                startpoint = e.GetPosition(this);
                // if an element clicked or a child of the element is clicked and has the IsDraggable property the proceed
                VisualTreeHelper.HitTest((UIElement)e.Source, new HitTestFilterCallback(MyHitTestFilter), new HitTestResultCallback(MyHitTestResult), new PointHitTestParameters(e.GetPosition((UIElement)e.Source)));
                if (isdrag)
                {
                    // if the element is a MwiChild then stop, otherwise find if the element has a parent type of MwiChild
                    if (e.Source.GetType().Equals(typeof(MwiChild))) 
                        originalElement = (UIElement)e.Source;
                    else
                        originalElement = (UIElement)VisualTreeSearch.FindByParentType((UIElement)e.Source, typeof(MwiChild)); 
                    if (originalElement != null)
                    {
                        int zindex = Canvas.GetZIndex(originalElement); // set the s-index of the element

                        ((MwiChild)originalElement).IsSelected = true;  // select the element because is was clicked

                        this.CaptureMouse();                            // capture the mouse
                        e.Handled = true;                               // handle the event
                    }
                }
                
            }
        }

        /// <summary>
        /// Handle the mouse left button up event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            // if the mouse is down and an element is selected then stop dragging
            if (isDown && originalElement != null)
            {
                DragFinished(false);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handle the mouse move event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            // if the mouse is down and an element is selected then get the mouse position and start dragging
            if (isDown && originalElement != null)
            {
                Point currentPosition = Mouse.GetPosition(this);
                if (!isDragging)
                    DragStarted(currentPosition);
                else if (isDragging)
                    DragMoved(currentPosition);
            }
        }

        #endregion 

        #region Private Methods

        /// <summary>
        /// Start the dragging process when an element with the IsDraggable property is click that has a MwiChild parent.
        /// </summary>
        /// <param name="currentPosition"></param>
        private void DragStarted(Point currentPosition)
        {
            isDragging = true;
            originalLeft = Canvas.GetLeft(originalElement);
            originalTop = Canvas.GetTop(originalElement);
            
            DragMoved(currentPosition);                  
        }

        /// <summary>
        /// Drag the element about the Canvas by updating its Canavas position.
        /// </summary>
        /// <param name="currentPosition"></param>
        private void DragMoved(Point currentPosition)
        {
            double elementLeft = (currentPosition.X - startpoint.X) + originalLeft;
            double elementTop = (currentPosition.Y - startpoint.Y) + originalTop;
            
            // Find the panel under the drag canavas and get its size. This way items can be dragged over
            // the minimized child elements
            DockPanel panel = (DockPanel)VisualTreeSearch.FindByParentType(this, typeof(DockPanel));
            Size panelSize = this.RenderSize;
            if (panel != null)
                panelSize = panel.RenderSize;

            // make sure the element is within the bounds of the DragCanvas
            if (elementLeft < 0)
                elementLeft = 0;
            else if ((elementLeft + originalElement.RenderSize.Width) > panelSize.Width)
                elementLeft = panelSize.Width - originalElement.RenderSize.Width;
            Canvas.SetLeft(originalElement, elementLeft);

            // make sure the element is within the bounds of the DragCanvas
            if (elementTop < 0)
                elementTop = 0;
            else if ((elementTop + originalElement.RenderSize.Height) > panelSize.Height)
                elementTop = panelSize.Height - originalElement.RenderSize.Height;
            Canvas.SetTop(originalElement, elementTop);
        }

        /// <summary>
        /// Stops the dragging of an element.
        /// </summary>
        /// <param name="canceled"></param>
        private void DragFinished(bool canceled)
        {
            Mouse.Capture(null); // release the mouse
            if (isDragging)
            {                
                originalElement = null;
            }
            
            isDragging = false;
            isDown = false;
        }

        #endregion 
    }
}
