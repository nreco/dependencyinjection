using System;
using System.ComponentModel;
using System.Reflection;

namespace NReco.DependencyInjection {

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class DependencyAttribute : Attribute
	{
		
		public string Name { get; private set; }
		
		public DependencyAttribute()  {
		}

		public DependencyAttribute(string name) {
			Name = name;
		}
		
	}
}
