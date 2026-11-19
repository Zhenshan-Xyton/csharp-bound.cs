// B2 STRESS FIXTURE (csharp): BOUND
// EFFECT: boundedness:UNBOUNDED -> boundedness:BOUNDED
// OBSERVABLE AT: R1 (syntax only -- no execution, no compilation, no dataflow required to see it)
// GROUNDED IN: codeql_census/repo/csharp/ql/lib/semmle/code/csharp/dataflow/internal/rangeanalysis/
//   ConstantUtils.qll (predicate systemArrayLengthAccess(PropertyAccess pa) { propertyOverrides(pa.getTarget(),
//   "System", "Array", "Length") } -- Array.Length is explicitly modeled as a bound expression by CodeQL's own
//   range analysis), codeql_census/repo/csharp/ql/lib/semmle/code/csharp/dataflow/Bound.qll
//   (interestingExprBound: "CU::systemArrayLengthAccess(e.getExpr())" -- the same Length access is treated as an
//   interesting bound for guard reasoning). List<T>.Count, string.Length and Array.GetLength(n) are the
//   idiomatic analogs of Array.Length for the other collection-shaped nouns below.
// REALIZATIONS (noun x verb-spelling, each CLOSED + OPEN):
//   1. noun=int array index, verb=index < values.Length
//   2. noun=List<T> index, verb=index < items.Count
//   3. noun=byte buffer span slice, verb=offset + length > buffer.Length
//   4. noun=string substring range, verb=start + length > record.Length
//   5. noun=stack pop count, verb=n > stack.Count
//   6. noun=2D matrix cell, verb=row/col vs matrix.GetLength(0)/GetLength(1)

using System;
using System.Collections.Generic;

namespace B2StressFixtures.CSharp.Bound
{
    public static class BoundFixtures
    {
        // --- realization 1: noun=int array index, verb=index < values.Length -----------------
        // CLOSED
        public static int ReadAt_Closed(int[] values, int index)
        {
            if (index < 0 || index >= values.Length) // bound check precedes the indexed access below
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return values[index];
        }

        // OPEN
        public static int ReadAt_Open(int[] values, int index)
        {
            return values[index]; // indexed access with no bound check anywhere before it
        }

        // --- realization 2: noun=List<T> index, verb=index < items.Count ----------------------
        // CLOSED
        public static string GetItem_Closed(List<string> items, int index)
        {
            if (index < 0 || index >= items.Count) // bound check precedes the indexed access below
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return items[index];
        }

        // OPEN
        public static string GetItem_Open(List<string> items, int index)
        {
            return items[index]; // indexed access with no bound check anywhere before it
        }

        // --- realization 3: noun=byte buffer span slice, verb=offset + length > buffer.Length
        // CLOSED
        public static ReadOnlySpan<byte> SliceFrame_Closed(byte[] buffer, int offset, int length)
        {
            if (offset < 0 || length < 0 || offset + length > buffer.Length) // bound check precedes the indexed access below
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            return buffer.AsSpan(offset, length);
        }

        // OPEN
        public static ReadOnlySpan<byte> SliceFrame_Open(byte[] buffer, int offset, int length)
        {
            return buffer.AsSpan(offset, length); // indexed slice with no bound check anywhere before it
        }

        // --- realization 4: noun=string substring range, verb=start + length > record.Length -
        // CLOSED
        public static string ExtractField_Closed(string record, int start, int length)
        {
            if (start < 0 || length < 0 || start + length > record.Length) // bound check precedes the indexed access below
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            return record.Substring(start, length);
        }

        // OPEN
        public static string ExtractField_Open(string record, int start, int length)
        {
            return record.Substring(start, length); // indexed access with no bound check anywhere before it
        }

        // --- realization 5: noun=stack pop count, verb=n > stack.Count ------------------------
        // CLOSED
        public static List<int> PopMany_Closed(Stack<int> stack, int n)
        {
            if (n < 0 || n > stack.Count) // bound check precedes the pop loop below
            {
                throw new ArgumentOutOfRangeException(nameof(n));
            }
            List<int> popped = new List<int>(n);
            for (int i = 0; i < n; i++)
            {
                popped.Add(stack.Pop());
            }
            return popped;
        }

        // OPEN
        public static List<int> PopMany_Open(Stack<int> stack, int n)
        {
            List<int> popped = new List<int>(n);
            for (int i = 0; i < n; i++)
            {
                popped.Add(stack.Pop()); // pop loop with no bound check against stack.Count anywhere before it
            }
            return popped;
        }

        // --- realization 6: noun=2D matrix cell, verb=row/col vs matrix.GetLength(0)/GetLength(1)
        // CLOSED
        public static double ReadCell_Closed(double[,] matrix, int row, int col)
        {
            if (row < 0 || row >= matrix.GetLength(0) || col < 0 || col >= matrix.GetLength(1)) // bound check precedes the indexed access below
            {
                throw new ArgumentOutOfRangeException(nameof(row));
            }
            return (double)matrix.GetValue(row, col);
        }

        // OPEN
        public static double ReadCell_Open(double[,] matrix, int row, int col)
        {
            return (double)matrix.GetValue(row, col); // access with no bound check anywhere before it
        }
    }
}
