using Laser.Orchard.Questionnaires.Models;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Questionnaires.Repository {
    public class RepositoryAnswer : Repository<AnswerRecord>, IRepository<AnswerRecord> {
        public RepositoryAnswer(ITransactionManager transactionManager) : base(transactionManager) {
        }

        public override void Update(AnswerRecord entity) {
            if (string.IsNullOrWhiteSpace(entity.Identifier)) {
                entity.Identifier = Guid.NewGuid().ToString("n");
            }
            base.Update(entity);
        }
    }
}