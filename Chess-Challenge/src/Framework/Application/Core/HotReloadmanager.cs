using System;
using ChessChallenge.Application;

[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(HotReloadmanager))]

namespace ChessChallenge.Application;

public static class HotReloadmanager
{
    public static void UpdateApplication(Type[]? updatedTypes)
    {
        ChallengeController.UpdateTokenCount();
    }
}