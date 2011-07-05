using System.Windows;

namespace DependencyPropertyWeaver.Tests.Models
{
    public static class Attached
    {
        public static readonly DependencyProperty WidthProperty = DependencyProperty.RegisterAttached("Width", typeof(int), typeof(Attached));
        public static readonly DependencyProperty HeightProperty = DependencyProperty.RegisterAttached("Height", typeof(int), typeof(Attached));
        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached("Text", typeof(string), typeof(Attached));

        public static readonly DependencyProperty ComplicatedProperty =
            DependencyProperty.RegisterAttached("Complicated",
                                                typeof(string),
                                                typeof(Attached),
                                                new PropertyMetadata("complicated", (obj, e) => e.ToString()),
                                                obj => true);

        public static readonly DependencyPropertyKey ReadOnlyProperty = DependencyProperty.RegisterAttachedReadOnly("ReadOnly", typeof(int), typeof(Attached), new PropertyMetadata(1));

        public static readonly DependencyPropertyKey ComplicatedReadOnlyProperty =
            DependencyProperty.RegisterAttachedReadOnly("ComplicatedReadOnly",
                                                        typeof(string),
                                                        typeof(Attached),
                                                        new PropertyMetadata("complicatedReadOnly", (obj, e) => e.ToString()),
                                                        obj => true);
    }
}