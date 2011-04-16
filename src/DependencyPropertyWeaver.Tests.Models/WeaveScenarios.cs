using System.Windows;

namespace DependencyPropertyWeaver.Tests.Models
{
    public class SimpleDependencyObject : DependencyObject
    {
        public static string Name { get; set; }

        public int Index { get; set; }
    }

    public abstract class Abstract
    {
        public int A { get; set; }
    }

    public class Subclass : Abstract
    {
        public int B { get; set; }
    }
}