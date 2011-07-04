using System.Windows;

namespace DependencyPropertyWeaver.Tests.Models
{
    public static class Attached
    {
        public static readonly DependencyProperty WidthProperty = DependencyProperty.RegisterAttached("Width", typeof(int), typeof(Attached));
        public static readonly DependencyProperty HeightProperty = DependencyProperty.RegisterAttached("Height", typeof(int), typeof(Attached));
        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached("Text", typeof(string), typeof(Attached));

        public static readonly DependencyPropertyKey ReadOnlyProperty = DependencyProperty.RegisterAttachedReadOnly("ReadOnly", typeof(int), typeof(Attached), new PropertyMetadata(1));
    }
}