namespace Pepper.Utilities
{
    public static class StringUtilities
    {
        public static string PluralY(long count)
        {
            var _ = Plural(count, "ies");
            return _.Length > 0 ? _ : "y";
        }

        public static string Plural(long count, string plural = "s", string singular = "")
        {
            return count > 1 ? plural : singular;
        }
    }
}