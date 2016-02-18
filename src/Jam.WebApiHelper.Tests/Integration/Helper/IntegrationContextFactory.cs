namespace Jam.WebApiHelper.Tests.Integration.Helper
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using Jam.WebApiHelper.Tests.Entities;

    public class IntegrationContextFactory : IDbContextFactory<AdventureWorksEntities>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationContextFactory"/> class.
        /// </summary>
        public IntegrationContextFactory()
        {
            // Database.SetInitializer(new CreateDatabaseIfNotExists<AdventureWorksEntities>());
            Database.SetInitializer(new DropCreateDatabaseAlways<AdventureWorksEntities>());
            var context = new AdventureWorksEntities();
            context.Database.Initialize(true);
            context.SaveChanges();
        }

        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns></returns>
        public AdventureWorksEntities Create()
        {
            return new AdventureWorksEntities();
        }
    }
}
