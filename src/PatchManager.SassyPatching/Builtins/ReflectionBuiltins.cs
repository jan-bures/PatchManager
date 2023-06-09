﻿using JetBrains.Annotations;
using PatchManager.SassyPatching.Attributes;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Builtins;

/// <summary>
/// This is the reflection library used by Sassy patching, its very simple as not much reflection is needed by the language
/// </summary>
[SassyLibrary("builtin","reflection")]
[PublicAPI]
public class ReflectionBuiltins
{
    /// <summary>
    /// Gets the type of a value
    /// </summary>
    /// <param name="dataValue">The value to get the type of</param>
    /// <returns>The values type as a lowercase string</returns>
    [SassyMethod("typeof")]
    public static string GetValueType(DataValue dataValue)
    {
        return dataValue.Type.ToString().ToLowerInvariant();
    }
}