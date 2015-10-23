// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using BoostTestAdapter.Utility;

namespace BoostTestAdapter.Boost.Results
{

    /// <summary>
    /// Boost Test Result abstraction needed for the proper handling of XML documents.
    /// </summary>
    public abstract class BoostTestResultXMLOutput : BoostTestResultOutputBase
    {
        protected BoostTestResultXMLOutput(string path)
            : base(AddXMLEncodingDeclaration(path))
        {
            this.CloseStreamOnDispose = true;
        }

        protected BoostTestResultXMLOutput(Stream stream)
            : base(stream)
        {

        }

        /// <summary>
        /// Boost UTF does not add any XML Encoding Declaration in the XML file so in case a file contains German characters,
        /// upon loading the xml document an exception will be thrown due to un-allowed characters
        /// </summary>
        /// <param name="path">path of the xml file to be loaded</param>
        /// <returns>Stream object containing the xml file data</returns>
        private static Stream AddXMLEncodingDeclaration(string path)
        {
            MemoryStream memoryStream = null;
            try
            {
                if (File.Exists(path))
                {
                    var enc = Encoding.GetEncoding("iso-8859-1");
                    enc = (Encoding)enc.Clone();
                    enc.EncoderFallback = new EncoderReplacementFallback(string.Empty);

                    var fileContent = File.ReadAllText(path, enc);
                    fileContent = ParseCDataSection(fileContent);
                    if (!fileContent.StartsWith("<?xml", StringComparison.Ordinal))
                        fileContent = fileContent.Insert(0, "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\n");

                    byte[] encodedBuffer = enc.GetBytes(fileContent).Where(x => x > 0).ToArray();

                    memoryStream = new MemoryStream(encodedBuffer);
                    memoryStream.Position = 0;
                }
            }
            catch
            {
                if (memoryStream != null)
                {
                    memoryStream.Close();
                }
                throw;
            }

            return memoryStream;
        }

        /// <summary>
        /// Replaces all the control characters (less than 32) in the CDATA section with the hexadecimal representation.
        /// </summary>
        /// <param name="fileContent">The XML content to be filtered.</param>
        /// <returns>The filtered content.</returns>
        /// <remarks>
        /// Until version 1.58 Boost Unit Test Library outputs non-filtered content of arrays in the log/report.
        /// This function fixes the output to avoid XML parsing crashes using the same approach the library uses starting from version 1.59.
        /// </remarks>
        private static string ParseCDataSection(string fileContent)
        {
            var startPos = fileContent.IndexOf("<![CDATA[", StringComparison.Ordinal);
            while (startPos > -1)
            {
                var endPos = fileContent.IndexOf("]]>", startPos, StringComparison.Ordinal);

                var dataSectionContent = fileContent.Substring(startPos, endPos - startPos);

                for (int i = 0; i < 32; i++)
                {
                    if (i == 10 || i == 13)
                        continue;

                    string c = char.ConvertFromUtf32(i);
                    dataSectionContent = dataSectionContent.Replace(c, string.Format(CultureInfo.InvariantCulture, "0x{0:X2}", i));
                }

                fileContent = fileContent.Substring(0, startPos) + dataSectionContent + fileContent.Substring(endPos);

                startPos = fileContent.IndexOf("<![CDATA[", endPos, StringComparison.Ordinal);
            }

            return fileContent;
        }
    }
}
