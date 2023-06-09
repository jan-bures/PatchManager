﻿using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Indexers;

/// <summary>
/// Represents a field indexer
/// </summary>
public abstract class Indexer : Node
{
    internal Indexer(Coordinate c) : base(c)
    {
    }

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
    }
}