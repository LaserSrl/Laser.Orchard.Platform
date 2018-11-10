using System;
using System.Collections.Generic;
using System.Linq;
using Contrib.Voting.Functions;
using Contrib.Voting.Models;

namespace Contrib.Voting.Services {
    public enum CalculationModes {
        Rebuild,
        Create,
        Update,
        Delete
    }

    public abstract class Calculus {
        public string FunctionName { get; set; }
        public int ContentId { get; set; }
        public string Dimension { get; set; }
        public abstract CalculationModes Mode { get; }
        public Func<IEnumerable<VoteRecord>> GetVotes { get; set; }

        public abstract void Execute(IFunction function, ResultRecord result);
    }

    public class RebuildCalculus : Calculus {
        public override CalculationModes Mode { get { return CalculationModes.Rebuild; } }

        public override void Execute(IFunction function, ResultRecord result) {
            double value;
            var votes = GetVotes().ToList();
            function.Calculate(votes, result.ContentItemRecord.Id, out value);
            result.Value = value;
            result.Count = votes.Count;
        }
    }

    public class CreateCalculus : Calculus {
        public double Vote { get; set; }
        public override CalculationModes Mode { get { return CalculationModes.Create; } }

        public override void Execute(IFunction function, ResultRecord result) {
            double value;
            function.Create(result.Value, result.Count, Vote, out value);
            result.Value = value;
            result.Count++;
        }
    }

    public class DeleteCalculus : Calculus {
        public double Vote { get; set; }
        public override CalculationModes Mode { get { return CalculationModes.Delete; } }

        public override void Execute(IFunction function, ResultRecord result) {
            double value;
            function.Delete(result.Value, result.Count, Vote, out value);
            result.Value = value;
            result.Count--;
        }
    }

    public class UpdateCalculus : Calculus {
        public double PreviousVote { get; set; }
        public double Vote { get; set; }
        public override CalculationModes Mode { get { return CalculationModes.Update; } }

        public override void Execute(IFunction function, ResultRecord result) {
            double value;
            function.Update(result.Value, result.Count, PreviousVote, Vote, out value);
            result.Value = value;
        }
    }
}