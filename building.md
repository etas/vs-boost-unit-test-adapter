# Building the Boost Unit Test Adapter solution

(__return back to the [main page](../blob/master/README.md)__)
  
## General Notes

* The Boost Unit Test Adapter for Visual Studio was mainly developed using __Microsoft Visual Studio Premium 2012 Update 4__. Further development should be possible with all the other editions of Microsoft Visual Studio apart from Microsoft Visual Studio Express/Community editions and as long as they are Visual Studio 2012 or newer. 
  
  
* In order for Visual Studio to be able to open all the seven projects within the Boost Unit Test Adapter solution, the [Visual Studio Software Development Kit (SDK)](https://msdn.microsoft.com/en-us/library/bb166441(v=vs.110).aspx) needs to be installed.  If the SDK is not installed for the Visual Studio version in which the Boost Unit Test adapter solution is opened with, Visual Studio will raise an error with the __BoostTestPlugin__ project and tag it as _incompatible_ with an error saying _This project is incompatible with current edition of Visual Studio_ with no indication saying that the issue is that the Visual Studio SDK is not installed.  
  
  
* Since the Boost Unit Test Adapter supports __Visual Studio 2012__, __Visual Studio 2013__ and __Visual Studio 2015__, all the three versions of Visual Studios' need to be installed on the development machine for a hassle free linking. This is required for assemblies Microsoft.VisualStudio.TestPlatform.ObjectModel and Microsoft.VisualStudio.VCProjectEngine.  

	__Microsoft.VisualStudio.TestPlatform.ObjectModel__ needs be referenced to the DLL shipped with the lowest Visual Studio version supported i.e. Visual Studio 2012. If for example the reference is made to point to the assembly shipped along Visual Studio 2013, the Boost Unit Test Adapter will not work for Visual Studio 2012 and will work only for Visual Studio 2013 and Visual Studio 2015.  In all cases the installer will still let the user install the compiled Boost Unit Test Adapter extension for all the three versions Visual Studio but the test adapter will not register successfully for the three versions. This means that if the user had to query the registered adapters via the command __vstest.console.exe /ListDiscoverers /UseVsixExtensions:true__ in the _Developer Command Prompt_ of an older Visual Studio version than the one linked to the assembly at build time, the Boost Unit Test Adapter will not be listed.  
    
    The references to assembly __Microsoft.VisualStudio.VCProjectEngine__ needs to point to the assemblies shipped along Visual Studio 2012, Visual Studio 2013 and Visual Studio 2015 in projects __VisualStudio2012Adapter__, __VisualStudio2013Adapater__ and __VisualStudio2015Adapater__ respectively.  The utilization of the wrong references at build time will result in cast failures at run time.


* In case a particular Visual Studio version is not available (out of the three supported Visual Studios versions), development can still be performed by temporarily re-referencing the reference of the unavailable __Microsoft.VisualStudio.VCProjectEngine__ to an available one.  This suppresses any compilation warnings but it also assumes that the generated installer won't be used for the Visual Studio version for which the re-referencing workaround has been applied. This means that in case for example Visual Studio 2015 is not available on a user's pc, the user can still build the solution successfully by first referencing __Microsoft.VisualStudio.VCProjectEngine__ to the assembly shipped with either Visual Studio 2012 or Visual Studio 2013.  The user however must not distribute the generated installer because in case that installer is used to install the extension on Visual Studio 2015, the extension won't function correctly.

  
* The Unit Test project for the adapter is based on [NUnit](http://www.nunit.org/) testing framework. Development was done using Version 2.6.4.  
  
  
*  The software components [log4net](http://logging.apache.org/log4net/), [NUnit](http://www.nunit.org/) and [FakeItEasy](http://fakeiteasy.github.io/) are managed via the package manager [NuGet](https://www.nuget.org/) which, starting from Visual Studio 2012, is included in every edition of Visual Studio apart from the Express/Community and the TFS editions.

## The Solution

As shown in the snippet below, the Boost Unit Test Adapter solution consists of seven projects.
  
![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/boost_test_adapter_solution.png) 
  
The solution is comprised of:

* Project __BoostTestAdapter__ which contains that majority of the logic and functionality offered of the Boost Unit Test Adapter.
* Project __BoostTestAdapterNUnit__ which is the Unit Test project so as to test the the functionality of the Boost Unit Test Adapter.  The project is based on the [NUnit](http://www.nunit.org/) test framework.
* Project __BoostTestPlugin__  which generates the Visual Studio extension installer package in [VSIX](https://msdn.microsoft.com/en-us/library/ff363239.aspx) format.
* Projects __VisualStudio2012Adapter__, __VisualStudio2013Adapter__, and __VisualStudio2015Adapter__ which all link to essentially the same code with very minor differences and contain abstractions which are specific to a Visual Studio version.  These projects are mainly required for the different external assemblies references respective to the Visual Studio versions, which at the time of writing this boils down to the functionality contained in the namespace __Microsoft.VisualStudio.VCProjectEngine__.
* Project __VisualStudioAdapter__ which mainly consists of Interfaces which were required so as to be able to fake the Visual Studio functionality in the Unit Test project.


## Versioning and Building

The Boost Unit Test adapter is versioned via the _source.extension.vsixmanifest_ file in the __BoostTestPlugin__ project, which as shown in the below snippet, upon clicking the file, a form is presented allowing the user to change the version number amongst many other settings. 
  
![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/source_extension_vsixmanifest.png)  
  
The version number consists of four segments, namely: __\<major version>.\<minor version>.\<revision>.\<build number>__ and always needs to be updated manually.  It is important to remember to update the version number when generating an updated package because in case the extension is already installed with the same version number, the updated package cannot be installed. Please refer this [MSDN page](https://msdn.microsoft.com/en-us/library/dd997169.aspx) for more info on how to update a Visual Studio Extension.

Owners of the master branch should update the build number whenever a commit is done on the master branch. This is useful to identify any issues originating from a particular build revision. Build numbers are to be used only for development purposes and when releasing pre-built binaries (for example on the [Visual Studio Gallery](https://visualstudiogallery.msdn.microsoft.com/5f4ae1bd-b769-410e-8238-fb30beda987f)), the build number should be always 0.

Building the project just requires the user to select the desired solution configuration (_Debug_ or _Release_) and press the _Build_ button in the Visual Studio IDE. A VSIX file called __BoostUnitTestAdapter.vsix__ will be created under the bin/\<selected configuration name> folder of the __BoostTestPlugin__ project.

## Debugging and stepping through the Boost Unit Test Adapter code  

The process of debugging and stepping through the Boost Unit Test Adapter code is:

1) The solution must be in a buildable state.  
2) The __BoostTestPlugin__ must be set as the startup project.  This is done by clicking on the  _Set as StartUp project_ in the right click context menu upon the __BoostTestPlugin__ project.

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/set_as_startup_project.png)

3) Depending on which Visual Studio version the testing will be performed for, option _Start external program_ in __Properties__ -> __Debug__ of the __BoostTestPlugin__ needs to be set accordingly.  The snippet below shows the setting applied for Visual Studio 2012.  If the default installation paths of Visual Studio are utilized and assuming that Visual Studio 2012, Visual Studio 2013 and Visual Studio 2015 are installed, the different respective Visual Studio versions can be launched by changing the number 11.0 to 12.0 or 14.0 in the external program path.

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/start_external_program.png)

4) A specific compilation symbol need to be set for project __BoostTestAdapter__. This is done via by accessing __Properties__ -> __Build__ and inserting the text _LAUNCH_DEBUGGER_ in the text box along side _Condtional compilation symbols_ as shown in the below snippet.  The solution configuration needs to be set to _Debug_ and the _Define DEBUG contant_ checkbox needs to be ticked

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/launch_debugger_compilation_symbol.png)

5) Press _Start without Debugging_ as shown in the snippet below or via the keyboard shortcut _CTRL+F5_. This will launch a so called [Experimental Instance](https://msdn.microsoft.com/en-us/library/bb166560.aspx) of Visual Studio.  An experimental instance can be easily identified via the text _Experimental Instance_ visible in the top bar of the Visual Studio IDE.

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/start_without_debugging.png)  

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/experimental_instance.png)

6) Open a test C++ project. Just after opening and assuming the that the _Text Explorer_ window in the experimental instance of Visual Studio is visible, a form saying that _An unhandled Microsoft .NET Framework exception occurred in vstest.discoveryenginex86.exe_ will appear as shown below.

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/visual_studio_jit_debugger.png)

7) Click _Yes, debug vstest.discoveryenginex86.exe_ and another form will appear asking for a Visual Studio instance, as shown below. If the experimental instance was started without a debugger attached (as instructed in Step 5), the Visual Studio instance that was used to launch the experimental instance will be presented amongst the list and needs to be selected.

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/jit_visual_studio_selection.png)

8) Upon clicking yes, the user will be switched to a particular line of code in the Visual Studio instance that was used to launch the experimental instance. The user can now continue debugging the program flow as with any other C# code.

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/visual_studio_debugging.png)
