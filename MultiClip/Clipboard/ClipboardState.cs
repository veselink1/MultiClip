using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiClip.Clipboard
{
    /// <summary>
    /// Encapsulates a past or present clipboard state.
    /// </summary>
    public class ClipboardState
    {
        /// <summary>
        /// The generic identifier of this state.
        /// </summary>
        public Guid Id { get; private set; }
        
        /// <summary>
        /// A DateTime object specifying the time of creation.
        /// </summary>
        public DateTime DateTime { get; private set; }

        /// <summary>
        /// A list of the clipboard items in this state.
        /// </summary>
        public IReadOnlyList<ClipboardItem> Items { get; private set; }

        /// <summary>
        /// Creates a new clipboard state containing the given list of items.
        /// </summary>
        /// <param name="items"></param>
        public ClipboardState(IReadOnlyList<ClipboardItem> items)
        {
            Id = Guid.NewGuid();
            DateTime = DateTime.Now;
            Items = items ?? throw new ArgumentNullException(nameof(items));
        }

        /// <summary>
        /// Creates a new clipboard state containing the given list of items.
        /// </summary>
        /// <param name="items"></param>
        public ClipboardState(Guid id, DateTime dateTime, IReadOnlyList<ClipboardItem> items)
        {
            Id = id;
            DateTime = dateTime;
            Items = items ?? throw new ArgumentNullException(nameof(items));
        }

        /// <summary>
        /// Compares two clipboard states for equality.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is ClipboardState other)
            {
                if (Items.Count != other.Items.Count)
                {
                    return false;
                }

                foreach (var x in Items)
                {
                    // The items in the collections are not ordered by 
                    // any criteria, and are usually laid out randomly.
                    var y = other.Items.FirstOrDefault(item => item.Format == x.Format);
                    if (!Equals(x, y))
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return 1838681114 + EqualityComparer<IReadOnlyList<ClipboardItem>>.Default.GetHashCode(Items);
        }
    }
}
