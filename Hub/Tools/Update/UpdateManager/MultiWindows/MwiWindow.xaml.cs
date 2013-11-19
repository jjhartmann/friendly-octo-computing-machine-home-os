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
using System.Collections.Specialized;

#endregion

namespace HomeOS.Hub.Tools.UpdateManager.MultiWindows
{
    /// <summary>
    /// Interaction logic for MwiWindow.xaml
    /// </summary>
    public partial class MwiWindow : System.Windows.Controls.UserControl
    {
        #region Public Static Varibales

        public static int zindex = 0;

        #endregion

        #region Dependency Properties

        /// <summary>
        /// The visible collection of attached children which are not minimized
        /// </summary> 
        public static readonly DependencyProperty ChildrenProperty = DependencyProperty.Register(
            "Children",
            typeof(ObservableCollection<MwiChild>), typeof(MwiWindow), 
            new PropertyMetadata(new ObservableCollection<MwiChild>()));
        public ObservableCollection<MwiChild> Children
        {
            get { return (ObservableCollection<MwiChild>)GetValue(ChildrenProperty); }
            set { SetValue(ChildrenProperty, value); }
        }
                
        /// <summary>
        /// The visible collection of minmized children
        /// </summary> 
        public static readonly DependencyProperty MinChildrenProperty = DependencyProperty.Register(
            "MinChildren",
            typeof(ObservableCollection<MwiChild>), typeof(MwiWindow),
            new PropertyMetadata(new ObservableCollection<MwiChild>()));
        public ObservableCollection<MwiChild> MinChildren
        {
            get { return (ObservableCollection<MwiChild>)GetValue(MinChildrenProperty); }
            set { SetValue(MinChildrenProperty, value); }
        }

        /// <summary>
        /// The collection of child which are currently detached
        /// </summary>
        public static readonly DependencyProperty DetachedChildrenProperty = DependencyProperty.Register(
            "DetachedChildren",
            typeof(List<MwiChild>), typeof(MwiWindow),
            new PropertyMetadata(new List<MwiChild>()));
        public List<MwiChild> DetachedChildren
        {
            get { return (List<MwiChild>)GetValue(DetachedChildrenProperty); }
            set { SetValue(DetachedChildrenProperty, value); }
        }

        /// <summary>
        /// The collection of children which are currently attached 
        /// </summary>
        public static readonly DependencyProperty AttachedChildrenProperty = DependencyProperty.Register(
            "AttachedChildren",
            typeof(ObservableCollection<MwiChild>), typeof(MwiWindow),
            new PropertyMetadata(new ObservableCollection<MwiChild>()));
        public ObservableCollection<MwiChild> AttachedChildren
        {
            get { return (ObservableCollection<MwiChild>)GetValue(AttachedChildrenProperty); }
            set { SetValue(AttachedChildrenProperty, value); }
        }    

        #endregion

        #region Private Variables

        /// <summary>
        /// The currently selected child
        /// </summary>
        private MwiChild mSelectedChild = null;
        public MwiChild SelectedChild
        {
            get { return mSelectedChild; }
            set { mSelectedChild = value; }
        }

        #endregion

        #region Commands

        /// <summary>
        /// A command to Cascade the attached windows 
        /// </summary>
        public readonly static RoutedUICommand Cascade = new RoutedUICommand("Cascade the windows.",
            "Cascade", typeof(MwiWindow));

        /// <summary>
        /// A command to Tile the attached windows
        /// </summary>
        public readonly static RoutedUICommand Tile = new RoutedUICommand("Tile the windows.",
            "Tile", typeof(MwiWindow));

        /// <summary>
        /// A command to horizontally tile attached windows
        /// </summary>
        public readonly static RoutedUICommand TileHorizontally = new RoutedUICommand("Tile the windows horizontally.",
            "TileHorizontally", typeof(MwiWindow));
        
        /// <summary>
        /// A command to vertically tile attached windows
        /// </summary>
        public readonly static RoutedUICommand TileVertically = new RoutedUICommand("Tile the windows vertically.",
            "TileVertically", typeof(MwiWindow));

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor for an MwiWindow
        /// </summary>
        public MwiWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            this.Children.CollectionChanged += new NotifyCollectionChangedEventHandler(Children_CollectionChanged);
            this.MinChildren.CollectionChanged += new NotifyCollectionChangedEventHandler(MinChildren_CollectionChanged);            
        }
                
