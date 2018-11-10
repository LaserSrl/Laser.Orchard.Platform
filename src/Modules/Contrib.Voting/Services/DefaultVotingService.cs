using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Contrib.Voting.Events;
using Contrib.Voting.Functions;
using Contrib.Voting.Models;
using Orchard.Caching;
using Orchard.Data;
using Orchard.Services;

namespace Contrib.Voting.Services {
    public class DefaultVotingService : IVotingService {
        private readonly IRepository<VoteRecord> _voteRepository;
        private readonly IRepository<ResultRecord> _resultRepository;
        private readonly IClock _clock;
        private readonly IFunctionCalculator _calculator;
        private readonly IEnumerable<IFunction> _functions;
        private readonly IVotingEventHandler _eventHandler;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;

        public DefaultVotingService(
            IRepository<VoteRecord> voteRepository,
            IRepository<ResultRecord> resultRepository,
            IClock clock,
            IFunctionCalculator calculator,
            IEnumerable<IFunction> functions,
            IVotingEventHandler eventHandler,
            ICacheManager cacheManager,
            ISignals signals) {
            _voteRepository = voteRepository;
            _resultRepository = resultRepository;
            _clock = clock;
            _calculator = calculator;
            _functions = functions;
            _eventHandler = eventHandler;
            _cacheManager = cacheManager;
            _signals = signals;
        }

        public VoteRecord Get(int voteId) {
            return _voteRepository.Get(voteId);
        }

        public IEnumerable<VoteRecord> Get(Expression<Func<VoteRecord, bool>> predicate) {
            return _voteRepository.Fetch(predicate);
        }

        public void RemoveVote(VoteRecord vote) {

            foreach (var function in _functions) {
                _calculator.Calculate(new DeleteCalculus { Dimension = vote.Dimension, ContentId = vote.ContentItemRecord.Id, Vote = vote.Value, FunctionName = function.Name });
            }
            
            _voteRepository.Delete(vote);
            _eventHandler.VoteRemoved(vote);
        }

        public void RemoveVote(IEnumerable<VoteRecord> votes) {
            foreach(var vote in votes)
                _voteRepository.Delete(vote);
        }

        public void Vote(Orchard.ContentManagement.ContentItem contentItem, string userName, string hostname, double value, string dimension = null) {
            var vote = new VoteRecord {
                Dimension = dimension,
                ContentItemRecord = contentItem.Record,
                ContentType = contentItem.ContentType,
                CreatedUtc = _clock.UtcNow,
                Hostname = hostname,
                Username = userName,
                Value = value
            };

            _voteRepository.Create(vote);

            foreach(var function in _functions) {
                _calculator.Calculate(new CreateCalculus { Dimension = dimension, ContentId = contentItem.Id, FunctionName = function.Name, Vote = value});
            }

            _eventHandler.Voted(vote);
        }

        public void ChangeVote(VoteRecord vote, double value) {
            var previousValue = value;

            foreach (var function in _functions) {
                _calculator.Calculate(new UpdateCalculus { Dimension = vote.Dimension, ContentId = vote.ContentItemRecord.Id, PreviousVote = vote.Value, Vote = value, FunctionName = function.Name });
            }

            vote.CreatedUtc = _clock.UtcNow;
            vote.Value = value;

            _eventHandler.VoteChanged(vote, previousValue);
        }

        public ResultRecord GetResult(int contentItemId, string function, string dimension = null) {
            var key = GetCacheKey(contentItemId, function, dimension);

            return _cacheManager.Get(key, ctx => {
                
                // invalidated when a result is recreated on the same contentItem/function/dimension
                ctx.Monitor(_signals.When(key));
                return _resultRepository.Get( r => 
                    r.Dimension == dimension
                    && r.ContentItemRecord.Id == contentItemId
                    && r.FunctionName == function);
                }
            );
        }

        public static string GetCacheKey(int contentItemId, string function, string dimension) {
            return String.Concat("vote_", contentItemId, "_", dimension ?? "");
        }
    }
}