﻿using System;
using System.Collections.Generic;
using System.Text;
using Chorizite.ACProtocol.Enums;

namespace Chorizite.ACProtocol.Messages {
    /// <summary>
    /// Server to client message
    /// </summary>
    public abstract class ACS2CMessage : ACMessage {
        /// <inheritdoc/>
        public override MessageDirection MessageDirection => MessageDirection.ServerToClient;

        /// <summary>
        /// The type of the message
        /// </summary>
        public abstract S2CMessageType MessageType { get; }
    }
}
