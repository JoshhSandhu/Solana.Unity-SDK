using NUnit.Framework;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace Solana.Unity.SDK.Tests.EditMode.MwaClient
{
    /// <summary>
    /// EditMode tests for CapabilitiesResult JSON deserialisation.
    /// This class was introduced in PR #269 (GetCapabilities() RPC support).
    /// Tests verify that snake_case JSON from the MWA wallet deserialises
    /// correctly into the C# model via JsonProperty attributes.
    /// No Android runtime required.
    /// </summary>
    public class CapabilitiesResultTests
    {
        // -------------------------------------------------------------------------
        // Full response — all fields populated
        // -------------------------------------------------------------------------

        [Test]
        public void CapabilitiesResult_Deserializes_AllFields_FromSnakeCaseJson()
        {
            // Arrange — matches real wallet response format
            const string json = @"{
                ""supports_clone_authorization"": true,
                ""max_transactions_per_request"": 10,
                ""max_messages_per_request"": 5,
                ""supported_transaction_versions"": [""legacy"", ""0""]
            }";

            // Act
            var result = JsonConvert.DeserializeObject<CapabilitiesResult>(json);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(true, result.SupportsCloneAuthorization);
            Assert.AreEqual(10, result.MaxTransactionsPerRequest);
            Assert.AreEqual(5, result.MaxMessagesPerRequest);
            Assert.AreEqual(2, result.SupportedTransactionVersions.Length);
            Assert.AreEqual("legacy", result.SupportedTransactionVersions[0]);
            Assert.AreEqual("0", result.SupportedTransactionVersions[1]);
        }

        // -------------------------------------------------------------------------
        // Partial response — wallet may omit optional fields
        // -------------------------------------------------------------------------

        [Test]
        public void CapabilitiesResult_Deserializes_WhenOptionalFields_AreAbsent()
        {
            // Arrange — minimal wallet response (only one field present)
            const string json = @"{ ""supports_clone_authorization"": false }";

            // Act
            var result = JsonConvert.DeserializeObject<CapabilitiesResult>(json);

            // Assert — absent nullable fields must be null, not throw
            Assert.IsNotNull(result);
            Assert.AreEqual(false, result.SupportsCloneAuthorization);
            Assert.IsNull(result.MaxTransactionsPerRequest,
                "MaxTransactionsPerRequest must be null when absent from JSON");
            Assert.IsNull(result.MaxMessagesPerRequest,
                "MaxMessagesPerRequest must be null when absent from JSON");
            Assert.IsNull(result.SupportedTransactionVersions,
                "SupportedTransactionVersions must be null when absent from JSON");
        }

        // -------------------------------------------------------------------------
        // Empty response — wallet returns empty object
        // -------------------------------------------------------------------------

        [Test]
        public void CapabilitiesResult_Deserializes_EmptyJson_WithoutThrowing()
        {
            // Arrange
            const string json = @"{}";

            // Act & Assert — must not throw; all nullable fields default to null
            var result = JsonConvert.DeserializeObject<CapabilitiesResult>(json);
            Assert.IsNotNull(result);
            Assert.IsNull(result.SupportsCloneAuthorization);
            Assert.IsNull(result.MaxTransactionsPerRequest);
            Assert.IsNull(result.MaxMessagesPerRequest);
            Assert.IsNull(result.SupportedTransactionVersions);
        }

        // -------------------------------------------------------------------------
        // SupportsCloneAuthorization — both true and false
        // -------------------------------------------------------------------------

        [Test]
        public void CapabilitiesResult_SupportsCloneAuthorization_CanBeFalse()
        {
            const string json = @"{ ""supports_clone_authorization"": false }";
            var result = JsonConvert.DeserializeObject<CapabilitiesResult>(json);
            Assert.AreEqual(false, result.SupportsCloneAuthorization);
        }

        [Test]
        public void CapabilitiesResult_SupportsCloneAuthorization_CanBeTrue()
        {
            const string json = @"{ ""supports_clone_authorization"": true }";
            var result = JsonConvert.DeserializeObject<CapabilitiesResult>(json);
            Assert.AreEqual(true, result.SupportsCloneAuthorization);
        }

        // -------------------------------------------------------------------------
        // SupportedTransactionVersions — empty array
        // -------------------------------------------------------------------------

        [Test]
        public void CapabilitiesResult_SupportedTransactionVersions_CanBeEmptyArray()
        {
            const string json = @"{ ""supported_transaction_versions"": [] }";
            var result = JsonConvert.DeserializeObject<CapabilitiesResult>(json);
            Assert.IsNotNull(result.SupportedTransactionVersions);
            Assert.AreEqual(0, result.SupportedTransactionVersions.Length);
        }
    }
}
