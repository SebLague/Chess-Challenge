﻿#pragma warning disable RCS1243, S125, S3963, S3358 // Duplicate word in a comment, commented code, static inline initialization and ternary operators

namespace Lynx.EvaluationConstantsEncoder;
public static class TeenyLynxEncoder
{
    /// <summary>
    /// Increase used when encoding so that negative values for <see cref="EvaluationConstants.PositionalScore"/>
    /// and <see cref="EvaluationConstants.EndgamePositionalScore"/> can be encoded.
    /// Needs to be applied when decoding
    /// </summary>
    public static readonly byte ConstantIncrease;

    /// <summary>
    /// <see cref="Lynx.EvaluationConstants.MostValueableVictimLeastValuableAttacker"/>
    /// but: adapted to single-color pieces, reduced to save space, avoiding 0 to facilitate decoding
    /// and flattened to facilitate encoding operations
    ///             (Victims)   Pawn Knight Bishop  Rook Queen King
    /// (Attackers)
    /// 	  Pawn			      6    16     26    36    46    56
    ///     Knight                5    15     25    35    45    55
    ///     Bishop                4    14     24    34    44    54
    ///       Rook                3    13     23    33    43    53
    ///      Queen                2    12     22    32    42    52
    ///       King                1    11     21    31    41    51
    /// </summary>
    public static readonly int[] TeenyLynxMVVLVA = new int[36]
    {
        6, 16, 26, 36, 46, 56,
        5, 15, 25, 35, 45, 55,
        4, 14, 24, 34, 44, 54,
        3, 13, 23, 33, 43, 53,
        2, 12, 22, 32, 42, 52,
        1, 11, 21, 31, 41, 51
    };

    /// <summary>
    /// Calculates <see cref="ConstantIncrease"/> and ensures encoded values fall inside the supported 0-254 range
    /// </summary>
    static TeenyLynxEncoder()
    {
        var positionValues = EvaluationConstants.PositionalScore.SelectMany(jag => jag.Select(_ => _))
            .Concat(EvaluationConstants.EndgamePositionalScore.SelectMany(jag => jag.Select(_ => _)));

        ConstantIncrease = Convert.ToByte(Math.Abs(positionValues.Min()));

        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(ConstantIncrease, byte.MaxValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(positionValues.Max() + ConstantIncrease, byte.MaxValue);

        Console.WriteLine($"ConstantIncrease = {ConstantIncrease}");
    }

    public static decimal[] EncodeLynxData()
    {
        // 6 bitboards * 64 squares * 8 bits/square = 3072 bits
        var positionalScore = EvaluationConstants.PositionalScore.Take(EvaluationConstants.PositionalScore.Length / 2).ToArray();

        // 1 bitboard * 64 squares * 8 bits/square = 512 bits
        var endgameScore = EvaluationConstants.EndgamePositionalScore[(int)Model.Piece.K];

        // 1 bitboard * 36 squares * 8 bits/square = 288 bits
        var mvvlva = TeenyLynxMVVLVA;

        // 3072 + 512 + 288 = 3872 bits;
        // 3872 / 96 = 40,33. 41 decimals will be generared
        // 3872 % 96 = 32 bits over the last decimal;
        // 96 - 32 = 64 extra bits we could still encode 'for free' in the last decimal
        // 64 / 8 = 8 bytes (numbers) we could still encode 'for free' in the last decimal

        var infoToEncode = positionalScore
            .SelectMany(b => b.Select(x => x))
            .Concat(endgameScore)
            .Concat(mvvlva)
            .ToArray();

        if (infoToEncode.Length != 3872 / 8)
        {
            throw new("Unexpected result array length, something's off");
        }

        return EncodeDecimalWithEndiannessConversion(infoToEncode);
    }

    /// <summary>
    /// Credits to @Skolin
    /// https://discord.com/channels/1132289356011405342/1132768358350200982/1132768358350200982
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private static decimal[] EncodeDecimalWithEndiannessConversion(int[] input)
    {
        var itemsToAdd = 12 - input.Length % 12;
        var data = new byte[input.Length + itemsToAdd];

        for (int i = 0; i < input.Length + itemsToAdd; ++i)
        {
            // Little-endian rank mapping to big endian-rank mapping transformation for <see cref="EvaluationConstants.PositionalScore"/> and <see cref="EvaluationConstants.EndgamePositionalScore"/> (^56)
            // No transformation for <see cref="TeenyLynxMVVLVA"/>
            // byte.MaxValue at the end to fill the gaps in a distinguisable way
            data[i] = i < input.Length
                ? Convert.ToByte(input[i < 448 ? i ^ 56 : i] + ConstantIncrease)
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

    /// <summary>
    /// Credits to @Skolin
    /// https://discord.com/channels/1132289356011405342/1132768358350200982/1132768358350200982
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
            Console.Write("{0,10:D2}", decompressed[i]);

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

    private static void PrintResult<T>(T[] encodedValues) => $"[{encodedValues.Length} items]\n{{{string.Join("m, ", encodedValues)}}}".Dump();
}

#pragma warning restore RCS1243, S125 // Duplicate word in a comment, commented code, static inline initialization and ternary operators
