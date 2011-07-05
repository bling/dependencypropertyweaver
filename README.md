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

Attached properties are also supported.  For example:

    public static class Attached
    {
        public static readonly DependencyProperty WidthProperty = DependencyProperty.RegisterAttached("Width", typeof(int), typeof(Attached));
		public static readonly DependencyPropertyKey ReadOnlyProperty = DependencyProperty.RegisterAttachedReadOnly("ReadOnly", typeof(int), typeof(Attached), new PropertyMetadata(1));
    }

Will get weaved with the static getter/setter methods, like this:

	public static class Attached
	{
		public static readonly DependencyProperty WidthDependencyProperty = DependencyProperty.RegisterAttached("Width", typeof(int), typeof(Attached));
		public static readonly DependencyPropertyKey ReadOnlyProperty = DependencyProperty.RegisterAttachedReadOnly("ReadOnly", typeof(int), typeof(Attached), new PropertyMetadata(1));
		
		public static int GetWidth(DependencyObject dependencyObject)
		{
			return (int)dependencyObject.GetValue(Attached.WidthProperty);
		}
		public static void SetWidth(DependencyObject dependencyObject, int value)
		{
			dependencyObject.SetValue(Attached.WidthProperty, value);
		}
		public static int GetReadOnly(DependencyObject dependencyObject)
		{
			return (int)dependencyObject.GetValue(Attached.ReadOnlyProperty.DependencyProperty);
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
