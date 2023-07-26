using static TeenyLynx.Encoder.PositionScoreEncoder;
using static TeenyLynx.Encoder.MVVLVAEncoder;
using Lynx;
using TeenyLynx.Encoder;

//TestGenerator();

//EncodePositionScoreLynxDataDecimal();

TeenyLynxEncoder.EncodeLynxData();

static void TestGenerator()
{
    EncodePositionScoreInt(PawnPositionalScore);
    EncodePositionScoreUlong(PawnPositionalScore);
    EncodePositionScoreUlongWithEndiannessConversion(PawnPositionalScore);
    EncodePositionScoreDecimalWithEndiannessConversion(PawnPositionalScore);

    EncodeMVVLVAPerRowInt(MVVLVA);
    EncodeMVVLVAPerRowUlong(MVVLVA);
    EncodeMVVLVAAltogetherUlong(MVVLVA);
    EncodeMVVLVAAltogetherDecimal(MVVLVA);
}

static void EncodeUlongLynxData()
{
    // Jagged arrays
    //$"{{\n{string.Join(",\n",
    //            EvaluationConstants.PositionalScore.Select(bitboard => $"\t{{{string.Join(",\t", EncodePositionScoreUlongWithEndiannessConversion(bitboard))}}}"))}\n}}".Dump();

    // Single array
    $"{{\n{string.Join(",\n",
                EvaluationConstants.PositionalScore.Take(EvaluationConstants.PositionalScore.Length / 2).Select(bitboard => $"\t{string.Join(", ", EncodePositionScoreUlongWithEndiannessConversion(bitboard))}"))}\n}}".Dump();

    string.Join(", ",
        EncodePositionScoreUlongWithEndiannessConversion(EvaluationConstants.EndgamePositionalScore[(int)Lynx.Model.Piece.K])).Dump();

    string.Join(",", EncodeMVVLVAAltogetherUlong(MVVLVA)).Dump();
}
