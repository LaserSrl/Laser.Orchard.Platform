using Laser.Orchard.StartupConfig.Helpers;
using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Security.Providers;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Security;
using Orchard.Services;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laser.Orchard.StartupConfig.Services {
    [OrchardFeature("Laser.Orchard.BearerTokenAuthentication")]
    public class ApiCredentialsManagementService : IApiCredentialsManagementService {

        private readonly IClock _clock;
        private readonly IContentManager _contentManager;
        private readonly IEnumerable<IApiCredentialsValidationProvider> _apiCredentialsValidationProviders;
        private readonly IEncryptionService _encryptionService;

        public ApiCredentialsManagementService(
            IClock clock,
            IContentManager contentManager,
            IEnumerable<IApiCredentialsValidationProvider> apiCredentialsValidationProviders,
            IEncryptionService encryptionService) {

            _clock = clock;
            _contentManager = contentManager;
            _apiCredentialsValidationProviders = apiCredentialsValidationProviders;
            _encryptionService = encryptionService;
        }

        public string GetSecret(ApiCredentialsPart part) {
            if (string.IsNullOrWhiteSpace(part.ApiSecret)) {
                return part.ApiSecret;
            }
            // decryption
            return Encoding.UTF8.GetString(
                _encryptionService.Decode(
                    Convert.FromBase64String(part.ApiSecret)));
        }

        public void GenerateNewCredentials(ApiCredentialsPart part) {
            // we use base64 to prevent possible encoding issues on transmission
            var key = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(BearerTokenHelpers.RandomString(24)),
                Base64FormattingOptions.None);
            // test that we haven't used this already. It's random but better safe than sorry.
            while (GetPartByKey(key) != null) {
                key = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(BearerTokenHelpers.RandomString(24)),
                    Base64FormattingOptions.None);
            }
            part.ApiKey = key;
            // encryption and hashing of the secret
            var secret = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(BearerTokenHelpers.RandomString()),
                Base64FormattingOptions.None);
            // save an encrypted secret so we can display it to authorized users
            part.ApiSecret = Convert.ToBase64String(
                _encryptionService.Encode(
                    Encoding.UTF8.GetBytes(secret)));
            // save an hashed secret for validation when signing in
            part.HashAlgorithm = BearerTokenHelpers.PBKDF2;
            BearerTokenHelpers.SetSecretHashed(part, secret);

            part.CreatedUtc = _clock.UtcNow;
        }

        public UserPart ValidateCredentials(string key, string secret) {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(secret)) {
                return null;
            }
            // get the part based on the ApiKey
            var userApiCredentials = GetPartByKey(key);
            if (userApiCredentials == null) {
                return null;
            }
            // is the secret valid?
            if (!TestSecret(userApiCredentials, secret)) {
                return null;
            }
            // Do the other tests
            if (_apiCredentialsValidationProviders.All(acvp => acvp.ValidateSignIn(userApiCredentials))) {
                // TODO: do we want to mark the login datetime?
                return userApiCredentials.As<UserPart>();
            }
            // validation failed
            return null;
        }

        private ApiCredentialsPart GetPartByKey(string key) {
            return _contentManager
                .Query<ApiCredentialsPart, ApiCredentialsPartRecord>()
                .Where(acpr => acpr.ApiKey == key)
                .Slice(0, 1)
                .FirstOrDefault();
        }

        private bool TestSecret(ApiCredentialsPart userApi, string secret) {
            var valid = BearerTokenHelpers.TestSecret(userApi, secret);

            // TODO: migrate secrets hashed with "old" algorithms
            // This will have to happen here whenever we change the default hash algorithm
            // See how the similar thing is done in Orchard.Users

            return valid;
        }
    }
}