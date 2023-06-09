﻿using System.Globalization;
using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;

/// <summary>
/// A binary expression which indexes the left hand side by the right hand side and returns the result
/// </summary>
public class Subscript : Binary
{
    internal Subscript(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

    // ReSharper disable once CognitiveComplexity
    internal override Value GetResult(Value leftHandSide, Value rightHandSide)
    {
        if (leftHandSide.IsList && rightHandSide.IsNumber)
        {
            try
            {
                return leftHandSide.List[(int)rightHandSide.Number];
            }
            catch
            {
                throw new ListIndexOutOfRangeException(Coordinate,
                    ((int)rightHandSide.Number) + " is out of range of the list being indexed");
            }
        }

        if (leftHandSide.IsString && rightHandSide.IsNumber)
        {
            try
            {
                return (double)leftHandSide.String[(int)rightHandSide.Number];
            }
            catch
            {
                throw new ListIndexOutOfRangeException(Coordinate,
                    ((int)rightHandSide.Number) + " is out of range of the string being indexed");
            }
        }

        if (leftHandSide.IsDictionary && rightHandSide.IsString)
        {
            try
            {
                return leftHandSide.Dictionary[rightHandSide.String];
            }
            catch
            {
                throw new DictionaryKeyNotFoundException(Coordinate,
                    rightHandSide.String + " is not a key found in the dictionary being indexed");
            }
        }

        throw new BinaryExpressionTypeException(Coordinate, "subscript", leftHandSide.Type.ToString(),
            rightHandSide.Type.ToString());
    }

    internal override bool ShortCircuitOn(Value value) => false;

    internal override Value ShortCircuitValue => null;
}