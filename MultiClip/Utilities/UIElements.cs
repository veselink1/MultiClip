using System;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MultiClip.Utilities
{
    public static class UIElements
    {
        /// <summary>
        /// Search the visual tree hirerarchy for a 
        /// child with the given unique identificator.
        /// </summary>
        /// <param name="parent">The parent object.</param>
        /// <param name="uid">The UID of the child.</param>
        /// <returns>The found element or null.</returns>
        public static UIElement FindUid(this DependencyObject parent, string uid)
        {
            var count = VisualTreeHelper.GetChildrenCount(parent);
            if (count == 0) return null; 

            for (int i = 0; i < count; i++)
            {
                var element = VisualTreeHelper.GetChild(parent, i) as UIElement;
                if (element == null) continue;

                if (element.Uid == uid) return element;

                element = element.FindUid(uid);
                if (element != null) return element;
            }
            return null;
        }

        /// <summary>
        /// Sets a list of resource references on the target object
        /// and returns the target object itself.
        /// </summary>
        /// <param name="self">The target of the method.</param>
        /// <param name="resRefs">A list of pairs of the DependencyProperty and the name of the resource.</param>
        public static T SetResourceReferences<T>(this T self, params (DependencyProperty, object)[] resRefs)
            where T : FrameworkElement
        {
            Contract.Requires(self != null);
            foreach (var (depProp, name) in resRefs)
            {
                self.SetResourceReference(depProp, name);
            }
            return self;
        }

        /// <summary>
        /// Executes the specified action on the target object,
        /// then returns the target object.
        /// </summary>
        /// <param name="self">The target object.</param>
        /// <param name="action">The specified action.</param>
        /// <returns>The target object.</returns>
        public static T With<T>(this T self, Action<T> action)
            where T : UIElement
        {
            action(self);
            return self;
        }

        /// <summary>
        /// Registers a syntetic click handler, implemented through <see cref="UIElement.MouseDown"/>
        /// and <see cref="UIElement.MouseUp"/>, on the target element and returns a callback action,
        /// which unregisters the handlers when invoked.
        /// </summary>
        /// <returns>An action, which removes all handers and releases resources.</returns>
        public static Action AddClickHandler<T>(this T self, MouseButtonEventHandler handler)
            where T : UIElement
        {
            Contract.Requires(self != null);
            Contract.Requires(handler != null);

            bool isMouseDown = false;

            MouseButtonEventHandler onMouseDown = delegate
            {
                isMouseDown = true;
            };

            MouseButtonEventHandler onMouseUp = delegate(object sender, MouseButtonEventArgs e)
            {
                if (isMouseDown)
                {
                    handler(sender, e);
                }
                isMouseDown = false;
            };

            self.MouseDown += onMouseDown;
            self.MouseUp += onMouseUp;

            return delegate
            {
                self.MouseDown -= onMouseDown;
                self.MouseUp -= onMouseUp;
            };
        }
    }
}
