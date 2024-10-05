using System;

namespace Assets._Scripts.Xbox
{
    internal class XboxSaveMetadata
    {
        public int LastSaveIndex; // Tracks the most recent save slot
        public DateTime LastUpdated; // The timestamp of the last save
    }
}
