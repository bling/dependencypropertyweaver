## Dependency Property Weaver

This project grew out of the need to easily work with creating properties for use with WPF.  There are 2 ways to implement change notification: INotifyPropertyChanged and Dependency Properties.

I chose the route of generating Dependency Properties with post build IL weaving as DPs are much more performant with WPF.

The code is work in progress but at this point it is capable of converting any POCO object into a DependencyObject, and converting auto properties into dependency properties.

As a basic example, the following code:

    public class Student {
      public int Age { get; set; }
    } 

Will get weaved into this:

    public class Student : DependencyObject {
      public static readonly DependencyProperty AgeProperty = DependencyProperty.Register("Age", typeof(int), typeof(Student));
      public int Age {
        get { return (int)GetValue(AgeProperty); }
        set { SetValue(AgeProperty, value); }
      }
    }

Attached properties are also supported by weaving static properties.  For example:

    public static class Attached
    {
        public static int Width { get; set; }
        public static int Height { get; set; }
        public static string Text { get; set; }
    }

Will get weaved into attached properties, like this:

	public static class Attached
	{
		public static readonly DependencyProperty WidthDependencyProperty = DependencyProperty.RegisterAttached("Width", typeof(int), typeof(Attached));
		public static readonly DependencyProperty HeightDependencyProperty = DependencyProperty.RegisterAttached("Height", typeof(int), typeof(Attached));
		public static readonly DependencyProperty TextDependencyProperty = DependencyProperty.RegisterAttached("Text", typeof(string), typeof(Attached));
		public static int GetWidth(UIElement uIElement)
		{
			return (int)uIElement.GetValue(Attached.WidthDependencyProperty);
		}
		public static void SetWidth(UIElement uIElement, int num)
		{
			uIElement.SetValue(Attached.WidthDependencyProperty, num);
		}
		public static int GetHeight(UIElement uIElement)
		{
			return (int)uIElement.GetValue(Attached.HeightDependencyProperty);
		}
		public static void SetHeight(UIElement uIElement, int num)
		{
			uIElement.SetValue(Attached.HeightDependencyProperty, num);
		}
		public static string GetText(UIElement uIElement)
		{
			return (string)uIElement.GetValue(Attached.TextDependencyProperty);
		}
		public static void SetText(UIElement uIElement, string value)
		{
			uIElement.SetValue(Attached.TextDependencyProperty, value);
		}
	}

## Quick Start

Once compiled, you simply need to add this into the *AfterBuild* target of your projects.  Like so:

    <UsingTask TaskName="DependencyPropertyWeaver.DependencyWeaverTask.dll"
               AssemblyFile="DependencyPropertyWeaver.dll" />
    <Target Name="AfterBuild">
        <ItemGroup>
            <Input Include="$(OutputPath)\*.dll" />
        </ItemGroup>
        <DependencyPropertyWeaverTask Files="@(Input)" />
    </Target>

## Disclaimer

This is **work in progress**.  Use your due diligence if you intend to use this for production needs.

## License

All source code is released under the [Ms-PL](http://www.opensource.org/licenses/ms-pl) license.
