using System;

namespace DependencyPropertyWeaver.Tests.Models
{
    public class Simple
    {
        public int Primitive { get; set; }

        public string Reference { get; set; }

        public TimeSpan ValueType { get; set; }
    }

    public class PublicGetterProtectedSetter
    {
        public int X { get; protected set; }
    }

    public static class Static
    {
        public static int X { get; set; }
    }

    public class Attached
    {
        public static double Mass { get; protected set; }
        public static double Volume { get; set; }
    }
}