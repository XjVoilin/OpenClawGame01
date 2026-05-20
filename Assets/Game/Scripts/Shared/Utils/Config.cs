namespace CozyYard
{
    public static class CfgTable
    {
        private static cfg.Tables _tables;

        public static cfg.Tables Tables => _tables;

        public static void Init(cfg.Tables tables)
        {
            _tables = tables;
        }
    }
}
