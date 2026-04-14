using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class ReadonlyAttribute : PropertyAttribute
{
    // Unity's own MessageType enum is part of UnityEditor, so using that will result in compiler errors when building.
    public enum MessageType
    {
        None = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }
    public readonly MessageType type;
    public readonly object defaultValue;

    // We do not reset if the game is being played. Runtime changes get priority.
    public bool Reset => reset && !Application.isPlaying;
    private readonly bool reset;

    // Use this constructor to enable the reset feature.
    public ReadonlyAttribute(object defaultValue, MessageType type)
    {
        this.type = type;
        this.defaultValue = defaultValue;
        reset = true;
    }

    // No constructor with only 'object defaultValue' to avoid ambiguïty between that and the MessageType ctor.
    // After all, MessageType can also be cast to System.Object... so should an instance of MessageType invoke the
    // MessageType ctor or the System.Object ctor?

    public ReadonlyAttribute(MessageType type)
    {
        this.type = type;
        defaultValue = null;
        reset = false;
    }

    public ReadonlyAttribute() : this(MessageType.Info) { }
}