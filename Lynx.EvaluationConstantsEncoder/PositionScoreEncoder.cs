#pragma warning disable RCS1243, S125, S3963 // Duplicate word in a comment, commented code

namespace Lynx.EvaluationConstantsEncoder;
public static class PositionScoreEncoder
{
    public static readonly byte ConstantIncrease;

    static PositionScoreEncoder()
    {
        var positionValues = EvaluationConstants.PositionalScore.SelectMany(jag => jag.Select(_ => _))
            .Concat(EvaluationConstants.EndgamePositionalScore.SelectMany(jag => jag.Select(_ => _)));

        ConstantIncrease = Convert.ToByte(Math.Abs(positionValues.Min()));

        ArgumentOutOfRangeException.ThrowIfGreaterThan(ConstantIncrease, byte.MaxValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(positionValues.Max() + ConstantIncrease, byte.MaxValue);

        Console.WriteLine($"ConstantIncrease = {ConstantIncrease}");
    }

    /// <summary>
    /// Provided as examaple
    /// </summary>
    internal static readonly int[] PawnPositionalScore = new int[64]
        {
            90,  90,  90,  90,  90,  90,  90,  90,
            40,  40,  40,  50,  50,  40,  40,  40,
            20,  20,  20,  30,  30,  30,  20,  20,
            10,  10,  10,  20,  20,  10,  10,  10,
             0,   0,  10,  20,  20,   0,   0,   0,
             0,   0,   0,  10,  10,   0,   0,   0,
             0,   0,   0, -10, -10,   0,   0,   0,
             0,   0,   0,   0,   0,   0,   0,   0
        };

    public static decimal[] EncodePositionScoreLynxDataDecimal()
    {
        // 6 bitboards * 64 squares * 8 bits/square = 3072 bits
        var positionalScore = EvaluationConstants.PositionalScore.Take(EvaluationConstants.PositionalScore.Length / 2).ToArray();

        // 1 bitboard * 64 squares * 8 bits/square = 512 bits
        var endgameScore = EvaluationConstants.EndgamePositionalScore[(int)Lynx.Model.Piece.K];

        // 3072 + 512 = 3584 psqt bits;
        // 3584 % 96 = 32 bits over the last decimal;
        // 96 - 32 = 64 extra bits we could encode for free
        // 64 / 8 = 8 bytes (numbers) we could encode for free

        var infoToEncode = positionalScore
            .SelectMany(b => b.Select(x => x))
            .Concat(endgameScore)
            .ToArray();

        if (infoToEncode.Length != 3584 / 8)
        {
            throw new("We're doing something wrong");
        }

        return EncodePositionScoreDecimalWithEndiannessConversion(infoToEncode);
    }


    /// <summary>
    /// 140 -> 1000 1100, 8 bits
    /// dddd dddd cccc cccc bbbb bbbb aaaa aaaa	-> 32 bits, 4 numbers
    /// 64 numbers / 4 numbers per int = 16 numbers per table
    /// </summary>
    public static int[] EncodePositionScoreInt(int[] input)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(PawnPositionalScore.Length, input.Length);

        var encodedValues = new int[16];
        var currentValue = 0;
        for (int i = 0; i < input.Length; ++i)
        {
            var numberToEncode = input[i] + ConstantIncrease;

            if (numberToEncode > byte.MaxValue)
            {
                throw new("Values too low or too high");
            }

            currentValue |= (numberToEncode << 8 * (i % 4));

            //$"{numberToEncode - 50} << {(8 * (i % 4))}".Dump();
            if (i % 4 == 3)
            {
                encodedValues[(i - 1) / 4] = currentValue;
                currentValue = 0;

                //$"{((i - 1) / 4)} -> {currentValue}".Dump();
                //byte[] test = encodedValues.SelectMany(BitConverter.GetBytes).ToArray();
                //test.Dump();
            }
        }

#if DEBUG
        var decompressed = DecodeInt(encodedValues);

        if (decompressed.Length != 64)
        {
            throw new();
        }
        PrintDecompressedData(decompressed);
#endif
        PrintResult(encodedValues);

        return encodedValues;
    }

    /// <summary>
    /// 140 -> 1000 1100, 8 bits
    /// dddd dddd cccc cccc bbbb bbbb aaaa aaaa	-> 64 bits, 8 numbers
    /// 64 numbers / 8 numbers per int = 8 numbers per table
    /// </summary>
    public static ulong[] EncodePositionScoreUlong(int[] input)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(PawnPositionalScore.Length, input.Length);

        var encodedValues = new ulong[8];
        var currentValue = 0ul;
        for (int i = 0; i < input.Length; ++i)
        {
            var numberToEncode = Convert.ToUInt64(input[i] + ConstantIncrease);

            if (numberToEncode > byte.MaxValue)
            {
                throw new("Values too low or too high");
            }

            currentValue |= (numberToEncode << 8 * (i % 8));

            //$"{numberToEncode - 50} << {(8 * (i % 8))}".Dump();
            if (i % 8 == 7)
            {
                encodedValues[(i - 1) / 8] = Convert.ToUInt64(currentValue);
                currentValue = 0;

                //$"{((i - 1) / 8)} -> {currentValue}".Dump();
                //byte[] test = encodedValues.SelectMany(BitConverter.GetBytes).ToArray();
                //test.Dump();
            }
        }
#if DEBUG
        var decompressed = Decode(encodedValues);

