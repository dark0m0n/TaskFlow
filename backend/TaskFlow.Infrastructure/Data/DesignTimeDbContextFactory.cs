using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskFlow.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TaskFlowDbContext>
{
    public TaskFlowDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TaskFlowDbContext>();

        // Connection string for creating migrations during development
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=taskflowdb;Username=myuser;Password=mypassword");

        return new TaskFlowDbContext(optionsBuilder.Options);
    }
}
