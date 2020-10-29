using Laser.Orchard.StartupConfig.RazorCodeExecution.Services;
using Laser.Orchard.StartupConfig.Services;
using Orchard.Localization;
using Orchard.Tokens;
using System;

namespace Laser.Orchard.StartupConfig.Tokens {
    public class RazorCodeExecutionToken : ITokenProvider {

        private readonly ICurrentContentAccessor _currentContentAccessor;
        private readonly IRazorExecuteService _razorExecuteService;

        private string _fullTokenName;

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
                .Token(t => t.StartsWith("RazorExecute", StringComparison.OrdinalIgnoreCase) ? t.Substring(0, (t.IndexOf(".") > 0 ? t.IndexOf(".") : t.Length)) : null,
                    (fullToken, data) => { _fullTokenName = fullToken; return ExecuteRazorCode(context, fullToken); })
                .Chain(_fullTokenName, "Text", d => ExecuteRazorCode(context, _fullTokenName));
        }

        private string ExecuteRazorCode(EvaluateContext context, string fullToken) {
            string fileName = fullToken.Substring("RazorExecute:".Length);
            return _razorExecuteService.Execute(fileName + ".cshtml", _currentContentAccessor.CurrentContentItem, context.Data);
        }
    }
}