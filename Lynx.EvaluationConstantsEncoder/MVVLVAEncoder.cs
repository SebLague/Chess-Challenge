#pragma warning disable RCS1243, S101, S125, S2368, IDE0047 // Class name, duplicate word in a comment, commented code,  multidimensional array parameters, extra parenthesis

namespace Lynx.EvaluationConstantsEncoder;
public static class MVVLVAEncoder
{
    /// <summary>
    /// <see cref="Lynx.EvaluationConstants.MostValueableVictimLeastValuableAttacker"/>
    /// but adapted to single-color pieces, reduced to save space
    /// and avoiding 0 to facilitate decoding
    ///             (Victims)   Pawn Knight Bishop  Rook Queen King
    /// (Attackers)
    /// 	  Pawn			      6    16     26    36    46    56
    ///     Knight                5    15     25    35    45    55
    ///     Bishop                4    14     24    34    44    54
    ///       Rook                3    13     23    33    43    53
    ///      Queen                2    12     22    32    42    52
    ///       King                1    11     21    31    41    51
    /// </summary>
    public static readonly int[,] MVVLVA = new int[6, 6]
    {
        { 6, 16, 26, 36, 46, 56 },
        { 5, 15, 25, 35, 45, 55 },
        { 4, 14, 24, 34, 44, 54 },
        { 3, 13, 23, 33, 43, 53 },
        { 2, 12, 22, 32, 42, 52 },
        { 1, 11, 21, 31, 41, 51 }
    };

    /// <summary>
    /// 56 -> 0011 1000, 8 bits
    /// dddd dddd cccc cccc bbbb bbbb aaaa aaaa	-> 32 bits, 4 numbers
    /// For simplicity, we use 2 per row
    /// 6 numbers / 4 numbers per int = 2 numbers per row
    /// xxxx xxxx cccc cccc bbbb bbbb aaaa aaaa	-> 32 bits, 3 numbers
    /// 6 rows * 2 numbers per row = 12 numbers
    /// </summary>
    public static int[] EncodeMVVLVAPerRowInt(int[,] input)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(36, input.Length);
        ArgumentOutOfRangeException.ThrowIfNotEqual(2, input.Rank);

        var encodedValues = new int[12];
        var currentValue = 0;
        for (int row = 0; row < 6; ++row)
        {
            for (int i = 0; i < 6; ++i)
            {
                var numberToEncode = input[row, i];
                currentValue |= (numberToEncode << 8 * (i % 3));

                //$"{numberToEncode} << {(8 * (i % 3))}".Dump();
                if (i % 3 == 2)
                {
                    encodedValues[row * 2 + (i - 1) / 3] = currentValue;
                    currentValue = 0;

                    //$"{((i - 1) / 3)} -> {currentValue}".Dump();
                    //byte[] test = encodedValues.SelectMany(BitConverter.GetBytes).ToArray();
                    //test.Dump();
                }
            }
        }

#if DEBUG
        var decompressed = DecodeInt(encodedValues);

        if (decompressed.Length != 36)
        {
            throw new();
        }
        PrintResult(decompressed);
        $"{{{string.Join(", ", encodedValues)}}}".Dump();
#endif

        return encodedValues;
    }

    /// <summary>
    /// 56 -> 0011 1000, 8 bits
    /// dddd dddd cccc cccc bbbb bbbb aaaa aaaa	-> 64 bits, 8 numbers
    /// 6 numbers / 8 numbers per int = 1 numbers per row
    /// 6 rows * 1 numbers per row = 6 numbers
    /// </summary>
    public static ulong[] EncodeMVVLVAPerRowUlong(int[,] input)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(36, input.Length);
        ArgumentOutOfRangeException.ThrowIfNotEqual(2, input.Rank);

        var encodedValues = new ulong[6];
        var currentValue = 0ul;
        for (int row = 0; row < 6; ++row)
        {
            for (int i = 0; i < 6; ++i)
            {
                var numberToEncode = Convert.ToUInt64(input[row, i]);
                currentValue |= (numberToEncode << 8 * (i % 8));

                //$"{numberToEncode} << {(8 * (i % 8))}".Dump();
                if (i == 5)
                {
                    encodedValues[row] = currentValue;
                    currentValue = 0;

                    //$"{((i - 1) / 8)} -> {currentValue}".Dump();
                    //byte[] test = encodedValues.SelectMany(BitConverter.GetBytes).ToArray();
                    //test.Dump();
                }
            }
        }

#if DEBUG
        var decompressed = Decode(encodedValues);

        if (decompressed.Length != 36)
        {
            throw new();
        }
        PrintResult(decompressed);
        $"{{{string.Join(", ", encodedValues)}}}".Dump();
