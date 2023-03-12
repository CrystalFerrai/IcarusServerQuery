// Copyright 2023 Crystal Ferrai
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace IcarusServerQuery
{
	/// <summary>
	/// Information about an Icarus prospect
	/// </summary>
	internal struct FProspectInfo
	{
		/// <summary>
		/// The saved name of the prospect
		/// </summary>
		public FString? ProspectID;

		/// <summary>
		/// The ID of the player that is hosting the prospect, usually empty for a server hosted prospect
		/// </summary>
		public FString? ClaimedAccountID;

		/// <summary>
		/// The name of the character that is hosting the prospect, usually empty for a server hosted prospect
		/// </summary>
		public int ClaimedAccountCharacter;

		/// <summary>
		/// The data table key that identifies this prospect in D_ProspectList
		/// </summary>
		public FString? ProspectDTKey;

		/// <summary>
		/// The data table key that identifies this prospect in D_FactionMissions if the prospect is a mission.
		/// Will be empty for non-mission prospects such as open worlds.
		/// </summary>
		public FString? FactionMissionDTKey;

		/// <summary>
		/// The name of the server lobby
		/// </summary>
		public FString? LobbyName;

		/// <summary>
		/// A legacy value that no longer ever set.
		/// </summary>
		public long ExpireTime;

		/// <summary>
		/// The state of the prospect
		/// </summary>
		public EProspectState ProspectState;

		/// <summary>
		/// List of data about all characters who are active in this prospect (meaning they have joined at some point and have not left via dropship)
		/// </summary>
		public FAssociatedMemberInfo[] AssociatedMembers;

		/// <summary>
		/// Unknown
		/// </summary>
		public int Cost;

		/// <summary>
		/// Unknown
		/// </summary>
		public int Reward;

		/// <summary>
		/// The game difficulty selected for the prospect.
		/// </summary>
		public EMissionDifficulty Difficulty;

		/// <summary>
		/// A legacy value that is now always false.
		/// </summary>
		public bool Insurance;

		/// <summary>
		/// Whether the prospect is set to "Hardcore"
		/// </summary>
		public bool NoRespawns;

		/// <summary>
		/// Amount of time elapsed since the prospect was started
		/// </summary>
		public int ElapsedTime;

		/// <summary>
		/// Deserialize prospect info from a binary stream
		/// </summary>
		/// <param name="stream">A stream cotnaining serialized prospect info</param>
		public static FProspectInfo Parse(Stream stream)
		{
			FProspectInfo info = new();

			using (BinaryReader reader = new BinaryReader(stream))
			{
				info.ProspectID = reader.ReadFString();
				info.ClaimedAccountID = reader.ReadFString();
				info.ClaimedAccountCharacter = reader.ReadInt32();
				info.ProspectDTKey = reader.ReadFString();
				info.FactionMissionDTKey = reader.ReadFString();
				info.LobbyName = reader.ReadFString();
				info.ExpireTime = reader.ReadInt64();
				info.ProspectState = (EProspectState)reader.ReadByte();
				info.Cost = reader.ReadInt32();
				info.Reward = reader.ReadInt32();
				info.Difficulty = (EMissionDifficulty)reader.ReadByte();
				info.Insurance = reader.ReadInt32() != 0;
				info.NoRespawns = reader.ReadInt32() != 0;
				info.ElapsedTime = reader.ReadInt32();

				int memberCount = reader.ReadInt32();
				info.AssociatedMembers = new FAssociatedMemberInfo[memberCount];
				for (int i = 0; i < memberCount; ++i)
				{
					info.AssociatedMembers[i].AccountName = reader.ReadFString();
					info.AssociatedMembers[i].CharacterName = reader.ReadFString();
					info.AssociatedMembers[i].UserId = reader.ReadFString();
					info.AssociatedMembers[i].ChrSlot = reader.ReadInt32();
					info.AssociatedMembers[i].Experience = reader.ReadInt32();
					info.AssociatedMembers[i].Status = (EProspectLocation)reader.ReadByte();
					info.AssociatedMembers[i].Settled = reader.ReadInt32() != 0;
					info.AssociatedMembers[i].IsCurrentlyPlaying = reader.ReadInt32() != 0;
				}
			}

			return info;
		}
	}

	/// <summary>
	/// Information about a character that is associated with a prospect
	/// </summary>
	internal struct FAssociatedMemberInfo
	{
		/// <summary>
		/// The character's name (always matches CharacterName)
		/// </summary>
		public FString? AccountName;

		/// <summary>
		/// The character's name
		/// </summary>
		public FString? CharacterName;

		/// <summary>
		/// The player's Steam ID
		/// </summary>
		public FString? UserId;

		/// <summary>
		/// The player's character slot index
		/// </summary>
		public int ChrSlot;

		/// <summary>
		/// The character's total experience
		/// </summary>
		public int Experience;

		/// <summary>
		/// The character's general location (what biome they are in)
		/// </summary>
		public EProspectLocation Status;

		/// <summary>
		/// The settled state of the character. Always false in this context.
		/// </summary>
		public bool Settled;

		/// <summary>
		/// Whether the charracter is currently connected
		/// </summary>
		public bool IsCurrentlyPlaying;
	}

	/// <summary>
	/// Possible states for a prospect
	/// </summary>
	internal enum EProspectState : byte
	{
		Unclaimed = 0,
		Claimed = 1,
		Active = 2,
		Ended = 3,
		MaxProspectStates = 4
	}

	/// <summary>
	/// Possible game difficulties
	/// </summary>
	internal enum EMissionDifficulty : byte
	{
		None = 0,
		Easy = 1,
		Medium = 2,
		Hard = 3,
		Extreme = 4
	}

	/// <summary>
	/// Possible associated character locations
	/// </summary>
	internal enum EProspectLocation : byte
	{
		Unknown = 0,
		Hab = 1,
		Prospect_Conifer = 2,
		Prospect_Arctic = 3,
		Prospect_Cave = 4,
		Prospect_Desert = 5
	}
}