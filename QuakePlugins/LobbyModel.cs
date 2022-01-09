using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakeEnhancedServerAnnouncer
{
    public class FunctionExecutionResult
    {
        public int Code { get; set; }
        public string Status { get; set; }
        public FunctionExecutionData Data { get; set; }
    }

    public class FunctionExecutionData
    {
        public int ExecutionTimeMilliseconds { get; set; }
        public string FunctionName { get; set; }
        public GetServerListResult FunctionResult { get; set; }
    }

    public class GetServerListResult
    {
        public int LobbyCount { get; set; }
        public Lobby[] LobbyArray { get; set; }
    }

    public class Lobby
    {
        public string Key { get; set; }
        public LobbyValue Value { get; set; }
    }

    public class LobbyValue
    {
        public string Game { get; set; }
        public string Ingame { get; set; }
        public string kMaxPlayers { get; set; }
        public string kPlayers { get; set; }
        public string kVersion { get; set; }
        public string Map { get; set; }
        public string Mode { get; set; }
        public string Name { get; set; }
        public string NetworkDescriptor { get; set; }
        public string pfPingToRegion { get; set; }
        public string pfRegion { get; set; }
        public string pfRoomCode { get; set; }
        public string Privacy { get; set; }
        public string RoomId { get; set; }
        public string Sublist { get; set; }
        public string UpdateTime { get; set; }
    }
}
