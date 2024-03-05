# PLang Custom Modules Guide

Welcome to the PLang Custom Modules repository! This guide is tailored to assist developers in crafting and incorporating custom modules into PLang, thereby broadening its utility with bespoke algorithms and functionalities. Whether your goal is to integrate complex mathematical operations, leverage existing libraries, or introduce novel features, this guide will lead you through the necessary steps, illustrated by the example module `ComplexAlgorithm`.

## Initial Setup

To kickstart module development, ensure the following prerequisites are met:

- **Clone PLang**: Obtain the PLang source code by cloning the [PLang GitHub Repository](https://github.com/PLangHQ/plang).
- **Development Environment**: Install [Visual Studio Community Edition](https://visualstudio.microsoft.com/vs/community/) if not already available. It offers a comprehensive, free development environment suitable for this project.

## Crafting Your Module

### 1. Project Creation

Begin by initiating a new Class Library project in Visual Studio, targeting .NET Core or .NET Standard, which will house your module.

### 2. Program File

Within this project, establish a `Program.cs` file, which will act as the entry point for your module.

### 3. Reference PLang

Now, incorporate a reference to the PLang project within your module project to tap into its foundational features.

### 4. Inherit BaseProgram

Structure your primary class to inherit from `BaseProgram`, residing in the PLang.Modules namespace. This step is crucial for linking your module with PLang and making its functions accessible within PLang code.

Consider this skeleton code for a hypothetical `ComplexAlgorithm` module:

```csharp
using PLang.Modules;

namespace YourModuleName; // Adapt 'YourModuleName' to fit your module's actual name

public class Program : BaseProgram
{
    public async Task<int> ComplexCalculate(int a, int b)
    {
        // Insert your sophisticated algorithm here
        int result = (a + b) * new Random().Next(1, 900);
        return result;
    }
}
```

Modify `ComplexCalculate` and its body to encapsulate the functionality you aim to provide.

### 5. Compilation

Build your project to produce a `.dll` file, constituting your compiled module.

### 6. Integration with PLang

Migrate your `.dll` file to the `.modules` directory at the root of the PLang project. Ensure `PlangLibrary.dll` is not transferred, as it is inherently part of PLang.

## Employing Your Module in PLang

To leverage your module within PLang code, the exposed functions should be asynchronous, with any resulting values assigned to PLang variables. For instance, to utilize the `ComplexCalculate` function from our illustrative module, you could employ the following in PLang code:

```plang
CallModule
- execute complex calc on %a% and %b%, write to %result%
```

This will invoke your `ComplexCalculate` function with the supplied parameters `%a%` and `%b%`, depositing the result in the `%result%` variable.

## Recommended Practices

- **Naming Convention**: Opt for clear, descriptive names for your modules and functions that accurately reflect their purpose.
- **Focus**: Maintain a sharp focus on a specific domain or functionality within your modules to enhance usability and composability.
- **Documentation**: Thoroughly document your functions, detailing parameters and expected outcomes to aid in comprehension and utilization by others.
- **Error Management**: Implement comprehensive error handling within your modules to gracefully address exceptions and unexpected inputs.

## Wrapping Up

Creating custom modules for PLang not only extends its functionality but also fosters a culture of code reuse within the PLang community. We encourage you to experiment, craft your unique modules, and contribute to the collective growth of the ecosystem. Happy coding!