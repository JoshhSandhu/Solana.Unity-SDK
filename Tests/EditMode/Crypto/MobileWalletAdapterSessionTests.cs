using NUnit.Framework;
using System;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Solana.Unity.SDK.Tests.EditMode.Crypto
{
    /// <summary>
    /// EditMode tests for MobileWalletAdapterSession.
    /// No Android runtime required — pure BouncyCastle + .NET only.
    /// </summary>
    public class MobileWalletAdapterSessionTests
    {
        // -------------------------------------------------------------------------
        // Test 4 — AssociationToken is valid Base64Url
        // -------------------------------------------------------------------------

        [Test]
        public void AssociationToken_IsValidBase64Url_NoStandardBase64Characters()
        {
            // Arrange
            var session = new MobileWalletAdapterSession();

            // Act
            string token = session.AssociationToken;

            // Assert — Base64Url must not contain '+', '/', or '='
            Assert.IsNotNull(token, "AssociationToken must not be null");
            Assert.IsNotEmpty(token, "AssociationToken must not be empty");
            Assert.IsFalse(token.Contains('+'),
                "AssociationToken must not contain '+' (standard Base64 char, breaks URI)");
            Assert.IsFalse(token.Contains('/'),
                "AssociationToken must not contain '/' (standard Base64 char, breaks URI)");
            Assert.IsFalse(token.Contains('='),
                "AssociationToken must not contain '=' padding (breaks URI)");
        }

        [Test]
        public void AssociationToken_OnlyContains_ValidBase64UrlCharacters()
        {
            // Arrange
            var session = new MobileWalletAdapterSession();

            // Act
            string token = session.AssociationToken;

            // Assert — only A-Z, a-z, 0-9, '-', '_' are valid Base64Url characters
            var validBase64Url = new Regex(@"^[A-Za-z0-9\-_]+$");
            Assert.IsTrue(validBase64Url.IsMatch(token),
                $"AssociationToken '{token}' contains characters outside Base64Url alphabet");
        }

        [Test]
        public void AssociationToken_IsDerivedFrom_PublicKeyBytes()
        {
            // Arrange
            var session = new MobileWalletAdapterSession();

            // Act — manually Base64Url-encode the public key bytes and compare
            byte[] pubKeyBytes = session.PublicKeyBytes;
            string expected = Convert.ToBase64String(pubKeyBytes)
                .Split('=')[0]
                .Replace('+', '-')
                .Replace('/', '_');

            // Assert
            Assert.AreEqual(expected, session.AssociationToken,
                "AssociationToken must be the Base64Url encoding of PublicKeyBytes");
        }

        // -------------------------------------------------------------------------
        // Test 5 — EncryptSessionPayload throws before ECDH key establishment
        // -------------------------------------------------------------------------

        [Test]
        public void EncryptSessionPayload_ThrowsInvalidOperationException_WhenNoSessionKeyEstablished()
        {
            // Arrange — fresh session, GenerateSessionEcdhSecret has NOT been called
            var session = new MobileWalletAdapterSession();
            var payload = new byte[] { 0x01, 0x02, 0x03 };

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                session.EncryptSessionPayload(payload));

            StringAssert.Contains("no session key has been established", ex.Message,
                "Exception message must mention that no session key has been established");
        }

        [Test]
        public void DecryptSessionPayload_ThrowsInvalidOperationException_WhenNoSessionKeyEstablished()
        {
            // Arrange — fresh session, GenerateSessionEcdhSecret has NOT been called
            var session = new MobileWalletAdapterSession();
            var payload = new byte[64]; // arbitrary non-empty payload

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                session.DecryptSessionPayload(payload));
        }

        // -------------------------------------------------------------------------
        // Additional — PublicKeyBytes is correct length
        // -------------------------------------------------------------------------

        [Test]
        public void PublicKeyBytes_IsUncompressedEcPoint_65Bytes()
        {
            // Arrange
            var session = new MobileWalletAdapterSession();

            // Act
            byte[] pubKeyBytes = session.PublicKeyBytes;

            // Assert
            Assert.AreEqual(65, pubKeyBytes.Length,
                "PublicKeyBytes must be 65 bytes (uncompressed EC point: 0x04 || X || Y)");
            Assert.AreEqual((byte)0x04, pubKeyBytes[0],
                "First byte of PublicKeyBytes must be 0x04 (uncompressed point marker)");
        }

        [Test]
        public void TwoSessions_HaveDifferent_AssociationTokens()
        {
            // Arrange — each session generates a fresh keypair
            var session1 = new MobileWalletAdapterSession();
            var session2 = new MobileWalletAdapterSession();

            // Assert
            Assert.AreNotEqual(session1.AssociationToken, session2.AssociationToken,
                "Two independent sessions must produce different AssociationTokens");
        }
    }
}
