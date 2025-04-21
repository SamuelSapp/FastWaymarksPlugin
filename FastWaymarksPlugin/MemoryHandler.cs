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
    public const int MaxPresetSlotNum = 30;

    public static unsafe FieldMarkerPreset ReadSlot(uint slotNum)
	{
		return FieldMarkerModule.Instance()->Presets[(int)slotNum-1];
	}

	public static unsafe bool WriteSlot(int slotNum, FieldMarkerPreset preset)
	{
		var module = FieldMarkerModule.Instance();
		if (module->Presets.Length < slotNum)
			return false;

		Plugin.Log.Debug($"Attempting to write slot {slotNum} with data:\r\n{preset}");

		// Zero-based index
		var pointer = module->Presets.GetPointer(slotNum - 1);
		*pointer = preset; // overwrite slot data
		return true;
	}

	private static bool IsSafeToDirectPlacePreset()
	{
		var currentContentLinkType = (byte) EventFramework.GetCurrentContentType();
		Plugin.Log.Debug($"Player is not null: {Plugin.ClientState.LocalPlayer != null}\r\n" +
										$"Player is not in combat: {!Plugin.Condition[ConditionFlag.InCombat]}\r\n" +
										$"Content Link Type is 1-3: {currentContentLinkType is > 0 and < 4}\r\n" +
										$"Content Line Type is: {currentContentLinkType}\r\n" +
										$"Is Safe to Direct Place: {Plugin.ClientState.LocalPlayer != null && !Plugin.Condition[ConditionFlag.InCombat] && currentContentLinkType is > 0 and < 4}");
		return Plugin.ClientState.LocalPlayer != null && !Plugin.Condition[ConditionFlag.InCombat] && currentContentLinkType is > 0 and < 4;
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

		public static void TestPlacePreset()
		{
			TestDirectPlacePreset();
		}
		private static unsafe void TestDirectPlacePreset()
		{
			if (!IsSafeToDirectPlacePreset())
				return;
			
			var testMarker = new MarkerPresetPlacement();

			var bitArray = new BitField8();

			foreach (var idx in Enumerable.Range(1,8))
			{
					testMarker.Active[idx] = bitArray[idx];
					testMarker.X[idx] = 0;
					testMarker.Y[idx] = 0;
					testMarker.Z[idx] = 0;
			}
			bitArray[0] = true;

			testMarker.Active[0] = bitArray[0];
			testMarker.X[0] = 0;
			testMarker.Y[0] = 0;
			testMarker.Z[0] = 0;

			MarkingController.Instance()->PlacePreset(&testMarker);
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

