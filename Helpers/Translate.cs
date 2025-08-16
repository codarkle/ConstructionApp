namespace ConstructionApp.Helpers
{
    public static class Italian
    {
        private static readonly Dictionary<string, string> _map = new(StringComparer.OrdinalIgnoreCase)
        {
            //Menu
            ["WorkSite"] = "WorkSite",
            ["Employee"] = "Employee",
            ["Vehicle"] = "Vehicle",
            ["Maintenance"] = "Maintenance",
            ["Purchases"] = "Purchases",
            ["Presence"] = "Presence",
            ["Materials"] = "Materials",
            ["Users"] = "Users",

            //Role
            ["Admin"] = "Admin",
            ["Surveyor"] = "Geometra",
            ["Manager"] = "Manager",
            ["Worker"] = "Worker",
            ["Supplier"] = "Supplier",

            //Item
            ["Select"] = "Select",
            ["All"] = "All",

            //Table
            ["No"] = "No",
            ["Name"] = "Name",
            ["Address"] = "Address",
            ["Amount"] = "Amount",
            ["Security"] = "Security",
            ["DateStart"] = "DateStart",
            ["DateEnd"] = "DateEnd",
            ["Action"] = "Action",
        };

        public static string T(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return string.Empty;
            return _map.TryGetValue(key.ToLower(), out var value) ? value : key;
        }
    }
}
