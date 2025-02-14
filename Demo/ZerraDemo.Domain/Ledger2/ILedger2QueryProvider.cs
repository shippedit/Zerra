﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zerra.CQRS;
using Zerra.Providers;
using ZerraDemo.Domain.Ledger2.Models;

namespace ZerraDemo.Domain.Ledger2
{
    [ServiceExposed]
    public interface ILedger2QueryProvider : IBaseProvider
    {
        Task<bool> HasBalance(Guid accountID);
        Task<Balance2Model> GetBalance(Guid accountID);
        Task<ICollection<Transaction2Model>> GetTransactions(Guid accountID);
    }
}
