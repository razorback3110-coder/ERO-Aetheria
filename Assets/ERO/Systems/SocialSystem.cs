using System.Collections.Generic; using UnityEngine; using ERO.Data;
namespace ERO.Systems { public sealed class SocialSystem:MonoBehaviour { public readonly List<string> Friends=new List<string>(); public readonly List<GuildData> Guilds=new List<GuildData>(); public bool AddFriend(string id){if(string.IsNullOrEmpty(id)||Friends.Contains(id))return false;Friends.Add(id);return true;} public GuildData CreateGuild(string name){var g=new GuildData{id=System.Guid.NewGuid().ToString("N"),name=name};Guilds.Add(g);return g;} }
}
