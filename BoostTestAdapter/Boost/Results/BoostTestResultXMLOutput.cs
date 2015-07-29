using System.IO;
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
                memoryStream = new MemoryStream();
                StreamWriter writer = new StreamWriter(memoryStream);
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>");
                writer.Flush();
                FileStream fileStream = null;
                try
                {
                    fileStream = File.OpenRead(path);
                    fileStream.CopyTo(memoryStream);
                }
                finally
                {
                    if (fileStream != null)
                    {
                        fileStream.Close();
                    }
                    else
                    {
                        Logger.Error("filestream was found to be null when handling path: " + path);
                    }
                }

                memoryStream.Position = 0;
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
    }
}
