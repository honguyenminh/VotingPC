using VotingPC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingDatabaseMaker
{
    /// <summary>
    /// A unique class that tries to emulate Dictionary with List and ObservableCollection to allow binding with ListView
    /// </summary>
    public class ObservableDictionary<TKey, TValue>
    {
        // Public property
        /// <summary>
        /// List of values
        /// </summary>
        public ObservableCollection<TValue> Values { get; } = new();
        /// <summary>
        /// List of keys
        /// </summary>
        public ObservableCollection<TKey> Keys { get; } = new();

        /// <summary>
        /// New ObservaleDictionary
        /// </summary>
        public ObservableDictionary() { }

        /// <summary>
        /// Get item from key
        /// </summary>
        /// <param name="key">Sector ID / name</param>
        /// <returns></returns>
        public TValue GetValue(TKey key)
        {
            return Values[Keys.IndexOf(key)];
        }

        /// <summary>
        /// Add key and value pair to the collection
        /// </summary>
        /// <param name="key">Key of added item</param>
        /// <param name="value">Item to add</param>
        /// <returns>false if not added (item already exists), true otherwise</returns>
        public bool Add(TKey key, TValue value)
        {
            if (Keys.Contains(key)) return false;
            Keys.Add(key);
            Values.Add(value);
            return true;
        }

        /// <summary>
        /// Remove item with given index
        /// </summary>
        /// <param name="index">index of item to remove</param>
        /// <returns>true if removed, false if item not found</returns>
        public bool Remove(TKey index)
        {
            int indexToRemove = Keys.IndexOf(index);
            if (indexToRemove == -1) return false;
            Values.RemoveAt(indexToRemove);
            Keys.RemoveAt(indexToRemove);
            return true;
        }
    }
}
