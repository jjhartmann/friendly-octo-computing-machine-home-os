#region Using Region

using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

#endregion

namespace HomeOS.Hub.Tools.UpdateManager.MultiWindows
{
    /// <summary>
    /// Class used to perform searches on a Visual Tree
    /// </summary>
    public class VisualTreeSearch
    {
        #region Private Static Variables

        // Static result of a search
        private static DependencyObject mSearchResult = null;

        #endregion

        #region Public Static Method

        /// <summary>
        /// Finds a child element of a DependencyObject that matches the String given.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static DependencyObject Find(DependencyObject obj, String name)
        {
            mSearchResult = null;
            FindChildElementByName(obj, name);
            return mSearchResult;
        }

        /// <summary>
        /// Finds the parent of a DependencyObject of a given Type
        /// </summary>
        /// <param name="child"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static DependencyObject FindByParentType(DependencyObject child, Type type)
        {
            return SearchForParentType(child, type);
        }

        #endregion
        
        #region Private Static Methods

        /// <summary>
        /// Searches for a child element of a DependencyObject that matches the String given.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        private static void FindChildElementByName(DependencyObject obj, String name)
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

        /// <summary>
        /// Searches for a the parent of a DependencyObject of a given Type.
        /// </summary>
        /// <param name="child"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static DependencyObject SearchForParentType(DependencyObject child, Type type)
        {
            DependencyObject result = VisualTreeHelper.GetParent(child);

            if (result == null)
                return null;

            if (!result.GetType().Equals(type))
                result = SearchForParentType(result, type);
            return result;
        }

        #endregion
    }
}
