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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

#endregion

namespace HomeOS.Hub.Tools.UpdateManager.MultiWindows
{
    #region Converters

    /// <summary>
    /// Converts a SolidColorBrush to a Color, an integer parameter can be passed to add to the colors value.
    /// A positive integer will lighten the color and a negative integer will darken then color.
    /// </summary>
    public class SolidBrushToColorConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Color cResult = Colors.Black;
            int add = 0;            
            try
            {
                SolidColorBrush brush = (SolidColorBrush)value;
                if (parameter != null)
                    add = int.Parse(parameter.ToString());

                cResult = Color.FromArgb(255, (byte)Math.Max(Math.Min(brush.Color.R + add, 255), 0),
                                              (byte)Math.Max(Math.Min(brush.Color.G + add, 255), 0),
                                              (byte)Math.Max(Math.Min(brush.Color.B + add, 255), 0));                    
                             
            }
            catch (Exception) { }
            
            return cResult;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Brush bResult = new SolidColorBrush(Colors.Black);
            try
            {
                Color color = (Color)value;
                bResult = new SolidColorBrush(color);
            }
            catch (Exception) { }

            return bResult;
        }

        #endregion
    }

    #endregion

    /// <summary>
    /// Interaction logic for MwiChild.xaml
    /// </summary>
    public partial class MwiChild : System.Windows.Controls.UserControl
    {
        #region Attached Properties

        /// <summary>
        /// An attached property which is used to make an UIElement the dragging point on 
        /// a DragCanvas. This should probably be moved to DragCanavas.
        /// </summary>
        public static readonly DependencyProperty IsDraggableProperty = DependencyProperty.RegisterAttached(
            "IsDraggable", typeof(bool), typeof(MwiChild), new FrameworkPropertyMetadata(false));

        public static void SetIsDraggable(UIElement element, bool value) // required for attached property
        {
            element.SetValue(IsDraggableProperty, value);
        }

        public static bool GetIsDraggable(UIElement element) // required for attached property
        {
            return (bool)element.GetValue(IsDraggableProperty);
        }

        /// <summary>
        /// An attached property which sets an UIElement active (used for min, max and close buttons)
        /// </summary>
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.RegisterAttached(
            "IsActive", typeof(bool), typeof(MwiChild), new FrameworkPropertyMetadata(false));

        public static void SetIsActive(UIElement element, bool value) // required for attached property
        {
            element.SetValue(IsActiveProperty, value);
        }

        public static bool GetIsActive(UIElement element) // required for attached property
        {
            return (bool)element.GetValue(IsActiveProperty);
        }

        /// <summary>
        /// An attached property which denotes whether or not a UIElement can be minimized.
        /// </summary>
        public static readonly DependencyProperty MinimizedProperty = DependencyProperty.RegisterAttached(
            "Minimized", typeof(bool), typeof(MwiChild), new FrameworkPropertyMetadata(false));

        public static void SetMinimized(UIElement element, bool value) // required for attached property
        {
            element.SetValue(MinimizedProperty, value);
        }

        public static bool GetMinimized(UIElement element) // required for attached property
        {
            return (bool)element.GetValue(MinimizedProperty);
        }

        #endregion

        #region Dependency Properties

        public static readonly double gMinHeight = 28.0d;   // the minimum height of a child window
        public static readonly double gButtonWidth = 24.0d; // the width of a titlebar button in a child window
        public static readonly double gButtonNumber = 4.0d; // the default number of buttons contained in a child window
        public static readonly double gBorderOffset = 3.0d; // the border offset for a child window     
        
        /// <summary>
        /// Property which denotes if a child is currently selected.
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected", typeof(bool), typeof(MwiChild),
            new PropertyMetadata(false)); 
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        /// <summary>
        /// Property which denotes if a child is currently maximized
        /// </summary>
        public static readonly DependencyProperty IsMaximizedProperty = DependencyProperty.Register(
            "IsMaximized", typeof(bool), typeof(MwiChild),
            new PropertyMetadata(false)); 
        public bool IsMaximized
        {
            get { return (bool)GetValue(IsMaximizedProperty); }
            set { SetValue(IsMaximizedProperty, value); }
        }

        /// <summary>
        /// Property which denotes if a child is currently minimized
        /// </summary>
        public static readonly DependencyProperty IsMinimizedProperty = DependencyProperty.Register(
            "IsMinimized", typeof(bool), typeof(MwiChild),
            new PropertyMetadata(false)); 
        public bool IsMinimized
        {
            get { return (bool)GetValue(IsMinimizedProperty); }
            set { SetValue(IsMinimizedProperty, value); }
        }

        /// <summary>
        /// Propert used to handle the titlebar icon of a child window
        /// </summary>
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon", typeof(ImageSource), typeof(MwiChild), new PropertyMetadata(new BitmapImage()));
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        /// <summary>
        /// The title currently displayed in the titlebar. This is subject to change and as such a user should only change the 
        /// Title property of a child if they want the titlebar title to permanently change.
        /// </summary>
        /*
        public static readonly DependencyProperty DisplayTitleProperty = DependencyProperty.Register(
            "DisplayTitle", typeof(String), typeof(MwiChild), new PropertyMetadata(""));
        public String DisplayTitle
        {
            get { return (String)GetValue(DisplayTitleProperty); }
            set { SetValue(DisplayTitleProperty, value); }
        }
        */

        /// <summary>
        /// A shorter version of the current title (for use in a menu or statusbar).
        /// </summary>
        public static readonly DependencyProperty ShortTitleProperty = DependencyProperty.Register(
            "ShortTitle", typeof(String), typeof(MwiChild), new PropertyMetadata(""));
        public String ShortTitle
        {
            get { return (String)GetValue(ShortTitleProperty); }
            set { SetValue(ShortTitleProperty, value); }
        }

        /// <summary>
        /// A property which denotes if a child is currently windowed (detached from the MwiWindow parent).
        /// </summary>
        public static readonly DependencyProperty IsWindowedProperty = DependencyProperty.Register(
            "IsWindowed", typeof(bool), typeof(MwiChild), new PropertyMetadata(false));
        public bool IsWindowed
        {
            get { return (bool)GetValue(IsWindowedProperty); }
            set { SetValue(IsWindowedProperty, value); }
        }

        /// <summary>
        /// A property used to manage the visibility of a child's titlebar buttons
        /// </summary>
        public static readonly DependencyProperty PanelButtonsVisiblityProperty = DependencyProperty.Register(
            "PanelButtonsVisiblity", typeof(Visibility), typeof(MwiChild), new PropertyMetadata(Visibility.Visible));
        protected Visibility PanelButtonsVisiblity
        {
            get { return (Visibility)GetValue(PanelButtonsVisiblityProperty); }
            set { SetValue(PanelButtonsVisiblityProperty, value); }
        }
        
        /// <summary>
        /// Collection containing the user defined titlebar buttons
        /// </summary>
        private bool mIsTitleBarButtonsInitialized = false; 
        public static readonly DependencyProperty TitleBarButtonsProperty = DependencyProperty.Register(
            "TitleBarButtons", typeof(ObservableCollection<Button>), typeof(MwiChild));
        public ObservableCollection<Button> TitleBarButtons
        {
            get
            {
                if (!mIsTitleBarButtonsInitialized)
                {
                    TitleBarButtons = new ObservableCollection<Button>();
                    mIsTitleBarButtonsInitialized = true;
                }
                return (ObservableCollection<Button>)GetValue(TitleBarButtonsProperty);
            }
            set { SetValue(TitleBarButtonsProperty, value); }
        }

        /// <summary>
        /// Collection containing the default titlebar buttons (attach/detach, min, max and close)
        /// </summary>
        private bool mIsStandardTitleBarButtonsInitialized = false;
        public static readonly DependencyProperty StandardTitleBarButtonsProperty = DependencyProperty.Register(
            "StandardTitleBarButtons", typeof(ObservableCollection<Button>), typeof(MwiChild));
        protected ObservableCollection<Button> StandardTitleBarButtons
        {
            get
            {
                if (!mIsStandardTitleBarButtonsInitialized)
                {
                    StandardTitleBarButtons = new ObservableCollection<Button>();
                    mIsStandardTitleBarButtonsInitialized = true;
                }
                return (ObservableCollection<Button>)GetValue(StandardTitleBarButtonsProperty);
            }
            set { SetValue(StandardTitleBarButtonsProperty, value); }
        }

        #endregion

        #region Private Properties

        private MwiWindow mMwiParent;           // the child's MwiWindow 
        private Size mSize = Size.Empty;        // used for title updated
        //private double mWidth = 0d;             // used for title updated
        private double iconWidth = 18d;         // the width of the child's titlebar icon
        private AdornerLayer al = null;         // the adorner layer for the child
        private ResizeAdorner drag = null;      // the resize adorner used of the child
        private double mOldMaxCanvasLeft = 0d;  // placeholder for the child left position before a child is maximized
        private double mOldMaxCanvasTop = 0d;   // placeholder for the child top position before a child is maximized
        private double mOldMaxWidth = 0d;       // placeholder for the width before a child is maximized
        private double mOldMaxHeight = 0d;      // placeholder for the height before a child is maximized
        private double mOldMinWidth = 0d;       // placeholder for the width before a child is minimized
        private double mOldMinHeight = 0d;      // placeholder for the height before a child is minimized
        private String mOriginalTitle = "";     // used for title updated    

        #endregion

        #region Properties

        /// <summary>
        /// The actual user defined title of a child. This is used with the DisplayTitle property to present the user
        /// with a window's like title.
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(String), typeof(MwiChild), new PropertyMetadata("")); 
        public String Title
        {
            get { return (String)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }            
        }
                
        /// <summary>
        /// The MwiWindow which the child belongs to.
        /// </summary>
        public MwiWindow MwiParent
        {
            get { return mMwiParent; }
            set { mMwiParent = value; }
        }

        /// <summary>
        /// The smallest size that a child can be.
        /// </summary>
        public Size MinSize
        {
            get
            {                
                iconWidth = 18;
                DependencyObject mSearchResult = VisualTreeSearch.Find(this, "ImageIcon");
                if (mSearchResult != null)
                    iconWidth = ((Image)mSearchResult).ActualWidth;
                return new Size((MwiChild.gButtonNumber + TitleBarButtons.Count) * MwiChild.gButtonWidth + 
                    iconWidth + 16, MwiChild.gMinHeight);
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// A command used to detach a child from its MwiWindow parent.
        /// </summary>
        public readonly static RoutedUICommand DetachWindow = new RoutedUICommand("Detaches a MWIChild from the parent.",
            "DetachWindow", typeof(MwiChild));

        #endregion

        #region Constructor

        /// <summary>
        /// The Constructor for a MwiChild
        /// </summary>
        public MwiChild()
        {
            InitializeComponent();

            this.DataContext = this;

            CommandBindings.Add(new CommandBinding(MwiChild.DetachWindow,
                new ExecutedRoutedEventHandler(delegate(object sender, ExecutedRoutedEventArgs e) { this.Detach(); })));

            this.Loaded += new RoutedEventHandler(MwiChild_Loaded);

            this.MouseDown += new MouseButtonEventHandler(MwiChild_MouseDown);
        }

        #endregion

        #region Event

        /// <summary>
        /// On a left mouse down event reverts a minimized child to its normal state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void MwiChild_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMinimized && (e.LeftButton == MouseButtonState.Pressed))
                Minimize(true);            
        }

        /// <summary>
        /// Initializes the width, height, adorner layer and adorner of a child. Afterwards it refreshs the displayed title.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void MwiChild_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Width < MinSize.Width)
                this.Width = MinSize.Width;

            if (this.Height < MinSize.Height)
                this.Height = MinSize.Height;

            if (!IsMinimized)
            {
                if (al == null)
                    al = AdornerLayer.GetAdornerLayer(this);
                if (drag == null)
                    drag = new ResizeAdorner(this);
                if (al != null && drag != null && al.GetAdorners(this) == null)
                    al.Add(drag);

                this.IsSelected = true;
            }

            //this.RefreshTitle();
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Once displayed sets the child to selected.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            this.IsSelected = true;
        }

        /// <summary>
        /// Handles property changes for Dependency Properties
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            // Update the active value of the titlebar buttons. Promote the child to topmost if it is selected and wasn't before.
            if (e.Property.Name.Equals("IsSelected"))
            {
                foreach (Button b in TitleBarButtons)
                {
                    b.SetValue(MwiChild.IsActiveProperty, e.NewValue);
                }
                if ((this.MwiParent != null) &&
                    ((bool)e.NewValue) && ((bool)e.NewValue != (bool)e.OldValue))
                    this.PromoteToFront();
            }

            // Update the mOriginalTitle for use when refreshing the displayed title.
            if (e.Property.Name.Equals("Title"))
            {
                mOriginalTitle = (String)e.NewValue;
                // limit title to 26 chars                
                String _short = mOriginalTitle;
                if (!String.IsNullOrEmpty(mOriginalTitle) && (mOriginalTitle.Length > 26))
                    _short = mOriginalTitle.Substring(0, 12) + "..." + mOriginalTitle.Substring(mOriginalTitle.Length - 11, 11);
                ShortTitle = _short;
            }
        }

        #region No Longer Used
        /* OnRenderSizeChanged no longer needed for refreshing the title
        /// <summary>
        /// When the size of the child is updated, update the title as well.
        /// </summary>
        /// <param name="sizeInfo"></param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            //this.RefreshTitle();
        }
        */
        #endregion

        /// <summary>
        /// Return the original title (based on the user defined Title property). 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return mOriginalTitle;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Minimize the child when the minButton is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void minButton_Click(object sender, RoutedEventArgs e)
        {
            this.Minimize(false);
        }

        /// <summary>
        /// Maximize a child when the maxButton is clicked or set it to normal if it is alredy maximized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void maxButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsMaximized)
                this.Maximize(false);
            else
                this.Maximize(true);
            
            IsMaximized = !IsMaximized;
        }

        /// <summary>
        /// Remove the child from the parent when closeButton is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        #region Public Methods

        #region No Longer Used
        /* UpdateTitle and RefreshTitle are no longer needed for the update of the window title
        /// <summary>
        /// Refresh the title of the child
        /// </summary>
        public void RefreshTitle()
        {            
            //this.DisplayTitle = mOriginalTitle;

            //DependencyObject  mSearchResult = VisualTreeSearch.Find(this, "ImageIcon");           
            //if (mSearchResult != null)
            //    iconWidth = ((Image)mSearchResult).ActualWidth;

            //double titleWidth = 0d;
            //mSearchResult = VisualTreeSearch.Find(this, "PanelTitleIcon");
            //if (mSearchResult != null)
            //    titleWidth = ((StackPanel)mSearchResult).ActualWidth;
            
            //mSearchResult = VisualTreeSearch.Find(this, "TextBlockTitle");
            //if (mSearchResult != null)
            //{
            //    ((TextBlock)mSearchResult).Width = titleWidth - iconWidth;                
            //}
            
            // limit title to 26 chars                
            String _short = mOriginalTitle;
            if (!String.IsNullOrEmpty(mOriginalTitle) && (mOriginalTitle.Length > 26))
                _short = mOriginalTitle.Substring(0, 12) + "..." + mOriginalTitle.Substring(mOriginalTitle.Length - 11, 11);
            ShortTitle = _short;

            DependencyObject mSearchResult = VisualTreeSearch.Find(this, "TextBlockTitle");
            if (mSearchResult != null)
            {
                ((TextBlock)mSearchResult).UpdateLayout();
                mWidth = ((TextBlock)mSearchResult).RenderSize.Width;
            }

            mSearchResult = VisualTreeSearch.Find(this, "ImageIcon");
            if (mSearchResult != null)
                iconWidth = ((Image)mSearchResult).ActualWidth;

            mSize = new Size((MwiChild.gButtonNumber + TitleBarButtons.Count) * MwiChild.gButtonWidth +
                iconWidth + mWidth, MwiChild.gMinHeight);
            if (IsMinimized)
                mSize = new Size(iconWidth + mWidth + 14, MwiChild.gMinHeight);

            this.UpdateTitle();
           
        }
        */

        /* No longer needed for the update of the window title
        /// <summary>
        /// Determine the size of the title and add '.' to the end if it is too big to fit in the title
        /// bar. This algorithm doesn't work consistently and should be updated at some point.
        /// </summary>
        public void UpdateTitle()
        {
            // can't user ActualWidth here because only the Width is bound to the attached window and
            // doesn't seem to be updating the ActualWidth untill it is reattached
           
            if (mSize != null && this.Width < mSize.Width && !String.IsNullOrEmpty(mOriginalTitle))
            {
                try
                {
                    double diff = mSize.Width - this.Width;
                    double avglen = mWidth / mOriginalTitle.Length; // average length of a char
                    int nchar = Math.Max((mOriginalTitle.Length) - (int)Math.Ceiling(diff / avglen) - 3, 0);
                    this.DisplayTitle = mOriginalTitle.Substring(0, 1) +
                        mOriginalTitle.Substring(1, nchar).PadRight(mOriginalTitle.Length - 1, '.');                    
                }
                catch (Exception ex)
                {
                    this.DisplayTitle = mOriginalTitle;
                }
            }
            else if (!String.IsNullOrEmpty(mOriginalTitle))
                this.DisplayTitle = mOriginalTitle;
                      
        }
        */
        #endregion

        /// <summary>
        /// Detach a child from its parent and place the content of the child within an attachable window. 
        /// </summary>        
        public void Detach()
        {
            this.MwiParent.DetachChild(this, false);
            AttachableWindow aWin = new AttachableWindow();
            this.IsWindowed = true;
            
            aWin.Title = this.mOriginalTitle; // needed for taskbar display
                        
            aWin.MinWidth = this.MinSize.Width;
            aWin.MinHeight = this.MinSize.Height;

            Point pnt = this.MwiParent.PointToScreen(new Point(Canvas.GetLeft(this), Canvas.GetTop(this)));
            try
            {
                // the icon may be in different formats so this code is required to process each
                try
                {
                    aWin.Icon = this.Icon;
                }
                catch (Exception)
                {
                    IconBitmapDecoder ibd = new IconBitmapDecoder(((BitmapImage)this.Icon).UriSource,
                        BitmapCreateOptions.None, BitmapCacheOption.Default);
                    aWin.Icon = ibd.Frames[0];
                }
            }
            catch (Exception) { }
            aWin.Left = pnt.X;
            aWin.Top = pnt.Y;
            aWin.Parent = this.MwiParent;
            aWin.Child = this;

            aWin.Show();            
        }

        /// <summary>
        /// Minimize the child or revers a minimized child back to normal
        /// </summary>
        /// <param name="reverse"></param>
        public void Minimize(bool reverse)
        {
            if (!reverse)
            {
                mOldMinWidth = this.Width;
                mOldMinHeight = this.Height;
                
                IsMinimized = true;
                //this.DisplayTitle = mOriginalTitle;
                this.IsSelected = false;
                PanelButtonsVisiblity = Visibility.Collapsed;

                this.Height = MinSize.Height;
                this.Width = MinSize.Width;

                al.Remove(drag);

                this.MwiParent.MinimizeChild(this, false);
            }
            else
            {              
                IsMinimized = false;
                //this.DisplayTitle = mOriginalTitle;
                this.IsSelected = true;
                PanelButtonsVisiblity = Visibility.Visible;

                this.Height = mOldMinHeight;
                this.Width = mOldMinWidth;

                this.IsSelected = true;                

                this.MwiParent.MinimizeChild(this, true);
            }
        }

        /// <summary>
        /// Maximized a child or reverse a maximized child back to normal
        /// </summary>
        /// <param name="reverse"></param>
        public void Maximize(bool reverse)
        {
            if (!reverse)
            {
                mOldMaxCanvasLeft = Canvas.GetLeft(this);
                mOldMaxCanvasTop = Canvas.GetTop(this);
                mOldMaxWidth = this.ActualWidth;
                mOldMaxHeight = this.ActualHeight;

                Canvas.SetLeft(this, 0);
                Canvas.SetTop(this, 0);
                this.Width = this.MwiParent.ActualWidth - MwiChild.gBorderOffset;
                this.Height = this.MwiParent.ActualHeight - MwiChild.gBorderOffset;
            }
            else
            {
                Canvas.SetLeft(this, mOldMaxCanvasLeft);
                Canvas.SetTop(this, mOldMaxCanvasTop);
                this.Width = mOldMaxWidth;
                this.Height = mOldMaxHeight;
            }
        }

        /// <summary>
        /// Close a child.
        /// </summary>
        public void Close()
        {
            this.MwiParent.Children.Remove(this);
        }

        /// <summary>
        /// Promote a child to the top-most in the MwiWindow.
        /// </summary>
        public void PromoteToFront()
        {
            this.MwiParent.PromoteChildToFront(this);
        }        

        #endregion        
    }    
}