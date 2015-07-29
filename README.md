# Boost Unit Test Adapter for Microsoft Visual Studio
### User Manual

## Contents
- [Introduction](#introduction)
- [Setup](#setup)  
  + [Pre-Requisites](#pre-requisites)    
  + [Installation](#installation)  
    * [Boost Unit Test Adapter Extension](#boost-unit-test-adapter-extension)  
    * [Installation of Boost test Template](#installation-of-boost-test-template)  
    * [Optional One-time Settings](#other-optional-one-time-settings)  
  + [Uninstallation](#uninstallation)  
- [Quick Start](#quick-start)  
- [Supported Macros](#supported-macros)  
- [Usage](#usage)  
  + [Add a New Boost Project Using the Boost Unit Test Project Template](#add-a-new-boost-project-using-the-boost-unit-test-project-template)  
  + [Add a New Boost Unit Test File Using the Boost Unit Test File Template](#add-a-new-boost-unit-test-file-using-the-boost-unit-test-file-template)  
  + [Display Test Cases](#display-test-cases)  
  + [Run All Test Cases](#run-all-test-cases)  
  + [Run Selected Test Cases](#run-selected-test-cases)  
  + [Group Test Cases](#group-test-cases)  
  + [Navigate to the Source Code](#navigate-to-the-source-code)  
  + [Find the Test Case Location in Source](#find-the-test-case-location-in-source)  
  + [Search and Filter the Test Case List](#search-and-filter-the-test-case-list)  
  + [Clear the Search Results](#clear-the-search-results)  
  + [View the Summary of the Test Case Results](#view-the-summary-and-output-of-the-test-case-results)  
  + [Add and Run a Playlist](#add-and-run-a-playlist)  
  + [Debug Unit Tests Using Test Explorer Window](#debug-unit-tests-using-test-explorer-window)  
  + [Analyzing Code Coverage for Boost Tests](#analyzing-code-coverage-for-boost-tests)
  + [Boost Unit Test Adapter Configuration](#boost-unit-test-adapter-configuration)
      + [Failing Tests in case Memory Leaks are detected](#failing-tests-in-case-memory-leaks-are-detected)
      + [Specifying a Test Execution Timeout](#specifying-a-test-execution-timeout)
      + [Disabling the Conditional Inclusions Filter](#disabling-the-conditional-inclusions-filter)
      + [Modifying Boost Test Log Verbosity](#modifying-boost-test-log-verbosity)
      + [Utilization of an External Test Runner](#utilization-of-an-external-test-runner)
          + [Tests discovery configuration](#tests-discovery-configuration)
          + [Tests execution configuration](#tests-execution-configuration)
- [Limitations](#limitations)  
- [License](#license)
  + [Boost Unit Test Adapter License](#boost-unit-test-adapter-license)
  + [Third-party Software Credits](#third-party-software-credits)
- [Appendix](#appendix)  
  + [Version History](#version-history)  
  + [Reporting Issues](#reporting-issues)  
  + [Building from sources](#building-from-sources)
  + [Troubleshooting](#troubleshooting)

## Introduction
The Boost Unit Test Adapter is available as a free extension for Microsoft Visual Studio. It makes use of the Unit Test Explorer (UTE) provided by Microsoft in the Visual Studio IDE to visualize and run unit test cases that are written using the open source Boost Unit Test Framework libraries. Boost provides free, peer-reviewed, portable C++ source libraries. Boost libraries are intended to be widely useful and usable across a broad spectrum of applications. Boost works on almost any modern operating system, including UNIX and Windows variants.

Refer to [http://www.boost.org/](http://www.boost.org/) for detailed information about the Boost libraries.

## Setup

### Pre-Requisites

In order to use the Boost Unit Test Adapter, the following components must be available:  
- [Boost Libraries](http://www.boost.org/users/download/)
- [Microsoft Visual Studio](https://www.visualstudio.com/). The following versions are supported:
  + Visual Studio 2012 Update 1 (Pro, Premium, Ultimate).
  + Visual Studio 2013 (Pro, Premium, Ultimate).
  + Visual Studio 2015 RC (Enterprise).

### Installation
#### Boost Unit Test Adapter Extension

The pre-built binary of the Boost Unit Test Adapter installation package ( __.vsix__ file ) can be downloaded and installed either by:
- downloading and installing the  __.vsix__ file via the [Microsoft Visual Studio gallery](https://visualstudiogallery.msdn.microsoft.com/5f4ae1bd-b769-410e-8238-fb30beda987f).
- searching for the tool via the Visual Studio's Extensions and Updates form via __Tools__ -> __Extensions and Updates__ and then clicking __Download__ as shown in the below snippet.
![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/boost_unit_test_adapter_in_extensions_and_updates.png)  
- downloading and compiling the Boost Unit Adapter packages from this [GitHub](https://github.com/etas/vs-boost-unit-test-adapter) location.

#### Boost Unit Test Project Template

The Boost Unit Test Project template as well as the Boost Unit Test File template are installed as part of Boost Unit Test Adapter extension installation above.

#### Optional One-time Settings

The below mentioned settings are optional and are here being suggested as a good working practice and are by no way required by the Visual Studio Boost Unit Test adapter. These setting relate to the setup of the Boost include file paths and the Boost library paths. Rather then setting the paths inside the project's property pages, it is suggested to make use of environment variables.

In order to set these environment variables:

1. Select advanced system settings in Control Panel
2. Select advanced tab in system properties window
3. Click environment variable button
4. Click new button in User variable section (or at System Level)
5. Give __BOOST_INC__ as variable name and boost install directory as variable value. Eg: __D:\boost_dir__ 
6. Create one more variable with __BOOST_LIB__ as variable name and boost library directory as variable value. Eg: __D:\boost_dir\lib__

In case multiple Boost library versions are utilized, when naming the environment variables it is suggested to include the library version as part of the environment variable name. The same concept should also be applied with the processor architecture type the library is targeted for. Some examples are the following __BOOST_1440_INC__, __BOOST_1440_LIB__, __BOOST_1440_INC_X64__, __BOOST_1440_LIB_X64__, __BOOST_1490_INC__,  __BOOST_1490_LIB__, __BOOST_1490_INC_X64__, __BOOST_1490_LIB_X64__

### Uninstallation

1. Open Microsoft Visual Studio.
2. Go to __Tools__ -> __Extensions and Updates__.
3. Select _Boost Unit Test Adapter_.
4. Click Uninstall.
5. Restart Microsoft Visual Studio.

## Quick start

- Double click BoostTestPlugin.vsix. This will install the Boost Unit Test Project template, Boost Unit Test File template and Boost Unit Test Adapter extension.
- Follow the optional installation steps as described in section [Other optional settings](#other-optional-settings) so as to create the environment variables. This is a one time process so if this has already been done, this step can be skipped.
- Create a new project using the __Boost Unit Test Project__ template available in the Add New Project dialog. This can be found under: __Installed__ -> __Visual C++__ -> __Test__ -> __Boost Unit Test Project__.
- This will result in a new solution having a project which contains 2 header files (viz. stdafx.h and targetver.h) and 3 source files (viz. stdafx.cpp, BoostUnitTestSample.cpp and BoostUnitTest.cpp).
- Set the Platform Toolset property value of the newly added project. This can be found under: Project Property Pages -> __Configuration Properties__ -> __General__ -> __Platform Toolset__.
- Build the solution and make sure that the project built correctly.
- Once a successful build is achieved, the test cases should have been discovered and made visible in the __Test Explorer Window__. (If the __Test Explorer Window__ is not visible, it can be accessed via the menu item __Test__ -> __Windows__ -> __Test Explorer__).
- The test cases appear with a light blue icon, indicating that they are newly discovered.
- Run the test cases by using the _Run All_ option. As a result, the units tests cases are executed and the corresponding results are updated. The green tick symbolizes that the test case has passed. In case the test case failed (due to failed assertions or memory leaks) the test icon will be red. In case of other failures, the icon will be a yellow triangle.
- Selecting a test case will provide additional information regarding execution duration, failed assertions, test output and test duration.

### Supported Boost Test Macros

The [Boost Unit Test Framework](http://www.boost.org/doc/libs/1_44_0/libs/test/doc/html/utf.html) supports a number of macros but for the scope of the Boost Unit Test Adapter only the below subset of macros are relevant:

| Macro Name                         | Boost Documentation Reference                  |
| ---------------------------------- | ---------------------------------------------- |
| BOOST_AUTO_TEST_CASE               | [Webpage Link](http://www.boost.org/doc/libs/1_44_0/libs/test/doc/html/utf/user-guide/test-organization/auto-test-suite.html)           |
| BOOST_AUTO_TEST_SUITE              | [Webpage Link](http://www.boost.org/doc/libs/1_44_0/libs/test/doc/html/utf/user-guide/test-organization/auto-test-suite.html)           |
| BOOST_AUTO_TEST_SUITE_END          | [Webpage Link](http://www.boost.org/doc/libs/1_44_0/libs/test/doc/html/utf/user-guide/test-organization/auto-test-suite.html)           |
| BOOST_FIXTURE_TEST_SUITE           | [Webpage Link](http://www.boost.org/doc/libs/1_44_0/libs/test/doc/html/utf/user-guide/fixture/test-suite-shared.html)       |
| BOOST_FIXTURE_TEST_CASE            | [Webpage Link](http://www.boost.org/doc/libs/1_44_0/libs/test/doc/html/utf/user-guide/fixture/per-test-case.html)        |
| BOOST_AUTO_TEST_CASE_TEMPLATE      | [Webpage Link](http://www.boost.org/doc/libs/1_44_0/libs/test/doc/html/utf/user-guide/test-organization/auto-test-case-template.html)  |


## Usage

### Add a New Boost Project Using the Boost Unit Test Project Template

A new project can be added using the Boost Unit Test Project template available in the __Add New Project__ context menu. This can be found under __Installed__ -> __Visual C++__ -> __Test__ -> __Boost Unit Test Project__ as shown in the below snippet.   

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/boost_unit_test_project.png)   
 
This will result in the creation of a new solution having a project which contains 2 header files (viz. stdafx.h and targetver.h) and 3 source files (viz. stdafx.cpp, BoostUnitTestSample.cpp and BoostUnitTest.cpp) as shown below.  

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/boost_unit_test_project_files.png)  

The relevant platform toolset property value need to set next for the newly created project.  This property can be accessed via the Property Pages -> __Configuration Properties__ -> __General__ -> __Platform Toolset__ as shown also in the below snippet.

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/platform_toolset.png)

It is important to note that by default the __Boost Unit Test Project__ template does not have any value set for __Platform Toolset__ property.  This means that by default, the property value is set to whatever Visual Studio version is the project being created and compiled with. Failure to properly set the __Platform Toolset__ version might cause linking failures in case any compiled libraries are not available for the __Platform Toolset__ version selected.  

As soon as a Boost Project is newly created using the Boost Unit Test Project template, it is suggested that the solution is built so as to make sure that no compilation or linking issues occur. The most common issue is that users might have incorrect references of either the Boost includes path or the Boost library paths, so in case of compiler warnings of include files not found, undefined identifiers or linker issues, it is suggested to check the __Additional Include Directories__ and the __Additional Library Directories__ as shown in the below two snippets.  The configuration shown in the below two snippets assume that the steps as indicated in section [Optional one time settings](#optional-one-time-settings) have been followed. In case not, the user has to write the paths of the Boost include directories and the Boost library directory respectively.  

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/boost_includes_path.png)  

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/boost_library_path.png) 

Another common pitfall is having an incorrectly set __Platform Toolset__ version. This generally occurs because in case the Boost pre-compiled binaries are utilized, some Boost libraries might not be available for certain levels of __Platform Toolset__ utilized. In such case it is suggested to change the __Platform Toolset__ version according the __Platform Toolset__ the library has been built for, which can be generally easily identified via the library filename(s) as shown in the below snippet.
 

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/indentifying_boost_library_toolset.png)    

Once a successful build is achieved, the test cases should have been discovered and should be visible in the __Test Explorer Window__. (If the __Test Explorer Window__ is not visible, it can be accessed here via the menu item __Test__ -> __Windows__ -> __Test Explorer__ as shown in the below snippet.)

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/accessing_the_test_explorer_window.png)  


### Add a New Boost Unit Test File Using the Boost Unit Test File Template

A new file can be added using the __Boost Unit Test File__ template available via the context menu __Add__ -> __New Item__ and selecting __Boost Unit Test File__ under __Visual C++__->__Test__ dialog as shown in the below two snippets.  
  
![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/context_menu_add_new_item.png)  
![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/adding_boost_unit_test_file.png)    

### Display Test Cases  

In order for the Boost Unit Test Adapter to enumerate the list of available unit tests in a Boost Unit Test project, the project should be successfully built first. Once the project is successfully built, the test cases are enumerated in the Test Explorer window as shown in the below snippet.  

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/enumerated_tests.png)  

If the Test Explorer Window is not visible, it can be accessed here via the menu item __Test__ -> __Windows__ -> __Test Explorer__ as shown in the below snippet

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/accessing_the_test_explorer_window.png)  

### Run All Test Cases  

When the __Run All__ button is clicked, the test cases are executed and test explorer window is updated accordingly. The passed and failed test cases are listed separately. Successful unit tests are indicated via a green icon while failed tests are indicated via a red icon. In case a discovered test case cannot be found whilst trying to execute a test, the test will be indicated by a yellow triangle.

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/run_all_execution.png)  

The execution of the tests can be monitored by selecting __Tests__ in the drop down menu available in the __Output window__.

### Run Selected Test Cases  

Test cases can be executed individually by selecting the desired test cases, then right clicking on them and then clicking the __Run Selected Tests__ option. Their execution can again be monitored by selecting __Tests__ in the drop down menu available in the __Output window__.

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/run_selected_tests.png)  

### Group Test Cases  

The test cases can be grouped by any of the four options given in the table below, by selecting the __Group By__ menu item in the __Test Explorer Window__ and selecting the desired option.

| Group Type    | Description |
| ------------- | ----------- |
| Project       | Groups test cases according to its project’s name. |
| Traits/Suites | Groups test cases according to the suite it belongs to. |
| Outcome       | Groups test cases by execution results: Failed Tests, Skipped Tests, and Passed Tests. |
| Duration      | Groups test cases by execution time: Fast (\<100 ms), Medium (>100 ms), and Slow (>1sec). |
| Class         | Unsupported by the Boost Unit Test Adapter. |

- Grouping by Project:  
![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/group_by_project.png)  
  
- Grouping by Traits/Suites:  
__[Available for MS Visual Studio 2012 Update 2 and onwards]__
By selecting the Traits option in the Test Explorer Window, the test cases are grouped via the test suite they fall under. If any of the test cases does not belong to any test suite, then it falls under the Master Test Suite.  
![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/group_by_traits.png)  
  
- Grouping by Outcome:  
By default, the test cases are grouped by outcome, with execution times listed.  
![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/group_by_outcome.png)  
  
- Grouping by Duration:  
![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/group_by_duration.png)  

### Find the Test Case Location in Source

The source file name as well as the line number of a unit test is displayed in the lower pane of the Test Explorer Window. Clicking on these hyper links will focus the source code window on the file and line in question. Additionally, double-clicking a test case will also navigate to the test case source code.

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/unit_test_location.png)  

### Search and Filter the Test Case List

When you type a string in the Test Explorer search box and press ENTER, the test case list is filtered to display only those test cases whose fully qualified names contain the string being searched. More advanced filtering can be done on also other aspects of the tests (such as on the Output) by using the search filters provided by Visual Studio.

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/search_filters.png)  

### Clear the Search Results  

The **X** button available on the right of the search filter clears the search box's content and resets the __Test Explorer__'s test filter.

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/clear_search.png)  

### View the Summary and Output of the Test Case Results

After a unit test has been executed, single-click the test case to view the summary of its results. The __Output__ hyperlink will focus on a new window showing the output generated during test execution. Information contained in the _Output_ window include:
- Standard Output Messages
- Standard Error Messages
- Failed Assertions
- Memory Leaks

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/test_case_output.png)  

### Add and Run a Playlist  

**[Available for MS Visual Studio 2012 Update 2 and onwards]**  
Test cases can be grouped using the _Playlist_ feature. After selecting a series of test cases, use the right-click context menu to save the tests in a playlist as shown below.

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/save_new_playlist.png)  

By selecting the Playlist option in the __Test Explorer__ window, all the test cases of the particular playlist are displayed and can be executed.  

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/myPlaylist.png)  

### Debug Unit Tests Using Test Explorer Window  

The unit test cases can be debugged using the __Test Explorer__ Window in the following manner:
1. Select the test cases to be debugged.  Assuming breakpoints have been already set in the code that is going to be debugged, right click on the applicable test case and select __Debug Selected Tests__.  

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/debug_selected_tests.png)  

2. The debug point would get hit and then the unit tests can be debugged normally via the Visual Studio debug functions/keyboard shortcuts.
3. After the debug run is complete, the result of the test case is shown in the __Test Explorer__ Window.  
4. If all the test cases in the solution need to be debugged, go to: __Test__ -> __Debug__ -> __All Tests__. All the test cases can be debugged and the result would be displayed in the __Test Explorer__ Window.  

### Analyzing Code Coverage for Boost Tests

Code coverage can be analyzed via the Boost unit tests discovered through the test adapter. By navigating to the __Test__ -> __Analyze Code Coverage__ -> __All Tests__ menu option from the Visual Studio menu bar, all discovered unit tests are executed and the debugger will analyze the amount of code covered by the executed tests.
  
![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/analyze_code_coverage_for_selected_tests.png)  
  
Apart from analyzing code coverage of all the tests, tests can be individually selected and code coverage metrics can be identified for the selected subset via the right-click context menu or via the __Test__ -> __Analyze Code Coverage__ -> __Selected Tests__ menu option.

Note that code coverage does not report standard output, standard error and memory leak information for executed tests. In order to minimise executable re-loading, tests are executed in batches and as a result, standard output, standard error and memory leaks cannot be distinguished per test case.  The difference in the execution behaviour can be seen via the Tests output log window as shown in the snippets below.

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/running_tests_not_in_code_coverage_mode.png)  

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/running_tests_in_code_coverage_mode.png)  

The top snippet shows the tests being executed __not__ for code coverage analysis purposes and as it can be noticed the tests are executed individually. This is done so as to permit the extraction of the standard output and the detection of memory leaks.  

The bottom snippet shows the tests being executed for code coverage purposes and as it can be noticed the tests are executed in a batch, grouped at a test suite level. 

For further information about code coverage in Visual Studio, please refer to the [_Using Code Coverage to Determine How Much Code is being Tested_](https://msdn.microsoft.com/en-us/library/dd537628.aspx) MSDN article.

### Boost Unit Test Adapter Configuration

The Boost Unit Test adapter can be configured via a [Visual Studio _.runsettings_ configuration file](https://msdn.microsoft.com/en-us/library/vstudio/jj635153.aspx). Selecting and using an appropriate test settings file containing a valid ```<BoostTest>``` section allows for running tests which cater for specific use-cases. The configuration file can be loaded via the Visual Studio menu option __Test__ -> __Test Settings__ -> __Select Test Settings File__ as shown in the below snippet. In general, omitting a configuration option implies that its default value is taken into consideration.

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/select_test_settings_file.png)

A [sample](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/sample.runsettings) settings file is available in the [repository](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/sample.runsettings). The supported configuration options will be described in the following sub sections.

#### Failing Tests in case Memory Leaks are detected

In case tests are required to fail in case memory leaks are detected, the configuration option ```<FailTestOnMemoryLeak>``` should be set to ```true``` (or ```1```). By default, this configuration option is set to ```false``` (or ```0```).

As soon as tests are executed, in case memory leaks are detected, the specific test with leaking memory is reported as failed as shown in the below snippet. If the user clicks on the Output link, the user can see the memory allocation number detail, the leak size and the leak contents. 

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/memory_leak_detection.png)

#### Specifying a Test Execution Timeout

Limiting test execution time can be configured via the ```<TimeoutMilliseconds>``` configuration option. The configuration value specifies the amount of time in milliseconds each test case is allowed to run. By default, this option is set to ```-1``` implying that tests are allowed to run indefinitely.

The below snippet shows a test scenario where a test execution lasted more than the configured 1000ms (i.e. 1 second)

![image](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/test_execution_timeout.png)

#### Disabling the Conditional Inclusions Filter

Conditional inclusions directives such as ```#if```, ```#elif```, ```#else```, ```#ifdef``` and ```#if defined``` are by default taken into consideration when the Boost Unit Test Adapter is parsing the source code so as to determine what code segments are to be included or excluded.  In case the user for any reason, requires this type of functionality to be disabled, (such as when utilizing some of the unsuppported lanague caveats as mentioned in the [Limiations](#limitations) section), the conditional inclusions filter can be disabled by setting the  ```<ConditionalInclusionsFilteringEnabled>``` configuration option to ```false``` (or ```0```). By default, this configuration option is set to ```true``` (or ```1```).

Test discovery for Boost Test exe modules parses the source files in the active solution in order to perform test discovery. In case of issues with test discovery, the conditional inclusions filter can be disabled via the ```<ConditionalInclusionsFilteringEnabled>``` option.

#### Modifying Boost Test Log Verbosity

To modify the amount of log information displayed in the test adapter, the ```<LogLevel>``` configuration option needs to be explicitly set to one of the following values:

- ``All``
- ``Success``
- ``TestSuite``
- ``Message``
- ``Warning``
- ``Error``
- ``CppException``
- ``SystemError``
- ``FatalError``
- ``Nothing``

By default, this option is set to ```TestSuite```. Please refer to the _log_level_ [Boost Runtime Configuration](http://www.boost.org/doc/libs/1_55_0/libs/test/doc/html/utf/user-guide/runtime-config/reference.html) for further information about log verbosity.

#### Utilization of an External Test Runner

The Boost Unit Test Adapter supports the utilization of an External Test Runner for test discovery and test execution. This is useful in case a user, for example, wants to compile the Boost unit tests project as a dll rather than as an executable.

The example configuration described in this section will make use of the sister project [Boost External Test Runner](https://github.com/netspiri/boost-external-test-runner/blob/master/README.md) available at [GitHub](https://github.com/netspiri/boost-external-test-runner) but a user can use any other compatible Boost Test Runner as long as the test results are in the same format as those generated typically by the [Boost Unit Test Framework](http://www.boost.org/doc/libs/1_46_0/libs/test/doc/html/utf/user-guide/test-output/log-xml-format.html).  In addition, with respect to the test execution aspect, the same [argument structure](http://www.boost.org/doc/libs/1_46_0/libs/test/doc/html/utf/user-guide/runtime-config/run-by-name.html) as the Boost Unit Test Framework needs to be supported.

##### Tests Discovery Configuration

The discovery phase of the tests can be done/configured in two ways:

* The first option is for the Boost Unit Test Adapter to issue a command to External Test Runner so as to initiate the discovery process.  This command is configured by the inclusion of the information element ```<DiscoveryCommandLine>``` as shown in the XML _.runsettings_ configuration snippet below.  

```xml
<ExternalTestRunner type=".dll">
	<DiscoveryCommandLine>C:\PROGRA~2\BoostExternalTestRunner.exe --test "{source}" --list-debug "{out}"</DiscoveryCommandLine>
    <ExecutionCommandLine>C:\PROGRA~2\BoostExternalTestRunner.exe --test "{source}" {boost-args}</ExecutionCommandLine>
</ExternalTestRunner>
```
   Before initiating the discovery process, the Boost Unit Test Adapter will read the contents of the ```<DiscoveryCommandLine>``` information element and replace the sub-string __{source}__ with the full path of the project for which the discovery is required and replace the sub-string __{out}__ with the full file path where the Boost Unit Test Adapter would like to have the generated XML file generated at, containing the enumeration of the tests in the Boost Unit Test Framework format. An example of the command generated is shown here below.
```
C:\PROGRA~2\BoostExternalTestRunner.exe –-test "D:\dev\svn\SampleBoostProject\Debug\TestProject.dll" –-list-debug "D:\dev\svn\SampleBoostProject\Debug\TestProject.detailed.xml"
```
The Boost Unit Test Framework will then execute the command and upon completion it will read the generated XML file.

A [sample _.runsettings_ configuration file](https://github.com/netspiri/vs-boost-unit-test-adapter/blob/master/Doku/sample-WithExternalTestRunnerDiscoveryType1.runsettings) configured for this type of test discovery methodology is provided at the [GitHub repository](https://github.com/netspiri/vs-boost-unit-test-adapter/tree/master/Doku)

* Alternatively, a user can configure the discovery phase of the tests so that the listing of the tests is read via a static file as shown by the XML _.runsettings_ configuration below.  

```xml
<ExternalTestRunner type=".dll">
	<DiscoveryFileMap>
		<File source="TestProject.dll">D:\dev\svn\SampleBoostProject\TestProject.TestCaseListing.xml</File>
	</DiscoveryFileMap>
    <ExecutionCommandLine>C:\PROGRA~2\BoostExternalTestRunner.exe --test "{source}" {boost-args}</ExecutionCommandLine>
 </ExternalTestRunner>
```
  This type of test case listing is useful in case a user wants to manipulate what tests gets listed in Test Explorer manually.
  
  A [sample _.runsettings_ configuration file](https://github.com/netspiri/vs-boost-unit-test-adapter/blob/master/Doku/sample-WithExternalTestRunnerDiscoveryType2.runsettings) utilizing this type of test discovery methodology is provided at the [GitHub repository](https://github.com/netspiri/vs-boost-unit-test-adapter/tree/master/Doku)
  
  For reference purposes the [GitHub repository](https://github.com/netspiri/vs-boost-unit-test-adapter/tree/master/Doku) contains also two sample XML outputs containing the two types of XML test case listing formats expected by the Boost Unit Test Adapter. The difference between the two samples is that [TestCasesListing.Simple.xml](https://github.com/netspiri/vs-boost-unit-test-adapter/blob/master/Doku/TestCasesListing.Simple.xml) when compared to [TestCasesListing.Detailed.xml](https://github.com/netspiri/vs-boost-unit-test-adapter/blob/master/Doku/TestCasesListing.Detailed.xml) does not contain the line number and the source file name where the testcase has been defined at.  These sample XML files can be utilized as a reference in case a custom External Test Runner is developed and integrated with the Boost Unit Test Adapter as mentioned in (1).  The same sample XML files can be also utilized in case the user wants to define the tests via a static file as mentioned in (2).

##### Tests Execution Configuration

The configuration of the execution phase is performed via the information element ```<ExecutionCommandLine>``` as shown in the _.runsettings_ XML configuration snippets shown above in the two discovery configuration options section.  Similar to the discovery phase the Boost Unit Test Adapter will replace the sub-string __{source}__ with the full path and name of the project and __{boost-args}__ with the boost arguments so as to run the test.  An example of the command generated is 
```
C:\PROGRA~2\BoostExternalTestRunner.exe –-test "D:\dev\svn\SampleBoostProject\Debug\TestProject.dll" "--run_test=SpecialCharactersInIdentifier" "--log_format=xml" "--log_level=test_suite" "--log_sink=SpecialCharacters.exe.test.log.xml" "--report_format=xml" "--report_level=detailed" "--report_sink=SpecialCharacters.exe.test.report.xml" > "SpecialCharacters.exe.test.stdout.log" 2> "SpecialCharacters.exe.test.stderr.log"
```
The Boost Unit Test Adapter will issue the generated command and upon the process completion it will read the generated tests results in XML format (in format that is typically generated by Boost Unit Test Framework XML format) along with the standard output and standard error stream output and then display the test results accordingly in the Test Explorer window.

## Limitations

- If the [log redirection functionality](http://www.boost.org/doc/libs/1_45_0/libs/test/doc/html/utf/user-guide/test-output/log-ct-config.html) of Boost test is used, the Boost Unit Test Adapter will not work. This is due to the fact that adapter internally uses the redirected log output.
- Test discovery for Boost Test exe modules makes use of an in-built C++ parser specifically written to detect the Boost Unit Test Framework macros as listed in the section [Supported Boost Test Macros](#supported-boost-test-macros), which acts on the un-preprocessed source code.  The in-built parser has not been written to support all the language caveats possible and so a number of limitations exist (and hence are areas of possible future work). The known limitations (but actually not limited to) with this regard are:  
  1.  ```#include``` directives are ignored.  This means that if for example ```#defines``` are defined, undefined or redefined within the _included_ files, these will be ignored. This also means if these defines (defined within the _included_ files) are utilized for any conditional inclusions or exclusions within the source code being parsed for the Boost Unit Test macros, the test might be included or excluded erroneously.
  2. The __Stringizing__ (_#_), the __Charzing__ and __Token-pasting__ (_##_) operators are not supported.
  3. Evaluation of __Multi-line defines__ such as in the case shown in the below code is not supported, but any symbol used in the defines (the token _VERSION_ in the below case) can be used on  ```#ifdef``` or ```#ifndef``` conditional in any subsequent code after the end of the Multi-line define.
```c++ 
1:   #define VERSION 5 	
2:
3: 	 #define TWICE(VERSION) \
4: 	        x*2
5:
6:   #if TWICE(VERSION) > 9
7:     ...
8:   #else
9:     ...
10:  #endif
```
  4. Defining a pre-processing symbol for a specific source file as explained on [https://msdn.microsoft.com/en-us/library/hhzbb5c8.aspx](https://msdn.microsoft.com/en-us/library/hhzbb5c8.aspx) is not supported.  
  5. The support for compiler predefined symbols as per [https://msdn.microsoft.com/en-us/library/b0084kay.aspx](https://msdn.microsoft.com/en-us/library/b0084kay.aspx) is limited. The only macros that are supported are ```__DATE__```,  ```__FILE__```, ```__LINE__```, ```__TIME__```, ```__TIMESTAMP__```, ```__COUNTER__```, ```__cplusplus```,  ```__FUNCDNAME__```, ```__FUNCSIG__```, ```__FUNCTION__```, ```_INTEGRAL_MAX_BITS```, ```_MSC_BUILD```, ```_MSC_FULL_VER```, ```_MSC_VER```, ```_WIN32R``` and can be utilized _only_ within the context of a ```#ifdef``` or a ```#ifndef``` conditional inclusions and _not_ for any text substitution.  This means that constructs like the below code snippet
```c++ 
1: #ifdef _MSC_VER
2:   .....
3: #endif
```  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;are supported but constructs like the below code snippet
```c++ 
1: #if _MSC_VER > 1000
2:   .....
3: #endif
```  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;are not supported.
  6. Unable to evaluate macros such as ```$(OS)``` ( that evaluates to ```Windows_NT``` ) in the [Preprocessor definitions](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/unsupported_preprocessor_evaluated_macros.png) property and/or in the [Undefine Preprocessor Definitions](https://raw.githubusercontent.com/netspiri/vs-boost-unit-test-adapter/master/Doku/images/unsupported_preprocessor_evaluated_macros.png) property. Hence tests should not be written in a fashion that their inclusion or exclusion depends on such definitions. 
  7. The C++ code parser of the Boost Unit Test Adapter does require that the Boost macros (and their parameters) as listed in section [Supported Boost Test Macros](#supported-boost-test-macros) need to be written within the same line of code.  This means that macros along with its parameters cannot be separated by any type of line breaks.  If this rule is not followed the Boost Internal Test Adapter will not recognise the test correctly.
- Running the tests via the [VSTest.console](https://msdn.microsoft.com/en-us/library/jj155800.aspx) is not supported.
- Test suites and/or test cases which are manually registered are not discovered for exe builds.  
- Any spaces in the test suite name and/or the test case name (in case the test cases are registered manually) are not supported due to a limitation in the Boost Unit Test Framework in the handling of spaces when specifying the ``--run_test`` option via the command line.
- ```BOOST_AUTO_TEST_CASE_TEMPLATE``` type lists containing a type which contains a space (e.g. ```unsigned int```) are not supported due to a limitation in the Boost Unit Test Framework in the handling of spaces when specifying the ``--run_test`` option via the command line.
  
## License

### Boost Unit Test Adapter License

The Boost Unit Test Adapter is released under the [Boost Software Licence - Version 1.0 - August 17th, 2003](https://visualstudiogallery.msdn.microsoft.com/site/5f4ae1bd-b769-410e-8238-fb30beda987f/eula?licenseType=None).

### Third-party Software Credits

The Boost Unit Test Adapter makes use of the below list of dynamically linked libraries for which we would like to thank their contributors:

* [Apache __log4net v1.2.13__](http://logging.apache.org/log4net/) distributed under [Apache License Version 2.0](http://www.apache.org/licenses/LICENSE-2.0), which is used for logging purposes within the Boost Unit Test Adapter.  The legal copyright for [Apache __log4net__](http://logging.apache.org/log4net/) is __Copyright 2001-2006 The Apache Software Foundation__.
* [__NCalc__ - Mathematical Expression Evaluator for .NET Version v1.3.8](https://ncalc.codeplex.com/) distributed under the [MIT License](https://ncalc.codeplex.com/license), which is used to the evaluate the pre-processor related expressions in the conditional inclusions filter.  The copyright statement for [NCalc](https://ncalc.codeplex.com/) is [__Copyright (c) 2011 Sebastien Ros__](https://ncalc.codeplex.com/license).
* [__Boost__](http://www.boost.org/) libraries and examples distributed under [Boost Software License, Version 1.0](http://www.boost.org/LICENSE_1_0.txt), which are used for the external Boost Unit Test Adapter.  The copyright statement for the examples utilized on which the external Boost Test Adapater is heavily based upon is __(C) Copyright Gennadiy Rozental 2005-2007__.
* [__NUnit__](http://www.nunit.org/) which is a unit-testing framework and is used to test the core functionality of the Boost Unit Test Adapter.  The Boost Unit Test Adapter Unit Test project was written using NUnit 2.6.4 and is released under the [NUnit License](http://nunit.org/nuget/license.html). The legal copyright statement for NUnit is [__Portions Copyright © 2002-2012 Charlie Poole or Copyright © 2002-2004 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov or Copyright © 2000-2002 Philip A. Craig__](http://nunit.org/nuget/license.html).
* [__FakeItEasy v1.13.1__](http://fakeiteasy.github.io/) which is used so as to easily mock classes in the unit test project of the Boost Unit Test Adapter.  This component is released under the [MIT license](https://github.com/FakeItEasy/FakeItEasy/blob/master/License.txt) and its the legal copyright statement is [__Copyright (c) FakeItEasy contributors. (fakeiteasy@hagne.se)__](https://github.com/FakeItEasy/FakeItEasy/blob/master/License.txt).

## Appendix

### Version history

The version history is maintained at the following [link](/../../wiki/Version-History).

### Reporting Issues

Reporting Issues can be done at the following [link](https://github.com/etas/vs-boost-unit-test-adapter/issues)

### Building from sources

The process of building the Boost Test Adapter from sources is maintained at the following [link](/../../wiki/building.md).

### Troubleshooting

The troubleshooting page is maintained at the following [link](/../../wiki/Troubleshooting).
