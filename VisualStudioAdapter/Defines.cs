using System;
using System.Collections.Generic;

namespace VisualStudioAdapter
{
    /// <summary>
    /// Manages the define collection. Substitution defines are kept seperate from the non substitution defines
    /// so that whenever an expression needs to evaluated, we do supply it only with the substitution defines.
    /// </summary>
    public class Defines
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Defines()
        {
            SubstitutionTokens = new Dictionary<string, object>();
            NonSubstitutionTokens = new HashSet<string>();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="previousDefine">Defines instance to be copied</param>
        public Defines(Defines previousDefine)
        {
            if (previousDefine == null) throw new ArgumentNullException("previousDefine");

            SubstitutionTokens = new Dictionary<string, object>(previousDefine.SubstitutionTokens);
            NonSubstitutionTokens = new HashSet<string>(previousDefine.NonSubstitutionTokens);
        }

        /// <summary>
        /// Returns the substitution tokens
        /// </summary>
        /// <returns>a dictionary with the substitution tokens</returns>
        public Dictionary<string, object> SubstitutionTokens { get; private set; }

        /// <summary>
        /// Returns the non substitution tokens
        /// </summary>
        /// <returns>a hashset with the non substitution tokens</returns>
        public HashSet<string> NonSubstitutionTokens { get; private set; }

        /// <summary>
        /// Adds a token to the define collection.  If the token exists it gets redefined.
        /// </summary>
        /// <param name="token">the define token</param>
        /// <param name="substitutionText">the substitution text</param>
        public void Define(string token, string substitutionText)
        {
            substitutionText = (substitutionText == null) ? string.Empty : substitutionText;

            /*if the collections already contains the same key we remove it and replace it with
            * the new key value pair...which means that we actually redefining
            */
            UnDefine(token);

            substitutionText = substitutionText.Trim();

            if (substitutionText.Length == 0)
            {
                NonSubstitutionTokens.Add(token);
            }
            else
            {
                SubstitutionTokens.Add(token, substitutionText);
            }
        }

        /// <summary>
        /// Undefines a token
        /// </summary>
        /// <param name="token">token to be undefined</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Un")]
        public void UnDefine(string token)
        {
            if (NonSubstitutionTokens.Contains(token))
            {
                NonSubstitutionTokens.Remove(token);
            }
            else
            {
                if (SubstitutionTokens.ContainsKey(token))
                {
                    SubstitutionTokens.Remove(token);
                }
            }
        }

        /// <summary>
        /// Clears any defines being handled
        /// </summary>
        public void Clear()
        {
            NonSubstitutionTokens.Clear();
            SubstitutionTokens.Clear();
        }

        /// <summary>
        /// Evaluates if a token is defined or not
        /// </summary>
        /// <param name="token">token to be checked whether it is defined or not</param>
        /// <returns>true if defined, false if not</returns>
        public bool IsDefined(string token)
        {
            return NonSubstitutionTokens.Contains(token) || SubstitutionTokens.ContainsKey(token);
        }
    }
}