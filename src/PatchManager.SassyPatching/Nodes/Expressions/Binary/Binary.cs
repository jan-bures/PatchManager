﻿using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;

/// <summary>
/// Represents a binary expressions which performs a computation on 2 values to return one value
/// </summary>
public abstract class Binary : Expression
{
    /// <summary>
    /// The left hand side of this expression
    /// </summary>
    public readonly Expression LeftHandSide;
    /// <summary>
    /// The right hand side of this expression
    /// </summary>
    public readonly Expression RightHandSide;

    internal Binary(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c)
    {
        LeftHandSide = leftHandSide;
        RightHandSide = rightHandSide;
    }

    internal abstract DataValue GetResult(DataValue leftHandSide, DataValue rightHandSide);

    internal abstract bool ShortCircuitOn(DataValue dataValue);

    internal abstract DataValue ShortCircuitDataValue { get; }

    /// <inheritdoc />
    public override DataValue Compute(Environment environment)
    {
        var lhs = LeftHandSide.Compute(environment);
        if (ShortCircuitOn(lhs))
        {
            return ShortCircuitDataValue;
        }

        var rhs = RightHandSide.Compute(environment);
        return GetResult(lhs, rhs);
    }
}