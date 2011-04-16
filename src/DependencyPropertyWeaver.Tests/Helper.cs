using System.Linq;
using System.Reflection;
using System.Windows;

namespace DependencyPropertyWeaver.Tests
{
    public static class Helper
    {
        public static DependencyProperty GetFirstStaticDependencyProperty<T>()
        {
            return (DependencyProperty)typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static).First().GetValue(null);
        }
    }
}