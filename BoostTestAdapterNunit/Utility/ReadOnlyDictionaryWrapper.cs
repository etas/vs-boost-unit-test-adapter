// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Collections;
using System.Collections.Generic;

namespace BoostTestAdapterNunit.Utility
{
    /// <summary>
    /// A wrapper class for a generic Dictionary which provides a read-only access interface.
    /// </summary>
    /// <typeparam name="TK">The key type</typeparam>
    /// <typeparam name="TV">The mapped-value type</typeparam>
    public class ReadOnlyDictionaryWrapper<TK, TV> : IReadOnlyDictionary<TK, TV>
    {
        #region Members

        /// <summary>
        /// The wrapped Dictionary instance
        /// </summary>
        private IDictionary<TK, TV> _dictionary = null;

        #endregion Members

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dictionary">The Dictionary instance which will be wrapped</param>
        public ReadOnlyDictionaryWrapper(IDictionary<TK, TV> dictionary)
        {
            this._dictionary = dictionary;
        }

        #endregion Constructors

        #region IReadOnlyDictionary<K, V>

        public bool ContainsKey(TK key)
        {
            return this._dictionary.ContainsKey(key);
        }

        public IEnumerable<TK> Keys
        {
            get { return this._dictionary.Keys; }
        }

        public bool TryGetValue(TK key, out TV value)
        {
            return this._dictionary.TryGetValue(key, out value);
        }

        public IEnumerable<TV> Values
        {
            get { return this._dictionary.Values; }
        }

        public TV this[TK key]
        {
            get { return this._dictionary[key]; }
        }

        public int Count
        {
            get { return this._dictionary.Count; }
        }

        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
        {
            return this._dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion IReadOnlyDictionary<K, V>
    }
}
