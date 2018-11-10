using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contrib.Voting.Models;
using Orchard.Events;

namespace Contrib.Voting.Events
{
    public interface IVotingEventHandler : IEventHandler {
        void Voted(VoteRecord vote);
        void VoteChanged(VoteRecord vote, double previousValue);
        void VoteRemoved(VoteRecord vote);
        void Calculated(ResultRecord result);
    }
}
