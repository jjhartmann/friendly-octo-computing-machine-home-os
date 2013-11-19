using System;
using System.Collections.Generic;
using System.Linq;
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

namespace HomeOS.Hub.Tools.UpdateManager.MultiTabs
{
    /// <summary>
    /// Interaction logic for MtWindow.xaml
    /// </summary>
    public partial class MtWindow : Window
    {
        private Dictionary<string, string> _mdiChildren=new Dictionary<string,string>();
        public MtWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Create tab1 if not exists or set focus if exist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuTab1_Click(object sender, RoutedEventArgs e)
        {
            ucTab1 mdiChild = new ucTab1();
            AddTab(mdiChild);
        }
        /// <summary>
        /// Create tab2 if not exists or set focus if exist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuTab2_Click(object sender, RoutedEventArgs e)
        {
            ucTab2 mdiChild = new ucTab2();
            AddTab(mdiChild);
        }
        /// <summary>
        /// Exit the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// Add tab item to the tab
        /// </summary>
        /// <param name="mdiChild">This is the user control</param>
        private void AddTab(ITabbed mdiChild)
        {
            //Check if the user control is already opened
            if (_mdiChildren.ContainsKey(mdiChild.UniqueTabName))
            {
                //user control is already opened in tab. 
                //So set focus to the tab item where the control hosted
                foreach (object item in tcMdi.Items)
                {
                    TabItem ti = (TabItem)item;
                    if (ti.Name == mdiChild.UniqueTabName)
                    {
                        ti.Focus();
                        break;
                    }
                }
            }
            else
            {
                //the control is not open in the tab item
                tcMdi.Visibility = Visibility.Visible;
                tcMdi.Width = this.ActualWidth;
                tcMdi.Height = this.ActualHeight;

                ((ITabbed)mdiChild).CloseInitiated += new delClosed(CloseTab);

                //create a new tab item
                TabItem ti = new TabItem();
                //set the tab item's name to mdi child's unique name
                ti.Name = ((ITabbed)mdiChild).UniqueTabName;
                //set the tab item's title to mdi child's title
                ti.Header = ((ITabbed)mdiChild).Title;
                //set the content property of the tab item to mdi child
                ti.Content = mdiChild;
                ti.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                ti.VerticalContentAlignment = VerticalAlignment.Top;
                //add the tab item to tab control
                tcMdi.Items.Add(ti);
                //set this tab as selected
                tcMdi.SelectedItem = ti;
                //add the mdi child's unique name in the open children's name list
                _mdiChildren.Add(((ITabbed)mdiChild).UniqueTabName,((ITabbed)mdiChild).Title);
            }
        }
        /// <summary>
        /// Close a tab item
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="e"></param>
        private void CloseTab(ITabbed tab, EventArgs e)
        {
            TabItem ti = null;
            foreach(TabItem item in tcMdi.Items)
            {
                if (tab.UniqueTabName == ((ITabbed)item.Content).UniqueTabName)
                {
                    ti = item;
                    break;
                }
            }
            if (ti != null)
            {
                _mdiChildren.Remove(((ITabbed)ti.Content).UniqueTabName);
                tcMdi.Items.Remove(ti);
            }
        }
        /// <summary>
        /// Adjust the tab height and weight during load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menu1_Loaded(object sender, RoutedEventArgs e)
        {
            tcMdi.Width = this.ActualWidth;
            tcMdi.Height = this.ActualHeight - 10;
        }
    }
}
