using Laser.Orchard.StartupConfig.RazorCodeExecution.Services;
using Laser.Orchard.StartupConfig.Services;
using Orchard.Localization;
using Orchard.Tokens;
using System;
using System.Collections.Generic;

namespace Laser.Orchard.StartupConfig.Tokens {
    public class RazorCodeExecutionToken : ITokenProvider {

        private readonly ICurrentContentAccessor _currentContentAccessor;
        private readonly IRazorExecuteService _razorExecuteService;

        public Localizer T { get; set; }

        public RazorCodeExecutionToken(ICurrentContentAccessor currentContentAccessor, IRazorExecuteService razorExecuteService) {
            _currentContentAccessor = currentContentAccessor;
            _razorExecuteService = razorExecuteService;
        }

        public void Describe(DescribeContext context) {
            context.For("Shape")
                // Usage:
                // {Shape.RazorExecute:filename}
                // don't append the ".cshtml" suffix.
                // The implementation will inject a bunch of stuff in the executing Razor.
                // The data from the EvaluateContex will be accessible as a Dictionary<string, object>
                // For example, to access the ContentItem for which the token is being evaluated from
                // within the razor:
                // var content = (IContent)Model.Tokens["Content"]
                   .Token("RazorExecute:*", T("RazorExecute:<file name>"), T("Executes the razor code in the specified file."));
        }

        public void Evaluate(EvaluateContext context) {
            context.For("Shape", "")
                .Token(t => t.StartsWith("RazorExecute", StringComparison.OrdinalIgnoreCase) ? t : null,
                (fullToken, defaultvalue) => {
                    // The execution of file will be run by the chain
                    // so if the token contains subtokens we don't execute nothing here
                    // and we return an empty string as result
                    if (fullToken.IndexOf(".") == -1) {
                        var razorResult = ExecuteRazorCode(context, fullToken);
                        return razorResult;
                    }
                    else { 
                        return "";
                    }
                })
                .Chain(
                    token => {
                        var cleanToken = token.StartsWith("RazorExecute", StringComparison.OrdinalIgnoreCase) ? token.Substring(0, (token.IndexOf(".") > 0 ? token.IndexOf(".") : token.Length)) : "";
                        if (string.IsNullOrWhiteSpace(cleanToken)) return null;
                        int cleanTokenLength = cleanToken.Length;
                        var subTokens = token.Length > cleanTokenLength ? token.Substring(cleanTokenLength + 1) : "";
                        return new Tuple<string, string>(
                            cleanToken, //The specific Token RazorExecute:razor.file-name, it is the key
                            subTokens //The subsequent Tokens (i.e Limit:10)
                            );
                    }, 
                    "Text", 
                    (fullToken, defaultvalue) => {
                        return ExecuteRazorCode(context, fullToken);
                    });
        }

        private string ExecuteRazorCode(EvaluateContext context, string fullToken) {
            string fileName = fullToken.Substring("RazorExecute:".Length);
            return _razorExecuteService.Execute(fileName + ".cshtml", _currentContentAccessor.CurrentContentItem, context.Data);
        }
    }
}