#endif

        return encodedValues;
    }

    /// <summary>
    /// 56 -> 0011 1000, 8 bits
    /// dddd dddd cccc cccc bbbb bbbb aaaa aaaa	-> 64 bits, 8 numbers
    /// 36 numbers / 8 numbers per int = 5 numbers
    /// </summary>
    public static ulong[] EncodeMVVLVAAltogetherUlong(int[,] input)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(36, input.Length);
        ArgumentOutOfRangeException.ThrowIfNotEqual(2, input.Rank);

        var encodedValues = new ulong[5];
        var currentValue = 0ul;
        for (int row = 0; row < 6; ++row)
        {
            for (int i = 0; i < 6; ++i)
            {
                var itemNumber = (6 * row) + i;

                var numberToEncode = Convert.ToUInt64(input[row, i]);
                currentValue |= (numberToEncode << 8 * (itemNumber % 8));

                //$"{numberToEncode} << {(8 * (itemNumber % 8))}".Dump();
                if (itemNumber % 8 == 7 || itemNumber == 35)
                {
                    encodedValues[(itemNumber - 1) / 8] = currentValue;
                    currentValue = 0;

                    //$"{((itemNumber - 1) / 8)} -> {currentValue}".Dump();
                    //byte[] test = encodedValues.SelectMany(BitConverter.GetBytes).ToArray();
                    //test.Dump();
                }
            }
        }

#if DEBUG
        var decompressed = Decode(encodedValues);

        if (decompressed.Length != 36)
        {
            throw new();
        }
        PrintResult(decompressed);
        $"{{{string.Join(", ", encodedValues)}}}".Dump();
#endif

        return encodedValues;
    }

    /// <summary>
    /// 56 -> 0011 1000, 8 bits
    /// dddd dddd cccc cccc bbbb bbbb aaaa aaaa	-> 64 bits, 8 numbers
    /// 36 numbers / 8 numbers per int = 5 numbers
    /// </summary>
    public static decimal[] EncodeMVVLVAAltogetherDecimal(int[,] input)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(36, input.Length);
        ArgumentOutOfRangeException.ThrowIfNotEqual(2, input.Rank);

        var flattenedInput = new int[input.Length];
        for (int row = 0; row < 6; ++row)
        {
            for (int i = 0; i < 6; ++i)
            {
                flattenedInput[(6 * row) + i] = input[row, i];
            }
        }

        var itemsToAdd = 12 - flattenedInput.Length % 12;
        var data = new byte[flattenedInput.Length + itemsToAdd];

        for (int i = 0; i < flattenedInput.Length + itemsToAdd; ++i)
        {
            data[i] = i < flattenedInput.Length
                ? Convert.ToByte(flattenedInput[i])
                : byte.MaxValue;
        }

        var encodedValues = new decimal[data.Length / 12];
        for (int idx = 0; idx < encodedValues.Length; idx++)
        {
            encodedValues[idx] = new decimal(
                BitConverter.ToInt32(data, idx * 12),
                BitConverter.ToInt32(data, idx * 12 + 4),
                BitConverter.ToInt32(data, idx * 12 + 8),
                false,
                0);
        }

#if DEBUG
        var decompressed = Decode(encodedValues);

        if (decompressed.Length != 36)
        {
            throw new();
        }
        PrintResult(decompressed);
        $"{{{string.Join(", ", encodedValues)}}}".Dump();
#endif

        return encodedValues;
    }

    public static byte[] DecodeInt(int[] encodedValues)
    {
        return encodedValues
            .SelectMany(BitConverter.GetBytes)
            .Where(v => v != default)
            .ToArray();
    }

    public static byte[] Decode(ulong[] encodedValues)
    {
        return encodedValues
            .SelectMany(BitConverter.GetBytes)
            .Where(v => v != default)
            .ToArray();
    }

    public static byte[] Decode(decimal[] encodedValues)
    {
        return encodedValues
            .SelectMany(decimal.GetBits)
            .Where((_, i) => i % 4 != 3)
            .SelectMany(BitConverter.GetBytes)
            .Where(b => b < byte.MaxValue)
            .ToArray();
    }

    private static void PrintResult(
        byte[] decompressed,
        [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
    {
        Console.WriteLine($"\n{caller}:");

        for (int i = 0; i < decompressed.Length; ++i)
        {
            Console.Write("{0,10}", decompressed[i]/*.ToString("+0;-#")*/);

            if ((i + 1) % 6 == 0)
            {
                Console.WriteLine();
            }
            else
            {
                Console.Write(",");
            }
        }
        Console.WriteLine();
    }
}

#pragma warning restore RCS1243, S101, S125, S2368, IDE0047 // Class name, duplicate word in a comment, commented code,  multidimensional array parameters