        if (decompressed.Length != input.Length)
        {
            throw new();
        }
        PrintDecompressedData(decompressed);
#endif
        PrintResult(encodedValues);

        return encodedValues;
    }

    /// <summary>
    /// Constant increase of 100
    /// 140 -> 1000 1100, 8 bits
    /// dddd dddd cccc cccc bbbb bbbb aaaa aaaa	-> 64 bits, 8 numbers
    /// 64 numbers / 8 numbers per int = 8 numbers per table
    /// i ^ 56 converts from little-endian rank mapping to big endian-rank mapping
    /// </summary>
    public static ulong[] EncodePositionScoreUlongWithEndiannessConversion(int[] input)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(PawnPositionalScore.Length, input.Length);

        var encodedValues = new ulong[8];
        var currentValue = 0ul;
        for (int i = 0; i < input.Length; ++i)
        {
            var numberToEncode = Convert.ToUInt64(input[i ^ 56] + ConstantIncrease);

            if (numberToEncode > byte.MaxValue)
            {
                throw new("Values too low or too high");
            }

            currentValue |= (numberToEncode << 8 * (i % 8));

            //$"{numberToEncode - 50} << {(8 * (i % 8))}".Dump();
            if (i % 8 == 7)
            {
                encodedValues[(i - 1) / 8] = Convert.ToUInt64(currentValue);
                currentValue = 0;

                //$"{((i - 1) / 8)} -> {currentValue}".Dump();
                //byte[] test = encodedValues.SelectMany(BitConverter.GetBytes).ToArray();
                //test.Dump();
            }
        }

#if DEBUG
        var decompressed = Decode(encodedValues);

        if (decompressed.Length != PawnPositionalScore.Length)
        {
            throw new();
        }
        PrintDecompressedData(decompressed);
#endif
        PrintResult(encodedValues);

        return encodedValues;
    }

    public static decimal[] EncodePositionScoreDecimalWithEndiannessConversion(int[] input)
    {
        var itemsToAdd = 12 - input.Length % 12;
        var data = new byte[input.Length + itemsToAdd];

        for (int i = 0; i < input.Length + itemsToAdd; ++i)
        {
            data[i] = i < input.Length
                ? Convert.ToByte(input[i ^ 56] + ConstantIncrease)
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

        if (decompressed.Length != input.Length)
        {
            throw new();
        }
        PrintDecompressedData(decompressed);
#endif
        PrintResult(encodedValues);

        return encodedValues;
    }

    public static int[] DecodeInt(int[] encodedValues)
    {
        return encodedValues
            .SelectMany(BitConverter.GetBytes)
            .Select(v => v - ConstantIncrease)
            .ToArray();
    }

    public static int[] Decode(ulong[] encodedValues)
    {
        return encodedValues
            .SelectMany(BitConverter.GetBytes)
            .Select(v => v - ConstantIncrease)
            .ToArray();
    }

    /// <summary>
    /// GetBits docs(https://learn.microsoft.com/en-us/dotnet/api/system.decimal.getbits?view=net-7.0&redirectedfrom=MSDN#System_Decimal_GetBits_System_Decimal_)
    /// The binary representation of a Decimal number consists of a 1 - bit sign, a 96 - bit integer number,
    ///    and a scaling factor used to divide the integer number and specify what portion of it is a decimal fraction.
    ///    The scaling factor is implicitly the number 10, raised to an exponent ranging from 0 to 28.
    /// The return value is a four - element array of 32 - bit signed integers.
    /// The first, second, and third elements of the returned array contain the low, middle, and high
    ///     32 bits of the 96 - bit integer number.
    /// The fourth element of the returned array contains the scale factor and sign.
    /// </summary>
    /// <param name="encodedValues"></param>
    /// <returns></returns>
    public static int[] Decode(decimal[] encodedValues)
    {
        return encodedValues
            .SelectMany(decimal.GetBits)
            .Where((_, i) => i % 4 != 3)
            .SelectMany(BitConverter.GetBytes)
            .Where(b => b < byte.MaxValue)
            .Select(b => b - ConstantIncrease)
            .ToArray();
    }

    private static void PrintDecompressedData<T>(
        T[] decompressed,
        [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
    {
        Console.WriteLine($"\n{caller}:");

        for (int i = 0; i < decompressed.Length; ++i)
        {
            Console.Write("{0,10:D2}", decompressed[i]/*.ToString("+0;-#")*/);

            if ((i + 1) % 8 == 0)
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

    private static void PrintResult<T>(T[] encodedValues) => $"[{encodedValues.Length} items]\n{{{string.Join(", ", encodedValues)}}}".Dump();
}

#pragma warning restore RCS1243, S125 // Duplicate word in a comment, commented code
