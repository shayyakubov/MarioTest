using System;
using UnityEngine;

namespace MarioTest.Core
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class InjectComponentAttribute : PropertyAttribute
    {
    }
}
