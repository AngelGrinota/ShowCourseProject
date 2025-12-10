using System.Windows;
using System.Windows.Input;

namespace EducationProgram.Helpers
{
    public static class KeyboardNavigationHelper
    {
        public static readonly DependencyProperty LeftCommandProperty =
            DependencyProperty.RegisterAttached(
                "LeftCommand",
                typeof(ICommand),
                typeof(KeyboardNavigationHelper),
                new PropertyMetadata(null, OnCommandChanged));

        public static readonly DependencyProperty RightCommandProperty =
            DependencyProperty.RegisterAttached(
                "RightCommand",
                typeof(ICommand),
                typeof(KeyboardNavigationHelper),
                new PropertyMetadata(null, OnCommandChanged));

        public static void SetLeftCommand(UIElement element, ICommand value) => element.SetValue(LeftCommandProperty, value);
        public static ICommand GetLeftCommand(UIElement element) => (ICommand)element.GetValue(LeftCommandProperty);

        public static void SetRightCommand(UIElement element, ICommand value) => element.SetValue(RightCommandProperty, value);
        public static ICommand GetRightCommand(UIElement element) => (ICommand)element.GetValue(RightCommandProperty);

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.PreviewKeyDown -= Element_PreviewKeyDown;
                element.PreviewKeyDown += Element_PreviewKeyDown;
                element.Focusable = true;
                element.Focus(); // сразу устанавливаем фокус на элемент
            }
        }

        private static void Element_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is UIElement element)
            {
                if (e.Key == Key.Left)
                {
                    var command = GetLeftCommand(element);
                    if (command != null && command.CanExecute(null))
                    {
                        command.Execute(null);
                        e.Handled = true;
                    }
                }
                else if (e.Key == Key.Right)
                {
                    var command = GetRightCommand(element);
                    if (command != null && command.CanExecute(null))
                    {
                        command.Execute(null);
                        e.Handled = true;
                    }
                }
            }
        }
    }
}
