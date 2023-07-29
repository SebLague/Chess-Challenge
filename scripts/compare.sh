#!/bin/bash
cp Chess-Challenge/src/My\ Bot/MyBot.cs Chess-Challenge/src/CompareBot/CompareBot.cs
sed -i 's/MyBot/CompareBot/g' Chess-Challenge/src/CompareBot/CompareBot.cs
echo "Saved current state to CompareBot"
