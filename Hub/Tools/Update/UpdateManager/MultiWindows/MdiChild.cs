using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
//using System.Windows.Forms;

namespace HomeOS.Hub.Tools.UpdateManager.MultiWindows
{
    /// <summary>
    /// ========================================
    /// .NET Framework 3.0 Custom Control
    /// ========================================
    ///
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WPFMdiWindows.Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WPFMdiWindows.Controls;assembly=WPFMdiWindows.Controls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file. Note that Intellisense in the
    /// XML editor does not currently work on custom controls and its child elements.
    ///
    ///     <MyNamespace:MdiChild/>
    ///
    /// </summary>
    public class MdiChild : System.Windows.Controls.Control
    {
        public static readonly DependencyProperty IsDraggableProperty = DependencyProperty.RegisterAttached(
            "IsDraggable", typeof(bool), typeof(MdiChild), new FrameworkPropertyMetadata(false));

        public static void SetIsDraggable(UIElement element, bool value)
        {
            element.SetValue(IsDraggableProperty, value);
        }

        public static bool GetIsDraggable(UIElement element)
        {
            return (bool)element.GetValue(IsDraggableProperty);
        }

        public static readonly DependencyProperty IsSelectableProperty = DependencyProperty.Register(
            "IsSelectable", typeof(bool), typeof(MdiChild),
            new PropertyMetadata(false)); //, new PropertyChangedCallback(OnIsSelectableChanged)));
        public bool IsSelectable
        {
            get { return (bool)GetValue(IsSelectableProperty); }
            set { SetValue(IsSelectableProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon", typeof(ImageSource), typeof(MdiChild), new PropertyMetadata(new BitmapImage()));
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(String), typeof(MdiChild), new PropertyMetadata(""));
        public String Title
        {
            get { return (String)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty ShortTitleVisiblityProperty = DependencyProperty.Register(
            "ShortTitleVisiblity", typeof(Visibility), typeof(MdiChild), new PropertyMetadata(Visibility.Collapsed));
        protected Visibility ShortTitleVisiblity
        {
            get { return (Visibility)GetValue(ShortTitleVisiblityProperty); }
            set { SetValue(ShortTitleVisiblityProperty, value); }
        }

        private bool mIsTitleBarButtonsInitialized = false;
        public static readonly DependencyProperty TitleBarButtonsProperty = DependencyProperty.Register(
            "TitleBarButtons", typeof(ObservableCollection<Button>), typeof(MdiChild));
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

        private bool mIsStandardTitleBarButtonsInitialized = false;
        public static readonly DependencyProperty StandardTitleBarButtonsProperty = DependencyProperty.Register(
            "StandardTitleBarButtons", typeof(ObservableCollection<Button>), typeof(MdiChild));
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

        private MwiWindow mMwiParent;
        public MwiWindow MwiParent
        {
            get { return mMwiParent; }
            set { mMwiParent = value; }
        }

        public Size MinSize
        {
            get 
            {
                String _title = this.Title.Length > 0 ? this.Title.Substring(0, 1) : "";
                //System.Drawing.Size s = System.Windows.Forms.TextRenderer.MeasureText(_title,
                //    new System.Drawing.Font(this.FontFamily.ToString(), (float)this.FontSize));
                FindChildElementByName(this, "MyTextBlock");
                //Console.Out.WriteLine(mSearchResult != null ? ((TextBlock)mSearchResult).RenderSize.Width.ToString() : "null");
                return new Size((StandardTitleBarButtons.Count + TitleBarButtons.Count) * 22 + 16 + 64, 24); 
            }           
        }

        static MdiChild()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MdiChild), new FrameworkPropertyMetadata(typeof(MdiChild)));
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            this.IsSelectable = true;             
        
            Button minButton = new Button();
            minButton.Width = 22;
            minButton.Height = 22;
            minButton.Content = "_";
            minButton.Click += new RoutedEventHandler(minButton_Click);
            StandardTitleBarButtons.Add(minButton);

            Button maxButton = new Button();
            maxButton.Width = 22;
            maxButton.Height = 22;
            maxButton.Content = "[]";
            maxButton.Click += new RoutedEventHandler(maxButton_Click);
            StandardTitleBarButtons.Add(maxButton);

            Button closeButton = new Button();
            closeButton.Width = 22;
            closeButton.Height = 22;
            closeButton.Content = "X";            
            //closeButton.Click += new RoutedEventHandler(closeButton_Click);
            StandardTitleBarButtons.Add(closeButton);

            if (this.Width < MinSize.Width)
                this.Width = MinSize.Width;

            if (this.Height < MinSize.Height)
                this.Height = MinSize.Height;

            if (al == null)
                al = AdornerLayer.GetAdornerLayer(this);
            if (drag == null)
                drag = new DragAdorner(this);
            if (al != null && drag != null)
                al.Add(drag);

            //Console.Out.WriteLine(this.MinSize.Width.ToString());
        }

        void minButton_Click(object sender, RoutedEventArgs e)
        {
            this.Minimize(false);
        }

        // should be a public property
        private bool mIsMaximized = false;
        void maxButton_Click(object sender, RoutedEventArgs e)
        {
            if (!mIsMaximized)
            {
                this.Maximize(false);
                ((Button)sender).Content = "[+";                
            }
            else
            {
                this.Maximize(true);
                ((Button)sender).Content = "[]";  
            }
            mIsMaximized = !mIsMaximized;
        }

        //protected void closeButton_Click(object sender, RoutedEventArgs e)
        //{
        //    this.Close();            
        //}

        private AdornerLayer al = null;
        private DragAdorner drag = null;
        
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property.Name.Equals("IsSelectable"))
            {
                //if (IsSelectable)
                //{
                //    if (al == null)
                //        al = AdornerLayer.GetAdornerLayer(this);
                //    if (drag == null)
                //        drag = new DragAdorner(this);
                //    if (al != null && drag != null)
                //        al.Add(drag);
                //}
                //else
                //{
                //    if (al != null && drag != null)
                //    {
                //        al.Remove(drag);
                //        drag = null;
                //    }
                //}
            }

