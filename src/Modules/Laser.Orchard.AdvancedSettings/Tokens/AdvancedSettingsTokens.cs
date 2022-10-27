using Laser.Orchard.AdvancedSettings.Services;
using Orchard.ContentManagement;
using Orchard.Core.Settings.Tokens;
using Orchard.Localization;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ITokenProvider = Orchard.Tokens.ITokenProvider;

namespace Laser.Orchard.AdvancedSettings.Tokens {
    public class AdvancedSettingsTokens : ITokenProvider {
        private readonly IAdvancedSettingsService _advancedSettingsService;
        public AdvancedSettingsTokens(IAdvancedSettingsService advancedSettingsService) {
            T = NullLocalizer.Instance;
            _advancedSettingsService = advancedSettingsService;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("AdvancedSettings", T("Advanced Settings"), T("Advanced settings tokens."))
                .Token("GetCachedSetting:(*)", T("GetCachedSetting:(<settings-name>)"), T("Gets the Advanced Settings and Chain its ContentItem."));
        }

        public void Evaluate(EvaluateContext context) {
            context.For("AdvancedSettings", new ContentItem())
                .Token(
                    token => token.StartsWith("GetCachedSetting:(", StringComparison.OrdinalIgnoreCase) ? GetAdvancedSettingsToken(token) : "",
                    (token, content) => GetSetting(token)
                )
                .Chain(
                    token => {
                        var cleanToken = GetAdvancedSettingsToken(token);
                        if (string.IsNullOrWhiteSpace(cleanToken)) return null;
                        int cleanTokenLength = cleanToken.Length;
                        var subTokens = token.Length > cleanTokenLength ? token.Substring(cleanTokenLength + 1) : "";
                        return new Tuple<string, string>(
                            cleanToken, //The specific Token GetCachedSetting:(setting-name): is the key
                            subTokens //The subsequent Tokens (i.e Fields.Part.Field)
                            );
                    },
                    "Content",
                    (token, content) => GetSetting(token));
        }

        private IContent GetSetting(string token) {
            var settingName = GetSettingName(token);
            if (string.IsNullOrWhiteSpace(settingName)) {
                return null;
            }
            return _advancedSettingsService.GetCachedSetting(settingName);
        }

        private string GetAdvancedSettingsToken(string fullToken) {
            // Input validation
            if (fullToken.IndexOf("(") == -1 || fullToken.IndexOf(")") == -1) {
                return "";
            }
            var startingTokenIndex = fullToken.IndexOf("GetCachedSetting:(");
            if (startingTokenIndex == -1) {
                return "";
            }
            return fullToken.Substring(startingTokenIndex, fullToken.IndexOf(")", startingTokenIndex) + 1 - startingTokenIndex); //Returns the entire token 'GetCachedSetting:(setting-name)'
        }
        private string GetSettingName(string token) {
            // Input validation
            if (!token.StartsWith("GetCachedSetting:(", StringComparison.OrdinalIgnoreCase) || token.IndexOf("(") == -1 || token.IndexOf(")") == -1) {
                throw new FormatException(string.Format("Malformed Token: {0}", token));
            }

            var settingsName = token.Substring(token.IndexOf("("));
            return settingsName.Substring(1, settingsName.IndexOf(")") - 1); //removes parentheses and return the setting name

        }
    }
}