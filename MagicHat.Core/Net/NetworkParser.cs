﻿using MagicHat.ACProtocol;
using MagicHat.Core.Backend;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicHat.Core.Net {
    public class NetworkParser : PacketReader, IDisposable {
        private readonly IMagicHatBackend _backend;

        public NetworkParser(IMagicHatBackend backend, ILogger<NetworkParser> logger) : base(logger, new MessageReader()) {
            _backend = backend;

            _backend.OnC2SData += Backend_OnC2SData;
            _backend.OnS2CData += Backend_OnS2CData;
        }

        private void Backend_OnS2CData(object? sender, PacketDataEventArgs e) {
            HandleS2CPacket(e.Data);
        }

        private void Backend_OnC2SData(object? sender, PacketDataEventArgs e) {
            HandleC2SPacket(e.Data);
        }

        internal void Init() {
            
        }

        public void Dispose() {
            _backend.OnC2SData -= Backend_OnC2SData;
            _backend.OnS2CData -= Backend_OnS2CData;
        }
    }
}
