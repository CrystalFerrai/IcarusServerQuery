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

using SteamQuery;

namespace IcarusServerQuery
{
    /// <summary>
    /// Queries an Icarus game server and stores received information
    /// </summary>
	internal class ServerInfo
	{
        private readonly EndPointAddress mServer;
		private readonly TextWriter mErrorWriter;

        private ServerInfoData? mServerInfo;
        private ServerRulesData? mServerRules;
        private ServerPlayersData? mServerPlayers;
        private float mPingMs;

        private bool mErrorOccured;

        /// <summary>
        /// The name of the server
        /// </summary>
        public string? ServerName { get; private set; }

        /// <summary>
        /// The server's query port
        /// </summary>
        public ushort QueryPort => (ushort)mServer.Port;

        /// <summary>
        /// The server's game port
        /// </summary>
        public ushort Port { get; private set; }

        /// <summary>
        /// The time it takes for the server to respond to a query, in milliseconds
        /// </summary>
        public static float PingMs { get; private set; }

        /// <summary>
        /// The version of the game the server is running
        /// </summary>
        public string? GameVersion { get; private set; }

        /// <summary>
        /// The number of players currently connected to the server
        /// </summary>
        public int PlayerCount { get; private set; }

        /// <summary>
        /// The maxinum number of players the server allows to be connected at the same time
        /// </summary>
        public int MaxPlayers { get; private set; }

        /// <summary>
        /// Information about the prospect the server is hosting, if it is currently hosting one
        /// </summary>
        public FProspectInfo? ProspectInfo { get; private set; }

        /// <summary>
        /// List of connected players
        /// </summary>
        public IReadOnlyCollection<ServerPlayerData>? Players { get; private set; }

		private ServerInfo(EndPointAddress server, TextWriter errorWriter)
		{
            mServer = server;
			mErrorWriter = errorWriter;
		}

        /// <summary>
        /// Queries an Icarus game server and returns the received information
        /// </summary>
        /// <param name="server">The server to query</param>
        /// <param name="errorWriter">Where to print messages about any errors the occur during the query</param>
        /// <returns></returns>
        public static ServerInfo QueryServer(EndPointAddress server, TextWriter errorWriter)
        {
            ServerInfo info = new ServerInfo(server, errorWriter);
			info.DoQueries();
            info.ParseResults();
            return info;
        }

        /// <summary>
        /// Prints the stored server information
        /// </summary>
        /// <param name="writer">The writer to print to</param>
        public void Print(TextWriter writer)
		{
            if (mErrorOccured)
            {
                return;
            }

            writer.WriteLine($"Server: {ServerName}");
            writer.WriteLine($"Ping: {PingMs:0.0#}ms");
            writer.WriteLine($"Version: {GameVersion}");
            writer.WriteLine($"QueryPort: {mServer.Port}");
            writer.WriteLine($"Port: {Port}");
            if (ProspectInfo.HasValue)
            {
                if (ProspectInfo.Value.LobbyName != null)
                {
                    writer.WriteLine("Status: Lobby");
                }
                else
                {
                    if (ProspectInfo.Value.FactionMissionDTKey != null)
                    {
                        if (ProspectInfo.Value.FactionMissionDTKey == "None")
                        {
                            writer.WriteLine("Status: Outpost");
                        }
                        else
                        {
                            writer.WriteLine($"Status: Mission - {ProspectInfo.Value.FactionMissionDTKey}");
                        }
                    }
                    else
                    {
                        writer.WriteLine("Status: Open World");
                    }
                    writer.WriteLine($"Prospect: {ProspectInfo.Value.ProspectDTKey}");
                    writer.WriteLine($"Save: {ProspectInfo.Value.ProspectID}");

                    writer.WriteLine($"Difficulty: {ProspectInfo.Value.Difficulty}");
                    writer.WriteLine($"Hard Core: {(ProspectInfo.Value.NoRespawns ? "Yes" : "No")}");

                    writer.WriteLine();
                    writer.WriteLine($"Associated Characters: {ProspectInfo.Value.AssociatedMembers.Length}");
                    foreach (FAssociatedMemberInfo player in ProspectInfo.Value.AssociatedMembers)
					{
                        writer.WriteLine($"  {player.CharacterName}");
					}
                }
            }
            writer.WriteLine();
            writer.WriteLine($"Online Players: {PlayerCount}/{MaxPlayers}");
            if (Players != null)
			{
                foreach (ServerPlayerData player in Players)
                {
                    writer.WriteLine($"  {player.Name} - {player.Duration:hh\\:mm\\:ss}");
                }
			}
		}

