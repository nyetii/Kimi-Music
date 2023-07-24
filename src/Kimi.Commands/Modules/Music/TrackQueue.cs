using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lavalink4NET.Player;

namespace Kimi.Commands.Modules.Music
{
    public static class TrackQueue
    {
        public static Queue<LavalinkTrack> Queue { get; set; } = new();
    }
}
