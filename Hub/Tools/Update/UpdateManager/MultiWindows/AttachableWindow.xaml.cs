#region Using Region

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

#endregion

namespace HomeOS.Hub.Tools.UpdateManager.MultiWindows
{
    /// <summary>
    /// Interaction logic for AttachableWindow.xaml
    /// </summary>
    public partial class AttachableWindow : System.Windows.Window
    {
        #region Attached Properties

        /// <summary>
        /// Attached to an uielement which needs its own active property, such as the min, max and close buttons
        /// </summary>
        public static readonly DependencyProperty IsElementActiveProperty = DependencyProperty.RegisterAttached(
            "IsElementActive", typeof(bool), typeof(AttachableWindow), new FrameworkPropertyMetadata(false));

        public static void SetIsElementActive(UIElement element, bool value)
        {
            element.SetValue(IsElementActiveProperty, value);
        }

        public static bool GetIsElementActive(UIElement element)
        {
            return (bool)element.GetValue(IsElementActiveProperty);
        }

        #endregion 

        #region Properties

        /// <summary>
        /// Exposes the MwiWinow parent
        /// </summary>
        private MwiWindow mParent;
        new public MwiWindow Parent
        {
            get { return mParent; }
            set { mParent = value; }
        }

        #endregion

        #region DependencyProperties

        /// <summary>
        /// The MwiChild that this window was created from.
        /// </summary>
        public static readonly DependencyProperty ChildProperty = DependencyProperty.RegisterAttached(
            "Child", typeof(MwiChild), typeof(AttachableWindow), new FrameworkPropertyMetadata(null));

        public MwiChild Child
        {
            get { return (MwiChild)GetValue(ChildProperty); }
            set { SetValue(ChildProperty, value); }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command used to maximize the window
        /// </summary>
        public readonly static RoutedUICommand MaximizeWindow = new RoutedUICommand("Maximize a window.",
            "MaximizeWindow", typeof(AttachableWindow));

        /// <summary>
        /// Command used to minimize the window
        /// </summary>
        public readonly static RoutedUICommand MinimizeWindow = new RoutedUICommand("Minimize a window.",
            "MinimizeWindow", typeof(AttachableWindow));

        /// <summary>
        /// Command used to normalize the window
        /// </summary>
        public readonly static RoutedUICommand NormalizeWindow = new RoutedUICommand("Set a window to normal.",
            "NormalizeWindow", typeof(AttachableWindow));

        /// <summary>
        /// Command used to reattach the window
        /// </summary>
        public readonly static RoutedUICommand AttachWindow = new RoutedUICommand("Attach a window to its parent.",
            "AttachWindow", typeof(AttachableWindow));

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize the window commands and uielements
        /// </summary>
        public AttachableWindow()
        {
            InitializeComponent();
                        
            this.DataContext = this;
            
            // set the close command
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close,
                new ExecutedRoutedEventHandler(delegate(object sender, ExecutedRoutedEventArgs e) { this.Close(); })));
            
            // set the maximize command
            CommandBindings.Add(new CommandBinding(AttachableWindow.MaximizeWindow,
                new ExecutedRoutedEventHandler(delegate(object sender, ExecutedRoutedEventArgs e) { this.WindowState = WindowState.Maximized; })));

            // set the minimize command
            CommandBindings.Add(new CommandBinding(AttachableWindow.MinimizeWindow,
                new ExecutedRoutedEventHandler(delegate(object sender, ExecutedRoutedEventArgs e) { this.WindowState = WindowState.Minimized; })));

            // set the normalize command
            CommandBindings.Add(new CommandBinding(AttachableWindow.NormalizeWindow,
                new ExecutedRoutedEventHandler(delegate(object sender, ExecutedRoutedEventArgs e) { this.WindowState = WindowState.Normal; })));

            // set the attach command
            CommandBindings.Add(new CommandBinding(AttachableWindow.AttachWindow,
                new ExecutedRoutedEventHandler(AttachWindowEvent)));
            
            this.Loaded += new RoutedEventHandler(AttachableWindow_Loaded);
        }

        #endregion

        #region Events

        /// <summary>
        /// An event which handles attaching an MwiChild back to an MwiWindow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void AttachWindowEvent(object sender, ExecutedRoutedEventArgs e)
        {            
            // if the point clicked is within the bounds of the parent then reposition it accordingly
            // otherwise it appears at its last know position.
            Point p = this.Child.MwiParent.PointFromScreen(this.PointToScreen(Mouse.GetPosition(this)));
            Rect parentBounds = new Rect(new Point(0, 0), this.Child.MwiParent.MwiGrid.RenderSize);
            if (parentBounds.Contains(p))
            {
                Point pTopLeft = this.Child.MwiParent.PointFromScreen(new Point(this.Left, this.Top));
                if (pTopLeft.X < 0) pTopLeft.X = 0;
                if (pTopLeft.Y < 0) pTopLeft.Y = 0;
                if ((pTopLeft.X + this.Width) > parentBounds.Width)
                    pTopLeft.X = parentBounds.Width - this.Width;
                if ((pTopLeft.Y + this.Height) > parentBounds.Height)
                    pTopLeft.Y = parentBounds.Height - this.Height;
                Canvas.SetLeft(this.Child, pTopLeft.X);
                Canvas.SetTop(this.Child, pTopLeft.Y);                
            }
            // return the child to the parent window 
            this.Child.IsWindowed = false;
            //this.Child.DisplayTitle = this.Child.Title;
            this.Child.MwiParent.DetachChild(this.Child, true);
            this.Close(); // close this window
        }

        /// <summary>
        /// Handles the load event of an AttachableWindow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void AttachableWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the BorderWindow and add a ResizeAdorner to its AdornerLayer
                DependencyObject mSearchResult = VisualTreeSearch.Find(this, "BorderWindow");
                if (mSearchResult != null)
                {
                    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer((UIElement)mSearchResult);
                    adornerLayer.Add(new ResizeAdorner((UIElement)mSearchResult, this));
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.ToString());
            }            
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Handles the dragging of a Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void DragWindow(object sender, MouseEventArgs e)
        {
            this.DragMove();
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Handle the OnPropertyChanged event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property.Name.Equals("IsActive"))
            {
                this.Child.IsSelected = (bool)e.NewValue;
            }
        }

        /// <summary>
        /// Handle the OnRenderSizeChanged changed event
        /// </summary>
        /// <param name="sizeInfo"></param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            //if (this.Child != null)
            //    this.Child.UpdateTitle();
        }

        #endregion
    }
}