using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace UI
{
    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordBoxHelper), new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordBoxHelper), new PropertyMetadata(false));

        public static string GetBoundPassword(DependencyObject d)
        {
            return (string)d.GetValue(BoundPasswordProperty);
        }

        public static void SetBoundPassword(DependencyObject d, string value)
        {
            d.SetValue(BoundPasswordProperty, value);
        }

        private static bool GetIsUpdating(DependencyObject d)
        {
            return (bool)d.GetValue(IsUpdatingProperty);
        }

        private static void SetIsUpdating(DependencyObject d, bool value)
        {
            d.SetValue(IsUpdatingProperty, value);
        }

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox box)
            {
                if (GetIsUpdating(box))
                {
                    return;
                }

                box.PasswordChanged -= HandlePasswordChanged;

                var newPassword = (string)e.NewValue;
                if (box.Password != newPassword)
                {
                    box.Password = newPassword;
                }

                box.PasswordChanged += HandlePasswordChanged;
            }
        }

        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox box)
            {
                try
                {
                    SetIsUpdating(box, true);
                    box.SetCurrentValue(BoundPasswordProperty, box.Password);
                    BindingOperations.GetBindingExpression(box, BoundPasswordProperty)?.UpdateSource();
                }
                finally
                {
                    SetIsUpdating(box, false);
                }
            }
        }
    }
}