        private void DoQueries()
        {
            mErrorOccured = false;

            ServerInfoQuery infoQuery = new ServerInfoQuery(mServer);

            WaitHandle[] waitHandles = new WaitHandle[3];
            for (int i = 0; i < 3; ++i)
            {
                waitHandles[i] = new AutoResetEvent(false);
            }

            {
                ServerInfoQuery query = new ServerInfoQuery(mServer);
                query.QueryComplete += (sender, response) =>
                {
                    HandleResponse(response, out mServerInfo, out mPingMs);
                    ((EventWaitHandle)waitHandles[0]).Set();
                };
                query.Send();
            }

            {
                ServerRulesQuery query = new ServerRulesQuery(mServer);
                query.QueryComplete += (sender, response) =>
                {
                    HandleResponse(response, out mServerRules);
                    ((EventWaitHandle)waitHandles[1]).Set();
                };
                query.Send();
            }

            {
                ServerPlayersQuery query = new ServerPlayersQuery(mServer);
                query.QueryComplete += (sender, response) =>
                {
                    HandleResponse(response, out mServerPlayers);
                    ((EventWaitHandle)waitHandles[2]).Set();
                };
                query.Send();
            }

            WaitHandle.WaitAll(waitHandles);
        }

        private void ParseResults()
		{
            PingMs = mPingMs;

            ServerName = mServerInfo?.Name;
            Port = mServerInfo?.GamePort ?? 0;
            PlayerCount = mServerInfo?.PlayerCount ?? 0;
            MaxPlayers = mServerInfo?.MaxPlayers ?? 0;

            if (mServerRules != null)
			{
                if (mServerRules.Rules.TryGetValue("G_s", out string? version))
                {
                    int hyphenIndex = version.IndexOf('-');
                    GameVersion = hyphenIndex > 0 ? version[..hyphenIndex] : version;
                }

                if (mServerRules.Rules.TryGetValue("ProspectInfo_s", out string? prospectInfo64))
                {
                    try
                    {
                        byte[] prospectInfo = Convert.FromBase64String(prospectInfo64);

                        using (MemoryStream stream = new MemoryStream(prospectInfo))
						{
                            ProspectInfo = FProspectInfo.Parse(stream);
						}
					}
                    catch
                    {
                        mErrorWriter?.WriteLine("Unable to parse prospect info");
                    }
                }
			}

            Players = mServerPlayers?.ActivePlayers;
		}

        private void HandleResponse<T>(ServerQueryResponse<T> response, out T? responseOutput)
        {
			HandleResponse(response, out responseOutput, out _);
		}

        private void HandleResponse<T>(ServerQueryResponse<T> response, out T? responseOutput, out float pingMs)
        {
            responseOutput = default;
            pingMs = response.PingMs;
            switch (response.Result)
            {
                case ServerQueryResult.QueryTimedOut:
                    mErrorWriter.WriteLine("Query timed out.");
                    mErrorOccured = true;
                    break;
                case ServerQueryResult.UnknownResponseReceived:
                    mErrorWriter.WriteLine("Query returned an unrecognized response.");
					mErrorOccured = true;
					break;
                case ServerQueryResult.ResponseReceived:
                    responseOutput = response.Data;
                    break;
            }
        }
    }
}
