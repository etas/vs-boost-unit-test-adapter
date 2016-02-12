// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.VCProjectEngine;

namespace VisualStudioAdapter.Shared
{
    /// <summary>
    /// Adapter class for Visual Studio C++ compiler options
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cpp")]
    public class VCppCompilerOptions : IVCppCompilerOptions
    {
        private static readonly Regex RegexPreProcesserDefines = new Regex(@"\A(.+?)(?:=|#)(.*)\z");

        private VSDebugConfiguration _configuration = null;
        private VCCLCompilerTool _compiler = null;

        private Defines _defines = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">The Visual Studio Configuration parent of the compiler options</param>
        /// <param name="compiler">The Visual Studio compiler tool which is to be adapted</param>
        public VCppCompilerOptions(VSDebugConfiguration configuration, VCCLCompilerTool compiler)
        {
            this._configuration = configuration;
            this._compiler = compiler;
        }

        #region IVCppCompilerOptions

        public Defines PreprocessorDefinitions
        {
            get
            {
                if (this._defines == null)
                {
                    this._defines = GetPreprocessorDefines();
                }

                return this._defines;
            }
        }

        #endregion IVCppCompilerOptions

        /// <summary>
        /// Traverses the necessary Visual Studio structures to retrieve any and all
        /// C++ Preprocessor definitions currently defined.
        /// </summary>
        /// <returns></returns>
        private Defines GetPreprocessorDefines()
        {
            Defines definesHandler = new Defines();

            RegisterAnsiCompliantPredefinedMacros(definesHandler);

            // Extract defines from property sheets
            foreach (VCPropertySheet sheet in this.PropertySheets)
            {
                VCCLCompilerTool tool = GetVCppCompilerOptions(sheet.Tools);
                if (tool != null)
                {
                    GetPreprocessorDefines(tool, definesHandler);
                }
            }

            // Avoid registering the Microsoft defines if the /u option is specified
            if (!this._compiler.UndefineAllPreprocessorDefinitions)
            {
                RegisterMicrosoftPreDefinedCompilerMacros(definesHandler);
            }

            // Extract defines from compiler options
            GetPreprocessorDefines(this._compiler, definesHandler);

            return definesHandler;
        }

        /// <summary>
        /// Enumerates property sheets sorted in evaluation order
        /// </summary>
        private IEnumerable<VCPropertySheet> PropertySheets
        {
            get
            {
                IVCCollection sheets = (this._configuration.VCConfiguration as VCConfiguration).PropertySheets as IVCCollection;
                if (sheets != null)
                {
                    /*
                     * It has been observed (i.e. we did not manage to find it documented anywhere)
                     * that when the property sheets are iterated over in reverse we are actually
                     * mimicking the evaluation order.
                     */

                    return sheets.OfType<VCPropertySheet>().Reverse();
                }

                return Enumerable.Empty<VCPropertySheet>();
            }
        }

        /// <summary>
        /// Given an IVCCollection of tools, identifies and returns the C++ tools.
        /// </summary>
        /// <param name="toolsCollection">The IVCCollection to search</param>
        /// <returns>The C++ tools or null if not available</returns>
        private static VCCLCompilerTool GetVCppCompilerOptions(dynamic toolsCollection)
        {
            IVCCollection tools = toolsCollection as IVCCollection;
            if (tools != null)
            {
                return tools.OfType<VCCLCompilerTool>().FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// Populates the provided definesHandler with the preprocessor defines available within the compiler argument
        /// </summary>
        /// <param name="compiler">The base Visual Studio C++ compiler configuration</param>
        /// <param name="definesHandler">The target structure which will host the extracted preprocessor definitions</param>
        private static void GetPreprocessorDefines(VCCLCompilerTool compiler, Defines definesHandler)
        {
            string definitions = compiler.PreprocessorDefinitions.Trim();
            string undefinitions = compiler.UndefinePreprocessorDefinitions.Trim();

            if (definitions.Length > 0)
            {
                string[] preProcessorDefinesArray = definitions.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string preProcessorDefine in preProcessorDefinesArray)
                {
                    // the below code is to support valued defines as per https://msdn.microsoft.com/en-us/library/vstudio/hhzbb5c8%28v=vs.100%29.aspx

                    Match matchResult = RegexPreProcesserDefines.Match(preProcessorDefine);

                    if (matchResult.Success)
                    {
                        definesHandler.Define(matchResult.Groups[1].Value.Trim(), matchResult.Groups[2].Value);
                    }
                    else
                    {
                        if (!preProcessorDefine.Contains("$(INHERIT)"))
                        {
                            definesHandler.Define(preProcessorDefine.Trim(), "1");
                            //by default user assigned pre-processor defines have a value of 1
                        }
                    }
                }
            }

            if (undefinitions.Length > 0)
            {
                string[] preProcessorUnDefinesArray = undefinitions.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var preProcessorUnDefine in preProcessorUnDefinesArray)
                {
                    if (!preProcessorUnDefine.Contains("$(NOINHERIT)"))
                    {
                        definesHandler.UnDefine(preProcessorUnDefine.Trim());
                    }
                }
            }
        }

        /// <summary>
        /// Registers the ANSI-Compliant Predefined Macros
        /// </summary>
        private static void RegisterAnsiCompliantPredefinedMacros(Defines definesHandler)
        {
            definesHandler.Define("__DATE__", "");
            definesHandler.Define("__FILE__", "");
            definesHandler.Define("__LINE__", "");
            definesHandler.Define("__TIME__", "");
            definesHandler.Define("__TIMESTAMP__", "");
        }

        /// <summary>
        /// Registers the Microsoft specific PreDefined compiler defines
        /// </summary>
        private static Defines RegisterMicrosoftPreDefinedCompilerMacros(Defines definesHandler)
        {
            /*
             * taken off https://msdn.microsoft.com/en-us/library/b0084kay(v=vs.110).aspx
             *
             * Only a limited number of pre defined macros is taken off the list present on Microsoft's website. The general philosophy used
             * is that the ones selected are those which are always present. i.e. that their presence is not dependent on the compiler settings, processor
             * architecture used.
             *
             */

            definesHandler.Define("__COUNTER__", "");
            definesHandler.Define("__cplusplus", "");
            definesHandler.Define("__FUNCDNAME__", "");
            definesHandler.Define("__FUNCSIG__", "");
            definesHandler.Define("__FUNCTION__", "");
            definesHandler.Define("_INTEGRAL_MAX_BITS", "");
            definesHandler.Define("_MSC_BUILD", "");
            definesHandler.Define("_MSC_FULL_VER", "");
            definesHandler.Define("_MSC_VER", "");
            definesHandler.Define("_WIN32", "");

            return definesHandler;
        }
    }
}