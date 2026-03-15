using Friendout.Domain.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Friendout.Test;

public static class TestDbContextFactory
{
    public static FriendoutDbContext CreateInMemoryContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<FriendoutDbContext>()
            .UseInMemoryDatabase(databaseName)
            // InMemory provider does not support real transactions -> ignore this warning
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new FriendoutDbContext(options);
    }
}

