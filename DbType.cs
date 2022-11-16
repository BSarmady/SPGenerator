namespace SPGenerator {

    public struct DbType {
        public string name;
        public int max_length;
        public int precision;
        public int scale;

        #region public DbType(...)
        public DbType(string Name, int Max_length, int Precision, int Scale) {
            this.name = Name;
            this.max_length = Max_length;
            this.precision = Precision;
            this.scale = Scale;
        }
        #endregion
    }
}