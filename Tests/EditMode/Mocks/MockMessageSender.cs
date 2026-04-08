using System.Collections.Generic;
using Solana.Unity.SDK;

// ReSharper disable once CheckNamespace
namespace Solana.Unity.SDK.Tests.EditMode.Mocks
{
    /// <summary>
    /// Test double for IMessageSender.
    /// Captures every byte[] passed to Send() so tests can assert on the wire payload.
    /// </summary>
    public class MockMessageSender : IMessageSender
    {
        public readonly List<byte[]> SentMessages = new List<byte[]>();

        public void Send(byte[] message)
        {
            SentMessages.Add(message);
        }

        /// <summary>
        /// Returns the most recent message sent, or null if none.
        /// </summary>
        public byte[] LastMessage => SentMessages.Count > 0
            ? SentMessages[SentMessages.Count - 1]
            : null;
    }
}
