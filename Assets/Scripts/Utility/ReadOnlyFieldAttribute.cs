using System;
using UnityEngine;

namespace Bejeweled.Utility
{
    /// <summary>
    /// Readonly field display support in editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyFieldAttribute : PropertyAttribute
    {
        //
    }
}