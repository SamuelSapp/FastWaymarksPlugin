using System;
using System.Collections;
using System.Linq;
using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.Interop;


namespace FastWaymarksPlugin;

public static class MemoryHandler
{
	//	Magic Numbers
		public const int YalmScalar = 1000;

	public static bool IsSafeToDirectPlacePreset()
	{
		var currentContentLinkType = (byte) EventFramework.GetCurrentContentType();
		/*
		Plugin.Log.Debug($"Player is not null: {Plugin.ClientState.LocalPlayer != null}\r\n" +
										$"Player is not in combat: {!Plugin.Condition[ConditionFlag.InCombat]}\r\n" +
										$"Content Link Type is 1-3: {currentContentLinkType is > 0 and < 3}\r\n" +
										$"Content Link Type is: {currentContentLinkType}\r\n" +
										$"Is Safe to Direct Place: {Plugin.ClientState.LocalPlayer != null && !Plugin.Condition[ConditionFlag.InCombat] && currentContentLinkType is > 0 and < 3}");
		*/
		return Plugin.ClientState.LocalPlayer != null && !Plugin.Condition[ConditionFlag.InCombat] && currentContentLinkType is > 0 and < 3;
	}

	public static void PlacePreset(FieldMarkerPreset preset)
	{
		DirectPlacePreset(preset);
	}

	private static unsafe void DirectPlacePreset(FieldMarkerPreset preset)
	{
			if (!IsSafeToDirectPlacePreset())
					return;

			var bitArray = new BitArray(new[] {preset.ActiveMarkers});

			var placementStruct = new MarkerPresetPlacement();
			foreach (var idx in Enumerable.Range(0,8))
			{
					placementStruct.Active[idx] = bitArray[idx];
					placementStruct.X[idx] = preset.Markers[idx].X;
					placementStruct.Y[idx] = preset.Markers[idx].Y;
					placementStruct.Z[idx] = preset.Markers[idx].Z;
			}
			Plugin.Log.Debug($"Data of FieldMarkerPreset:\r\n" +
											$"Territory: {Plugin.ClientState.TerritoryType}\r\n" +
											$"ContentFinderCondition: {preset.ContentFinderConditionId}\r\n" +
											$"Waymark Struct:\r\n{preset.AsString()}");
			
			Plugin.Log.Debug($"Data of MarkerPresetPlacement:\r\n" +
											$"Waymark Struct:\r\n{placementStruct.AsString()}");
			MarkingController.Instance()->PlacePreset(&placementStruct);
	}

	public static unsafe bool GetCurrentWaymarksAsPresetData(ref FieldMarkerPreset rPresetData)
	{
		var currentContentLinkType = (byte) EventFramework.GetCurrentContentType();
		if(currentContentLinkType is >= 0 and < 4)	//	Same as the game check, but let it do overworld maps too.
		{
			var bitArray = new BitField8();
			var markerSpan = MarkingController.Instance()->FieldMarkers;
			foreach (var index in Enumerable.Range(0, 8))
			{
				var marker = markerSpan[index];
				bitArray[index] = marker.Active;

				rPresetData.Markers[index] = new GamePresetPoint { X = marker.X, Y = marker.Y, Z = marker.Z };
			}

			rPresetData.ActiveMarkers = bitArray.Data;
			rPresetData.ContentFinderConditionId = ZoneInfoHandler.GetContentFinderIDFromTerritoryTypeID(Plugin.ClientState.TerritoryType);
			rPresetData.Timestamp = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

			Plugin.Log.Debug($"Obtained current waymarks with the following data:\r\n" +
											$"Territory: {Plugin.ClientState.TerritoryType}\r\n" +
											$"ContentFinderCondition: {rPresetData.ContentFinderConditionId}\r\n" +
											$"Waymark Struct:\r\n{rPresetData.AsString()}");
			return true;
		}

		Plugin.Log.Warning($"Error in MemoryHandler.GetCurrentWaymarksAsPresetData: Disallowed ContentLinkType: {currentContentLinkType}");
		return false;
	}
}

