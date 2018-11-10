using System;
using System.Collections.Generic;
using System.IO;
using Orchard.Localization;
using System.Text.RegularExpressions;

namespace Laser.Orchard.SEO.Services {
	public class RobotsFileValidatingReader : IDisposable {
		private const string SyntaxNotUnderstoodError = "Syntax not understood";
		private const string NoUserAgentSpecifiedError = "No user-agent specified";
		private const string NewLineRequiredError = "An empty line is required between user agent blocks";
		private const string InvalidSitemapUrl = "The Sitemap URL provided is invalid or is not absolute";
		private const string InvalidCrawlDelayValue = "The Crawl-delay value must be a non-negative whole number";

		private List<RobotsFileValidationError> _errors;
		private StringReader _reader;
		private int _currentLineNumber;
		private string _currentUserAgent = null;

		public RobotsFileValidatingReader(string robotsFileText) {
			_errors = new List<RobotsFileValidationError>();
			_reader = new StringReader(robotsFileText);
			_currentLineNumber = 0;
			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

		public bool ValidateNext() {
			string line = ReadLine();
			if (line == null)
				return false;
			Validate(line);
			_currentLineNumber++;
			return true;
		}

		public void ValidateToEnd() {
			string line = ReadLine();
			while (line != null) {
				Validate(line);
				line = ReadLine();
			}
		}

		private string ReadLine() {
			string line = null;
			try {
				line = _reader.ReadLine();
				return line;
			}
			finally {
				if (line != null)
					_currentLineNumber++;
			}
		}

		private void Validate(string line) {
			// Allow blank lines
			if (string.IsNullOrWhiteSpace(line)) {
				// If the line is blank, then that means we should get a new user agent directive on the following line or EOF
				_currentUserAgent = null;
				return;
			}
			// Allow comments, robots.txt comments start with # (pound), in this case, the entire line is ignored
			if (line.StartsWith("#"))
				return;

			// Will match any valid line in a good robots.txt file (assumes single line)
			Regex syntaxMatcher = new Regex(@"^(?<Key>(User-agent|Allow|Disallow|Crawl-delay|Sitemap)):\s?(?<Value>.*)$", RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
			// Will match any comments in-line with the current line (valid if not a full line of comments)
			Regex commentMatcher = new Regex(@"(?<Comment>\#.*$)", RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

			// Ensure correct syntax
			var match = syntaxMatcher.Match(line);
			if (!match.Success) {
				_errors.Add(new RobotsFileValidationError(_currentLineNumber, line, T(SyntaxNotUnderstoodError).ToString()));
				return;
			}

			// Get our key match group
			string key = match.Groups["Key"].Value;
			// Get our value match group (used for validation of the value in some cases)
			string keyValue = commentMatcher
				.Replace(match.Groups["Value"].Value, "") // Replace any comments from the end of the line
				.Trim(); // Trim any whitespace from the value after removing comments

			// Depending on the key and context, determine the best course to finish validating the line
			switch (key) {
				case "User-agent":
					// Ensure we had a blank line before this one, or our user agent is not already set
					if (_currentUserAgent != null)
						_errors.Add(new RobotsFileValidationError(_currentLineNumber, line, T(NewLineRequiredError).ToString()));
					_currentUserAgent = keyValue;
					return;
				case "Allow":
					// Ensure we have a valid user agent, otherwise this applies to no-one and is invalid
					if (_currentUserAgent == null)
						_errors.Add(new RobotsFileValidationError(_currentLineNumber, line, T(NoUserAgentSpecifiedError).ToString()));
					return;
				case "Disallow":
					// Ensure we have a valid user agent, otherwise this applies to no-one and is invalid
					if (_currentUserAgent == null)
						_errors.Add(new RobotsFileValidationError(_currentLineNumber, line, T(NoUserAgentSpecifiedError).ToString()));
					return;
				case "Crawl-delay":
					// Ensure we have a valid user agent, otherwise this applies to no-one and is invalid
					if (_currentUserAgent == null)
						_errors.Add(new RobotsFileValidationError(_currentLineNumber, line, T(NoUserAgentSpecifiedError).ToString()));
					// Ensure that the crawl delay value is a positive whole number
					int crawlDelay;
					if (!int.TryParse(keyValue, out crawlDelay) && crawlDelay >= 0)
						_errors.Add(new RobotsFileValidationError(_currentLineNumber, keyValue, T(InvalidCrawlDelayValue).ToString()));
					return;
				case "Sitemap":
					// Ensure we had a blank line before this one, or our user agent is not already set
					if (_currentUserAgent != null)
						_errors.Add(new RobotsFileValidationError(_currentLineNumber, line, T(NewLineRequiredError).ToString()));
					// Ensure that the URL provided for the sitemap is a valid, Absolute URI (relative URIs are not allowed in the robots.txt file)
					// Ensure that the URL provided is either using HTTP or HTTPS
					Uri sitemapUri;
					if (!Uri.TryCreate(keyValue, UriKind.Absolute, out sitemapUri) || (sitemapUri.Scheme != "http" && sitemapUri.Scheme != "https"))
						_errors.Add(new RobotsFileValidationError(_currentLineNumber, keyValue, T(InvalidSitemapUrl).ToString()));
					return;
				default:
					// Can't figure how we got here, this should never happen since our group match only accepts the above, but handle this case anyway
					_errors.Add(new RobotsFileValidationError(_currentLineNumber, line, T(SyntaxNotUnderstoodError).ToString()));
					return;
			}
		}

		public List<RobotsFileValidationError> Errors { get { return _errors; } }
		public bool IsValid { get { return _errors.Count == 0; } }
		public int CurrentLineNumber { get { return _currentLineNumber; } }
		public string CurrentUserAgent { get { return _currentUserAgent; } }

		#region IDisposable Members

		public void Dispose() {
			_reader.Dispose();
		}

		#endregion
	}

	public class RobotsFileValidationError {
		public RobotsFileValidationError(int lineNumber, string badContent, string error) {
			LineNumber = lineNumber;
			BadContent = badContent;
			Error = error;
		}
		public int LineNumber { get; private set; }
		public string BadContent { get; private set; }
		public string Error { get; private set; }
	}
}