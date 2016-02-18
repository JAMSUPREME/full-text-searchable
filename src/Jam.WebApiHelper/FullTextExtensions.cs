namespace Jam.WebApiHelper
{
    using System;
    using System.Data.Entity.SqlServer;
    using System.Linq;
    using System.Linq.Expressions;

    using LinqKit;

    /// <summary>
    /// Containers helper extensions for supporting full-text search on a queryable.
    /// For further reading on Expression building, see http://www.albahari.com/nutshell/linqkit.aspx
    /// </summary>
    public static class FullTextExtensions
    {
        /// <summary>
        /// The maximum amount of digits a long can have.
        /// Necessary in order to properly evaluate SqlFunctions.StringConvert properly.
        /// </summary>
        private const int MaxLongDigits = 19;
        /// <summary>
        /// The maximum amount of digits a decimal can have.
        /// Defaulting to 20 even though decimal can support 29 digits.
        /// This can be expanded if we actually need 29 digits of matching.
        /// Necessary in order to properly evaluate SqlFunctions.StringConvert properly.
        /// </summary>
        private const int MaxDecimalDigits = 20;

        /// <summary>
        /// Very small abstraction of the AsExpandable() extension that already exists.
        /// Added just to maintain 1 layer of indirection.
        /// </summary>
        /// <typeparam name="T">The type being queried upon.</typeparam>
        /// <param name="tq">The queryable collection of T.</param>
        /// <returns>An "Expandable" Queryable, which enables function invocation and building predicates (e.g. appending items conditionally to a where clause)</returns>
        public static IQueryable<T> AsExpandable<T>(this IQueryable<T> tq)
        {
            return Extensions.AsExpandable(tq);
        }

        /// <summary>
        /// Builds the initial expression required for appending more expressions that a full-text searchable queryable would require.
        /// The initial predicate will match true (everything) if no search is passed, or false (nothing) if a search exists.
        /// </summary>
        /// <typeparam name="T">The type being queried upon.</typeparam>
        /// <param name="tq">The queryable collection of T.</param>
        /// <param name="search">The text to search for within the queryable.</param>
        /// <returns>The initial expression for matching a full-text search.</returns>
        public static Expression<Func<T, bool>> FullTextSearchable<T>(this IQueryable<T> tq, string search)
        {
            if (String.IsNullOrWhiteSpace(search))
                return CustomPredicateBuilder.True<T>(); //get all
            Expression<Func<T, bool>> pred = CustomPredicateBuilder.False<T>(); //get none

            return pred;
        }

        /// <summary>
        /// Enables full-text search on a datetime.
        /// Assumes that the passed search will match an int (08) or a full date time (01/01/2014).
        /// If an int was passed, it will be compared against all date parts (year, month, day) - and optionally hour & minute if "includeTime" is passed.
        /// If a full date time was passed, it will be compared against the datetime at the day level, or at the minute level if "includeTime" is passed.
        /// "includeTime" is false by default.
        /// </summary>
        /// <typeparam name="T">Type containing a datetime.</typeparam>
        /// <param name="origin">Origin selector (where clause).</param>
        /// <param name="dateSelector">MemberExpression for selecting the datetime within the object T.</param>
        /// <param name="search">The text to search for within the queryable.</param>
        /// <param name="includeTime">Whether or not this comparison should include time. FALSE by default.</param>
        /// <returns>A modified version of the origin selector (where clause).</returns>
        public static Expression<Func<T, bool>> FullTextSearchable<T>(
            this Expression<Func<T, bool>> origin,
            Expression<Func<T, DateTime>> dateSelector,
            string search,
            bool includeTime = false)
        {
            return origin.DateContains(dateSelector, search, includeTime);
        }

        /// <summary>
        /// Enables full-text search on a nullable datetime.
        /// Assumes that the passed search will match an int (08) or a full date time (01/01/2014).
        /// If an int was passed, it will be compared against all date parts (year, month, day) - and optionally hour & minute if "includeTime" is passed.
        /// If a full date time was passed, it will be compared against the datetime at the day level, or at the minute level if "includeTime" is passed.
        /// "includeTime" is false by default.
        /// </summary>
        /// <typeparam name="T">Type containing a datetime.</typeparam>
        /// <param name="origin">Origin selector (where clause).</param>
        /// <param name="dateSelector">MemberExpression for selecting the datetime within the object T.</param>
        /// <param name="search">The text to search for within the queryable.</param>
        /// <param name="includeTime">Whether or not this comparison should include time. FALSE by default.</param>
        /// <returns>A modified version of the origin selector (where clause).</returns>
        public static Expression<Func<T, bool>> FullTextSearchable<T>(
            this Expression<Func<T, bool>> origin,
            Expression<Func<T, DateTime?>> dateSelector,
            string search,
            bool includeTime = false)
        {
            return origin.NullableDateContains(dateSelector, search, includeTime);
        }

        /// <summary>
        /// Enables full-text search on a string.
        /// </summary>
        /// <param name="origin">Origin selector (where clause).</param>
        /// <param name="strSelector">MemberExpression for selecting the string within the object T.</param>
        /// <param name="search">The text to search for within the queryable.</param>
        /// <typeparam name="T">Type containing a string.</typeparam>
        /// <returns>A modified version of the origin selector (where clause).</returns>
        public static Expression<Func<T, bool>> FullTextSearchable<T>(
            this Expression<Func<T, bool>> origin,
            Expression<Func<T, string>> strSelector,
            string search)
        {
            return origin.Or(model => strSelector.Invoke(model).Contains(search));
        }

        /// <summary>
        /// Enables full-text search on a int.
        /// </summary>
        /// <param name="origin">Origin selector (where clause).</param>
        /// <param name="intSelector">MemberExpression for selecting the int within the object T.</param>
        /// <param name="search">The text to search for within the queryable.</param>
        /// <typeparam name="T">Type containing a int.</typeparam>
        /// <returns>A modified version of the origin selector (where clause).</returns>
        public static Expression<Func<T, bool>> FullTextSearchable<T>(
            this Expression<Func<T, bool>> origin,
            Expression<Func<T, int>> intSelector,
            string search)
        {
            return origin.Or(model => SqlFunctions.StringConvert((double)intSelector.Invoke(model)).Contains(search));
        }

        /// <summary>
        /// Enables full-text search on a long.
        /// </summary>
        /// <param name="origin">Origin selector (where clause).</param>
        /// <param name="longSelector">MemberExpression for selecting the long within the object T.</param>
        /// <param name="search">The text to search for within the queryable.</param>
        /// <typeparam name="T">Type containing a long.</typeparam>
        /// <returns>A modified version of the origin selector (where clause).</returns>
        public static Expression<Func<T, bool>> FullTextSearchable<T>(
            this Expression<Func<T, bool>> origin,
            Expression<Func<T, long>> longSelector,
            string search)
        {
            return origin.Or(model => SqlFunctions.StringConvert((decimal)longSelector.Invoke(model), MaxLongDigits).Contains(search));
        }

        /// <summary>
        /// Enables full-text search on a nullable long.
        /// </summary>
        /// <param name="origin">Origin selector (where clause).</param>
        /// <param name="longSelector">MemberExpression for selecting the long within the object T.</param>
        /// <param name="search">The text to search for within the queryable.</param>
        /// <typeparam name="T">Type containing a long.</typeparam>
        /// <returns>A modified version of the origin selector (where clause).</returns>
        public static Expression<Func<T, bool>> FullTextSearchable<T>(
            this Expression<Func<T, bool>> origin,
            Expression<Func<T, long?>> longSelector,
            string search)
        {
            return origin.Or(model => longSelector.Invoke(model).HasValue && SqlFunctions.StringConvert((decimal)longSelector.Invoke(model), MaxLongDigits).Contains(search));
        }

        /// <summary>
        /// Enables full-text search on a double.
        /// </summary>
        /// <param name="origin">Origin selector (where clause).</param>
        /// <param name="doubleSelector">MemberExpression for selecting the double within the object T.</param>
        /// <param name="search">The text to search for within the queryable.</param>
        /// <typeparam name="T">Type containing a double.</typeparam>
        /// <returns>A modified version of the origin selector (where clause).</returns>
        public static Expression<Func<T, bool>> FullTextSearchable<T>(
            this Expression<Func<T, bool>> origin,
            Expression<Func<T, double>> doubleSelector,
            string search)
        {
            return origin.Or(model => SqlFunctions.StringConvert(doubleSelector.Invoke(model)).Contains(search));
        }

        /// <summary>
        /// Enables full-text search on a decimal.
        /// </summary>
        /// <param name="origin">Origin selector (where clause).</param>
        /// <param name="decSelector">MemberExpression for selecting the decimal within the object T.</param>
        /// <param name="search">The text to search for within the queryable.</param>
        /// <typeparam name="T">Type containing a decimal.</typeparam>
        /// <returns>A modified version of the origin selector (where clause).</returns>
        public static Expression<Func<T, bool>> FullTextSearchable<T>(
            this Expression<Func<T, bool>> origin,
            Expression<Func<T, decimal>> decSelector,
            string search)
        {
            return origin.Or(model => SqlFunctions.StringConvert(decSelector.Invoke(model), MaxDecimalDigits).Contains(search));
        }

        /// <summary>
        /// Checks if a date matches the passed search value.
        /// Assumes that the passed search will match an int (08) or a full date time (01/01/2014).
        /// If an int was passed, it will be compared against all date parts (year, month, day) - and optionally hour & minute if "includeTime" is passed.
        /// If a full date time was passed, it will be compared against the datetime at the day level, or at the minute level if "includeTime" is passed.
        /// </summary>
        /// <typeparam name="T">Type containing a datetime</typeparam>
        /// <param name="origin">Origin selector (where clause)</param>
        /// <param name="dateSelector">MemberExpression for selecting the datetime within the object T</param>
        /// <param name="search">The text to search for within the queryable</param>
        /// <param name="includeTime">Whether or not this comparison should include time</param>
        /// <returns>A modified version of the origin selector (where clause)</returns>
        private static Expression<Func<T, bool>> DateContains<T>(
            this Expression<Func<T, bool>> origin,
            Expression<Func<T, DateTime>> dateSelector,
            string search,
            bool includeTime)
        {
            int searchInt;
            DateTime searchDate;
            DateTime.TryParse(search, out searchDate);
            int.TryParse(search, out searchInt);

            if (DateTime.TryParse(search, out searchDate))
            {
                //IMPORTANT: DO NOT REFACTOR THIS! DateDiff's datePartArg MUST be a string literal (not a variable)
                if (includeTime)
                {
                    return origin.Or(oad => SqlFunctions.DateDiff("minute", dateSelector.Invoke(oad), searchDate) == 0);
                }
                return origin.Or(oad => SqlFunctions.DateDiff("day", dateSelector.Invoke(oad), searchDate) == 0);
            }
            if (int.TryParse(search, out searchInt))
            {
                var clause = origin.Or(oad =>
                    SqlFunctions.DatePart("year", dateSelector.Invoke(oad)) == searchInt
                    || SqlFunctions.DatePart("month", dateSelector.Invoke(oad)) == searchInt
                    || SqlFunctions.DatePart("day", dateSelector.Invoke(oad)) == searchInt);
                if (includeTime)
                {
                    clause = origin.Or(oad =>
                        SqlFunctions.DatePart("minute", dateSelector.Invoke(oad)) == searchInt
                        || SqlFunctions.DatePart("hour", dateSelector.Invoke(oad)) == searchInt);
                }
                return clause;
            }

            return origin;
        }

        /// <summary>
        /// Checks if a date matches the passed search value.
        /// Assumes that the passed search will match an int (08) or a full date time (01/01/2014).
        /// If an int was passed, it will be compared against all date parts (year, month, day) - and optionally hour & minute if "includeTime" is passed.
        /// If a full date time was passed, it will be compared against the datetime at the day level, or at the minute level if "includeTime" is passed.
        /// </summary>
        /// <typeparam name="T">Type containing a datetime</typeparam>
        /// <param name="origin">Origin selector (where clause)</param>
        /// <param name="dateSelector">MemberExpression for selecting the datetime within the object T</param>
        /// <param name="search">The text to search for within the queryable</param>
        /// <param name="includeTime">Whether or not this comparison should include time</param>
        /// <returns>A modified version of the origin selector (where clause)</returns>
        private static Expression<Func<T, bool>> NullableDateContains<T>(
            this Expression<Func<T, bool>> origin,
            Expression<Func<T, DateTime?>> dateSelector,
            string search,
            bool includeTime)
        {
            int searchInt;
            DateTime searchDate;
            DateTime.TryParse(search, out searchDate);
            int.TryParse(search, out searchInt);

            if (DateTime.TryParse(search, out searchDate))
            {
                //IMPORTANT: DO NOT REFACTOR THIS! DateDiff's datePartArg MUST be a string literal (not a variable)
                if (includeTime)
                {
                    return origin.Or(oad => dateSelector.Invoke(oad).HasValue && SqlFunctions.DateDiff("minute", dateSelector.Invoke(oad), searchDate) == 0);
                }
                return origin.Or(oad => dateSelector.Invoke(oad).HasValue && SqlFunctions.DateDiff("day", dateSelector.Invoke(oad), searchDate) == 0);
            }
            if (int.TryParse(search, out searchInt))
            {
                var clause = origin.Or(oad =>
                    dateSelector.Invoke(oad).HasValue &&
                    (SqlFunctions.DatePart("year", dateSelector.Invoke(oad)) == searchInt
                        || SqlFunctions.DatePart("month", dateSelector.Invoke(oad)) == searchInt
                        || SqlFunctions.DatePart("day", dateSelector.Invoke(oad)) == searchInt));
                if (includeTime)
                {
                    clause = origin.Or(oad =>
                        dateSelector.Invoke(oad).HasValue &&
                        (SqlFunctions.DatePart("minute", dateSelector.Invoke(oad)) == searchInt
                            || SqlFunctions.DatePart("hour", dateSelector.Invoke(oad)) == searchInt));
                }
                return clause;
            }

            return origin;
        }
    }
}
