using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Contrib.Voting.Events;
using Contrib.Voting.Functions;
using Contrib.Voting.Models;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Services;

namespace Contrib.Voting.Services {
    public class DefaultFunctionCalculator : IFunctionCalculator {
        private readonly IContentManager _contentManager;
        private readonly IEnumerable<IFunction> _functions;
        private readonly IRepository<VoteRecord> _voteRepository;
        private readonly IRepository<ResultRecord> _resultRepository;
        private readonly IClock _clock;
        private readonly ICalculusQueue _queue;
        private readonly IVotingEventHandler _eventHandler;
        private readonly ISignals _signals;

        public DefaultFunctionCalculator(
            IContentManager contentManager,
            IEnumerable<IFunction> functions, 
            IRepository<VoteRecord> voteRepository,
            IRepository<ResultRecord> resultRepository,
            IClock clock,
            ICalculusQueue queue,
            IVotingEventHandler eventHandler,
            ISignals signals) {
            _contentManager = contentManager;
            _functions = functions;
            _voteRepository = voteRepository;
            _resultRepository = resultRepository;
            _clock = clock;
            _queue = queue;
            _eventHandler = eventHandler;
            _signals = signals;
        }

        public ILogger Logger { get; set; }

        public void Calculate(Calculus calculus) {

            // look for the corresponding external function
            var function = _functions.Where(f => f.Name == calculus.FunctionName).FirstOrDefault();

            if(function == null) {
                // no corresponding function found
                return;
            }

            // if an optimized calculation can't be executed, convert it to a Rebuild
            if(calculus.Mode == CalculationModes.Create && !function.CanCreate
                || calculus.Mode == CalculationModes.Delete && !function.CanDelete
                || calculus.Mode == CalculationModes.Update && !function.CanUpdate ) {
                    calculus = new RebuildCalculus {
                        ContentId = calculus.ContentId,
                        FunctionName = calculus.FunctionName,
                        Dimension = calculus.Dimension
                    };
            }

            lock (_queue) {
                // if a rebuild is already waiting for the same content item and function, don't add a new one))
                if (_queue.Any(c =>
                                c.Mode == CalculationModes.Rebuild
                                && c.ContentId == calculus.ContentId
                                && c.FunctionName == calculus.FunctionName
                                && c.Dimension == calculus.Dimension)) {
                    return;
                }
                _queue.Enqueue(calculus);
            }

            if (Monitor.TryEnter(_queue)) {
                while (_queue.Count > 0) {
                    var currentCalculus = _queue.Dequeue();
                    calculus.GetVotes = () => {
                        return _voteRepository
                            .Fetch(v => v.ContentItemRecord.Id == currentCalculus.ContentId && v.Dimension == currentCalculus.Dimension)
                            .ToList();
                    };

                    // get the current result for this function and content item
                    var result = _resultRepository
                        .Fetch(r => r.ContentItemRecord.Id == currentCalculus.ContentId && r.FunctionName == currentCalculus.FunctionName)
                        .SingleOrDefault();

                    var contentItem = _contentManager.Get(currentCalculus.ContentId);

                    if (result == null) {
                        result = new ResultRecord {
                            Dimension = calculus.Dimension,
                            ContentItemRecord = contentItem.Record,
                            ContentType = contentItem.ContentType,
                            FunctionName = calculus.FunctionName,
                            Value = 0,
                            Count = 0
                        };
                    }

                    // either it's a new result or not, do update the CreatedUtc
                    result.CreatedUtc = _clock.UtcNow;

                    currentCalculus.Execute(function, result);
                    
                    if(result.Id == 0) {
                        _resultRepository.Create(result);   
                    }

                    // invalidates the cached result
                    _signals.Trigger(DefaultVotingService.GetCacheKey(result.ContentItemRecord.Id, result.FunctionName, result.Dimension));
                    
                    _eventHandler.Calculated(result);
                }

                Monitor.Exit(_queue);
            }
        }
    }
}