        #endregion

        #region Events

        /// <summary>
        /// Handle a change in the MinChildren collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void MinChildren_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (MwiChild _child in e.NewItems)
                {
                    AttachedChildren.Add(_child);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (MwiChild _child in e.OldItems)
                {
                    AttachedChildren.Remove(_child);
                }
            }
        }
        
        /// <summary>
        /// Handle a change in the Children collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (MwiChild _child in e.NewItems)
                {
                    if (_child.Width > this.MwiGrid.ActualWidth)
                    {
                        _child.Width = this.MwiGrid.ActualWidth;
                        Canvas.SetLeft(_child, 0);
                    }
                    if (_child.Height > this.MwiGrid.ActualHeight)
                    {
                        _child.Height = this.MwiGrid.ActualHeight;
                        Canvas.SetTop(_child, 0);
                    }
                    // if the child doesn't exist as an attached or detached window the init its zindex, parent and set it unselected
                    if (!this.AttachedChildren.Contains(_child) && !this.DetachedChildren.Contains(_child))
                    {
                        _child.MwiParent = this; // a chld was added, make sure its parent is set to this control
                        _child.IsSelected = false;
                        Canvas.SetZIndex(_child, zindex++);
                    }
                    AttachedChildren.Add(_child);                    
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (MwiChild _child in e.OldItems)
                {
                    AttachedChildren.Remove(_child);
                }
            }
        }

        #endregion

        #region Public Methods

        #region Arrangement Methods

        /// <summary>
        /// Vertically Tile the children which are attached and not minimized
        /// </summary>
        public void VerticallyTileChildren()
        {
            int col = 0;
            double maxheight = (this.ActualHeight - 3);
            double maxwidth = (this.ActualWidth - 3) / Children.Count;

            foreach (MwiChild _child in Children)
            {
                if (maxwidth < _child.MinSize.Width)
                    maxwidth = _child.MinSize.Width;
                if (maxheight < _child.MinSize.Height)
                    maxheight = _child.MinSize.Height;

                _child.Width = maxwidth;
                _child.Height = maxheight;

                Canvas.SetLeft(_child, col * maxwidth);
                Canvas.SetTop(_child, 0);

                col++;

                if (((col + 1) * maxwidth) > (this.ActualWidth - 3)) 
                    col = 0;
            }
        }

        /// <summary>
        /// Horizontally Tile the children which are attached and not minimized
        /// </summary>
        public void HorizontallyTileChildren()
        {
            int row = 0;
            double maxheight = (this.ActualHeight - 3) / Children.Count;
            double maxwidth = (this.ActualWidth - 3);

            foreach (MwiChild _child in Children)
            {
                if (maxwidth < _child.MinSize.Width)
                    maxwidth = _child.MinSize.Width;
                if (maxheight < _child.MinSize.Height)
                    maxheight = _child.MinSize.Height;

                _child.Width = maxwidth;
                _child.Height = maxheight;

                Canvas.SetLeft(_child, 0);
                Canvas.SetTop(_child, row * maxheight);

                row++;

                if (((row + 1) * maxheight) > (this.ActualHeight - 3))
                    row = 0;
            }
        }

        /// <summary>
        /// Tile the children which are attached and not minimized
        /// </summary>
        public void TileChildren()
        {
            int row = -1;
            int column = 0;
            double max = Math.Ceiling(Math.Sqrt(this.Children.Count));
            double remainder = (max * max) - Children.Count;
            double maxwidth = (this.ActualWidth - 3) / max;
            double maxheight = 0;
            foreach (MwiChild _child in Children)
            {
                if (remainder > 0)
                {
                    
                    if (row < (max - 1 - (Math.Ceiling(remainder / max))))
                        row++;
                    else
                    {
                        row = 0;
                        column++;
                        remainder--;                        
                    }

                    maxheight = (this.ActualHeight - 3) / (max - (Math.Ceiling(remainder / max)));

                }
                else
                {
                    maxheight = (this.ActualHeight - 3) / max;

                    if (row < (max - 1))
                        row++;
                    else
                    {
                        row = 0;
                        column++;
                    }
                }

                _child.Width = maxwidth;
                _child.Height = maxheight;
                Canvas.SetLeft(_child, column * maxwidth);
                Canvas.SetTop(_child, row * maxheight);
            }
        }

        /// <summary>
        /// Cascade the children which are attached and not minimized
        /// </summary>
        public void CascadeChildren()
        {
            // min height and min width based on number of children and size of MdiWindow
            double minheight = 100d;
            double minwidth = 100d;
            double maxheight = (this.ActualHeight - 3) - (Children.Count - 1) * 28d;
            double maxwidth = (this.ActualWidth - 3) - (Children.Count - 1) * 28d;
            int row = -1;
            int column = -1;
            foreach (MwiChild _child in Children)
            {
                minheight = 100; // _child.MinSize.Height; // 100;
                minwidth = _child.MinSize.Width;

                if (maxheight < minheight)
                    maxheight = minheight;
                if (maxwidth < minwidth)
                    maxwidth = minwidth;
                
                row++;
                column++;

                if ((((column * _child.MinSize.Height) + /*_child.ActualHeight*/maxheight) > (this.ActualHeight - 3)) ||
                    (((row * _child.MinSize.Height) + /*_child.ActualWidth*/maxwidth) > (this.ActualWidth - 3)))
                {
                    column = 0;
                    row = 0;
                }
                
                _child.Width = maxwidth;
                _child.Height = maxheight;                

                _child.IsSelected = true;                

                Canvas.SetLeft(_child, column * _child.MinSize.Height);
                Canvas.SetTop(_child, row * _child.MinSize.Height);                
            }
        }

        #endregion
                
        /// <summary>
        /// Creates a new MwiChild and places it in the Children collection
        /// </summary>
        public void CreateNewMwiChild()
        {
            MwiChild child = new MwiChild();
            
            double offset = 14;
            double limit = Math.Floor(((this.ActualWidth - child.MinSize.Width) / offset) - 2);
            if (Math.Floor(((this.ActualHeight - child.MinSize.Height) / offset) - 2) < limit)
                limit = Math.Floor((this.ActualHeight / offset) - 2);
                        
            child.Width = this.ActualWidth - (((this.Children.Count % limit) + 1) * offset);
            child.Height = this.ActualHeight - (((this.Children.Count % limit) + 1) * offset);
            child.Background = new SolidColorBrush(Colors.Blue);
            child.BorderBrush = new SolidColorBrush(Colors.Black);
            
            child.Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/app.ico"));
            child.Title = String.Format("My{0}Computer Test 1234567890", zindex); 
            
            Canvas.SetLeft(child, (((this.Children.Count % limit)) * offset));
            Canvas.SetTop(child, (((this.Children.Count % limit)) * offset));
            
            Children.Add(child);
        }

        /// <summary>
        /// Minimizes a child in the MwiWindow
        /// </summary>
        /// <param name="child"></param>
        /// <param name="reverse"></param>
        public void MinimizeChild(MwiChild child, bool reverse)
        {
            if (!reverse)
            {
                if (Children.Remove(child))
                {
                    MinChildren.Add(child);
                }
            }
            else
            {
                if (MinChildren.Remove(child))
                {
                    Children.Add(child);
                }
            }
        }

        /// <summary>
        /// Detaches or attaches a child to the MwiWindow
        /// </summary>
        /// <param name="child"></param>
        /// <param name="reverse"></param>
        public void DetachChild(MwiChild child, bool reverse)
        {
            if (!reverse)
            {
                if (Children.Remove(child))
                {
                    DetachedChildren.Add(child);                    
                }
            }
            else
            {
                if (DetachedChildren.Remove(child))
                {
                    Children.Add(child);                    
                }
            }
        }

        /// <summary>
        /// Promotes the child to top-most in the MwiWindow
        /// </summary>
        /// <param name="child"></param>
        public void PromoteChildToFront(MwiChild child)
        {
            int zindex = Canvas.GetZIndex(child);
            foreach (MwiChild _child in AttachedChildren) //Children)
            {
                _child.IsSelected = _child.Equals(child);
                
                int _zindex = Canvas.GetZIndex(_child);
                Canvas.SetZIndex(_child, _child.Equals(child) ? MwiWindow.zindex - 1 : _zindex < zindex ? _zindex : _zindex - 1);
            }
            if (child.IsSelected && child.IsMinimized)
                child.Minimize(true);
            this.SelectedChild = child;
        }

        #endregion
   
    }
}