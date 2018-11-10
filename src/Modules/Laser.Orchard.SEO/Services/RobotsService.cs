using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.Data;
using Orchard.Localization;
using Laser.Orchard.SEO.Models;
using Orchard;

namespace Laser.Orchard.SEO.Services {

    public interface IRobotsService : IDependency {
        RobotsFileRecord Get();
        Tuple<bool, IEnumerable<string>> Save(string text);
    }

    public class RobotsService : IRobotsService {
        private string DefaultRobotsText = @"User-agent: *" + Environment.NewLine + @"Disallow: /";

        private readonly IRepository<RobotsFileRecord> _repository;
		private readonly ISignals _signals;

		public RobotsService(IRepository<RobotsFileRecord> repository, ISignals signals) {
			_repository = repository;
			_signals = signals;
			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

		public RobotsFileRecord Get() {
			var robotsFileRecord = _repository.Table.FirstOrDefault();
			if (robotsFileRecord == null) {
				robotsFileRecord = new RobotsFileRecord() {
					FileContent = DefaultRobotsText
				};
				_repository.Create(robotsFileRecord);
			}
			return robotsFileRecord;
		}

		public Tuple<bool, IEnumerable<string>> Save(string text) {
			var robotsFileRecord = Get();
			robotsFileRecord.FileContent = text;
			_signals.Trigger("RobotsFile.SettingsChanged");
			var validationResult = Validate(text);
			return validationResult;
		}

		private Tuple<bool, IEnumerable<string>> Validate(string text) {
			using (var validatingReader = new RobotsFileValidatingReader(text)) {
				validatingReader.ValidateToEnd();
				return new Tuple<bool, IEnumerable<string>>(validatingReader.IsValid, validatingReader.Errors.Select(error =>
					T("Line {0}: {1}, {2}", error.LineNumber, error.BadContent, error.Error).ToString()
				));
			}
		}
	}
}