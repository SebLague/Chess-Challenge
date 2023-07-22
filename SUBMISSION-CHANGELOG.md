### 1.0
Initial release - randoom legal mover

53 tokens

```
Score of ChessChallenge1.0 vs ChessChallengeEvil: 2 - 4211 - 787  [0.079] 5000
...      ChessChallenge1.0 playing White: 2 - 2081 - 416  [0.084] 2499
...      ChessChallenge1.0 playing Black: 0 - 2130 - 371  [0.074] 2501
...      White vs Black: 2132 - 2081 - 787  [0.505] 5000
Elo difference: -426.4 +/- 12.1, LOS: 0.0 %, DrawRatio: 15.7 %
```

### 1.1
Basics for a searching engine:
* Iterative deepening
* Brute-force search
* Basic time tracking
* Material-only evaluation
* Mate and stalemate detection

329 tokens

```
Score of ChessChallenge1.1 vs ChessChallenge1.0: 1783 - 0 - 217  [0.946] 2000
...      ChessChallenge1.1 playing White: 886 - 0 - 114  [0.943] 1000
...      ChessChallenge1.1 playing Black: 897 - 0 - 103  [0.949] 1000
...      White vs Black: 886 - 897 - 217  [0.497] 2000
Elo difference: 496.6 +/- 23.2, LOS: 100.0 %, DrawRatio: 10.8 %

Score of ChessChallenge1.1 vs ChessChallengeEvil: 1667 - 0 - 333  [0.917] 2000
...      ChessChallenge1.1 playing White: 828 - 0 - 171  [0.914] 999
...      ChessChallenge1.1 playing Black: 839 - 0 - 162  [0.919] 1001
...      White vs Black: 828 - 839 - 333  [0.497] 2000
Elo difference: 416.7 +/- 18.6, LOS: 100.0 %, DrawRatio: 16.7 %
```

### 1.2
Centrality evaluation

383 tokens

```
Score of ChessChallenge vs ChessChallengeDev: 338 - 81 - 1581  [0.564] 2000
...      ChessChallenge playing White: 189 - 33 - 778  [0.578] 1000
...      ChessChallenge playing Black: 149 - 48 - 803  [0.550] 1000
...      White vs Black: 237 - 182 - 1581  [0.514] 2000
Elo difference: 44.9 +/- 6.8, LOS: 100.0 %, DrawRatio: 79.0 %
```

### 1.3
Poor man's repetition detection

448 tokens

```
Score of ChessChallenge vs ChessChallengeDev: 272 - 143 - 585  [0.565] 1000
...      ChessChallenge playing White: 126 - 75 - 300  [0.551] 501
...      ChessChallenge playing Black: 146 - 68 - 285  [0.578] 499
...      White vs Black: 194 - 221 - 585  [0.486] 1000
Elo difference: 45.1 +/- 13.8, LOS: 100.0 %, DrawRatio: 58.5 %

Score of ChessChallenge vs ChessChallengeEvil: 1902 - 0 - 98  [0.976] 2000
...      ChessChallenge playing White: 932 - 0 - 68  [0.966] 1000
...      ChessChallenge playing Black: 970 - 0 - 30  [0.985] 1000
...      White vs Black: 932 - 970 - 98  [0.490] 2000
Elo difference: 640.0 +/- 34.8, LOS: 100.0 %, DrawRatio: 4.9 %
```

### 1.4
Alpha-beta pruning

506 tokens

```
Score of ChessChallenge vs ChessChallengeDev: 730 - 39 - 231  [0.846] 1000
...      ChessChallenge playing White: 353 - 14 - 133  [0.839] 500
...      ChessChallenge playing Black: 377 - 25 - 98  [0.852] 500
...      White vs Black: 378 - 391 - 231  [0.493] 1000
Elo difference: 295.3 +/- 22.3, LOS: 100.0 %, DrawRatio: 23.1 %
```