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
        private string _fullToken = "";
        private IContent _tokenValue;
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
            context.For("AdvancedSettings", _tokenValue)
                .Token(
                    token => token.StartsWith("GetCachedSetting:(", StringComparison.OrdinalIgnoreCase) ? GetAdvancedSettingsToken(token) : "",
                    (token, content) => GetSetting(token)
                )
                .Chain(_fullToken, "Content", (token) => _tokenValue);
        }

        private IContent GetSetting(string token) {
            var settingName = GetSettingName(token);
            if (string.IsNullOrWhiteSpace(settingName)) {
                return null;
            }
            _tokenValue = _advancedSettingsService.GetCachedSetting(settingName);
            return _tokenValue;


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
            _fullToken = fullToken.Substring(startingTokenIndex, fullToken.IndexOf(")", startingTokenIndex) + 1 - startingTokenIndex); //Returns the entire token 'GetCachedSetting:(setting-name)'
            return _fullToken;

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