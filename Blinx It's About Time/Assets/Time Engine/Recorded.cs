using System;

namespace TimeControl
{
    /// <summary>
    /// Indicates a property or field can be recorded by the time engine.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property |
        AttributeTargets.Field,
        AllowMultiple = true
    )]
    public class Recorded : Attribute
    {
    }

}