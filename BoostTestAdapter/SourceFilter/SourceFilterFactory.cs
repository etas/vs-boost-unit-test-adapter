using BoostTestAdapter.Settings;
using BoostTestAdapter.Utility;

namespace BoostTestAdapter.SourceFilter
{
    /// <summary>
    /// Factory class responsible to create the right set of filters to apply onto a sourcefile
    /// </summary>
    public static class SourceFilterFactory
    {
        /// <summary>
        /// Provides a collection of ISourceFilters based on the provided BoostTestAdapterSettings.
        /// </summary>
        /// <param name="settings">The BoostTestAdapterSettings</param>
        /// <returns>An collection of ISourceFilters based on the provided settings.</returns>
        public static ISourceFilter[] Get(BoostTestAdapterSettings settings)
        {
            Utility.Code.Require(settings, "settings");

            if (settings.ConditionalInclusionsFilteringEnabled)
            {
                return new ISourceFilter[]
                {
                    new QuotedStringsFilter(),
                    new MultilineCommentFilter(),
                    new SingleLineCommentFilter(),
                    new ConditionalInclusionsFilter(
                        new ExpressionEvaluation()
                    )
                };
            }
            else
            {
                Logger.Warn("Conditional inclusions filtering is disabled.");

                return new ISourceFilter[]
                {
                    new QuotedStringsFilter(),
                    new MultilineCommentFilter(),
                    new SingleLineCommentFilter(),
                    //conditional inclusions filter omitted
                };
            }
        }
    }
}