            //Console.Out.WriteLine(e.Property.Name);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
                       
            System.Drawing.Size s = System.Windows.Forms.TextRenderer.MeasureText(this.Title,
                    new System.Drawing.Font(this.FontFamily.ToString(), (float)this.FontSize));
            double _width = ((StandardTitleBarButtons.Count + TitleBarButtons.Count) - 1) * 22 + 16 + s.Width;
            if (_width > this.Width)
                ShortTitleVisiblity = Visibility.Visible;
            else
                ShortTitleVisiblity = Visibility.Collapsed;
            //Console.Out.WriteLine(VisualTreeHelper.GetChildrenCount(this).ToString());
        }

        private double mOldCanvasLeft = 0d;
        private double mOldCanvasTop = 0d;
        private double mOldWidth = 0d;
        private double mOldHeight = 0d;

        public void Minimize(bool reverse)
        {
            this.Height = 24;
            Canvas.SetTop(this, this.MwiParent.Height - this.Height);
        }

        public void Maximize(bool reverse)
        {
            if (!reverse)
            {
                mOldCanvasLeft = Canvas.GetLeft(this);
                mOldCanvasTop = Canvas.GetTop(this);
                mOldWidth = this.Width;
                mOldHeight = this.Height;

                Canvas.SetLeft(this, 0);
                Canvas.SetTop(this, 0);
                this.Width = this.MwiParent.Width;
                this.Height = this.MwiParent.Height;
            }
            else
            {
                Canvas.SetLeft(this, mOldCanvasLeft);
                Canvas.SetTop(this, mOldCanvasTop);
                this.Width = mOldWidth;
                this.Height = mOldHeight;
            }
        }

        //public void Close()
        //{
        //    this.MdiParent.Children.Remove(this);
        //}

        //public void PromoteToFront()
        //{
        //    this.MdiParent.PromoteChildToFront(this);
        //}

        protected DependencyObject mSearchResult = null;
        public void FindChildElementByName(DependencyObject obj, String name)
        {   
            if ((obj == null)) return;

            object oname = obj.GetValue(Control.NameProperty);
            if (oname != null && oname.ToString().Equals(name))
            {
                mSearchResult = obj;
                return;
            } 

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                FindChildElementByName(VisualTreeHelper.GetChild(obj, i), name);
        }
    }
}
