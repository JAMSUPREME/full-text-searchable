namespace Jam.WebApiHelper.Tests.Integration
{
    using System;
    using System.Linq;
    using Jam.WebApiHelper.Tests.Entities;
    using Jam.WebApiHelper.Tests.Integration.Helper;
    using Xbehave;
    using Xunit.Should;

    /// <summary>
    /// Tests full-text extensions.
    /// Integration test because the FT extensions operate on top of queryable.
    /// Currently does not include coverage of double or decimal since we haven't got those as data types yet.
    /// </summary>
    public class TestFullTextExt
    {
        private IntegrationContextFactory ctxFactory = null;
        private AdventureWorksEntities GetContext()
        {
            if (ctxFactory == null)
            {
                ctxFactory = new IntegrationContextFactory();
            }
            return ctxFactory.Create();
        }

        /// <summary>
        /// Ensures that if no search is passed, that the queryable is still evaluated properly.
        /// </summary>
        [Scenario]
        public void TestNoSearch()
        {
            IQueryable<Contact> contactsAfterQuery = null;
            var ctx = GetContext();
            string lazyHash = "XylyRwiKnyNPKbC1r4FSqA5YN9shIgsNik5ADyqStZc=";
            string lazySalt = "TVGXbhY=";
            string name = "NoSearchUser";

            "Given a DB collection of contacts".f(
                () =>
                {
                    try
                    {
                        ctx.Contacts.Add(new Contact() { FirstName = name, LastName = name, PasswordHash = lazyHash, PasswordSalt = lazySalt });
                        ctx.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        throw;
                    }
                });
            ("When I query for '" + name + "'").f(
                () =>
                {
                    var contacts = ctx.Contacts.AsExpandable();
                    var whereClause = contacts
                        .FullTextSearchable("")
                        .FullTextSearchable(c => c.FirstName, "");
                    contactsAfterQuery = contacts.Where(whereClause);
                });
            "Then I should get one result".f(
                () =>
                {
                    var results = contactsAfterQuery.ToList();
                    results.Count.ShouldBe(1);
                    results.First().FirstName.ShouldContain(name, StringComparison.OrdinalIgnoreCase);
                });
        }

        /// <summary>
        /// Ensures that if a search string is passed, that the queryable is evaluated properly.
        /// </summary>
        [Scenario]
        public void TestSearchAgainstString()
        {
            IQueryable<Contact> contactsAfterQuery = null;
            var ctx = GetContext();
            string lazyHash = "XylyRwiKnyNPKbC1r4FSqA5YN9shIgsNik5ADyqStZc=";
            string lazySalt = "TVGXbhY=";
            string name = "MrWhiskers";
            string searchString = "whisk";

            "Given a DB collection of contacts".f(
                () =>
                {

                    ctx.Contacts.Add(new Contact() { FirstName = name, LastName = name, PasswordHash = lazyHash, PasswordSalt = lazySalt });
                    ctx.SaveChanges();
                });
            ("When I query for '" + searchString + "'").f(
                () =>
                {
                    var contacts = ctx.Contacts.AsExpandable();
                    var whereClause = contacts
                        .FullTextSearchable(searchString)
                        .FullTextSearchable(c => c.FirstName, searchString);
                    contactsAfterQuery = contacts.Where(whereClause);
                });
            "Then I should get one result".f(
                () =>
                {
                    var results = contactsAfterQuery.ToList();
                    results.Count.ShouldBe(1);
                    results.First().FirstName.ShouldContain(name, StringComparison.OrdinalIgnoreCase);
                });
        }

        /// <summary>
        /// Ensures that if a search numeric (int) is passed, that the queryable is evaluated properly.
        /// </summary>
        [Scenario]
        public void TestSearchAgainstNumeric()
        {
            IQueryable<Contact> contactsAfterQuery = null;
            var ctx = GetContext();
            string lazyHash = "XylyRwiKnyNPKbC1r4FSqA5YN9shIgsNik5ADyqStZc=";
            string lazySalt = "TVGXbhY=";
            string name = "NumericTest";
            string searchString = "whisk";

            "Given a DB collection of contacts".f(
                () =>
                {
                    ctx.Contacts.Add(new Contact() { FirstName = name, LastName = name, PasswordHash = lazyHash, PasswordSalt = lazySalt });
                    ctx.SaveChanges();
                });
            "When I query for a specific ID as full-text".f(
                () =>
                {
                    Contact newContact = ctx.Contacts.First(s => s.FirstName.Contains(name));
                    string newContactId = newContact.ContactID.ToString();
                    var contacts = ctx.Contacts.AsExpandable();
                    Console.WriteLine("Querying full text search for ID string: " + newContactId);
                    var whereClause = contacts
                        .FullTextSearchable(newContactId)
                        .FullTextSearchable(c => c.ContactID, newContactId);
                    contactsAfterQuery = contacts.Where(whereClause);
                });
            "Then I should get one result (assuming a clean context)".f(
                () =>
                {
                    var results = contactsAfterQuery.ToList();
                    results.Count.ShouldBe(1);
                    results.First().FirstName.ShouldContain(name, StringComparison.OrdinalIgnoreCase);
                });
        }

        /// <summary>
        /// Ensures that if a search datetime is passed, that the queryable is evaluated properly.
        /// </summary>
        [Scenario]
        public void TestSearchAgainstDateTime()
        {
            IQueryable<Contact> contactsAfterQuery = null;
            var ctx = GetContext();
            string partialDateTime = DateTime.Now.Year.ToString();
            string fullDateTime = DateTime.Now.ToString("MM/dd/yyyy");
            string lazyHash = "XylyRwiKnyNPKbC1r4FSqA5YN9shIgsNik5ADyqStZc=";
            string lazySalt = "TVGXbhY=";
            string name = "UserForDateTimeTest";

            "Given a DB collection of contacts".f(
                () =>
                {
                    ctx.Contacts.Add(new Contact() { FirstName = name, LastName = name, PasswordHash = lazyHash, PasswordSalt = lazySalt, ModifiedDate = DateTime.Now });
                    ctx.SaveChanges();
                });
            "When I query for any valid number".f(
                () =>
                {
                    var contacts = ctx.Contacts.AsExpandable();
                    Console.WriteLine("Querying full text search for partial date matching: " + partialDateTime);
                    var whereClause = contacts
                        .FullTextSearchable(partialDateTime)
                        .FullTextSearchable(c => c.ModifiedDate, partialDateTime);
                    contactsAfterQuery = contacts.Where(whereClause);
                });
            "Then I should get results with day, month, or year matching that number".f(
                () =>
                {
                    var results = contactsAfterQuery.ToList();
                    results.Count.ShouldBeGreaterThan(0);
                    results.Any(r => r.FirstName.Equals(name, StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
                });
            "When I query for a valid date".f(
                () =>
                {
                    var contacts = ctx.Contacts.AsExpandable();
                    Console.WriteLine("Querying full text search for full date matching: " + fullDateTime);
                    var whereClause = contacts
                        .FullTextSearchable(fullDateTime)
                        .FullTextSearchable(c => c.ModifiedDate, fullDateTime);
                    contactsAfterQuery = contacts.Where(whereClause);
                });
            "Then I should get results matching that exact date (time is ignored)".f(
                () =>
                {
                    var results = contactsAfterQuery.ToList();
                    results.Count.ShouldBeGreaterThan(0);
                    results.Any(r => r.FirstName.Equals(name, StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
                });
        }
        
        /// <summary>
        /// Ensures that if an invalid search datetime is passed, that the queryable is evaluated properly.
        /// </summary>
        [Scenario]
        public void TestSearchAgainstDateTime_InvalidFormat()
        {
            IQueryable<Contact> contactsAfterQuery = null;
            var ctx = GetContext();
            string invalidDateTimeSearch = "banana";
            string lazyHash = "XylyRwiKnyNPKbC1r4FSqA5YN9shIgsNik5ADyqStZc=";
            string lazySalt = "TVGXbhY=";
            string name = "UserForDateTimeTest_Invalid";

            "Given a DB collection of contacts".f(
                () =>
                {
                    ctx.Contacts.Add(new Contact() { FirstName = name, LastName = name, PasswordHash = lazyHash, PasswordSalt = lazySalt, ModifiedDate = DateTime.Now });
                    ctx.SaveChanges();
                });
            "When I query for a non-parsable date time value".f(
                () =>
                {
                    var contacts = ctx.Contacts.AsExpandable();
                    Console.WriteLine("Querying full text search for partial date matching: " + invalidDateTimeSearch);
                    var whereClause = contacts
                        .FullTextSearchable(invalidDateTimeSearch)
                        .FullTextSearchable(c => c.ModifiedDate, invalidDateTimeSearch);
                    contactsAfterQuery = contacts.Where(whereClause);
                });
            "Then I should not get any results".f(
                () =>
                {
                    var results = contactsAfterQuery.ToList();
                    results.Count.ShouldBe(0);
                });
        }

        /// <summary>
        /// Ensures that if a search datetime (with time specificity) is passed, that the queryable is evaluated properly.
        /// </summary>
        [Scenario]
        public void TestSearchAgainstDateTime_IncludeTime()
        {
            IQueryable<Contact> contactsAfterQuery = null;
            var ctx = GetContext();
            string partialDateTime = DateTime.Now.Minute.ToString();
            string fullDateTime = DateTime.Now.ToString("O");
            string lazyHash = "XylyRwiKnyNPKbC1r4FSqA5YN9shIgsNik5ADyqStZc=";
            string lazySalt = "TVGXbhY=";
            string name = "UserForDateTimeTest_Invalid";

            "Given a DB collection of contacts".f(
                () =>
                {
                    ctx.Contacts.Add(new Contact() { FirstName = name, LastName = name, PasswordHash = lazyHash, PasswordSalt = lazySalt, ModifiedDate = DateTime.Now });
                    ctx.SaveChanges();
                });
            "When I query for any valid number".f(
                () =>
                {
                    var contacts = ctx.Contacts.AsExpandable();
                    Console.WriteLine("Querying full text search for partial date matching: " + partialDateTime);
                    var whereClause = contacts
                        .FullTextSearchable(partialDateTime)
                        .FullTextSearchable(c => c.ModifiedDate, partialDateTime, true);
                    contactsAfterQuery = contacts.Where(whereClause);
                });
            "Then I should get results with minute, hour, day, month, or year matching that number".f(
                () =>
                {
                    var results = contactsAfterQuery.ToList();
                    results.Count.ShouldBeGreaterThan(0);
                    results.Any(r => r.FirstName.Equals(name, StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
                });
            "When I query for a valid date time".f(
                () =>
                {
                    var contacts = ctx.Contacts.AsExpandable();
                    Console.WriteLine("Querying full text search for full date matching: " + fullDateTime);
                    var whereClause = contacts
                        .FullTextSearchable(fullDateTime)
                        .FullTextSearchable(c => c.ModifiedDate, fullDateTime, true);
                    contactsAfterQuery = contacts.Where(whereClause);
                });
            "Then I should get results matching that exact date and time (up to the minute)".f(
                () =>
                {
                    var results = contactsAfterQuery.ToList();
                    results.Count.ShouldBeGreaterThan(0);
                    results.Any(r => r.FirstName.Equals(name, StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
                });
        }

        /// <summary>
        /// Ensures that if a search datetime is passed (and evaluated against a nullable datetime),
        ///  that the queryable is evaluated properly.
        /// </summary>
        [Scenario]
        public void TestSearchAgainstDateTime_Nullable()
        {
            IQueryable<Contact> contactsAfterQuery = null;
            var ctx = GetContext();
            var favoriteDate = new DateTime(2016, 05, 05);
            string fullDateTime = favoriteDate.ToString("MM/dd/yyyy");
            string partialDateTime = favoriteDate.Year.ToString();
            string lazyHash = "XylyRwiKnyNPKbC1r4FSqA5YN9shIgsNik5ADyqStZc=";
            string lazySalt = "TVGXbhY=";
            string name = "UserForDateTimeTest_Nullable";

            "Given a DB collection of contacts".f(
                () =>
                {
                    ctx.Contacts.Add(new Contact() { FirstName = name, LastName = name, PasswordHash = lazyHash, PasswordSalt = lazySalt, FavoriteDate = favoriteDate});
                    ctx.SaveChanges();
                });
            "When I query for any valid number".f(
                () =>
                {
                    var contacts = ctx.Contacts.AsExpandable();
                    Console.WriteLine("Querying full text search for partial date matching: " + partialDateTime);
                    var whereClause = contacts
                        .FullTextSearchable(partialDateTime)
                        .FullTextSearchable(c => c.FavoriteDate, partialDateTime);
                    contactsAfterQuery = contacts.Where(whereClause);
                });
            "Then I should get results with day, month, or year matching that number".f(
                () =>
                {
                    var results = contactsAfterQuery.ToList();
                    results.Count.ShouldBeGreaterThan(0);
                    results.Any(r => r.FirstName.Equals(name, StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
                });
            "When I query for a valid (nullable) date".f(
                () =>
                {
                    var contacts = ctx.Contacts.AsExpandable();
                    Console.WriteLine("Querying full text search for full date matching: " + fullDateTime);
                    var whereClause = contacts
                        .FullTextSearchable(fullDateTime)
                        .FullTextSearchable(c => c.FavoriteDate, fullDateTime);
                    contactsAfterQuery = contacts.Where(whereClause);
                });
            "Then I should get results matching that exact date (time is ignored)".f(
                () =>
                {
                    var results = contactsAfterQuery.ToList();
                    results.Count.ShouldBeGreaterThan(0);
                    results.Any(r => r.FirstName.Equals(name, StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
                });
        }
        
        /// <summary>
        /// Ensures that if a search datetime (with time specificity) is passed (and evaluated against a nullable datetime),
        ///  that the queryable is evaluated properly.
        /// </summary>
        [Scenario]
        public void TestSearchAgainstDateTime_Nullable_IncludeTime()
        {
            IQueryable<Contact> contactsAfterQuery = null;
            var ctx = GetContext();
            var favoriteDate = new DateTime(2016, 05, 05, 08, 42, 40);
            string partialDateTime = favoriteDate.Minute.ToString();
            string fullDateTime = favoriteDate.ToString("O");
            string lazyHash = "XylyRwiKnyNPKbC1r4FSqA5YN9shIgsNik5ADyqStZc=";
            string lazySalt = "TVGXbhY=";
            string name = "UserForDateTimeTest_Nullable_WithTime";

            "Given a DB collection of contacts".f(
                () =>
                {
                    ctx.Contacts.Add(new Contact() { FirstName = name, LastName = name, PasswordHash = lazyHash, PasswordSalt = lazySalt, FavoriteDate = favoriteDate});
                    ctx.SaveChanges();
                });
            "When I query for any valid number".f(
                () =>
                {
                    var contacts = ctx.Contacts.AsExpandable();
                    Console.WriteLine("Querying full text search for partial date matching: " + partialDateTime);
                    var whereClause = contacts
                        .FullTextSearchable(partialDateTime)
                        .FullTextSearchable(c => c.FavoriteDate, partialDateTime, true);
                    contactsAfterQuery = contacts.Where(whereClause);
                });
            "Then I should get results with minute, hour, day, month, or year matching that number".f(
                () =>
                {
                    var results = contactsAfterQuery.ToList();
                    results.Count.ShouldBeGreaterThan(0);
                    results.Any(r => r.FirstName.Equals(name, StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
                });
            "When I query for a valid (nullable) date time".f(
                () =>
                {
                    var contacts = ctx.Contacts.AsExpandable();
                    Console.WriteLine("Querying full text search for full date matching: " + fullDateTime);
                    var whereClause = contacts
                        .FullTextSearchable(fullDateTime)
                        .FullTextSearchable(c => c.FavoriteDate, fullDateTime, true);
                    contactsAfterQuery = contacts.Where(whereClause);
                });
            "Then I should get results matching that exact date and time (up to the minute)".f(
                () =>
                {
                    var results = contactsAfterQuery.ToList();
                    results.Count.ShouldBeGreaterThan(0);
                    results.Any(r => r.FirstName.Equals(name, StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
                });
        }
    }
}
