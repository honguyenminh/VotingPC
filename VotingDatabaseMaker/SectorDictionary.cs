using VotingPC;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingDatabaseMaker
{
    /// <summary>
    /// A unique class that tries to emulate Dictionary with List to allow binding with ListView
    /// </summary>
    public class SectorDictionary
    {
        public readonly List<Info> sectors = new();
        //public readonly List<string> sectorsIndex = new();
        public readonly System.Collections.ObjectModel.ObservableCollection<string> sectorsIndex = new();
        public SectorDictionary() { }

        /// <summary>
        /// Get sector's Info object
        /// </summary>
        /// <param name="index">Sector ID / name</param>
        /// <returns></returns>
        public Info GetSectorInfo(string index)
        {
            return sectors[sectorsIndex.IndexOf(index)];
        }

        /// <summary>
        /// Add index and info to the collection
        /// </summary>
        /// <param name="index">Index of added item</param>
        /// <param name="info">Info item to add</param>
        /// <returns>false if not added (item already exists), true otherwise</returns>
        public bool Add(string index, Info info)
        {
            if (sectorsIndex.Contains(index)) return false;
            sectorsIndex.Add(index);
            sectors.Add(info);
            return true;
        }

        /// <summary>
        /// Remove item with given index
        /// </summary>
        /// <param name="index">index of item to remove</param>
        /// <returns>true if removed, false if item not found</returns>
        public bool Remove(string index)
        {
            int indexToRemove = sectorsIndex.IndexOf(index);
            if (indexToRemove == -1) return false;
            sectors.RemoveAt(indexToRemove);
            sectorsIndex.RemoveAt(indexToRemove);
            return true;
        }
    }
}
