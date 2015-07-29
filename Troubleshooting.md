# Troubleshooting

(__return back to the [main page](../blob/master/README.md)__)
  
##  Boost Unit Test Adapter not discovering tests

In case the Boost Unit Test Adapter is not discovering tests, it is suggested to check if the adapter is actually being called by Visual Studio so as to discover the tests.  This can be verified by checking for the presence of a particular log type in the section __Test__ of the __Output__ window.  The log that needs to be looked for, is the one similar to the snippet shown below, indicating that the Logger has been initialized.

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/logger_initialized.png)

If the presence of this log entry is not found, there could be an issue with the adapter registration in Visual Studio.  The registered adapters can be listed via the __Developer Command Prompt__ for the specific Visual Studio version being utilized.  For instance, for Visual Studio 2012, __Developer Command Prompt for VS2012__ needs to be utilized as shown below.

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/test_adapter_registration.png)

Upon issuing the command ```vstest.console.exe /ListDiscoverers /UseVsixExtensions:true``` the listing for Boost Test Adapter should appear as shown in the above snippet.  If no listing for the Boost Test Adapter appears  or any of the __Supported File Types__ (_dll_ or _exe_) is/are missing, the Boost Test Adapter will not be called by Visual Studio to discover tests.

Test adapter registration may vary from one Visual Studio version to the other. It is recommended that the extension is installed and properly tested for correct installation on supported versions.

In case of such a scenario, the user is requested to open an issue in the relevant sections on GitHub providing a dump of the console as shown in this guide, stating which is the Visual Studio version utilized and the Boost Unit Test Adapter version that can be obtained from the __Extensions and Updates__ form in the Visual Studio IDE.

##  Memory leaks not failing tests (despite setting) and/or standard Output not available

In case a _.runsettings_ file is being used in which configuration in relation to the code coverage is present (similar to the one shown in the below snippet),

```xml
<?xml version="1.0" encoding="utf-8"?>
<!-- File name extension must be .runsettings -->
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="Code Coverage" uri="datacollector://Microsoft/CodeCoverage/2.0" assemblyQualifiedName="Microsoft.VisualStudio.Coverage.DynamicCoverageDataCollector, Microsoft.VisualStudio.TraceCollector, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a">
        <Configuration>
          <CodeCoverage>
            <!-- Match assembly file paths: -->
            <ModulePaths>
              <Include>
                <ModulePath>.*\.dll$</ModulePath>
                <ModulePath>.*\.exe$</ModulePath>
              </Include>
              <Exclude>
               <ModulePath>.*boosttestadapter\.dll$</ModulePath>            
              </Exclude>
            </ModulePaths>

            <!-- Match fully qualified names of functions: -->
            <!-- (Use "\." to delimit namespaces in C# or Visual Basic, "::" in C++.)  -->
            <Functions>
              <Exclude>                 
                <Function>^boost::.*</Function>                
                <Function>^UnitTestUnitTest::.*</Function>
                <Function>^.*::PrintDataStructure</Function>
                <!--TestCases Exclusion-->
              </Exclude>
            </Functions>

            <!-- Match attributes on any code element: -->
            <Attributes>
              <Exclude>
                <!-- Don’t forget "Attribute" at the end of the name -->               
              </Exclude>
            </Attributes>

            <!-- Match the path of the source files in which each method is defined: -->
            <Sources>
              <Exclude>
                <Source>.*\\atlmfc\\.*</Source>
                <Source>.*\\vctools\\.*</Source>
                <Source>.*\\public\\sdk\\.*</Source>
                <Source>.*\\microsoft sdks\\.*</Source>
                <Source>.*\\vc\\include\\.*</Source>
                <Source>.*\\Microsoft Visual Studio 10.0\\VC\\include\\crtdbg\.h</Source>
              </Exclude>
            </Sources>

            <!-- Match the company name property in the assembly: -->
            <CompanyNames>
              <Exclude>
                <CompanyName>.*microsoft.*</CompanyName>
              </Exclude>
            </CompanyNames>
            
            <!-- We recommend you do not change the following values: -->
            <UseVerifiableInstrumentation>True</UseVerifiableInstrumentation>
            <AllowLowIntegrityProcesses>True</AllowLowIntegrityProcesses>
            <CollectFromChildProcesses>True</CollectFromChildProcesses>
            <CollectAspDotNet>False</CollectAspDotNet>

          </CodeCoverage>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>

```
Visual studio will request the Boost Unit Test adapter to run the tests in code coverage mode, irrespective whether the user requested the code coverage analysis or not.  This behaviour will occur even if the specific Visual Studio version being utilized does not have support for code coverage analysis (the feature is only available in Visual Studio Ultimate or in Visual Studio Premium).  This will have the effect as documented in section [Analyzing code coverage for boost tests](../blob/master/README.md#analyzing-code-coverage-for-boost-tests) of the readme, where the standard output, the standard error and the memory leak information will not be made available. The suggested workaround is to make use of a different _.runsettings_ configuration file, not containing a code coverage configuration section, whilst execution code not for coverage analysis purposes.
