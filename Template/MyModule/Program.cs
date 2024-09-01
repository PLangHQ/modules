using PLang.Modules;
using System.ComponentModel;

namespace MyModule
{
	[Description("MyModule lists out available modules")]
	public class MyModule : BaseProgram
	{ 
		public record ModuleType(string module, string moduleType);

		[Description("give the list of modules that are available by module name")]
		public List<ModuleType> WhatModulesHave(string module)
		{ 
			List<ModuleType> modules = new();
			modules.Add(new ModuleType("file", "MyModule.FileClass"));
			modules.Add(new ModuleType("file", "MyModule.FileInfoClass"));
			modules.Add(new ModuleType("http", "MyModule.HttpClass"));

			return modules.Where(p => p.module == module).ToList();

		}
		/*
		 * This is how you execute this code in Plang. Compile the project.
		 * Copy the MyModule.dll into your .module folder
		 * 
		 * Create a plang file, e.g. Start.goal
		 * 
		```plang
		Start
		- [mymodule] What modules has 'file' in it, write to %modules%
		- write out %modules%
		```
		*/
	}
}
