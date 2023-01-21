namespace Differ.Enums
{
    /// <summary>
    /// Describes the change between two states. 
    /// </summary>
    public enum DifferenceType
    {
        /// <summary>
        /// Represents a difference where something was added.
        /// </summary>
        Add = 0,
        /// <summary>
        /// Represents a difference where something was removed.
        /// </summary>
        Remove = 1,
        /// <summary>
        /// Represents a difference where something was updated.
        /// </summary>
        Update = 2,
        /// <summary>
        /// Represents an assignment to an added object.
        /// </summary>
        Assign = 3,
        /// <summary>
        /// Represents a difference where something was moved.
        /// </summary>
        Move = 4
    }
}
